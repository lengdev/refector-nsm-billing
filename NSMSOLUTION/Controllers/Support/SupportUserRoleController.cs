using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Support
{
    public class SupportUserRoleController : Controller
    {
        // GET: UserRole
        [Authorize(Roles = "Add User Role, Edit User Role, Delete User Role")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add User Role"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit User Role"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete User Role"))
                html.AllowDelete = true;

            html.DialogBoxHeight = 500;
            html.DialogBoxWidth = "800";

            html.AddButton = "Add User Role";
            html.AddURL = "SupportUserRole/CreateUserRole";
            html.TableName = "TblUserRole";
            html.PrimaryField = "UserRoleID";
            html.OrderBy = "UserRoleID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Role Name",
                SQL = "select UserRoleID, RoleName from dbo.TblUserRole where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RoleName",
                SelectValue = "UserRoleID",
                DisplayField = "RoleName",
                RadioValue = EncryAndDecry.Encryp("st.TrackerID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.IsHaveWhereCondition = true;
            html.OrderBy = "UserRoleID desc";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Role Name", FieldName = "RoleName",Width="100%" },
                new GridField() { Header = "Action", FieldName = "UserRoleID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add User Role, Edit User Role")]
        public ActionResult CreateUserRole(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateUserRole",
                FieldIDFocus = "TxtRoleName",
                TableName = "TblUserRole",
                PrimaryField = "UserRoleID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Role Name:",
                ID = "TxtRoleName",
                DBField = "RoleName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            string SQLCheckBoxGroup = @"select p.PermissionPageID, p.PermissionPageCode, p.PermissionPageName 
            from TblUserRolePagePermission up
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserRoleID = up.UserRoleID
            inner join dbo.TblPagePermission p on p.PermissionPageID = up.PermissionPageID
            where sw.UserLoginID= " + html.UserID + " and sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID + " order by p.PermissionPageCode";

            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLCheckBoxGroup,
                Colspan = "5",
                Required = true,
                CheckBoxGroupTitle = "Choose Permission Page",
                TableName = "TblUserRolePagePermission",
                GetIDENTITYByFiled = "UserRoleID",
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
                    new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
                }
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}