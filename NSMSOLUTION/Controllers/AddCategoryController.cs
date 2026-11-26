using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddCategoryController : Controller
    {
        // GET: AddCategory
        public ActionResult Index()
        {
            Common common = new Common();

            ViewBag.PageName = "Add Category";
            //if (DoActionData.Instance.IsHavePermission(common.getSessionName("UserID", "").ToString(), "ASN", "ESN", "DSN"))
            //{
            HTML html = new HTML()
            {
                DialogBoxWidth = "500",
                DialogBoxHeight = 400,
                AddButton = "Add Category",
                AddURL = "AddCategory/AddCategory",
                Caption = "Add Category"
            };
            html.SetFormProperties.Add("ProcedureName", "SP_SHOW_CATEGORY");
            html.SetFormProperties.Add("TblName", "TblCategory");
            html.SetFormProperties.Add("AddPermission", "ASN");
            html.SetFormProperties.Add("EditPermission", "ESN");
            html.SetFormProperties.Add("DeletePermission", "DSN");

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Category Name",
                SQL = "SELECT CategoryID, CategoryName FROM dbo.TblCategory where WarehouseID = 1 ORDER BY CategoryID",
                SelectValue = "CategoryID",
                DisplayField = "CategoryName",
                RadioValue = EncryAndDecry.Encryp("CategoryID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Category Name",FieldName = "CategoryName" },
                new GridField() { Header = "CreateByUserID", FieldName = "CreateByUserID" },
                new GridField() { Header = "Action", DeleteUseProcedureName = "SP_DELETE_STOCK_NAME", FieldName = "CategoryID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1);
            //ViewsModel viewsModel = new ViewsModel()
            //{
            //    DrawSearchForm = html.DrawHTMLSearchForm(1)
            //};
            return View();
            //}
            //else
            //{
            //  return Redirect("Login");
            //}
        }
    }
}