using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Account
{
    public class SubOfAccountTypeController : Controller
    {
        // GET: SubOfAccountType
        [Authorize(Roles = "Add Sub Of Account Type, Edit Sub Of Account Type, Delete Sub Of Account Type")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Sub Of Account Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Sub Of Account Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Sub Of Account Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Sub Of Account Type";
            html.AddURL = "SubOfAccountType/CreateSubAccountType";
            html.TableName = "TblSubOfAccountType";
            html.PrimaryField = "SubOfAccountTypeID";
            html.OrderBy = "SubOfAccountTypeID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Sub Of Account Type",
                SQL = "select SubOfAccountTypeID, SubOfAccountType from dbo.TblSubOfAccountType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SubOfAccountType",
                SelectValue = "SubOfAccountTypeID",
                DisplayField = "SubOfAccountType",
                RadioValue = EncryAndDecry.Encryp("SubOfAccountTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Sub Of Account Type", FieldName = "SubOfAccountType" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "SubOfAccountTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Add Sub Of Account Type, Edit Sub Of Account Type")]
        public ActionResult CreateSubAccountType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateSubOfAccountType",
                FieldIDFocus = "TxtSubOfAccountType",
                TableName = "TblSubOfAccountType",
                PrimaryField = "SubOfAccountTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Sub Of Account Type:",
                ID = "TxtSubOfAccountType",
                DBField = "SubOfAccountType",
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