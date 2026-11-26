using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddAccountController : Controller
    {
        // GET: AddAccount
        [Authorize(Roles = "Add Account, Edit Account, Delete Account")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Account"
            };
            if (html.HasPermission("Add Account"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Account"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Account"))
                html.AllowDelete = true;

            html.AddButton = "Add Account";
            html.AddURL = "AddAccount/CreateAccount";
            html.TableName = "TblAccount";
            html.PrimaryField = "AccountID";
            html.OrderBy = "AccountTitle";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Account Title",
                SQL = "select AccountID, AccountTitle from dbo.TblAccount where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountTitle",
                SelectValue = "AccountID",
                DisplayField = "AccountTitle",
                RadioValue = EncryAndDecry.Encryp("AccountID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "BankName", FieldName = "AccountTitle" },
                new GridField() { Header = "AccountNumber", FieldName = "AccountNumber", TextAlign = "center" },
                new GridField() { Header = "AccountName", FieldName = "AccountName" },
                new GridField() { Header = "OwnerName", FieldName = "OwnerName" },
                new GridField() { Header = "Phone", FieldName = "Phone", TextAlign="center" },
                new GridField() { Header = "InitialBalance", FieldName = "InitialBalance", Format="#,#.00", TextAlign = "right" },
                new GridField() { Header = "InternetBankingURL", FieldName = "InternetBankingURL" },
                new GridField() { Header = "Action", FieldName = "AccountID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Account, Edit Account")]
        public ActionResult CreateAccount(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateAccount",
                FieldIDFocus = "TxtAccountTitle",
                TableName = "TblAccount",
                PrimaryField = "AccountID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bank Name:",
                ID = "TxtAccountTitle",
                DBField = "AccountTitle",
                Required = true,
                //ISUniqueFiled = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Number:",
                ID = "TxtAccountNumber",
                DBField = "AccountNumber"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Name:",
                ID = "TxtAccountName",
                DBField = "AccountName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Owner Name:",
                ID = "TxtOwnerName",
                DBField = "OwnerName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone:",
                ID = "TxtPhone",
                DBField = "Phone"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Initial Balance:",
                ID = "TxtInitialBalance",
                DBField = "InitialBalance",
                Value = "0",
                ISNumeric = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Internet Banking URL:",
                ID = "TxtInternetBankingURL",
                DBField = "InternetBankingURL",
                isURL = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}