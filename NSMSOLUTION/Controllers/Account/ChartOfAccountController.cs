using MVC;
using NSMSOLUTION.Models;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Utilities;
using Excel = Microsoft.Office.Interop.Excel;

namespace NSMSOLUTION.Controllers.Account
{
    public class ChartOfAccountController : Controller
    {
        // GET: ChartOfAccount
        [Authorize(Roles = "Add Chart Of Account, Edit Chart Of Account, Delete Chart Of Account")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            ViewBag.logo = html.SystemLogo;
            ViewBag.CanAdd = html.HasPermission("Add Chart Of Account");
            ViewBag.CanEdit = html.HasPermission("Edit Chart Of Account");
            ViewBag.CanDelete = html.HasPermission("Delete Chart Of Account");
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Add Chart Of Account, Edit Chart Of Account, Delete Chart Of Account")]
        public ActionResult GetChartOfAccountTree()
        {
            HTML html = new HTML();
            int systemId;
            int warehouseId;
            if (!int.TryParse(html.SystemID, out systemId) ||
                !int.TryParse(html.WarehouseID, out warehouseId))
                return new HttpStatusCodeResult(400, "The current system or warehouse is unavailable.");

            using (var db = new DBContext())
            {
                IQueryable<NSMSOLUTION.Models.EFCore.DataFields.ChartOfAccount> accountsQuery =
                    db.TblChartOfAccount.AsNoTracking()
                        .Where(account =>
                            account.SystemID == systemId &&
                            account.WarehouseID == warehouseId);
                if (!string.IsNullOrWhiteSpace(html.DomainID))
                    accountsQuery = accountsQuery.Where(account =>
                        account.DomainID != null &&
                        account.DomainID.Contains(html.DomainID));

                var accounts = accountsQuery
                    .Select(account => new
                    {
                        account.ChartOfAccountID,
                        account.ParentAccountID,
                        account.Memo,
                        account.AccountNo,
                        account.AccountName,
                        account.AccountType,
                        AccountTypesID = (int?)account.AccountTypesID
                    })
                    .ToList();
                var accountIds = new HashSet<int>(
                    accounts.Select(account => account.ChartOfAccountID));

                var journalEntries = db.TblAccountJournalEntries.AsNoTracking()
                    .Where(entry =>
                        entry.SystemID == systemId &&
                        entry.WarehouseID == warehouseId);
                var transactions = db.TblAccountTransaction.AsNoTracking();
                var accountBalances = (
                    from entry in journalEntries
                    join transaction in transactions
                        on entry.TransactionID equals transaction.TransactionID
                    where entry.AccountID.HasValue
                    group entry by entry.AccountID.Value into accountGroup
                    select new
                    {
                        AccountID = accountGroup.Key,
                        Balance = accountGroup.Sum(entry =>
                            (entry.Debit ?? 0m) - (entry.Credit ?? 0m))
                    }).ToList()
                    .ToDictionary(row => row.AccountID, row => row.Balance);

                var accountTypes = db.TblAccountType.AsNoTracking()
                    .ToDictionary(type => type.AccountTypesID, type => type.AccountTypes);

                var rows = accounts
                    .Select(account =>
                    {
                        int? parentId = account.ParentAccountID.HasValue &&
                                        account.ParentAccountID.Value != 0 &&
                                        accountIds.Contains(account.ParentAccountID.Value)
                            ? account.ParentAccountID
                            : (int?)null;
                        decimal balance;
                        accountBalances.TryGetValue(account.ChartOfAccountID, out balance);
                        string accountType;
                        if (!account.AccountTypesID.HasValue ||
                            !accountTypes.TryGetValue(account.AccountTypesID.Value, out accountType))
                            accountType = account.AccountType ?? "";

                        return new ChartOfAccountTreeRow
                        {
                            ChartOfAccountID = account.ChartOfAccountID,
                            ParentAccountID = parentId,
                            AccountNo = account.AccountNo ?? "",
                            AccountName = account.AccountName ?? "",
                            AccountTypes = accountType,
                            Memo = account.Memo ?? "",
                            Balance = balance
                        };
                    })
                    .OrderBy(row => row.AccountNo)
                    .ToList();

                var directBalances = rows.ToDictionary(
                    row => row.ChartOfAccountID,
                    row => row.Balance);
                var childIdsByParent = rows
                    .Where(row => row.ParentAccountID.HasValue)
                    .GroupBy(row => row.ParentAccountID.Value)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(row => row.ChartOfAccountID).ToList());

                foreach (var row in rows)
                {
                    var totalBalance = directBalances[row.ChartOfAccountID];
                    var descendants = new Stack<int>();
                    var visited = new HashSet<int> { row.ChartOfAccountID };
                    List<int> childIds;
                    if (childIdsByParent.TryGetValue(row.ChartOfAccountID, out childIds))
                    {
                        foreach (var childId in childIds)
                            descendants.Push(childId);
                    }

                    while (descendants.Count > 0)
                    {
                        var childId = descendants.Pop();
                        if (!visited.Add(childId))
                            continue;

                        decimal childBalance;
                        directBalances.TryGetValue(childId, out childBalance);
                        totalBalance += childBalance;

                        if (childIdsByParent.TryGetValue(childId, out childIds))
                        {
                            foreach (var descendantId in childIds)
                                descendants.Push(descendantId);
                        }
                    }

                    row.Balance = totalBalance;
                }

                return Json(new { rows }, JsonRequestBehavior.AllowGet);
            }
        }

