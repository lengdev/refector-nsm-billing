using MVC;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Account
{
    public class SetDefaultOnAddItemFormController : Controller
    {
        // GET: SetDefaultOnAddItemForm
        [Authorize(Roles = "Set Default On Add Item Form")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                FormID = "SetDefaultOnAddItemForm",
                //html.FieldIDFocus = "TxtDepartmentName";
                TableName = "TblSetDefaultAddItemForm",
                PrimaryField = "ItemFormID",
                PrimaryFieldValue = "0"
            };
            html.SQLForEdit = "select * from TblSetDefaultAddItemForm where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            html.TblWidth = "50%";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Type:",
                ID = "TxtType",
                DBField = "TypeID",
                Required = true,
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ServiceName",
                DisplayText = "ServiceName",
                Value = "ServiceID",
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Inventory Account:",
                ID = "TxtInventoryAccount",
                DBField = "InventoryAccountID",
                Required = true,
                SQL_CBO = "select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountType",
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Expense Account:",
                ID = "TxtExpenseAccount",
                DBField = "ExpenseAccountID",
                Required = true,
                SQL_CBO = "select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountType",
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "IncomeAccount:",
                ID = "TxtIncomeAccount",
                DBField = "IncomeAccountID",
                Required = true,
                SQL_CBO = "select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountType",
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                Infinity = false
            });
            html.DrawBtnSaveChange = true;
            html.SaveChangeAndDeleteOldDataWhenDrawBtnSave = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}