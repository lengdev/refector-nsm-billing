using MVC;
using Newtonsoft.Json;
using NSM.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Utilities;

namespace NSM.Controllers
{
    public class AdministratorController : Controller
    {
        // GET: Administrator
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        public class UserClaim
        {
            //public string Type { get; set; }
            public string UserRole { get; set; }
        }
        public ActionResult GetUserRole()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            List<UserClaim> userClaim = new List<UserClaim>();
            foreach (Claim str in claims)
            {
                userClaim.Add(new UserClaim { UserRole = str.Value });
            }
            //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //string strclaims = serializer.Serialize(claims);
            var serializedIdentity = JsonConvert.SerializeObject(userClaim);
            return Json(new { responseText = serializedIdentity }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Add Page Category, Edit Page Category, Delete Page Category")]
        public ActionResult CreatePageCategory()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Page Category"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Page Category"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Page Category"))
                html.AllowDelete = true;

            html.AddButton = "Add Page Category";
            html.AddURL = "Administrator/CreatePageCategoryForm";
            html.TableName = "TblPageCategory";
            html.PrimaryField = "PageCategoryID";
            html.OrderBy = "CreateDate desc";
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Category Name",
                SQL = "select PageCategoryID, PageCategory from dbo.TblPageCategory order by PageCategory",
                SelectValue = "PageCategoryID",
                DisplayField = "PageCategory",
                RadioValue = EncryAndDecry.Encryp("PageCategoryID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "PageCategory", FieldName = "PageCategory", Width = "100%" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", TextAlign = "center", FormatDateTime="dd/MM/yyyy" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "PageCategoryID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Page Category, Edit Page Category")]
        public ActionResult CreatePageCategoryForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreatePageCategoryForm",
                FieldIDFocus = "TxtPageCategory",
                TableName = "TblPageCategory",
                PrimaryField = "PageCategoryID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Category:",
                ID = "TxtPageCategory",
                DBField = "PageCategory",
                Required = true,
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Link:",
                ID = "TxtPageLink",
                DBField = "PageLink",
                //Required = true,
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 PageLink from dbo.TblPageCategory where lower(PageLink) like @SearchText  order by PageLink"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "NAV-ICON:",
                ID = "TxtNavIcon",
                DBField = "NAVICON"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order Number:",
                ID = "TxtOrderNumber",
                DBField = "OrderNumber",
                ISInteger = true,
                Required = true,
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        // GET: Administrator/CreateSystem
        //[Authorize(Roles = "Add System, Edit System, Delete System")]
        public ActionResult CreateSystem()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add System"))
                AllowAdd = true,
                //if (html.HasPermission("Edit System"))
                AllowEdit = true,
                //if (html.HasPermission("Delete System"))
                AllowDelete = true,

                AddButton = "Add System",
                AddURL = "Administrator/CreateSystemForm",
                TableName = "TblSystem",
                PrimaryField = "SystemID",
                OrderBy = "CreateDate desc"
            };
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "System Name",
                SQL = "select SystemID, SystemName from dbo.TblSystem order by SystemName",
                SelectValue = "SystemID",
                DisplayField = "SystemName",
                RadioValue = EncryAndDecry.Encryp("SystemID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "System Name", FieldName = "SystemName" },
                new GridField() { Header = "Body", FieldName = "Body" },
                new GridField() { Header = "Aside Light Sidebar", FieldName = "AsideLightSidebar" },
                new GridField() { Header = "Logo", FieldName = "Logo", ImagePath="Uploads/SystemLogo/" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", TextAlign = "center", FormatDateTime="dd/MM/yyyy" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "SystemID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        // GET: Administrator/CreateSystemForm
        //[Authorize(Roles = "Add System, Edit System")]
        public ActionResult CreateSystemForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateSystemForm",
                FieldIDFocus = "TxtSystemName",
                TableName = "TblSystem",
                PrimaryField = "SystemID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "System Name:",
                ID = "TxtSystemName",
                DBField = "SystemName",
                Required = true,
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Body:",
                ID = "TxtBody",
                DBField = "Body"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Aside Light Sidebar:",
                ID = "TxtAsideLightSidebar",
                DBField = "AsideLightSidebar",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Logo:",
                ID = "Logo",
                DBField = "Logo",
                Required = true,
                ISFile = true,
                DestinationFile = "Uploads/SystemLogo",
                AcceptImageOnly = true,
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult CreateWarehouse()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Warehouse"))
                AllowAdd = true,
                //if (html.HasPermission("Edit Warehouse"))
                AllowEdit = true,
                //if (html.HasPermission("Delete Warehouse"))
                AllowDelete = true,
                AddButton = "Add Warehouse",
                AddURL = "Administrator/CreateWarehouseForm",
                TableName = "TblWarehouse"
            };
            //html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "System Name",
            //    SQL = "select distinct ts.SystemName from dbo.TblSystem ts inner join dbo.TblWarehouseSystem tw on tw.SystemID = ts.SystemID order by ts.SystemName",
            //    SelectValue = "SystemName",
            //    DisplayField = "SystemName",
            //    RadioValue = EncryAndDecry.Encryp("s.SystemName") + "," + EncryAndDecry.Encryp("w.WarehouseName"),
            //    DefaultCheckBok = true
            //});
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Warehouse Name",
                SQL = "select distinct tw.WarehouseName from dbo.TblWarehouse tw order by tw.WarehouseName",
                SelectValue = "WarehouseName",
                DisplayField = "WarehouseName",
                RadioValue = EncryAndDecry.Encryp("w.WarehouseName"),
                DefaultCheckBok = true
            });
            //string SQL = "select ROW_NUMBER() OVER(ORDER BY ts.SystemName) AS[No], tw.WarehouseID, ts.SystemName, tw.WarehouseName ";
            //SQL += "from TblWarehouse tw inner join dbo.TblSystem ts on ts.SystemID = tw.SystemID";
            string SQL = @"select ROW_NUMBER() OVER(ORDER BY w.WarehouseName) AS [No], w.WarehouseID, w.WarehouseName, w.OrderNumber,
