using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class AddTypeBadgeController : Controller
    {
        // GET: AddTypeBadge
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Type Badge",

                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,

                AddButton = "Add Type Badge",
                AddURL = "AddTypeBadge/CreateTypeBadge",
                TableName = "TblOtherInformationType",
                PrimaryField = "TypeID",
                OrderBy = "TypeID desc",
                DialogBoxHeight = 350,
                DialogBoxWidth = "600"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Type Name",
                SQL = "select distinct top 10 TypeName from dbo.TblOtherInformationType where lower(TypeName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by TypeName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("TypeName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select ROW_NUMBER() over (order by t.TypeID desc) [No],
t.TypeID, t.TypeName, cat.CategoryName, t.CreateBy, t.CreateDate
from dbo.TblOtherInformationType t
left join dbo.TblOtherInformationCategory cat on cat.CategoryID = t.CategoryID
where t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'";

            html.OrderBy = " t.TypeID desc";
            html.GH.SQL = sql;
            html.ShowPaging = false;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Type Name", FieldName = "TypeName" },
                new GridField() { Header = "Category Name", FieldName = "CategoryName", Width = "100%" },
                new GridField() { Header = "Created By", FieldName = "CreateBy" },
                new GridField() { Header = "Created Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yy" },
                new GridField() { Header = "Action", FieldName = "TypeID", TextAlign = "Center", IsModifyField = true }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public ActionResult CreateTypeBadge(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateTypeBadge",
                FieldIDFocus = "TxtTypeName",
                TableName = "TblOtherInformationType",
                PrimaryField = "TypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Type Name:",
                ID = "TxtTypeName",
                DBField = "TypeName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Category Name:",
                ID = "TxtCategoryID",
                DBField = "CategoryID",
                SQL_CBO = "select CategoryID, CategoryName from dbo.TblOtherInformationCategory where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CategoryName",
                Value = "CategoryID",
                DisplayText = "CategoryName",
                Required = false
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}