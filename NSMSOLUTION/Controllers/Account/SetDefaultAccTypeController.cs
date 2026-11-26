using MVC;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Account
{
    public class SetDefaultAccTypeController : Controller
    {
        // GET: SetDefaultAccTypeController
        [Authorize(Roles = "Set Default Account Type")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                FormID = "SetDefaultOnAddItemForm",
                FieldIDFocus = "TxtPageName",
                TableName = "TblSetDefaultCustomerPayment",
                PrimaryField = "PaymentID",
                PrimaryFieldValue = "0"
            };
            html.SQLForEdit = "select * from TblSetDefaultCustomerPayment where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            html.TblWidth = "50%";
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Page Name:",
            //    ID = "TxtPageName",
            //    DBField = "PageName",
            //    Required = true,
            //});
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Payment Account:",
                ID = "TxtPaymentAccount",
                DBField = "PaymentAccountID",
                Required = true,
                SQL_CBO = "select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountType",
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Payment Method:",
                ID = "TxtPaymentMethod",
                DBField = "PaymentMethodID",
                Required = true,
                SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PaymentMethod",
                DisplayText = "PaymentMethod",
                Value = "PaymentMethodID",
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Category:",
                ID = "TxtCategory",
                DBField = "Category",
                Required = true,
                SQL_CBO = "select PaymentCategoryID, Category from dbo.TblPaymentCategory where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by Category",
                DisplayText = "Category",
                Value = "PaymentCategoryID",
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "PayTo:",
                ID = "TxtPayTo",
                DBField = "PayTo",
                Required = true,
                SQL_CBO = "select AccountID, (case when AccountNumber <> '' then (AccountName + ' - ' + AccountNumber) else AccountTitle end) AccountName from dbo.TblAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by (AccountName + ' - ' + AccountNumber)",
                DisplayText = "AccountName",
                Value = "AccountID",
                Infinity = false
            });
            html.DrawBtnSaveChange = true;
            html.SaveChangeAndDeleteOldDataWhenDrawBtnSave = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}