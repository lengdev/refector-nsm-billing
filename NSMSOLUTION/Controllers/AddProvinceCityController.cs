using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddProvinceCityController : Controller
    {
        // GET: AddProvinceCity
        [Authorize(Roles = "Add Province City, Edit Province City, Delete Province City")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Province City"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Province City"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Province City"))
                html.AllowDelete = true;

            html.AddButton = "Add Province City";
            html.AddURL = "AddProvinceCity/CreateProvinceCity";
            html.TableName = "TblProvinceCity";
            html.PrimaryField = "ProvinceCityID";
            html.OrderBy = "ProvinceCity";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Province City",
                SQL = "select ProvinceCityID, ProvinceCity from dbo.TblProvinceCity where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ProvinceCity",
                SelectValue = "ProvinceCityID",
                DisplayField = "ProvinceCity",
                RadioValue = EncryAndDecry.Encryp("ProvinceCityID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Province City", FieldName = "ProvinceCity" , Width = "100%"},
                new GridField() { Header = "Province City KH", FieldName = "ProvinceCityKH" },
                new GridField() { Header = "Action", FieldName = "ProvinceCityID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Province City, Edit Province City")]
        public ActionResult CreateProvinceCity(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateProvinceCity",
                FieldIDFocus = "TxtProvinceCity",
                TableName = "TblProvinceCity",
                PrimaryField = "ProvinceCityID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Province City:",
                ID = "TxtProvinceCity",
                DBField = "ProvinceCity",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Province City KH:",
                ID = "TxtProvinceCityKH",
                DBField = "ProvinceCityKH",
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}