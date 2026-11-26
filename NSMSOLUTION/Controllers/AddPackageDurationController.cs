using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddPackageDurationController : Controller
    {
        // GET: AddPackageDuration
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Expense Type"))
                AllowAdd = true,
                //if (html.HasPermission("Edit System"))
                AllowEdit = true,
                //if (html.HasPermission("Delete System"))
                AllowDelete = true,

                AddButton = "Add Package Duration",
                AddURL = "AddPackageDuration/CreatePackageDuration",
                TableName = "TblPackageDuration",
                PrimaryField = "PackageDurationID",
                OrderBy = "PackageDurationName"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Package Duration",
                SQL = "select PackageDurationID, PackageDurationName from dbo.TblPackageDuration where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageDurationName",
                SelectValue = "PackageDurationID",
                DisplayField = "PackageDurationName",
                RadioValue = EncryAndDecry.Encryp("PackageDurationID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Package Duration", FieldName = "PackageDuration" },
                new GridField() { Header = "Package Duration Name", FieldName = "PackageDurationName" },
                new GridField() { Header = "Action", FieldName = "PackageDurationID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public ActionResult CreatePackageDuration(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreatePackageDuration",
                FieldIDFocus = "TxtPackageDuration",
                TableName = "TblPackageDuration",
                PrimaryField = "PackageDurationID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Duration:",
                ID = "TxtPackageDuration",
                DBField = "PackageDuration",
                Required = true,
                Type = "number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Duration Name:",
                ID = "TxtPackageDurationName",
                DBField = "PackageDurationName",
                Required = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}