using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddWebMenuController : Controller
    {
        // GET: AddWebMenu
        [Authorize(Roles = "Add Web Menu, Edit Web Menu, Delete Web Menu")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Web Menu"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Web Menu"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Web Menu"))
                html.AllowDelete = true;

            html.AddButton = "Add Menu";
            html.AddURL = "AddWebMenu/CreateWebMenu";
            html.TableName = "TblWebMenu";
            html.PrimaryField = "MenuID";
            html.OrderBy = "MenuName";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Menu Name",
                SQL = "select MenuID, MenuName from dbo.TblWebMenu where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by MenuName",
                SelectValue = "MenuID",
                DisplayField = "MenuName",
                RadioValue = EncryAndDecry.Encryp("MenuID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.OrderBy = "OrderBy";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "MenuName", FieldName = "MenuName" },
                new GridField() { Header = "Link", FieldName = "Link" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Order", FieldName = "OrderBy" },
                new GridField() { Header = "Action", FieldName = "MenuID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Web Menu, Edit Web Menu")]
        public ActionResult CreateWebMenu(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateWebMenu",
                FieldIDFocus = "TxtWebMenuName",
                TableName = "TblWebMenu",
                PrimaryField = "MenuID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Menu Name:",
                ID = "TxtWebMenuName",
                DBField = "MenuName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Link:",
                ID = "TxtLink",
                DBField = "Link",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order:",
                ID = "TxtOrder",
                DBField = "OrderBy",
                Required = true,
                ISInteger = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Create By:",
                ID = "TxtCreateBy",
                DBField = "CreateBy",
                Hide = true,
                Value = html.UserName
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}