        public sealed class ChartOfAccountTreeRow
        {
            public int ChartOfAccountID { get; set; }
            public int? ParentAccountID { get; set; }
            public string AccountNo { get; set; }
            public string AccountName { get; set; }
            public string AccountTypes { get; set; }
            public decimal Balance { get; set; }
            public string Memo { get; set; }
        }

        [Authorize(Roles = "Add Chart Of Account, Edit Chart Of Account")]
        public ActionResult CreateChartOfAccount(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            DateTime myDate = DateTime.Parse(html.GetCurrentDate());
            DateTime firstDay = new DateTime(myDate.Year, myDate.Month, 1);
            if (Type == "NewChartOfAccount")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                //html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewChartOfAccount();";
            }
            html.FormID = "CreateChartOfAccount";
            html.FieldIDFocus = "TxtAccountNo";
            html.TableName = "TblChartOfAccount";
            html.PrimaryField = "ChartOfAccountID";
            html.PrimaryFieldValue = PrimaryFieldValue;

            // html.HTMLControls.Add(new HtmlComboBox()
            // {
            //     Label = "Class Name:",
            //     ID = "TxtClassName",
            //     DBField = "ClassID",
            //     Required = false,
            //     SQL_CBO = "select ClassID, ClassName from TblClass where SystemID = " + html.SystemID+" and WarehouseID = "+html.WarehouseID+" and DomainID like '%"+html.DomainID+ "%' order by ClassName",
            //     DisplayText = "ClassName",
            //     Value = "ClassID",
            // });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account No:",
                ID = "TxtAccountNo",
                DBField = "AccountNo",
                Required = true,
                ISUniqueField = true,
                //CheckExistingWithField = "SystemID, WarehouseID, DomainID",
                //CheckExistingWithDefaultValue = $"{html.SystemID},{html.WarehouseID}, {html.DomainID}"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Name:",
                ID = "TxtAccountName",
                DBField = "AccountName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Hide = true,
                Label = "AccountTypesID:",
                ID = "AccountTypesID",
                DBField = "AccountTypesID",
                Required = true,
                Value = ""
            });

            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Account Type:",
            //    ID = "TxtAccountType",
            //    DBField = "AccountType",
            //    Required = true,
            //    SQL_CBO = "select ac.AccountCategory, act.AccountTypesID, act.AccountTypes from dbo.TblAccountTypes act\r\ninner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID where act.SystemID = " + html.SystemID + " and act.WarehouseID = " + html.WarehouseID + " and act.DomainID like '%" + html.DomainID + "%' order by ac.AccountCategory",
            //    DisplayText = "AccountTypes",
            //    Value = "AccountTypesID",
            //    GroupByField = "AccountCategory",
            //    //EventChange = "OnAccountTypeChange(this)"
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
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Account Type:",
                ID = "TxtAccountCategory",
                DBField = "AccountCategoryID",
                Required = true,
                SQL_CBO = "select distinct ac.AccountCategoryID, ac.AccountCategory from dbo.TblAccountCategories ac where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%' order by ac.AccountCategory",
                Value = "AccountCategoryID",
                DisplayText = "AccountCategory",
                placeholder = "Account Category",
                //HiddenField = "AccountTypesID",
                DisplayOnFields = "AccountTypesID",
                IsCallBack =true,
                //CallBackDisplayText = "AccountTypesID",
                //CallBackDisplayValue = "AccountTypesID",
                SQLCallBack = "select AccountTypesID from TblAccountCategories where AccountCategoryID = @SearchText"
            });
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
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Sub of Account:",
                ID = "TxtSubOfAccountType",
                DBField = "ParentAccountID",
                SQL_CBO = "select ChartOfAccountID ParentAccountID, AccountNo + ' - ' + AccountName SubOfAccountType from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountNo",
                //HiddenFeild= "ParentAccountID",
                DisplayText = "SubOfAccountType",
                Value = "ParentAccountID",
                placeholder = "Sub of Account"
                //SQL = "select AccountName SubOfAccountType from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' union select SubOfAccountType from dbo.TblChartOfAccount where SubOfAccountType <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' group by SubOfAccountType order by SubOfAccountType",
                //isTypeahead = true,
                //placeholder="None",
                //Infinity = false
            });
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

            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Opening Balance:",
            //    ID = "TxtOpeningBalance",
            //    DBField = "OpeningBalance",
            //    Value = "0",
            //    ISNumeric = true,
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Opening Balance Date:",
            //    ID = "TxtOpeningBalanceDate",
            //    DBField = "OpeningBalanceDate",
            //    SingleDatePicker = true,
            //    DefaultShowDateValue = true,
            //    Value= firstDay.ToString("yyyy-MM-dd"),
            //});

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Is Default:",
                ID = "IsDefault",
                DBField = "IsDefault",
                Type = "checkbox",
                Class = "form-control form-control-sm col-1",
                Hide = true
            });
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
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "StatusID",
                Required = true,
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in(40, 43) order by StatusID",
                DisplayText = "Status",
                Value = "StatusID",
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ActivateAccountAsOf:",
                ID = "TxtActivateAccountAsOf",
                DBField = "ActivateAccountAsOf",
                Hide = true,
                Value = html.GetCurrentDate(),
            });

            //html.SaveChangeCallbackOnCloseDialog = true;
            //html.OnDialogCloseCallbackFunction = "savechangecallback();";
            html.SaveChangeCallbackFunction = "Reload_TxtSubOfAccountType()";
            html.RestFields = "TxtAccountNo,TxtAccountName,TxtOpeningBalance,TxtOpeningBalanceDate,TxtMemo";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public JsonResult PostOpeningBalance(int AccountID, double Amount, int OpeningBalanceID, DateTime OpeningBalanceDate)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = "SP_PostOpeningBalance_FY";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
                { "AccountID", AccountID.ToString() },
                { "Amount", Amount.ToString() },
                { "CreateByUserID", html.UserID },
                { "OpeningBalanceID", OpeningBalanceID.ToString() },
                { "OpeningBalanceDate", OpeningBalanceDate.ToString() }
            };
            sqlResult = html.SaveChangeData(sql, true, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public string OpeningBalance(int AccountTypesID)
        {
            HTML html = new HTML();
            string sql = @"select distinct ac.[Statement] from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID where act.AccountTypesID = @AccountTypesID";
            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "AccountTypesID", AccountTypesID.ToString() }
            };
            DataSet ds = html.LoadData(sql, false, dict);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                return dr["Statement"].ToString();
            }

            return "";
        }

        public ActionResult ImportChartOfAccount()
        {
            ExcelUploadFile excelUploadFile = new ExcelUploadFile();
            var Excel = new Microsoft.Office.Interop.Excel.Application();
            var FilePath = HttpContext.Request.Files;

            HttpPostedFileBase file = HttpContext.Request.Files[0];
            var desf = HttpContext.Server.MapPath("~/Uploads/");
            string savedFileName = System.IO.Path.Combine(desf, file.FileName);
            FileInfo fileInfo = new FileInfo(savedFileName);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            file.SaveAs(savedFileName);
            Excel.Workbook xlWb = Excel.Workbooks.Open(savedFileName);
            Excel.Worksheet xlWs = (Excel.Worksheet)xlWb.Worksheets[1];
            Excel.Range xlRange = xlWs.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            HTML html = new HTML();
            for (int rowNum = 2; rowNum <= rowCount; rowNum++)
            {
                var AccountNo = xlRange.Cells[rowNum, 1];
                var AccountType = xlRange.Cells[rowNum, 2];
                var AccountName = xlRange.Cells[rowNum, 3];
                var SubOfAccount = xlRange.Cells[rowNum, 4];
                var Memo = xlRange.Cells[rowNum, 5];
                var Status = xlRange.Cells[rowNum, 6];

                string sql = @"declare @AccountType  int, @SubOfAccount int

select @AccountType = AccountTypesID from dbo.TblAccountTypes where AccountTypes = '" + AccountType + @"'
select @SubOfAccount = ChartOfAccountID from dbo.TblChartOfAccount where AccountName = '" + AccountName + @"'
if(not exists (select AccountNo from TblChartOfAccount where AccountNo='" + AccountNo + @"'))
begin
	insert into TblChartOfAccount (AccountNo, AccountName, AccountType, ParentAccountID,StatusID,Memo,ActivateAccountAsOf,OpeningBalance,SystemID, WarehouseID,DomainID)
	select '" + AccountNo + "', N'" + AccountName + "',''+@AccountType+'',@SubOfAccount,40,N'" + Memo + "','" + html.GetCurrentDate() + "',0," + html.SystemID + "," + html.WarehouseID + ",'" + html.DomainID + @"'
end";
                html.SaveChangeData(sql, false, false);
            }

            Excel.Workbooks.Close();

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            return View();
        }
    }
}