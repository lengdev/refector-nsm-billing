using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class AddInformationCategoryController : Controller
    {
        // GET: AddInformationCategory
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Information Category",

                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,

                AddButton = "Add Information Category",
                AddURL = "AddInformationCategory/CreateInformationCategory",
                TableName = "TblOtherInformationCategory",
                PrimaryField = "CategoryID",
                OrderBy = "CategoryID desc",
                DialogBoxHeight = 300,
                DialogBoxWidth = "500"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Category Name",
                SQL = "select CategoryID, CategoryName from dbo.TblOtherInformationCategory where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CategoryName",
                SelectValue = "CategoryID",
                DisplayField = "CategoryName",
                RadioValue = EncryAndDecry.Encryp("CategoryID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Category Name", FieldName = "CategoryName", Width = "100%" },
                new GridField() { Header = "Action", FieldName = "CategoryID", TextAlign = "Center", IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public ActionResult CreateInformationCategory(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateInformationCategory",
                FieldIDFocus = "TxtCategoryName",
                TableName = "TblOtherInformationCategory",
                PrimaryField = "CategoryID",
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
                Label = "Category Name:",
                ID = "TxtCategoryName",
                DBField = "CategoryName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}