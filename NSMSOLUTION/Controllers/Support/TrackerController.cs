using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Support
{
    public class TrackerController : Controller
    {
        // GET: Tracker
        [Authorize(Roles = "Add Tracker, Edit Tracker, Delete Tracker")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Tracker"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Tracker"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Tracker"))
                html.AllowDelete = true;

            html.AddButton = "Add Tracker";
            html.AddURL = "Tracker/CreateTracker";
            html.TableName = "TblSupportTracker";
            html.PrimaryField = "TrackerID";
            html.OrderBy = "TrackerID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Tracker Name",
                SQL = "select TrackerID, TrackerName from dbo.TblSupportTracker where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by TrackerName",
                SelectValue = "TrackerID",
                DisplayField = "TrackerName",
                RadioValue = EncryAndDecry.Encryp("st.TrackerID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() OVER(ORDER BY st.TrackerID desc) AS[No], st.TrackerID, st.TrackerName
            ,STUFF((SELECT ', ' + s.ProjectName  FROM dbo.TblSupportProject s
            inner join dbo.TblSupportTrackerProject stp on stp.SupportProjectID = s.SupportProjectID and stp.TrackerID = st.TrackerID
            ORDER BY s.ProjectName FOR XML PATH('')), 1, 1, '') ProjectName, st.Memo
            from dbo.TblSupportTracker st
            where st.SystemID = " + html.SystemID + " and st.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "st.TrackerID desc";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Tracker Name", FieldName = "TrackerName" },
                new GridField() { Header = "Project Name", FieldName = "ProjectName" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "TrackerID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Tracker, Edit Tracker")]
        public ActionResult CreateTracker(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateTracker",
                FieldIDFocus = "TxtTrackerName",
                TableName = "TblSupportTracker",
                PrimaryField = "TrackerID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Tracker Name:",
                ID = "TxtTrackerName",
                DBField = "TrackerName",
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
            string SQLCheckBoxGroup = "select SupportProjectID, ProjectName from dbo.TblSupportProject where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ProjectName";

            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLCheckBoxGroup,
                Colspan = "5",
                Required = true,
                CheckBoxGroupTitle = "Choose Project Name",
                TableName = "TblSupportTrackerProject",
                GetIDENTITYByFiled = "TrackerID",
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="ProjectName", Value="SupportProjectID", Label="ProjectName", DBField="SupportProjectID" }
                }
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}