using MVC;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddUserLoginController : Controller
    {
        // GET: AddUserLogin
        [Authorize(Roles = "Add User Login, Edit User Login, Delete User Login")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add User Login"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit User Login"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete User Login"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 500;
            html.AddButton = "Add User Login";
            html.AddURL = "AddUserLogin/CreateUserLogin";
            html.TableName = "TblUserLogin";
            html.PrimaryField = "UserLoginID";
            html.OrderBy = "UserName";
            string SQL = "select top 10 tu.UserName from dbo.TblUserLogin tu ";
            SQL += "inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = tu.UserLoginID ";
            SQL += "where lower(tu.UserName) like @SearchText and ts.SystemID = " + html.SystemID + " and ts.WarehouseID=" + html.WarehouseID + " order by tu.UserName";
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = SQL,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("UserName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            SQL = "select distinct ROW_NUMBER() OVER(ORDER BY tu.UserName) [No], ";
            SQL += "tu.UserLoginID, tu.FullName, tu.UserName, tu.Telephone, s.Status, ISNULL(NULLIF(ts.DefaultDashboardPage, ''), 'Default') AS DefaultDashboardPage,";
            SQL += "tu.[Address], tu.TelegramUserName,FORMAT(tu.CreateDate, 'dd-MM-yyyy') AS CreateDate, tu.CreateBy from dbo.TblUserLogin tu ";
            SQL += "inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = tu.UserLoginID ";
            SQL += "inner join dbo.TblStatus s on s.StatusID = tu.StatusID ";
            SQL += "where ts.SystemID = " + html.SystemID + " and ts.WarehouseID = " + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            html.GH.SQL = SQL;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Full Name", FieldName = "FullName", ClassName="text-uppercase" },
                new GridField() { Header = "User Name", FieldName = "UserName" },
                new GridField() { Header = "Telephone", FieldName = "Telephone" },
                new GridField() { Header = "Telegram User Name", FieldName = "TelegramUserName" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Default Dashboard Page", FieldName = "DefaultDashboardPage" },
                new GridField() { Header = "CreateDate", FieldName = "CreateDate", TextAlign = "Center" },
                new GridField() { Header = "CreateBy", FieldName = "CreateBy", ClassName="text-uppercase"},
                new GridField() { Header = "Action", FieldName = "UserLoginID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add User Login, Edit User Login")]
        public ActionResult CreateUserLogin(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            string uasid = "0";
            string DefaultDashboardPage = "";
            if (PrimaryFieldValue != "0")
            {
                string sql = "select UserAccessSystemAndWarehouseID, DefaultDashboardPage from TblSetUserAccessSystemAndWarehouse ";
                sql += "where UserLoginID = " + PrimaryFieldValue + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                DataSet ds = html.LoadData(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    uasid = ds.Tables[0].Rows[0]["UserAccessSystemAndWarehouseID"].ToString();
                    DefaultDashboardPage = ds.Tables[0].Rows[0]["DefaultDashboardPage"].ToString();
                }
            }
            html.FormID = "CreateUser";
            html.ISUniqueFiledWithSystemID = false;
            html.FieldIDFocus = "TxtFullName";
            html.UseTable = true;
            html.TableColumn = 2;
            html.TableName = "TblUserLogin";
            html.PrimaryField = "UserLoginID";
            html.PrimaryFieldValue = PrimaryFieldValue;

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Full Name:",
                ID = "TxtFullName",
                DBField = "FullName",
                Required = true,
                SQL = "https://192.168.69.83:9797/api/GetEmployee?CompanyID=1",
                isTypeahead = true,
                IsGetDataFromAPI = true,
                //HiddenFeild= "PhoneNumber",
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkZjgwMTJmLTA4MTQtNDYzMS1hNmU5LTUzMjU5Y2IyYWUyOSIsIm5iZiI6MTcxMDQ3MDU0MSwiZXhwIjoxNzQyMDA2NTQxLCJpYXQiOjE3MTA0NzA1NDEsImlzcyI6Imh0dHBzOi8vaHIubnNtc29sdXRpb24uY29tLmtoLyIsImF1ZCI6Imh0dHBzOi8vaHIubnNtc29sdXRpb24uY29tLmtoOjk3OTcvYXBpLyJ9.nWQXSDg9at2qhe-BdN6sXT9XryVvXxDoDBQ7Qhi57EY",
                ReplaceJsonKey = new Dictionary<string, string>
                {
                    { "employeeName", "FullName" },
                    { "employeePhoneNumber", "TxtTelephone" },
                    { "positionName", "TxtJob" },
                    { "employeeTelegramUsername", "TxtTelegramUserName" }
                },
                DisplayOnFields = "TxtTelephone,TxtJob,TxtTelegramUserName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Login Name:",
                ID = "TxtUserName",
                DBField = "UserName",
                Required = true,
                ISUniqueField = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "TxtPassword",
                DBField = "Password",
                Required = true,
                Type = "password",
                Value = "NsM@2023",
                ExLabel = "Default Password: NsM@2023"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Confirm Password:",
                ID = "TxtConfirmPassword",
                Required = true,
                Type = "password",
                EqualTo = "TxtPassword",
                Value = "NsM@2023",
                ExLabel = "Default Password: NsM@2023"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telephone:",
                ID = "TxtTelephone",
                DBField = "Telephone",
                Required = true,
                Type = "tel"
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "Address"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmail",
                DBField = "Email",
                Type = "email",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Position:",
                ID = "TxtJob",
                DBField = "Job",
                Required = true
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatusID",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in (3,4)",
                Value = "StatusID",
                DisplayText = "Status"
            });
            string sqlCallBack = "select '' PageLink, 'Default' PermissionPageCode union all select distinct p.PageLink, p.PermissionPageCode ";
            sqlCallBack += "from dbo.TblPagePermission p ";
            sqlCallBack += "inner join dbo.TblUserRolePagePermission ur on ur.PermissionPageID = p.PermissionPageID ";
            sqlCallBack += "where p.PageLink <> '' and ur.UserRoleID = @SearchText";
            html.HTMLControls.Add(new HtmlComboBox
            {
                Label = "User Role:",
                ID = "TxtUserRole",
                DBField = "UserRoleID",
                SQL_CBO = "select UserRoleID, RoleName from TblUserRole where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by RoleName",
                DisplayText = "RoleName",
                Value = "UserRoleID",
                ISCallBackToCbo = true,
                SQLCallBack = sqlCallBack,
                CallBackDisplayText = "PermissionPageCode",
                CallBackDisplayValue = "PageLink",
                CallBackToControlID = "CboDefaultDashboardPage",
                CboValueComboCallBack = DefaultDashboardPage
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Default Dashboard Page:",
                ID = "CboDefaultDashboardPage",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Change Password On Login:",
                ID = "TxtISChangePasswordOnNextLogon",
                DBField = "ISChangePasswordOnNextLogon",
                Value = "1",
                DefaultCheckBoxChecked = true,
                Type = "checkbox",
                Class = "",
                DivClass = "custom-control custom-checkbox p-0",
                PrimaryFieldValue = PrimaryFieldValue
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Roles:",
                ID = "CboUserType",
                DBField = "UserTypeID",
                Value = "2",
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telegram User Name:",
                ID = "TxtTelegramUserName",
                DBField = "TelegramUserName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "",
                ID = "Cbo",
                Hide = true
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Signature:",
                ID = "Signature",
                DBField = "Signature",
                ISAddAttachmentFile = true,
                AcceptImageOnly = true,
                Multiple = true,
                UploadMultiple = false,
                IsDragDropFile = true,
                DestinationFile = "Uploads/Users",
                Type = "file",
                ISFile = true,
            });
            //html.HTMLControls.Add(new HtmlFile()
            //{
            //    Label = "Signature VAT:",
            //    ID = "TxtSignatureVAT",
            //    DBField = "SignatureVAT",
            //    ISAddAttachmentFile = true,
            //    AcceptImageOnly = true,
            //    //UploadMultiple = false,
            //    //IsDragDropFile = true,
            //    DestinationFile = "Uploads/Users",
            //    Type = "file",
            //});

            html.insTable.Add(new InsertTable()
            {
                TableName = "TblSetUserAccessSystemAndWarehouse",
                Fields = new
                {
                    SystemID = html.SystemID,
                    WarehouseID = html.WarehouseID,
                    UserRoleID = "$(\"#TxtUserRole\").val()",
                    DefaultDashboardPage = "$(\"#CboDefaultDashboardPage\").val()"
                },
                GetIdentityField = "UserLoginID",
                PrimaryField = "UserAccessSystemAndWarehouseID",
                PrimaryFieldValue = uasid.ToString(),
                ResponseFieldsValuesToCheckBox = new
                {
                    UserLoginID = "",
                    UserAccessSystemAndWarehouseID = ""
                }
            });

            //HtmlCheckBoxGroup PermissionPage = new HtmlCheckBoxGroup();
            //string SQLPermissionPage = "select distinct p.PageTypeID, sp.PermissionPageID, p.PermissionPageCode, p.PermissionPageName from dbo.TblSetPagePermission sp ";
            //SQLPermissionPage += "inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = sp.UserLoginID ";
            //SQLPermissionPage += "inner join dbo.TblPagePermission p on p.PermissionPageID = sp.PermissionPageID ";
            //if (PrimaryFieldValue != 0)
            //    SQLPermissionPage += "where sw.UserLoginID = " + html.UserID + " ";
            //else
            //    SQLPermissionPage += "where 1=1 ";
            //SQLPermissionPage += "and sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID;
            //SQLPermissionPage += " order by p.PermissionPageCode";
            //PermissionPage.SqlCheckBoxGroup = SQLPermissionPage;
            //PermissionPage.CheckBoxGroupID = "PermissionPage";
            //PermissionPage.CheckBoxGroupClass = "PermissionPage";
            //PermissionPage.Colspan = "5";
            //PermissionPage.Required = true;
            //PermissionPage.ISTab = true;
            //PermissionPage.TabTitle = "Accessing Page: ";
            //PermissionPage.LinkTabField = "PageTypeID";
            //PermissionPage.SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType";
            //PermissionPage.TableName = "TblSetPagePermission";
            //PermissionPage.GetIDENTITYByFiled = "UserAccessSystemAndWarehouseID";
            //PermissionPage.GetValueFromDBField = "UserLoginID";
            //PermissionPage.CheckBoxGroup = new CheckBoxValue_Lable[]
            //{
            //    new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
            //    new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
            //};
            //html.HTMLControls.Add(PermissionPage);

            //dynamic UserInfo = new { UserID = html.UserID, SystemID = html.SystemID, WarehouseID = html.WarehouseID, PrimaryFieldValue = PrimaryFieldValue };
            //ViewBag.UserInfo = JsonConvert.SerializeObject(UserInfo);

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public string GetSetUserPermission(int UserLoginID, int SystemID, int WarehouseID, string PrimaryFieldValue)
        {
            HTML html = new HTML();
            string sql = "select UserAccessSystemAndWarehouseID from TblSetUserAccessSystemAndWarehouse ";
            sql += "where UserLoginID = " + UserLoginID + " and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + "";
            DataTable dt = html.GetDataForEdit(sql);
            html.PrimaryFieldValue = "0";
            if (dt.Rows.Count > 0)
            {
                html.TableName = "TblSetUserAccessSystemAndWarehouse";
                html.PrimaryField = "UserAccessSystemAndWarehouseID";
                html.PrimaryFieldValue = dt.Rows[0]["UserAccessSystemAndWarehouseID"].ToString();
            }
            //PermissionPage.CheckBoxGroupTblName = EncryAndDecry.Encryp("select * from TblSetPagePermission where UserAccessSystemAndWarehouseID = " + dt.Rows[0]["UserAccessSystemAndWarehouseID"].ToString());
            //  html.PrimaryFieldValue = int.Parse(dt.Rows[0]["UserAccessSystemAndWarehouseID"].ToString());

            //string SQLPermissionPage = "select PageTypeID, PermissionPageID, PermissionPageCode, PermissionPageName ";
            //SQLPermissionPage += "from dbo.TblPagePermission where SystemID = " + SystemID + " order by OrderNumber, PermissionPageCode";
            HtmlCheckBoxGroup PermissionPage = new HtmlCheckBoxGroup();
            string SQLPermissionPage = "select p.PageTypeID, sp.PermissionPageID, p.PermissionPageCode, p.PermissionPageName from dbo.TblSetPagePermission sp ";
            SQLPermissionPage += "inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = sp.UserLoginID ";
            SQLPermissionPage += "inner join dbo.TblPagePermission p on p.PermissionPageID = sp.PermissionPageID ";
            SQLPermissionPage += "where sw.UserLoginID = " + UserLoginID + " and sw.SystemID = " + SystemID + " and sw.WarehouseID = " + WarehouseID;

            PermissionPage.SqlCheckBoxGroup = SQLPermissionPage;
            PermissionPage.CheckBoxGroupID = "PermissionPage";
            PermissionPage.CheckBoxGroupClass = "PermissionPage";
            //PermissionPage.Colspan = "2";
            PermissionPage.Required = true;
            PermissionPage.ISTab = true;
            PermissionPage.TabTitle = "Accessing Page: ";
            PermissionPage.LinkTabField = "PageTypeID";
            PermissionPage.SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType";
            PermissionPage.TableName = "TblSetPagePermission";
            PermissionPage.GetIDENTITYByFiled = "UserAccessSystemAndWarehouseID";
            PermissionPage.GetValueFromDBField = "UserLoginID";
            PermissionPage.CheckBoxGroup = new CheckBoxValue_Lable[]
            {
                new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
                new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
            };
            html.HTMLControls.Add(PermissionPage);
            //DataSet ds = html.LoadData(sql);

            return html.DrawHTMLForm();
        }

        public string DeleteSignature()
        {
            HttpFileCollectionBase file = HttpContext.Request.Files;

            return "";
        }


    }
}