STUFF((SELECT ', ' + s.SystemName FROM dbo.TblSystem s inner join dbo.TblWarehouseSystem ws on ws.WarehouseID = w.WarehouseID and ws.SystemID = s.SystemID
ORDER BY s.SystemName FOR XML PATH('')), 1, 1, '') SystemName, w.CreateDate, w.CreateBy,w.Logo from dbo.TblWarehouse w";
            html.GH.SQL = SQL;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Warehouse Name", FieldName = "WarehouseName" },
                new GridField() { Header = "System Name", FieldName = "SystemName", Style="overflow: hidden; text-overflow: ellipsis; width:300px" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", TextAlign="center", FormatDateTime="dd/MM/yyyy" },
                new GridField() { Header = "Logo", FieldName = "Logo", TextAlign="center", ImagePath="Uploads/WarehouseLogo/" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", TextAlign="center" },
                new GridField() { Header = "Order Number", FieldName = "OrderNumber", TextAlign="right" },
                new GridField() { Header = "Action", FieldName = "WarehouseID", TextAlign = "Center",  IsModifyField = true } //DeleteUseProcedureName = "SP_DELETE_STOCK_NAME"
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreateWarehouseForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateWarehouseForm",
                FieldIDFocus = "TxtWarehouseName",
                RestFields = "TxtWarehouseName, SystemName",
                TableName = "TblWarehouse",
                PrimaryField = "WarehouseID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "System Name:",
            //    ID = "CboSysName",
            //    DBField = "SystemID",
            //    SQL_CBO = "select SystemID, SystemName from dbo.TblSystem order by SystemName",
            //    Value = "SystemID",
            //    DisplayText = "SystemName",
            //    Multiple = PrimaryFieldValue == 0 ? true : false,
            //    Required = true
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Warehouse Name:",
                ID = "TxtWarehouseName",
                DBField = "WarehouseName",
                Required = true,
                ISUniqueField = true,
                //CheckExsitingWithField = "SystemID",
                //CheckExsitingWithControllName = "CboSysName",
                //ISAutoComplete = true,
                //SQL_AutoComplete = "select distinct top 10 WarehouseName from dbo.TblWarehouse where lower(WarehouseName) like @SearchText order by WarehouseName"
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Logo:",
                ID = "Logo",
                DBField = "Logo",
                ISFile = true,
                AcceptImageOnly = true,
                DestinationFile = "Uploads/WarehouseLogo"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order Number:",
                ID = "TxtOrderNumber",
                DBField = "OrderNumber",
                ISInteger = true,
                Required = true,
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Create By:",
            //    ID = "TxtCreateBy",
            //    DBField = "CreateBy",
            //    Hide = true,
            //    Value = html.UserName
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Create Date:",
            //    ID = "TxtCreateDate",
            //    DBField = "CreateDate",
            //    Value = html.GetCurrentDate(),
            //    Hide = true
            //});
            string SQLCheckBoxGroup = "select SystemID, SystemName from dbo.TblSystem order by SystemName";

            HtmlCheckBoxGroup htmlCheckBoxGroup = new HtmlCheckBoxGroup
            {
                SqlCheckBoxGroup = SQLCheckBoxGroup,
                Colspan = "5",
                Required = true,
                CheckBoxGroupTitle = "Choose System Name",
                TableName = "TblWarehouseSystem",
                GetIDENTITYByFiled = "WarehouseID",
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="SystemName", Value="SystemID", Label="SystemName", DBField="SystemID" },
                //new CheckBoxValue_Lable(){ Name="WarehouseID", Value="WarehouseID", Label="WarehouseName", DBField="WarehouseID" }
                }
            };
            html.HTMLControls.Add(htmlCheckBoxGroup);

            //SQLCheckBoxGroup = "select DomainID, DomainName from dbo.TblDomain order by DomainName";

            //htmlCheckBoxGroup = new HtmlCheckBoxGroup();
            //htmlCheckBoxGroup.SqlCheckBoxGroup = SQLCheckBoxGroup;
            //htmlCheckBoxGroup.Colspan = "5";
            //htmlCheckBoxGroup.Required = true;
            //htmlCheckBoxGroup.CheckBoxGroupTitle = "Choose Domain Name";
            //htmlCheckBoxGroup.TableName = "TblWarehouseSystem";
            //htmlCheckBoxGroup.GetIDENTITYByFiled = "WarehouseSysID";
            //htmlCheckBoxGroup.CheckBoxGroup = new CheckBoxValue_Lable[]
            //{
            //	new CheckBoxValue_Lable(){ Name="DomainName", Value="DomainID", Label="DomainName" },
            //             //new CheckBoxValue_Lable(){ Name="WarehouseID", Value="WarehouseID", Label="WarehouseName", DBField="WarehouseID" }
            //         };
            //html.HTMLControls.Add(htmlCheckBoxGroup);

            //html.insTable.Add(
            //	new InsertTable()
            //	{
            //		PrimaryFieldValue = "1",
            //		GetIndetityFiled= "WarehouseSysID",
            //		TableName = "TblWarehouseSystem",
            //		Fields = new
            //		{
            //                     DomainID = "'hello'",
            //		},
            //		UpdateDateConditionFields = new
            //		{
            //                     WarehouseSysID = PrimaryFieldValue,
            //		}
            //	}
            //);

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult AddPageToSystem()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Page Permission"))
                AllowAdd = true,
                //if (html.HasPermission("Edit Page Permission"))
                AllowEdit = true,
                //if (html.HasPermission("Delete Page Permission"))
                AllowDelete = true,

                DialogBoxWidth = "850",
                DialogBoxHeight = 500,
                AddButton = "Add Page To System",
                AddURL = "Administrator/AddPageToSystemForm"
            };
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "System Name",
                SQL = "select distinct s.SystemID, s.SystemName from dbo.TblSystem s inner join dbo.TblAdminAddPageToSystem ats on ats.SystemID = s.SystemID",
                SelectValue = "SystemID",
                DisplayField = "SystemName",
                RadioValue = EncryAndDecry.Encryp("s.SystemID"),
                DefaultCheckBok = true
            });
            string SQL = @"select ROW_NUMBER() over(PARTITION by s.SystemName order by s.SystemName desc, p.PermissionPageCode) as [No],
			         s.SystemID, s.SystemName, p.PermissionPageCode, p.PermissionPageName
			         from dbo.TblSystem s
			         inner join dbo.TblAdminAddPageToSystem ats on ats.SystemID = s.SystemID
			         inner join dbo.TblPagePermission p on p.PermissionPageID = ats.PermissionPageID";
            html.OrderBy = "[No]";
            html.GH.SQL = SQL;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "System Name", FieldName = "SystemName", IsGroupByField = true },
                new GridField() { Header = "Permission Page Code", FieldName = "PermissionPageCode", Width = "50%"},
                new GridField() { Header = "Permission Page Name", FieldName = "PermissionPageName", Width = "50%"},
                new GridField() { Header = "Action", FieldName = "SystemID", TextAlign = "Center",
                    IsModifyField = true, DeleteWithTableName="TblAdminAddPageToSystem", IsGroupByField = true
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult AddPageToSystemForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                //string SQLCheckBoxGroup = "select SystemID, SystemName from dbo.TblSystem order by SystemName";
                //html.HTMLControls.Add(new HtmlCheckBoxGroup()
                //{
                //    SqlCheckBoxGroup = SQLCheckBoxGroup,
                //    Colspan = "5",
                //    Required = true,
                //    CheckBoxGroupTitle = "Choose System Name",
                //    TableName = "TblPagePermission",
                //    GetIDENTITYByFiled = "PermissionPageID",
                //    CheckBoxGroup = new CheckBoxValue_Lable[]
                //    {
                //        new CheckBoxValue_Lable(){ Name="SystemName", Value="SystemID", Label="SystemName", DBField="SystemID" }
                //    }
                //});
                FormID = "AddPageToSystemForm",
                TableName = "TblSystem",
                PrimaryField = "SystemID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "System Name:",
                ID = "TxtSystemName",
                DBField = "SystemName",
                Required = true,
                ISUniqueField = true,
            });

            //string SQLPermissionPage = @"select distinct p.PermissionPageID, 
            //         p.PermissionPageCode, p.PermissionPageName, p.PageTypeID
            //         from TblPageAccessingUserRole ur
            //         inner join dbo.TblPagePermission p on p.PermissionPageID = ur.PermissionPageID
            //         order by p.PermissionPageCode";
            string SQLPermissionPage = @"select distinct p.PermissionPageID, 
