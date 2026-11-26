using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddIndustryTypeController : Controller
    {
        // GET: AddIndustryType
        [Authorize(Roles = "Add Business Type, Edit Business Type, Delete Business Type")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers Business Type"
            };
            if (html.HasPermission("Add Business Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Business Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Business Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Business Type";
            html.AddURL = "AddIndustryType/CreateIndustryType";
            html.TableName = "TblIndustryType";
            html.PrimaryField = "IndustryTypeID";
            html.OrderBy = "IndustryTypeID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Business Type",
                SQL = "select IndustryTypeID, IndustryType from dbo.TblIndustryType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by IndustryType",
                SelectValue = "IndustryTypeID",
                DisplayField = "IndustryType",
                RadioValue = EncryAndDecry.Encryp("IndustryTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Business Type", FieldName = "IndustryType" , Width = "100%"},
                new GridField() { Header = "Action", FieldName = "IndustryTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Business Type, Edit Business Type")]
        public ActionResult CreateIndustryType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateIndustryType",
                FieldIDFocus = "TxtIndustryType",
                TableName = "TblIndustryType",
                PrimaryField = "IndustryTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Business Type:",
                ID = "TxtIndustryType",
                DBField = "IndustryType",
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