using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddDepartmentController : Controller
    {
        // GET: AddDepartment
        [Authorize(Roles = "Add Department, Edit Department, Delete Department")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Department"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Department"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Department"))
                html.AllowDelete = true;

            html.AddButton = "Add Department";
            html.AddURL = "AddDepartment/CreateDepartment";
            html.TableName = "TblDepartment";
            html.PrimaryField = "DepartmentID";
            html.OrderBy = "DepartmentID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Department Name",
                SQL = "select DepartmentID, DepartmentName from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by DepartmentName",
                SelectValue = "DepartmentID",
                DisplayField = "DepartmentName",
                RadioValue = EncryAndDecry.Encryp("DepartmentID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Department", FieldName = "DepartmentName" ,Width="100%"},
                new GridField() { Header = "Action", FieldName = "DepartmentID", TextAlign = "Center",  IsModifyField = true }
            };
            //tml.ShowGridOnly = true;
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Department, Edit Department")]
        public ActionResult CreateDepartment(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateDepartment",
                FieldIDFocus = "TxtDepartmentName",
                TableName = "TblDepartment",
                PrimaryField = "DepartmentID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Department Name:",
                ID = "TxtDepartmentName",
                DBField = "DepartmentName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            //html.BtnSaveChangeDialog = "SaveChange2";
            //html.BtnCloseDialog = "CloseModal2";
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}