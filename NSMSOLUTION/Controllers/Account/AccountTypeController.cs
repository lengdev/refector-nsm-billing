using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Account
{
    public class AccountTypeController : Controller
    {
        // GET: AccountType
        [Authorize(Roles = "Add Account Type, Edit Account Type, Delete Account Type")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Account Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Account Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Account Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Account Type";
            html.AddURL = "AccountType/CreateAccountType";
            html.TableName = "TblAccountType";
            html.PrimaryField = "AccountTypeID";
            html.OrderBy = "AccountTypeID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Account Type",
                SQL = "select AccountTypeID, AccountType from dbo.TblAccountType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountType",
                SelectValue = "AccountTypeID",
                DisplayField = "AccountType",
                RadioValue = EncryAndDecry.Encryp("AccountTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Account Type", FieldName = "AccountType" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "AccountTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Add Account Type, Edit Account Type")]
        public ActionResult CreateAccountType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateAccountType",
                FieldIDFocus = "TxtAccountType",
                TableName = "TblAccountType",
                PrimaryField = "AccountTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Type:",
                ID = "TxtAccountType",
                DBField = "AccountType",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "StatusID",
                Required = true,
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in(40, 43) order by StatusID",
                DisplayText = "Status",
                Value = "StatusID"
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}