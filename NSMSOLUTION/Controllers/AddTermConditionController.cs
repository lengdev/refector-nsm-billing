using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddTermConditionController : Controller
    {
        // GET: AddTermConditon
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

                AddButton = "Add Term Condition",
                AddURL = "AddTermCondition/CreateTermCondition",
                TableName = "TblTermCondition",
                PrimaryField = "TermConditionID",
                OrderBy = "TermConditionName"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "TermConditionName",
                SQL = "select TermConditonID, TermConditionName from dbo.TblTermCondition where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by TermConditionName",
                SelectValue = "TermConditionID",
                DisplayField = "TermConditionName",
                RadioValue = EncryAndDecry.Encryp("TermConditionID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "TermConditionName", FieldName = "TermConditionName" },
                new GridField() { Header = "Description", FieldName = "Descp", ClassName="text-nowrap-pre" },
                new GridField() { Header = "Action", FieldName = "TermConditonID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public ActionResult CreateTermCondition(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateTermCondition",
                FieldIDFocus = "TxtTermConditionName",
                TableName = "TblTermCondition",
                PrimaryField = "TermConditionID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Term Condition Name:",
                ID = "TxtTermConditionName",
                DBField = "TermConditionName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextarea
            {
                Label = "Descrition:",
                ID = "TxtDescrition",
                DBField = "Descp",
                Required = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}