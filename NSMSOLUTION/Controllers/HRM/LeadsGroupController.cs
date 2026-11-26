using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class LeadsGroupController : Controller
    {
        // GET: LeadsGroup
        [Authorize(Roles = "Add Leads Group, Edit Leads Group, Delete Leads Group")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Leads Group"
            };
            if (html.HasPermission("Add Leads Group"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Leads Group"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Leads Group"))
                html.AllowDelete = true;

            html.AddButton = "Add Leads Group";
            html.AddURL = "LeadsGroup/CreateLeadsGroup";
            html.TableName = "TblLeadsGroup";
            html.PrimaryField = "LeadGroupID";
            html.OrderBy = "LeadGroupID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Group Name",
                SQL = "select LeadGroupID, GroupName from dbo.TblLeadsGroup where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by GroupName",
                SelectValue = "LeadGroupID",
                DisplayField = "GroupName",
                RadioValue = EncryAndDecry.Encryp("LeadGroupID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Group Name", FieldName = "GroupName" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "LeadGroupID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Leads Group, Edit Leads Group")]
        public ActionResult CreateLeadsGroup(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateLeadsGroup",
                FieldIDFocus = "TxtGroupName",
                TableName = "TblLeadsGroup",
                PrimaryField = "LeadGroupID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Group Name:",
                ID = "TxtGroupName",
                DBField = "GroupName",
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
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}