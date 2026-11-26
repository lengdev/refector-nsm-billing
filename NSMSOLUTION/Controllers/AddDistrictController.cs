using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddDistrictController : Controller
    {
        // GET: AddDistrict
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Expense Type"))
                AllowAdd = true,
                //if (html.HasPermission("Edit System"))
                AllowEdit = true,
                //if (html.HasPermission("Delete System"))
                AllowDelete = true,

                AddButton = "Add District",
                AddURL = "AddDistrict/CreateDistrict",
                TableName = "TblDistrict",
                PrimaryField = "DistrictID"
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY d.District) AS[No], d.DistrictID, p.ProvinceCity, d.District, d.PostalCode, d.DistrictKH ";
            SQL += "from dbo.TblDistrict d ";
            SQL += "left join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID ";
            SQL += " where d.SystemID = " + html.SystemID + " and d.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = SQL;
            html.OrderBy = "District";
            html.IsHaveWhereCondition = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "District",
                SQL = "select DistrictID, District from dbo.TblDistrict where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by District",
                SelectValue = "DistrictID",
                DisplayField = "District",
                RadioValue = EncryAndDecry.Encryp("DistrictID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "District", FieldName = "District" , Width = "100%"},
                new GridField() { Header = "District KH", FieldName = "DistrictKH" },
                new GridField() { Header = "ProvinceCity", FieldName = "ProvinceCity" },
                new GridField() { Header = "Postal Code", FieldName = "PostalCode" },
                new GridField() { Header = "Action", FieldName = "DistrictID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreateDistrict(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateDistrict",
                FieldIDFocus = "TxtDistrict",
                TableName = "TblDistrict",
                PrimaryField = "DistrictID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Province City:",
                ID = "TxtProvinceCity",
                DBField = "ProvinceCityID",
                SQL_CBO = "select ProvinceCityID, ProvinceCity from dbo.TblProvinceCity where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ProvinceCity",
                Value = "ProvinceCityID",
                DisplayText = "ProvinceCity",
                Infinity = false,
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "District:",
                ID = "TxtDistrict",
                DBField = "District",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "District KH:",
                ID = "TxtDistrictKH",
                DBField = "DistrictKH",
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Postal Code:",
                ID = "TxtPostalCode",
                DBField = "PostalCode"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}