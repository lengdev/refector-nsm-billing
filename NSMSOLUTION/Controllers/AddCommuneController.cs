using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddCommuneController : Controller
    {
        // GET: AddCommune
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

                AddButton = "Add Commune",
                AddURL = "AddCommune/CreateCommune",
                TableName = "TblCommune",
                PrimaryField = "CommuneID"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Commune",
                SQL = "select CommuneID, Commune from dbo.TblCommune where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Commune",
                SelectValue = "CommuneID",
                DisplayField = "Commune",
                RadioValue = EncryAndDecry.Encryp("CommuneID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string SQL = "select ROW_NUMBER() OVER(ORDER BY d.District) AS[No], d.DistrictID, d.District, c.CommuneID, c.Commune, c.PostalCode, c.CommuneKH ";
            SQL += "from dbo.TblCommune c ";
            SQL += "left join dbo.TblDistrict d on d.DistrictID = c.DistrictID ";
            SQL += "where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = SQL;
            html.OrderBy = "District, Commune";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "District", FieldName = "District", IsGroupByField = true, Colspan=5 },
                new GridField() { Header = "Commune", FieldName = "Commune" , Width = "100%"},
                new GridField() { Header = "Commune KH", FieldName = "CommuneKH" },
                new GridField() { Header = "Postal Code", FieldName = "PostalCode" },
                new GridField() { Header = "Action", FieldName = "CommuneID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreateCommune(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateCommnue",
                FieldIDFocus = "TxtCommune",
                TableName = "TblCommune",
                PrimaryField = "CommuneID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "District:",
                ID = "TxtDistrict",
                DBField = "DistrictID",
                SQL_CBO = "select DistrictID, District from dbo.TblDistrict where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by District",
                Value = "DistrictID",
                DisplayText = "District",
                Required = true,
                Infinity = false
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Commune:",
                ID = "TxtCommune",
                DBField = "Commune",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Commune KH:",
                ID = "TxtCommuneKH",
                DBField = "CommuneKH",
                Required = true,
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