p.PermissionPageCode, p.PermissionPageName, p.PageTypeID
from  dbo.TblPagePermission p 
order by p.PermissionPageCode";
            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLPermissionPage,
                CheckBoxGroupID = "PermissionPage",
                CheckBoxGroupClass = "PermissionPage",
                Required = true,
                ISTab = true,
                TabTitle = "Accessing Page: ",
                LinkTabField = "PageTypeID",
                SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType",
                TableName = "TblAdminAddPageToSystem",
                GetIDENTITYByFiled = "SystemID",
                SetDefaultCheck = true,
                //GetValueFromDBField = "SystemID",
                //GetIDENTITYFromFieldName= "SystemID",
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
                    new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
                }
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        // GET: Administrator/CreatePagePermission
        //[Authorize(Roles = "Add Page Permission, Edit Page Permission, Delete Page Permission")]
        public ActionResult CreatePagePermission()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Page Permission"))
                AllowAdd = true,
                //if (html.HasPermission("Edit Page Permission"))
                AllowEdit = true,
                //if (html.HasPermission("Delete Page Permission"))
                AllowDelete = true,

                DialogBoxWidth = "850",
                AddButton = "Add Page Permission",
                AddURL = "Administrator/CreatePagePermissionForm",
                TableName = "TblPagePermission",
                PrimaryField = "PermissionPageID"
            };
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                Label = "System Name:",
                ID = "CboSysName",
                SQL_CBO = "select distinct tp.SystemID, ts.SystemName from dbo.TblPagePermission tp inner join dbo.TblSystem ts on ts.SystemID = tp.SystemID order by ts.SystemName",
                DisplayText = "SystemName",
                Value = "SystemID",
                DBField = "sp.SystemID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                Label = "Page Code:",
                ID = "CboPageCode",
                SQLAutoComplete = "select distinct top 10 PermissionPageCode from [dbo].[TblPagePermission] where lower(PermissionPageCode) like @SearchText order by PermissionPageCode",
                DBField = "PermissionPageCode",
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Page Type",
                SQL = "select distinct pt.PageTypeID, pt.PageType from dbo.TblPageType pt inner join dbo.TblPagePermission tp on tp.PageTypeID = pt.PageTypeID order by pt.PageType",
                SelectValue = "PageTypeID",
                DisplayField = "PageType",
                RadioValue = EncryAndDecry.Encryp("sp.PageTypeID"),
                DefaultCheckBok = true
            });
            string SQL = "select ROW_NUMBER() OVER(ORDER BY (ts.SystemName + ' - ' + tp.PageType), sp.PermissionPageCode, sp.PermissionPageName) AS[No], ";
            SQL += "ts.SystemID, (ts.SystemName + ' - ' + tp.PageType) SystemName, ";
            SQL += "sp.PermissionPageID, sp.PageTypeID, tp.PageType, sp.PageLink, ";
            SQL += "sp.PermissionPageCode, sp.PermissionPageName, case when isnull(sp.NAVICON, '') = '' then 'ti ti-inner-shadow-bottom-left' else sp.NAVICON end  NAVICON, ";
            SQL += "STUFF((SELECT ', ' + tuu.UserType FROM dbo.TblUserType tuu ";
            SQL += "inner join dbo.TblPageAccessingUserRole tp on tp.UserRoleID = tuu.UserTypeID ";
            SQL += "and tp.PermissionPageID = sp.PermissionPageID ORDER BY tuu.UserType ";
            SQL += "FOR XML PATH('')), 1, 1, '') UserRole ";
            SQL += "from dbo.TblPagePermission sp inner join dbo.TblPageType tp on tp.PageTypeID = sp.PageTypeID ";
            SQL += "inner join dbo.TblSystem ts on ts.SystemID = sp.SystemID ";
            html.OrderBy = "(ts.SystemName + ' - ' + tp.PageType), sp.PermissionPageCode, sp.PermissionPageName";
            html.GH.SQL = SQL;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SystemName", FieldName = "SystemName", IsGroupByField = true },
                new GridField() { Header = "PageLink", FieldName = "PageLink" },
                new GridField() { Header = "PermissionPageCode", FieldName = "PermissionPageCode" },
                new GridField() { Header = "NAVICON", FieldName = "NAVICON" },
                new GridField() { Header = "Permission Page Name", FieldName = "PermissionPageName" },
                //new GridField() { Header = "UserPage", FieldName = "UserType" },
                new GridField() { Header = "UserRole", FieldName = "UserRole" },
                new GridField() { Header = "Action", FieldName = "PermissionPageID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        // GET: Administrator/CreatePagePermissionForm
        //[Authorize(Roles = "Add Page Permission, Edit Page Permission")]
        public ActionResult CreatePagePermissionForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateSetPagePermission",
                FieldIDFocus = "CboSystemName",
                TableName = "TblPagePermission",
                PrimaryField = "PermissionPageID",
                PrimaryFieldValue = PrimaryFieldValue,
                UseTable = true,
                TableColumn = 2
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "System Name:",
                ID = "CboSystemName",
                DBField = "SystemID",
                SQL_CBO = "select SystemID, SystemName from dbo.TblSystem order by SystemName",
                Value = "SystemID",
                DisplayText = "SystemName",
                Multiple = PrimaryFieldValue == "0",
                Required = true,
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Page Type:",
                ID = "CboPageType",
                DBField = "PageTypeID",
                SQL_CBO = "select PageTypeID, PageType from dbo.TblPageType order by PageType",
                Value = "PageTypeID",
                DisplayText = "PageType"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Link:",
                ID = "TxtPageLink",
                DBField = "PageLink",
                //Required = true,
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 PageLink from dbo.TblPagePermission where lower(PageLink) like @SearchText  order by PageLink"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "NAV-ICON:",
                ID = "TxtNavIcon",
                DBField = "NAVICON",
                Value = "ti ti-inner-shadow-bottom-left"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Code:",
                ID = "TxtPermissionPageCode",
                DBField = "PermissionPageCode",
                Required = true,
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 PermissionPageCode from dbo.TblPagePermission where lower(PermissionPageCode) like @SearchText  order by PermissionPageCode"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Name:",
                ID = "TxtPermissionPageName",
                DBField = "PermissionPageName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithControlName = "CboSystemName",
                CheckExistingWithField = "SystemID"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order Number:",
                ID = "TxtOrderNumber",
                DBField = "OrderNumber",
                Required = true,
                ISInteger = true
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Menu Page:",
                ID = "CboMenPage",
                DBField = "MenuID",
                SQL_CBO = "select UserTypeID, UserType from dbo.TblUserType where UserTypeID in (1, 2) order by UserType",
                Value = "UserTypeID",
                DisplayText = "UserType"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Parent Page Code:",
                ID = "TxtParentPageCode",
                DBField = "ParentPageCode",
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 PermissionPageCode from dbo.TblPagePermission where lower(PermissionPageCode) like @SearchText  order by PermissionPageCode"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Page Category:",
                ID = "CboPageCategory",
                DBField = "PageCategoryID",
                SQL_CBO = "select PageCategoryID, PageCategory from dbo.TblPageCategory order by PageCategory",
                Value = "PageCategoryID",
                DisplayText = "PageCategory",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Sub Category:",
                ID = "TxtPageSubCategory",
                DBField = "PageSubCategory",
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 PageSubCategory from dbo.TblPagePermission where lower(PageSubCategory) like @SearchText  order by PageSubCategory"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Page Sub Category:",
                ID = "Txt",
                Hide = true
            });
            //string SQLCheckBoxGroup = "select UserTypeID, UserType from dbo.TblUserType order by UserType";
            //HtmlCheckBoxGroup htmlCheckBoxGroup = new HtmlCheckBoxGroup();
            //htmlCheckBoxGroup.SqlCheckBoxGroup = SQLCheckBoxGroup;
            //htmlCheckBoxGroup.Colspan = "5";
            //htmlCheckBoxGroup.Required = true;
            //htmlCheckBoxGroup.CheckBoxGroupTitle = "Choose User Roles";
            //htmlCheckBoxGroup.TableName = "TblPageAccessingUserRole";
            //htmlCheckBoxGroup.GetIDENTITYByFiled = "PermissionPageID";
            //htmlCheckBoxGroup.CheckBoxGroup = new CheckBoxValue_Lable[]
            //{
            //	new CheckBoxValue_Lable(){ Name="UserType", Value="UserTypeID", Label="UserType", DBField="UserRoleID" },
            //         };
            //html.HTMLControls.Add(htmlCheckBoxGroup);
            html.RestFields = "TxtPermissionPageName";
            html.FocusFieldIDAfterReset = "TxtPermissionPageName";

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        // GET: Administrator/CreateUserType
        public ActionResult CreateUserType()
        {
            Common common = new Common();
            DoActionData doActionData = new DoActionData();
            //if (doActionData.IsHavePermission(common.getSessionName("UserID", "").ToString(), "ASN", "ESN", "DSN"))
            //{
            HTML html = new HTML()
            {
                AddButton = "Add User Type",
                AddURL = "Administrator/CreateUserTypeForm"
            };
            Session["UserID"] = "1";
            string UserID = Session["UserID"].ToString();
            html.SetFormProperties.Add("TblName", "TblUserType");
            html.SetFormProperties.Add("PrimaryField", "UserTypeID");
            html.SetFormProperties.Add("AddPermission", "ASN");
            html.SetFormProperties.Add("EditPermission", "ESN");
            html.SetFormProperties.Add("DeletePermission", "DSN");

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Type",
                SQL = "select UserTypeID, UserType from dbo.TblUserType",
                SelectValue = "UserTypeID",
                DisplayField = "UserType",
                RadioValue = EncryAndDecry.Encryp("UserTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "User Type", FieldName = "UserType" },
                new GridField() { Header = "Action", FieldName = "UserTypeID", TextAlign = "Center",  IsModifyField = true } //DeleteUseProcedureName = "SP_DELETE_STOCK_NAME"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1);
            return View();
        }
        // GET: Administrator/CreateUserTypeForm
        public ActionResult CreateUserTypeForm(string PrimaryFieldValue)
        {
            //doActionData.IsHavePermission(common.getSessionName("UserID", "").ToString(), "ASN", "ESN", "");
            HTML html = new HTML
            {
                FormID = "CreateUserTypeForm",
                FieldIDFocus = "TxtUserType"
            };
            html.SetFormProperties.Add("TblName", "TblUserType");
            html.SetFormProperties.Add("PrimaryField", "UserTypeID");
            html.SetFormProperties.Add("PrimaryFieldValue", PrimaryFieldValue.ToString());
            string WarehouseID = Session["WarehouseID"].ToString();
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Type:",
                ID = "TxtUserType",
                DBField = "UserType",
                Required = true,
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "WarehouseID:",
                ID = "TxtWarehouseID",
                DBField = "WarehouseID",
                Value = WarehouseID,
                Hide = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        // GET: Administrator/CreateUser
        //[Authorize(Roles = "Add User, Edit User, Delete User")]
        public ActionResult CreateUser()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add User"))
                AllowAdd = true,
                //if (html.HasPermission("Edit User"))
                AllowEdit = true,
                //if (html.HasPermission("Delete User"))
                AllowDelete = true,

                AddButton = "Add User",
                AddURL = "Administrator/CreateUserForm",
                DialogBoxWidth = "1024",
                TableName = "TblUserLogin"
            };
            //html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "System Name",
            //    SQL = "select distinct ts.SystemID, ts.SystemName from dbo.TblSetUserAccessSystemAndWarehouse tsa inner join dbo.TblUserLogin tu on tu.UserLoginID = tsa.UserLoginID inner join dbo.TblSystem ts on ts.SystemID = tsa.SystemID order by ts.SystemName",
            //    SelectValue = "SystemID",
            //    DisplayField = "SystemName",
            //    RadioValue = EncryAndDecry.Encryp("tsa.SystemID") + ", " + EncryAndDecry.Encryp("tl.UserName"),
            //    DefaultCheckBok = true
            //});
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = "select distinct top 10 UserName from dbo.TblUserLogin where LOWER(UserName) LIKE @SearchText ORDER BY UserName", //WarehouseID = " + Session["WarehouseID"].ToString() + " and
                SelectValue = "UserName",
                DisplayField = "UserName",
                RadioValue = EncryAndDecry.Encryp("tl.UserName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            string SQL = @"select ROW_NUMBER() OVER(ORDER BY tl.UserName) AS [No],
tl.UserLoginID, STUFF((SELECT ', ' + ts.SystemName FROM dbo.TblSystem ts
inner join dbo.TblSetUserAccessingSystem tsas on tsas.UserLoginID = tl.UserLoginID
and tsas.SystemID = ts.SystemID ORDER BY ts.SystemName
FOR XML PATH('')), 1, 1, '') SystemName,
tl.FullName, tl.UserName, tl.Telephone, tl.[Address],
tl.Email, tl.Job, case when ts.[Status] = 'Enable' then '<span class=""badge badge-primary"">Enable</span>' else '<span class=""badge badge-danger"">Disable</span>' end Status, 
convert(varchar, tl.CreateDate, 103) CreateDate, tl.CreateBy, tl.DomainID
from dbo.TblUserLogin tl
inner join dbo.TblStatus ts on ts.StatusID = tl.StatusID ";
            html.GH.SQL = SQL;
            html.OrderBy = "[No]";
            html.ShowTblTitle = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "UserName", FieldName = "UserName" },
                new GridField() { Header = "Telephone", FieldName = "Telephone" },
                new GridField() { Header = "Position", FieldName = "Job" },
                new GridField() { Header = "CreateDate", FieldName = "CreateDate", TextAlign="center" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                //new GridField() { Header = "Edit", FieldName = "Edit", IsGroupByField = true, Colspan = 4 },
                //new GridField() { Header = "Delete", FieldName = "Delete", IsGroupByField = true, Colspan = 4 },
                new GridField() { Header = "System Name", FieldName = "SystemName" },
                new GridField() { Header = "Domain ID", FieldName = "DomainID", ClassName = "text-right" },                
                //new GridField() { Header = "Address", FieldName = "Address" },
                //new GridField() { Header = "Email", FieldName = "Email" },                
                //new GridField() { Header = "Description", FieldName = "Description" },
                
                new GridField() { Header = "Status", FieldName = "Status", ClassName = "text-center" },
                new GridField() { Header = "Action", FieldName = "UserLoginID", TextAlign = "Center",  IsModifyField = true } //DeleteUseProcedureName = "SP_DELETE_STOCK_NAME"
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult ShowUserByRole()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add User"))
                AllowAdd = true,
                //if (html.HasPermission("Edit User"))
                AllowEdit = true,
                //if (html.HasPermission("Delete User"))
                AllowDelete = true,

                TableName = "TblUserLogin"
            };
            //html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "System Name",
            //    SQL = "select distinct ts.SystemID, ts.SystemName from dbo.TblSetUserAccessSystemAndWarehouse tsa inner join dbo.TblUserLogin tu on tu.UserLoginID = tsa.UserLoginID inner join dbo.TblSystem ts on ts.SystemID = tsa.SystemID order by ts.SystemName",
            //    SelectValue = "SystemID",
            //    DisplayField = "SystemName",
            //    RadioValue = EncryAndDecry.Encryp("tsa.SystemID") + ", " + EncryAndDecry.Encryp("tl.UserName"),
            //    DefaultCheckBok = true
            //});
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRoleName",
                Label = "Role Name:",
                DBField = "ul.UserRoleID",
                SQL_CBO = "select distinct ul.UserRoleID, ul.RoleName\r\nfrom dbo.TblUserLogin tl\r\ninner join dbo.TblUserRole ul on ul.UserRoleID = tl.UserRoleID\r\norder by ul.RoleName",
                DisplayText = "RoleName",
                Value = "UserRoleID",
                ComboBox = true,
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = "select distinct top 10 UserName from dbo.TblUserLogin where LOWER(UserName) LIKE @SearchText ORDER BY UserName", //WarehouseID = " + Session["WarehouseID"].ToString() + " and
                SelectValue = "UserName",
                DisplayField = "UserName",
                RadioValue = EncryAndDecry.Encryp("ul.UserName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            string SQL = @"select ROW_NUMBER() OVER(PARTITION by ul.FullName ORDER BY ul.UserName) AS [No],
ul.UserLoginID, s.SystemName, ur.RoleName,
ul.FullName, ul.UserName, ul.Telephone, ul.[Address],
ul.Email, ul.Job, case when ts.[Status] = 'Enable' then '<span class=""badge badge-primary"">Enable</span>' else '<span class=""badge badge-danger"">Disable</span>' end Status, convert(varchar, ul.CreateDate, 103) CreateDate, ul.CreateBy
from dbo.TblUserLogin ul
inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
inner join dbo.TblStatus ts on ts.StatusID = ul.StatusID
inner join dbo.TblSystem s on s.SystemID = sw.SystemID
inner join dbo.TblUserRole ur on ur.UserRoleID = sw.UserRoleID";
            html.GH.SQL = SQL;
            html.OrderBy = "[No]";
            html.ShowTblTitle = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Full Name", FieldName = "FullName", IsGroupByField = true },
                new GridField() { Header = "UserName", FieldName = "UserName" },
                new GridField() { Header = "Role Name", FieldName = "RoleName", },
                new GridField() { Header = "Access System", FieldName = "SystemName" },
                new GridField() { Header = "Telephone", FieldName = "Telephone" },
                new GridField() { Header = "Job", FieldName = "Job" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        // GET: Administrator/CreateUserForm
        //[Authorize(Roles = "Add User, Edit User")]
        public ActionResult CreateUserForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateUser",
                FieldIDFocus = "TxtFullName",
                UseTable = true,
                TableColumn = 2,
                TableName = "TblUserLogin",
                PrimaryField = "UserLoginID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Full Name:",
                ID = "TxtFullName",
                DBField = "FullName",
                Required = true,
                SQL = "https://192.168.69.83:9797/api/GetEmployee?CompanyID=1",
                isTypeahead = true,
                IsGetDataFromAPI = true,
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkZjgwMTJmLTA4MTQtNDYzMS1hNmU5LTUzMjU5Y2IyYWUyOSIsIm5iZiI6MTcxMDQ3MDU0MSwiZXhwIjoxNzQyMDA2NTQxLCJpYXQiOjE3MTA0NzA1NDEsImlzcyI6Imh0dHBzOi8vaHIubnNtc29sdXRpb24uY29tLmtoLyIsImF1ZCI6Imh0dHBzOi8vaHIubnNtc29sdXRpb24uY29tLmtoOjk3OTcvYXBpLyJ9.nWQXSDg9at2qhe-BdN6sXT9XryVvXxDoDBQ7Qhi57EY",
                ReplaceJsonKey = new Dictionary<string, string>
                {
                    { "employeeName", "FullName" },
                    { "employeePhoneNumber", "TxtTelephone" },
                    { "positionName", "TxtJob" },
                },
                DisplayOnFields = "TxtTelephone,TxtJob"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Name:",
                ID = "TxtUserName",
                DBField = "UserName",
                Required = true,
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "TxtPassword",
                DBField = "Password",
                Required = true,
                Type = "password"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Confirm Password:",
                ID = "TxtConfirmPassword",
                Required = true,
                Type = "password",
                EqualTo = "TxtPassword"

            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telephone:",
                ID = "TxtTelephone",
                DBField = "Telephone",
                Required = true,
                //Type = "tel"
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
                Type = "email"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Position:",
                ID = "TxtJob",
                DBField = "Job",
                Required = true
            });
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //    Label = "Description:",
            //    ID = "TxtDescription",
            //    DBField = "Description"
            //});
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatusID",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in (3,4)",
                Value = "StatusID",
                DisplayText = "Status"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Domain Name:",
                ID = "CboDomainName",
                DBField = "DomainID",
                Required = true,
                Multiple = true,
                SQL_CBO = @"select Code DomainID, Code + '-'+ 
(select STUFF((SELECT ',' + d.DomainName
from dbo.TblDomain d
where d.DomainID in (select distinct value from dbo.TblAccessDomain a 
CROSS APPLY STRING_SPLIT(a.DomainID, ',') where a.AccessDomainID = aa.AccessDomainID )
ORDER BY d.DomainName FOR XML PATH('')), 1, 1, '')) DomainName
from dbo.TblAccessDomain aa order by aa.Code",
                Value = "DomainID",
                DisplayText = "DomainName",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Change Password On Login:",
                ID = "TxtISChangePasswordOnNextLogon",
                DBField = "ISChangePasswordOnNextLogon",
                LabelValign = "center",
                Value = "1",
                DefaultCheckBoxChecked = true,
                Type = "checkbox",
                Class = "",
                DivClass = "custom-control custom-checkbox p-0",
                PrimaryFieldValue = PrimaryFieldValue
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Template:",
                ID = "CboTemplateName",
                //DBField = "TemplateID",
                //SQL_CBO = "select TemplateID, TemplateName from dbo.TblTemplate",
                //Value = "TemplateID",
                //DisplayText = "TemplateName",
                Hide = true
            });


            string SQLCheckBoxGroup = "select ts.SystemID, ts.SystemName ";
            SQLCheckBoxGroup += "from dbo.TblSystem ts ";
            SQLCheckBoxGroup += "order by ts.SystemName";

            HtmlCheckBoxGroup htmlCheckBoxGroup = new HtmlCheckBoxGroup
            {
                SqlCheckBoxGroup = SQLCheckBoxGroup,
                ISCheckBoxGroup = true,
                Colspan = "5",
                Required = true,
                CheckBoxGroupTitle = "Choose Accesing System",
                TableName = "TblSetUserAccessingSystem",
                GetIDENTITYByFiled = "UserLoginID",
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="SystemID", Value="SystemID", Label="SystemName", DBField="SystemID" },
                //new CheckBoxValue_Lable(){ Name="WarehouseID", Value="WarehouseID", Label="WarehouseName", DBField="WarehouseID" }
                }
            };
            html.HTMLControls.Add(htmlCheckBoxGroup);


            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        // GET: Administrator/SetUserPermission
        //[Authorize(Roles = "Add User, Edit User, Delete User")]
        public ActionResult SetUserPermission()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add User"))
                AllowAdd = true,
                //if (html.HasPermission("Edit User"))
                AllowEdit = true,
                //if (html.HasPermission("Delete User"))
                AllowDelete = true,

                AddButton = "Set User Permission",
                AddURL = "Administrator/SetUserPermissionForm",
                DialogBoxWidth = "850",
                DialogBoxHeight = 600,
                TableName = "TblSetUserAccessSystemAndWarehouse"
            };
            //html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "System Name",
            //    SQL = "select distinct ts.SystemID, ts.SystemName from dbo.TblSetUserAccessSystemAndWarehouse tsa inner join dbo.TblUserLogin tu on tu.UserLoginID = tsa.UserLoginID inner join dbo.TblSystem ts on ts.SystemID = tsa.SystemID order by ts.SystemName",
            //    SelectValue = "SystemID",
            //    DisplayField = "SystemName",
            //    RadioValue = EncryAndDecry.Encryp("tsa.SystemID") + ", " + EncryAndDecry.Encryp("tl.UserName"),
            //    DefaultCheckBok = true
            //});
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = "select distinct top 10 tu.UserName from dbo.TblUserLogin tu inner join dbo.TblSetUserAccessSystemAndWarehouse tsw on tsw.UserLoginID = tu.UserLoginID where LOWER(tu.UserName) LIKE @SearchText ORDER BY tu.UserName",
                SelectValue = "UserName",
                DisplayField = "UserName",
                RadioValue = EncryAndDecry.Encryp("tu.UserName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            string SQL = "select ROW_NUMBER() OVER(ORDER BY tu.UserName) AS [No], ";
            SQL += "tu.UserLoginID, tu.UserName, ts.SystemName, tw.WarehouseName, tsw.UserAccessSystemAndWarehouseID ";
            SQL += "from dbo.TblUserLogin tu inner join dbo.TblSetUserAccessSystemAndWarehouse tsw on tsw.UserLoginID = tu.UserLoginID ";
            SQL += "inner join dbo.TblSetPagePermission tp on tp.UserAccessSystemAndWarehouseID = tsw.UserAccessSystemAndWarehouseID ";
            SQL += "inner join dbo.TblSystem ts on ts.SystemID = tsw.SystemID ";
            SQL += "inner join dbo.TblWarehouse tw on tw.WarehouseID = tsw.WarehouseID ";
            SQL += "group by tu.UserLoginID, tu.UserName, ts.SystemName, tw.WarehouseName, tsw.UserAccessSystemAndWarehouseID ";
            html.GH.SQL = SQL;
            html.OrderBy = "[No]";
            html.IsUseHavingCondition = true;
            html.ShowTblTitle = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "UserName", FieldName = "UserName" },
                new GridField() { Header = "SystemName", FieldName = "SystemName" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", Width = "100%" },
                new GridField() { Header = "Action", FieldName = "UserAccessSystemAndWarehouseID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblSetPagePermission" }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        // GET: Administrator/CreateUserForm
        //[Authorize(Roles = "Add User, Edit User")]
        public ActionResult SetUserPermissionForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "SetUserPermissionForm",
                FieldIDFocus = "TxtUserName",
                //html.UseTable = true;
                //html.TableColumn = 2;
                TableName = "TblSetUserAccessSystemAndWarehouse",
                PrimaryField = "UserAccessSystemAndWarehouseID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            string[] editdata = new string[11];
            editdata[6] = "";
            editdata[7] = "";
            editdata[8] = "";
            if (PrimaryFieldValue != "0")
            {
                string SQLForEdit = "select tu.UserLoginID, tu.UserName, tu.FullName, tu.Telephone, tu.Job, ts.SystemID, ts.WarehouseID, ts.UserRoleID, ts.DefaultDashboardPage, ts.DomainID from dbo.TblUserLogin tu ";
                SQLForEdit += "inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = tu.UserLoginID  ";
                SQLForEdit += "where ts.UserAccessSystemAndWarehouseID = " + PrimaryFieldValue;
                DataTable dt = html.GetDataForEdit(SQLForEdit);
                editdata[0] = dt.Rows[0]["UserLoginID"].ToString();
                editdata[1] = dt.Rows[0]["UserName"].ToString();
                editdata[2] = dt.Rows[0]["FullName"].ToString();
                editdata[3] = dt.Rows[0]["Telephone"].ToString();
                editdata[4] = dt.Rows[0]["Job"].ToString();
                editdata[5] = dt.Rows[0]["SystemID"].ToString();
                editdata[6] = dt.Rows[0]["WarehouseID"].ToString();
                editdata[7] = dt.Rows[0]["UserRoleID"].ToString();
                editdata[8] = dt.Rows[0]["DefaultDashboardPage"].ToString();
                editdata[9] = dt.Rows[0]["DomainID"].ToString();
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Name:",
                ID = "TxtUserName",
                Required = true,
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 tu.UserName from dbo.TblUserLogin tu where lower(tu.UserName) like @SearchText order by tu.UserName",
                SQLCallBack = "select UserLoginID, FullName, Telephone, Job from dbo.TblUserLogin where lower(UserName) = @SearchText",
                DisplayOnFields = "TxtUserLoginID, TxtFullName, TxtTelephone, TxtJob",
                ISCallBack = true,
                SetChangeEventOnControlID = "TxtUserLoginID",
                Value = editdata[1]
            });
            string sqlCallBacktxtulid = "select distinct ts.SystemID, ts.SystemName from TblSetUserAccessingSystem tsu ";
            sqlCallBacktxtulid += "inner join dbo.TblSystem ts on ts.SystemID = tsu.SystemID ";
            sqlCallBacktxtulid += "where tsu.UserLoginID = @SearchText order by ts.SystemName";
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "UserLoginID:",
                ID = "TxtUserLoginID",
                DBField = "UserLoginID",
                IsReadOnly = true,
                ISCallBackToCbo = true,
                CallBackToControlID = "CboSystemName",
                SQLCallBack = sqlCallBacktxtulid,
                CallBackDisplayText = "SystemName",
                CallBackDisplayValue = "SystemID",
                Required = true,
                Value = editdata[0]
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Full Name:",
                ID = "TxtFullName",
                IsReadOnly = true,
                Value = editdata[2]
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telephone:",
                ID = "TxtTelephone",
                IsReadOnly = true,
                Value = editdata[3]
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Job:",
                ID = "TxtJob",
                IsReadOnly = true,
                Value = editdata[4]
            });
            string sqlwarehouse = "select w.WarehouseID, w.WarehouseName from dbo.TblWarehouseSystem ws ";
            sqlwarehouse += "inner join dbo.TblWarehouse w on w.WarehouseID = ws.WarehouseID ";
            sqlwarehouse += "inner join dbo.TblSystem s on s.SystemID = ws.SystemID ";
            sqlwarehouse += "where ws.SystemID = @SearchText order by w.WarehouseName;";
            sqlwarehouse += "select ur.UserRoleID, ur.RoleName from dbo.TblUserRole ur where ur.SystemID = @SearchText";
            //sqlwarehouse += "select distinct p.PageLink, p.PermissionPageCode ";
            //sqlwarehouse += "from dbo.TblPagePermission p ";
            //sqlwarehouse += "inner join dbo.TblUserRolePagePermission up on up.PermissionPageID = p.PermissionPageID ";
            //sqlwarehouse += "where up.UserRoleID = @SearchText";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "System Name:",
                ID = "CboSystemName",
                ISCallBackToCbo = true,
                SQLCallBack = sqlwarehouse,
                CallBackDisplayText = "WarehouseName, RoleName",
                CallBackDisplayValue = "WarehouseID, UserRoleID",
                CallBackToControlID = "CboWarehouse, CboRoleName",
                DBField = "SystemID",
                Required = true,
                CboValue = editdata[5],
                SetValueToNextControl = editdata[6] + "~" + editdata[7],
                SetValueToNextControlID = "CboWarehouse, CboRoleName",
                IsUseSelect2 = false,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Warehouse Name:",
                ID = "CboWarehouse",
                DBField = "WarehouseID",
                Required = true,
                IsUseSelect2 = false,
            });
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //	Label = "Domain Name:",
            //	ID = "CboDomainName",
            //	DBField = "DomainID",
            //	Required = true,
            //	Multiple = true,
            //	SQL_CBO = "select DomainID, DomainName from dbo.TblDomain order by DomainName",
            //	Value = "DomainID",
            //	DisplayText = "DomainName",
            //	CboValue = editdata[9]
            //});
            string sql = "select '' PageLink, 'Default' PermissionPageCode union all select distinct p.PageLink, p.PermissionPageCode ";
            sql += "from dbo.TblPagePermission p ";
            sql += "inner join dbo.TblUserRolePagePermission ur on ur.PermissionPageID = p.PermissionPageID ";
            sql += "where p.PageLink <> '' and ur.UserRoleID = @SearchText";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Role Name:",
                ID = "CboRoleName",
                DBField = "UserRoleID",
                Required = true,
                IsUseSelect2 = false,
                ISCallBackToCbo = true,
                SQLCallBack = sql,
                CallBackDisplayText = "PermissionPageCode",
                CallBackDisplayValue = "PageLink",
                CallBackToControlID = "CboDefaultDashboardPage",
                SetValueToNextControl = editdata[8],
                SetValueToNextControlID = "CboDefaultDashboardPage",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Default Dashboard Page:",
                ID = "CboDefaultDashboardPage",
                DBField = "DefaultDashboardPage",
                //Required = true,
                IsUseSelect2 = false,
            });

            ////html.HTMLControls.Add(new HtmlTextarea()
            ////{
            ////    Label = "Description:",
            ////    ID = "TxtDescription",
            ////    DBField = "Description"
            ////});

            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "CreateDate:",
            //    ID = "TxtCreateDate",
            //    DBField = "CreateDate",
            //    Value = html.GetCurrentDate(),
            //    Hide = true
            //});

            //string SQLCheckBoxGroup = "select ts.SystemID, ts.SystemName ";
            //SQLCheckBoxGroup += "from dbo.TblSystem ts ";
            //SQLCheckBoxGroup += "order by ts.SystemName";

            //HtmlCheckBoxGroup htmlCheckBoxGroup = new HtmlCheckBoxGroup();
            //htmlCheckBoxGroup.SqlCheckBoxGroup = SQLCheckBoxGroup;
            //htmlCheckBoxGroup.Colspan = "5";
            //htmlCheckBoxGroup.Required = true;
            //htmlCheckBoxGroup.CheckBoxGroupTitle = "Choose System";
            //htmlCheckBoxGroup.TableName = "TblSetUserAccessingSystem";
            //htmlCheckBoxGroup.GetIDENTITYByFiled = "UserLoginID";
            //htmlCheckBoxGroup.CheckBoxGroup = new CheckBoxValue_Lable[]
            //{
            //    new CheckBoxValue_Lable(){ Name="SystemID", Value="SystemID", Label="SystemName", DBField="SystemID" },
            //    //new CheckBoxValue_Lable(){ Name="WarehouseID", Value="WarehouseID", Label="WarehouseName", DBField="WarehouseID" }
            //};
            //html.HTMLControls.Add(htmlCheckBoxGroup);

            string SQLPermissionPage = "select PageTypeID, PermissionPageID, PermissionPageCode, PermissionPageName ";
            SQLPermissionPage += "from dbo.TblPagePermission where SystemID = 0 order by PermissionPageCode, OrderNumber";

            HtmlCheckBoxGroup PermissionPage = new HtmlCheckBoxGroup
            {
                //PermissionPage.Hide = true;
                SqlCheckBoxGroup = SQLPermissionPage,
                CheckBoxGroupID = "PermissionPage",
                CheckBoxGroupClass = "PermissionPage",
                //PermissionPage.Colspan = "2";
                Required = true,
                ISTab = true,
                TabTitle = "Accessing Page: ",
                LinkTabField = "PageTypeID",
                SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType",
                TableName = "TblSetPagePermission",
                GetIDENTITYByFiled = "UserAccessSystemAndWarehouseID",
                GetValueFromDBField = "UserLoginID",
                SetDefaultCheck = true,

                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
                    new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
                }
            };
            html.HTMLControls.Add(PermissionPage);

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public string GetSetUserPermission(int UserLoginID, int UserRoleID, int SystemID, int WarehouseID)
        {
            //string sql = "select tua.UserLoginID, tua.SystemID, tua.WarehouseID, tsp.PermissionPageID";
            //sql += "from dbo.TblSetUserAccessSystemAndWarehouse tua ";
            //sql += "inner join dbo.TblSetPagePermission tsp on tsp.UserAccessSystemAndWarehouseID = tua.UserAccessSystemAndWarehouseID ";
            //sql += "inner join dbo.TblPagePermission tp on tp.PermissionPageID = tsp.PermissionPageID ";
            //sql += "where tua.UserLoginID = " + UserLoginID + " and tua.SystemID = " + SystemID + " and tua.WarehouseID = " + WarehouseID;
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

            string SQLPermissionPage = @"select p.PageTypeID, p.PermissionPageID, p.PermissionPageCode, p.PermissionPageName
            from dbo.TblPagePermission p
            inner join dbo.TblAdminAddPageToSystem ats on ats.PermissionPageID = p.PermissionPageID
            inner join dbo.TblUserRolePagePermission up on up.PermissionPageID = p.PermissionPageID 
            where ats.SystemID = " + SystemID + " and up.UserRoleID = " + UserRoleID + "  order by p.OrderNumber, p.PermissionPageCode";

            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLPermissionPage,
                CheckBoxGroupID = "PermissionPage",
                CheckBoxGroupClass = "PermissionPage",
                Required = true,
                ISTab = true,
                TabTitle = "Accessing Page: ",
                LinkTabField = "PageTypeID",
                SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType",
                TableName = "TblSetPagePermission",
                GetIDENTITYByFiled = "UserAccessSystemAndWarehouseID",
                GetValueFromDBField = "UserLoginID",
                SetDefaultCheck = true,

                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable() { Name = "PermissionPageID", Value = "PermissionPageID", Label = "PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode", },
                    new CheckBoxValue_Lable() { Name = "PermissionPageName", Value = "PermissionPageID", Label = "PermissionPageName", DBField = "PermissionPageID" }
                }
            });
            return html.DrawHTMLForm();
        }
        public ActionResult CreateUserLoginTmp()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Expense Type"))
                AllowAdd = true,
                //if (html.HasPermission("Edit System"))
                AllowEdit = true,
                //if (html.HasPermission("Delete System"))
                AllowDelete = true,

                AddButton = "Add User Login Tmp",
                AddURL = "Administrator/AddUserLoginTmp",
                TableName = "TblUserLoginTmp",
                PrimaryField = "LoginTmpID",
                OrderBy = "LoginTmpName"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "LoginTmpName",
                SQL = "select LoginTmpID, LoginTmpName from dbo.TblUserLoginTmp order by LoginTmpName",
                SelectValue = "LoginTmpID",
                DisplayField = "LoginTmpName",
                RadioValue = EncryAndDecry.Encryp("LoginTmpID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "LoginTmpName", FieldName = "LoginTmpName" },
                new GridField() { Header = "Image", FieldName = "Image", ImagePath="Content/UserLoginTmp", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "LoginTmpID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public ActionResult AddUserLoginTmp(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateUserLoginTmp",
                FieldIDFocus = "TxtLoginTmpName",
                TableName = "TblUserLoginTmp",
                PrimaryField = "LoginTmpID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Login Tmp Name:",
                ID = "TxtLoginTmpName",
                DBField = "LoginTmpName",
                Required = true,
                //ISUniqueFiled = true,
                //CheckExsitingWithField = "WarehouseID",
                //CheckExsitingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Image:",
                ID = "TxtImage",
                DBField = "Image",
                DestinationFile = "UserLoginTmp",
                AcceptImageOnly = true,
                ISFile = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult UserRole()
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
            html.AddURL = "Administrator/CreateUserRole";
            html.TableName = "TblUserRole";
            html.PrimaryField = "UserRoleID";
            html.OrderBy = "UserRoleID desc";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "SystemName",
                Label = "System Name:",
                DBField = "SystemName",
                SQL_CBO = "select distinct s.SystemID, s.SystemName from dbo.TblSystem s inner join dbo.TblAdminAddPageToSystem ats on ats.SystemID = s.SystemID",
                DisplayText = "SystemName",
                Value = "SystemName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Role Name",
                SQL = "select distinct ur.RoleName from dbo.TblUserRole ur inner join dbo.TblUserRolePagePermission up on up.UserRoleID = ur.UserRoleID order by ur.RoleName",
                SelectValue = "RoleName",
                DisplayField = "RoleName",
                RadioValue = EncryAndDecry.Encryp("ur.RoleName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over(PARTITION by ur.RoleName order by ur.RoleName, p.PermissionPageCode) as [No], ur.UserRoleID, ur.RoleName, s.SystemName, p.PermissionPageCode, p.PermissionPageName
            from dbo.TblUserRole ur
            inner join dbo.TblUserRolePagePermission up on up.UserRoleID = ur.UserRoleID
            inner join dbo.TblPagePermission p on p.PermissionPageID = up.PermissionPageID
            inner join dbo.TblSystem s on s.SystemID = ur.SystemID
            ";
            html.OrderBy = "[No], s.SystemName, p.ParentPageCode";
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Role Name", FieldName = "RoleName", IsGroupByField = true, GroupLabel="Role Name: " },
                new GridField() { Header = "Permission Page Code", FieldName = "PermissionPageCode", Width = "30%"},
                new GridField() { Header = "Permission Page Name", FieldName = "PermissionPageName", Width = "30%" },
                new GridField() { Header = "System Name", FieldName = "SystemName", Width = "40%" },
                new GridField() { Header = "Action", FieldName = "UserRoleID", TextAlign = "Center",
                    IsModifyField = true, DeleteWithTableName="TblUserRolePagePermission",
                    IsGroupByField = true
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public ActionResult CreateUserRole(string PrimaryFieldValue)
        {
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            HTML html = new HTML
            {
                FormID = "CreateUserRole",
                FieldIDFocus = "TxtRoleName",
                TableName = "TblUserRole",
                PrimaryField = "UserRoleID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "System Name:",
                ID = "Cbo_SystemName",
                DBField = "SystemID",
                Required = true,
                SQL_CBO = "select distinct s.SystemID, s.SystemName from dbo.TblSystem s inner join dbo.TblAdminAddPageToSystem ats on ats.SystemID = s.SystemID",
                DisplayText = "SystemName",
                Value = "SystemID",
                placeholder = "Please choose System Name",
                EventChange = "CboSystemNameChange(this)"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Warehouse Name:",
                ID = "Cbo_WarehouseName",
                DBField = "WarehouseID",
                Required = true,
                SQL_CBO = "select distinct w.WarehouseID, w.WarehouseName from dbo.TblWarehouse w order by w.WarehouseName",
                DisplayText = "WarehouseName",
                Value = "WarehouseID",
                placeholder = "Please choose Warehouse Name"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Role Name:",
                ID = "TxtRoleName",
                DBField = "RoleName",
                Required = true,
                ISUniqueField = true
            });
            string SQLPermissionPage = @"select p.PageTypeID, p.PermissionPageID, p.PermissionPageCode, p.PermissionPageName
            from dbo.TblPagePermission p
            inner join dbo.TblAdminAddPageToSystem ats on ats.PermissionPageID = p.PermissionPageID
            where ats.SystemID = 0  order by p.OrderNumber, PermissionPageCode";
            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLPermissionPage,
                CheckBoxGroupID = "PermissionPage",
                CheckBoxGroupClass = "PermissionPage",
                Required = true,
                ISTab = true,
                TabTitle = "Accessing Page: ",
                LinkTabField = "PageTypeID",
                SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType",
                TableName = "TblUserRolePagePermission",
                GetIDENTITYByFiled = "UserRoleID",
                SetDefaultCheck = true,
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
                    new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
                }
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public string GetUserRolePermission(int SystemID, string PrimaryFieldValue = "0")
        {
            HTML html = new HTML
            {
                PrimaryFieldValue = PrimaryFieldValue
            };
            string SQLPermissionPage = @"select p.PageTypeID, p.PermissionPageID, p.PermissionPageCode, p.PermissionPageName
            from dbo.TblPagePermission p
            inner join dbo.TblAdminAddPageToSystem ats on ats.PermissionPageID = p.PermissionPageID ";
            if (PrimaryFieldValue != "0")
            {
                html.PrimaryField = "UserRoleID";
                //SQLPermissionPage += "inner join dbo.TblUserRolePagePermission up on up.PermissionPageID = p.PermissionPageID ";
            }
            SQLPermissionPage += "where ats.SystemID = " + SystemID;
            //if (PrimaryFieldValue != 0)
            //{
            //    SQLPermissionPage += " and up.UserRoleID =" + PrimaryFieldValue;
            //}
            SQLPermissionPage += " order by PermissionPageCode";

            html.HTMLControls.Add(new HtmlCheckBoxGroup()
            {
                SqlCheckBoxGroup = SQLPermissionPage,
                CheckBoxGroupID = "PermissionPage",
                CheckBoxGroupClass = "PermissionPage",
                Required = true,
                ISTab = true,
                TabTitle = "Accessing Page: ",
                LinkTabField = "PageTypeID",
                SqlTab = "select PageTypeID, PageType from dbo.TblPageType order by PageType",
                TableName = "TblUserRolePagePermission",
                GetIDENTITYByFiled = "UserRoleID",
                SetDefaultCheck = true,
                CheckBoxGroup = new CheckBoxValue_Lable[]
                {
                    new CheckBoxValue_Lable(){ Name="PermissionPageID", Value="PermissionPageID", Label="PermissionPageCode", GroupByCheckBoxVale = "PermissionPageCode" },
                    new CheckBoxValue_Lable(){ Name="PermissionPageName", Value="PermissionPageID", Label="PermissionPageName", DBField="PermissionPageID" }
                }
            });
            return html.DrawHTMLForm();
        }

        [Authorize(Roles = "Add Domain Name, Edit Domain Name, Delete Domain Name")]
        public ActionResult AddDomainName()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Domain Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Domain Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Domain Name"))
                html.AllowDelete = true;

            html.AddButton = "Add Domain Name";
            html.AddURL = "Administrator/CreateDomainName";
            html.TableName = "TblDomain";
            html.PrimaryField = "DomainID";
            html.OrderBy = "CreateDate desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Domain Name",
                SQL = "select DomainID, DomainName from dbo.TblDomain where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by DomainName",
                SelectValue = "DomainID",
                DisplayField = "DomainName",
                RadioValue = EncryAndDecry.Encryp("DomainID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Domain Name", FieldName = "DomainName", Width="100%" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy", ClassName="text-center" },
                new GridField() { Header = "Action", FieldName = "DomainID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Domain Name, Edit Domain Name")]
        public string CreateDomainName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateDomainName",
                FieldIDFocus = "TxtDomainName",
                TableName = "TblDomain",
                PrimaryField = "DomainID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Domain Name:",
                ID = "TxtDomainName",
                DBField = "DomainName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            return html.DrawHTMLForm();
        }
        [Authorize(Roles = "Add Access Domain Name, Edit Access Domain Name, Delete Access Domain Name")]
        public ActionResult SetAccessDomainName()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Access Domain Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Access Domain Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Access Domain Name"))
                html.AllowDelete = true;

            html.AddButton = "Add Access Domain Name";
            html.AddURL = "Administrator/CreateAccessDomainName";
            html.TableName = "TblAccessDomain";
            html.PrimaryField = "AccessDomainID";
            html.OrderBy = "Code";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Domain Name",
                SQL = "select AccessDomainID, Code from dbo.TblAccessDomain where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Code",
                SelectValue = "AccessDomainID",
                DisplayField = "Code",
                RadioValue = EncryAndDecry.Encryp("AccessDomainID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY a.AccessDomainID desc) AS[No], a.AccessDomainID,
a.Code, STUFF((SELECT ', ' + d.DomainName FROM dbo.TblDomain d 
CROSS APPLY STRING_SPLIT(isnull(a.DomainID,''), ',')
where value in(d.DomainID)
ORDER BY d.DomainName FOR XML PATH('')), 1, 1, '') AccessDomain, a.CreateBy, CreateDate
from dbo.TblAccessDomain a";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Code", FieldName = "Code" },
                new GridField() { Header = "Access Domain", FieldName = "AccessDomain", Width = "30%", ClassName="text-truncate"},
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy", TextAlign = "Center" },
                new GridField() { Header = "Action", FieldName = "AccessDomainID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Access Domain Name, Edit Access Domain Name")]
        public string CreateAccessDomainName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateAccessDomainName",
                FieldIDFocus = "TxtAccessDomainName",
                TableName = "TblAccessDomain",
                PrimaryField = "AccessDomainID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Code:",
                ID = "TxtCode",
                DBField = "Code",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Domain Name:",
                ID = "CboDomainName",
                DBField = "DomainID",
                Required = true,
                Multiple = true,
                SQL_CBO = "select DomainID, DomainName from dbo.TblDomain order by DomainName",
                Value = "DomainID",
                DisplayText = "DomainName",
            });
            return html.DrawHTMLForm();
        }
        [Authorize(Roles = "Add Template, Edit Template, Delete Template")]
        public ActionResult AddTemplateName()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Template"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Template"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Template"))
                html.AllowDelete = true;

            html.AddButton = "Add Template Name";
            html.AddURL = "Administrator/CreateTemplateName";
            html.TableName = "TblTemplate";
            html.PrimaryField = "TemplateID";
            html.OrderBy = "TemplateID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Template Name",
                SQL = "select TemplateID, TemplateName from dbo.TblTemplate where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by TemplateName",
                SelectValue = "TemplateID",
                DisplayField = "TemplateName",
                RadioValue = EncryAndDecry.Encryp("TemplateID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Template Name", FieldName = "TemplateName", Width="100%"},
                new GridField() { Header = "Named Templates", FieldName = "NamedTemplates" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "Action", FieldName = "TemplateID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Template, Edit Template")]
        public string CreateTemplateName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateCreateTemplateName",
                FieldIDFocus = "TxtCreateTemplateName",
                TableName = "TblTemplate",
                PrimaryField = "TemplateID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Template Name:",
                ID = "TxtTemplateName",
                DBField = "TemplateName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Named Templates:",
                ID = "TxtNamedTemplates",
                DBField = "NamedTemplates",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Login Tmp Name:",
                ID = "TxtLoginTmpName",
                DBField = "LoginTmpName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Domain Name:",
                ID = "CboDomainName",
                DBField = "ForDomainID",
                Required = true,
                Multiple = true,
                SQL_CBO = "select DomainID, DomainName from dbo.TblDomain order by DomainName",
                Value = "DomainID",
                DisplayText = "DomainName",
            });
            return html.DrawHTMLForm();
        }

        [Authorize(Roles = "Add User Template, Edit User Template, Delete User Template")]
        public ActionResult AddUserTemplate()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add User Template"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit User Template"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete User Template"))
                html.AllowDelete = true;
            html.AddButton = "Add User Template";
            html.AddURL = "Administrator/CreateUserTemplate";
            html.TableName = "TblUserTemplate";
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = "select u.UserLoginID, u.UserName\r\nfrom TblUserTemplate t\r\ninner join dbo.TblUserLogin u on u.UserLoginID = t.UserLoginID",
                SelectValue = "UserLoginID",
                DisplayField = "UserName",
                RadioValue = EncryAndDecry.Encryp("UserLoginID"),
                DefaultCheckBok = true
            });
            //string SQL = "select ROW_NUMBER() OVER(ORDER BY ts.SystemName) AS[No], tw.WarehouseID, ts.SystemName, tw.WarehouseName ";
            //SQL += "from TblWarehouse tw inner join dbo.TblSystem ts on ts.SystemID = tw.SystemID";
            string SQL = @"SELECT 
    ROW_NUMBER() OVER(ORDER BY ut.CreateDate DESC) AS [No], 
    u.UserName, 
    ut.UserTemplateID, 
    t.TemplateName,

    STUFF((
        SELECT ', ' + d.DomainName
        FROM dbo.TblDomain d
        WHERE d.DomainID IN (
            SELECT TRY_CAST(value AS UNIQUEIDENTIFIER)
            FROM string_split(ut.ForDomainID, ',')
        )
        FOR XML PATH('')
    ), 1, 2, '') AS DomainName

