using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Support
{
    [Authorize]
    public class GroupController : Controller
    {
        // GET: Group
        [Authorize(Roles = "Add Group, Edit Group, Delete Group")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Group"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Group"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Group"))
                html.AllowDelete = true;

            html.AddButton = "Add Group";
            html.AddURL = "Group/CreateGroup";
            html.TableName = "TblSupportGroup";
            html.PrimaryField = "GroupID";
            html.OrderBy = "GroupID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Group Name",
                SQL = "select GroupID, GroupName from dbo.TblSupportGroup where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by GroupName",
                SelectValue = "GroupID",
                DisplayField = "GroupName",
                RadioValue = EncryAndDecry.Encryp("GroupID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.TblID = "TblSGroup";
            string sql = @"select ROW_NUMBER() OVER(ORDER BY sp.GroupName, sp.GroupID desc) AS[No], sp.GroupID, sp.GroupName
            ,STUFF((SELECT ', ' + s.ProjectName FROM dbo.TblSupportProject s
            inner join dbo.TblSupportGroupProject stp on stp.SupportProjectID = s.SupportProjectID and stp.GroupID = sp.GroupID
            ORDER BY s.ProjectName FOR XML PATH('')), 1, 1, '') ProjectName, sp.Memo, sp.TelegramGroupID
            from dbo.TblSupportGroup sp where sp.SystemID = " + html.SystemID + " and sp.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "GroupID", FieldName = "GroupID", IsHiddenField = true },
                new GridField() { Header = "GroupName", FieldName = "GroupName" },
                new GridField() { Header = "Project Name", FieldName = "ProjectName" },
                new GridField() { Header = "Telegram Group ID", FieldName = "TelegramGroupID" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "GroupID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting()
            {
                GridEventClick = "showUserGroup(this)",
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Group, Edit Group")]
        public ActionResult CreateGroup(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateGroup",
                FieldIDFocus = "TxtGroupName",
                TableName = "TblSupportGroup",
                PrimaryField = "GroupID",
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
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telegram Group ID:",
                ID = "TxtTelegramGroupID",
                DBField = "TelegramGroupID",
                ISUniqueField = true,
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            string SQLCheckBoxGroup = "select SupportProjectID, ProjectName from dbo.TblSupportProject where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ProjectName";

            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLCheckBoxGroup,
                Colspan = "5",
                Required = true,
                CheckBoxGroupTitle = "Choose Project Name",
                TableName = "TblSupportGroupProject",
                GetIDENTITYByFiled = "GroupID",
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="ProjectName", Value="SupportProjectID", Label="ProjectName", DBField="SupportProjectID" }
                }
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult UserGroup(int GroupID)
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Group"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Group"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Group"))
                html.AllowDelete = true;
            html.AddButton = "Add Member To Group";
            html.TableName = "TblSupportUserGroup";
            html.PrimaryField = "SupportUserGroupID";
            html.OrderBy = "SupportUserGroupID desc";
            html.AddFUNCTION = "AddUserToGroup(" + GroupID + ")";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Member Name",
                SQL = "select distinct top 10 ul.UserName from dbo.TblSupportUserGroup ug inner join dbo.TblUserLogin ul on ul.UserLoginID = ug.UserLoginID where lower(ul.UserName) like @SearchText and ul.SystemID = " + html.SystemID + " and ul.WarehouseID=" + html.WarehouseID + " order by ul.UserName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ul.UserName"),
                DefaultCheckBok = true,
                ID = "lblusername",
            };
            html.BunSearch = "btnsearchUGroup";
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over (order by ug.GroupID) [No], ul.FullName, sg.GroupName, ug.SupportUserGroupID from dbo.TblSupportUserGroup ug
            inner join dbo.TblUserLogin ul on ul.UserLoginID = ug.UserLoginID
            inner join dbo.TblSupportGroup sg on sg.GroupID = ug.GroupID
            where ug.GroupID = " + GroupID;
            html.OrderBy = "ul.UserName";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Member Name", FieldName = "FullName" },
                new GridField() { Header = "Group Name", FieldName = "GroupName" },
                new GridField() { Header = "Action", FieldName = "SupportUserGroupID",
                    TextAlign = "Center",  IsModifyField = true, HideEdit = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadSGroup");
            return View();
        }

        public ActionResult AddUser(int GroupID)
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Group"))
                html.AllowAdd = true;

            html.GH.SQL = @"select ROW_NUMBER() over(order by ul.FullName) [No], 
            ul.UserLoginID, ul.FullName 
            from dbo.TblUserLogin ul 
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID + " and ul.UserLoginID not in (select UserLoginID from dbo.TblSupportUserGroup where GroupID=" + GroupID + ")";
            html.IsHaveWhereCondition = true;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.ShowSearchButton = false;
            html.UsingJqueryDataTable = true;
            html.TblID = "AddUser";
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
                string GroupID = item.GroupID;
                string UserID = item.UserID;
                string sql = "insert into dbo.TblSupportUserGroup (UserLoginID, GroupID) values (@UserLoginID, @GroupID);";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    { "UserLoginID", UserID },
                    { "GroupID", GroupID }
                };
                sqlResult = html.SaveChangeData(sql, false, para);
            }

            return Json(new { Data = sqlResult.Message, Result = sqlResult.Result }, JsonRequestBehavior.AllowGet);
        }
    }
}