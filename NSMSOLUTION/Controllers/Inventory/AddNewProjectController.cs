using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    [Authorize]
    public class AddNewProjectController : Controller
    {
        [Authorize(Roles = "Add Project, Edit Project, Delete Project")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Project",
                DialogBoxWidth = "800"
            };
            if (html.HasPermission("Add Project"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Project"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Project"))
                html.AllowDelete = true;
            html.AddButton = "Add Project";
            html.AddURL = "AddNewProject/CreateNewProject";
            html.TableName = "TblProject";
            html.PrimaryField = "ProjectID";
            html.OrderBy = "ProjectID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Project Name",
                SQL = "select ProjectID, ProjectName from TblProject where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                SelectValue = "ProjectID",
                DisplayField = "ProjectName",
                RadioValue = EncryAndDecry.Encryp("ProjectID"),
                DefaultCheckBok = true
            };

            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
              new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
              new GridField() { Header = "Project Name", FieldName = "ProjectName" },
              new GridField() { Header = "Description", FieldName = "ProjectDescription", Width="100%"  },
              new GridField() { Header = "Create By", FieldName = "CreateBy" },
              new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yyyy", ClassName="text-center" },
              new GridField() { Header = "Action", FieldName = "ProjectID", TextAlign = "Center",  IsModifyField = true }
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Project, Edit Project")]
        public ActionResult CreateNewProject(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateNewProject",
                FieldIDFocus = "TxtProjectName",
                TableName = "TblProject",
                PrimaryField = "ProjectID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Project Name:",
                ID = "TxtProjectName",
                DBField = "ProjectName",
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Description:",
                ID = "TxtDescription",
                DBField = "ProjectDescription",
            });

            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}