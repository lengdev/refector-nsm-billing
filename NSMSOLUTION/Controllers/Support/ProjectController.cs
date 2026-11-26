using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Support
{
    [Authorize]
    public class ProjectController : Controller
    {
        // GET: Project
        [Authorize(Roles = "Add Project, Edit Project, Delete Project")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Project"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Project"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Project"))
                html.AllowDelete = true;

            html.AddButton = "Add Project";
            html.AddURL = "Project/CreateProject";
            html.TableName = "TblSupportProject";
            html.PrimaryField = "SupportProjectID";
            html.OrderBy = "SupportProjectID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Project Name",
                SQL = "select SupportProjectID, ProjectName from dbo.TblSupportProject where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ProjectName",
                SelectValue = "SupportProjectID",
                DisplayField = "ProjectName",
                RadioValue = EncryAndDecry.Encryp("SupportProjectID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select *, '<a href=""javascript:void(0);"" class=""btn btn-danger btn-sm"" onclick=""showUserProject('
					+ CONVERT(VARCHAR(10), SupportProjectID)+');"">Assign</a>' as Member from dbo.TblSupportProject";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SupportProjectID", FieldName = "SupportProjectID", IsHiddenField = true },
                new GridField() { Header = "Project Name", FieldName = "ProjectName" },
                new GridField() { Header = "Memo", FieldName = "Memo",Width="100%" },
                new GridField() { Header = "Member", FieldName = "Member", ClassName="text-center"},
                new GridField() { Header = "Action", FieldName = "SupportProjectID", TextAlign = "Center",Width="150px", ClassName="truncate-cell",  IsModifyField = true }
            };
            //html.GridEventClick = "showUserProject(this);";
            html.TblID = "TblSProject";
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Project, Edit Project")]
        public ActionResult CreateProject(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateProject",
                FieldIDFocus = "TxtProjectName",
                TableName = "TblSupportProject",
                PrimaryField = "SupportProjectID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Project Name:",
                ID = "TxtProjectName",
                DBField = "ProjectName",
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

        public ActionResult Project(int ProjectID)
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Project"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Project"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Project"))
                html.AllowDelete = true;
            html.AddButton = "Add Member To Project";
            html.TableName = "TblSupportUserProject";
            html.PrimaryField = "SupportUserGroupID";
            html.OrderBy = "SupportProjectID desc";
            html.AddFUNCTION = "AddUserToProject(" + ProjectID + ")";
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Member Name",
                SQL = "select distinct top 10 ul.UserName from dbo.TblSupportUserProject sp inner join dbo.TblUserLogin ul on ul.UserLoginID = sp.UserLoginID where lower(ul.UserName) like @SearchText and ul.SystemID = " + html.SystemID + " and ul.WarehouseID=" + html.WarehouseID + " order by ul.UserName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ul.UserName"),
                DefaultCheckBok = true,
                ID = "lblusername",
            };
            html.BunSearch = "btnsearchUProject";
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over (order by sp.SupportProjectID) [No], ul.FullName, sp.ProjectName, up.SupportUserProjectID
            from dbo.TblSupportUserProject up
            inner join dbo.TblUserLogin ul on ul.UserLoginID = up.UserLoginID
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = up.SupportProjectID
            where sp.SupportProjectID = " + ProjectID;
            html.OrderBy = "ul.FullName";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Member Name", FieldName = "FullName" },
                new GridField() { Header = "Project Name", FieldName = "ProjectName", IsGroupByField =true, GroupLabel="Project Name: ", Colspan = 3 },
                new GridField() { Header = "Action", FieldName = "SupportUserProjectID",
                    TextAlign = "Center",  IsModifyField = true, HideEdit = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadSProject");
            return View();
        }

        public ActionResult AddUser(int ProjectID)
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Project"))
                html.AllowAdd = true;

            html.GH.SQL = @"select ROW_NUMBER() over(order by ul.FullName) [No], ul.UserLoginID, ul.FullName 
            from dbo.TblUserLogin ul 
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID + " and ul.UserLoginID not in (select UserLoginID from dbo.TblSupportUserProject where SupportProjectID=" + ProjectID + ")";
            html.IsHaveWhereCondition = true;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.ShowSearchButton = false;
            html.UsingJqueryDataTable = true;
            html.TblID = "AddUser";
            html.IsShowJqueryDataTablePrint = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Member Name", FieldName = "FullName" },
                new GridField() { Header = "Action", FieldName = "UserLoginID", DisplayCheckBox= true },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadUser");
            return View();
        }

        public JsonResult SaveAddUser(string[] Data)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            dynamic dic = JsonConvert.DeserializeObject(Data[0]);
            foreach (dynamic item in dic)
            {
                string SupportProjectID = item.ProjectID;
                string UserID = item.UserID;
                string sql = "insert into dbo.TblSupportUserProject (UserLoginID, SupportProjectID) values (@UserLoginID, @SupportProjectID);";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    { "UserLoginID", UserID },
                    { "SupportProjectID", SupportProjectID }
                };
                sqlResult = html.SaveChangeData(sql, false, para);
            }

            return Json(new { Data = sqlResult.Message, Result = sqlResult.Result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProjectLists()
        {

            return View();
        }
    }
}