FROM TblUserTemplate ut
INNER JOIN dbo.TblUserLogin u 
    ON u.UserLoginID = ut.UserLoginID
INNER JOIN dbo.TblTemplate t 
    ON t.TemplateID = ut.TemplateID
";
            html.GH.SQL = SQL;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "User Name", FieldName = "UserName" },
                new GridField() { Header = "Template Name", FieldName = "TemplateName", Width ="100%" },
                new GridField() { Header = "Domain Name", FieldName = "DomainName" },
                new GridField() { Header = "Action", FieldName = "UserTemplateID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add User Template, Edit User Template")]
        public string CreateUserTemplate(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateUserTemplate",
                FieldIDFocus = "TxtUserTemplate",
                TableName = "TblUserTemplate",
                PrimaryField = "UserTemplateID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "User Name:",
                ID = "TxtUserTemplate",
                DBField = "UserLoginID",
                SQL_CBO = "select UserLoginID, UserName from dbo.TblUserLogin",
                Value = "UserLoginID",
                DisplayText = "UserName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Template Name:",
                ID = "TxtTemplateName",
                DBField = "TemplateID",
                SQL_CBO = "select TemplateID, TemplateName from dbo.TblTemplate",
                Value = "TemplateID",
                DisplayText = "TemplateName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "DomainName:",
                ID = "TxtDomainName",
                DBField = "ForDomainID",
                SQL_CBO = "select DomainID, DomainName from dbo.TblDomain",
                Value = "DomainID",
                DisplayText = "DomainName",
                Required = true,
                Multiple = true
            });

            return html.DrawHTMLForm();
        }
    }
}