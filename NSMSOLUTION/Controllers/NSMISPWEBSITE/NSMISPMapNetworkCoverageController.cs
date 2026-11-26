using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.NSMISPWEBSITE
{
    public class NSMISPMapNetworkCoverageController : Controller
    {
        // GET: NSMISPMapNetworkCoverage
        [Authorize(Roles = "Add Network Coverage, Edit Network Coverage, Delete Network Coverage")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Network Coverage"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Network Coverage"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Network Coverage"))
                html.AllowDelete = true;

            html.AddButton = "Add Network Coverage";
            html.AddURL = "NSMISPMapNetworkCoverage/CreateNetworkCoverage";
            //html.TableName = "NSMISPWEB_MapNetworkCoverage";
            html.PrimaryField = "MapNetworkCoverageID";
            //html.OrderBy = "MapNetworkCoverageID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Map Name",
                SQL = "select MapNetworkCoverageID, MapName from dbo.NSMISPWEB_MapNetworkCoverage where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by MapName",
                SelectValue = "MapNetworkCoverageID",
                DisplayField = "MapName",
                RadioValue = EncryAndDecry.Encryp("MapNetworkCoverageID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select ROW_NUMBER() over(order by MapNetworkCoverageID desc) as [No], *,
'<a target=""_blank"" href=""https://www.google.com/maps/place/'+ convert(varchar, GoogleMap_Latitude)+ ','+
convert(varchar, GoogleMap_Longitude)+'/@'+convert(varchar, GoogleMap_Latitude)+ ','+
convert(varchar, GoogleMap_Longitude)+',15z"">'+GoogleMap_FullAddress+'<a>' GoogleMap
from NSMISPWEB_MapNetworkCoverage";
            html.OrderBy = "MapNetworkCoverageID desc";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Map Name", FieldName = "MapName" },
                new GridField() { Header = "Commune", FieldName = "Commune" },
                new GridField() { Header = "District", FieldName = "District" },
                new GridField() { Header = "ProvinceCity", FieldName = "ProvinceCity" },
                new GridField() { Header = "Google Map", FieldName = "GoogleMap" },
                new GridField() { Header = "Action", FieldName = "MapNetworkCoverageID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Add Network Coverage, Edit Network Coverage")]
        public ActionResult CreateNetworkCoverage(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateNetworkCoverage",
                FieldIDFocus = "TxtMapName",
                TableName = "NSMISPWEB_MapNetworkCoverage",
                PrimaryField = "MapNetworkCoverageID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Main Location Name:",
                ID = "TxtMainLocationName",
                DBField = "MapName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Main Location ID:",
                ID = "TxtMainMap_FullAddress",
                DBField = "GoogleMap_FullAddress",
                IsSetLocationWithGoogleMap = true,
                HiddenField = "GoogleMap_Latitude, GoogleMap_Longitude",
                HiddenFieldValue = "0,0",
                InputLatitude = "TxtMainMap_FullAddress_GoogleMap_Latitude",
                InputLongitude = "TxtMainMap_FullAddress_GoogleMap_Longitude",
                MapURL = "MAP/NewVersion",
                Required = true,
                IsReadOnly = true,
            });

            //            html.HTMLControls.Add(new HtmlTextBox()
            //            {
            //                Label = "Commune:",
            //                ID = "TxtCommune",
            //                DBField = "Commune",
            //                isTypeahead = true,
            //                SQL = @"select distinct c.Commune, d.District, p.ProvinceCity from dbo.TblCommune c
            //inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
            //inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
            //order by c.Commune", DisplayOnFields= JsonConvert.SerializeObject(new { TxtDistrict= "District", TxtProvinceCity = "ProvinceCity" })
            //            });
            //            html.HTMLControls.Add(new HtmlTextBox()
            //            {
            //                Label = "District:",
            //                ID = "TxtDistrict",
            //                DBField = "District",
            //                isTypeahead = true,
            //                SQL = "select distinct District from dbo.TblDistrict order by District",
            //            });
            //            html.HTMLControls.Add(new HtmlTextBox()
            //            {
            //                Label = "ProvinceCity:",
            //                ID = "TxtProvinceCity",
            //                DBField = "ProvinceCity",
            //                isTypeahead = true,
            //                SQL = "select distinct ProvinceCity from dbo.TblProvinceCity order by ProvinceCity",
            //            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Sub Location Name:",
                ID = "TxtSubLocationName",
                DBField = "MapName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Sub Google Map:",
                ID = "TxtSubMap_FullAddress",
                DBField = "GoogleMap_FullAddress",
                IsSetLocationWithGoogleMap = true,
                HiddenField = "GoogleMap_Latitude, GoogleMap_Longitude",
                HiddenFieldValue = "0,0",
                InputLatitude = "TxtSubMap_FullAddress_GoogleMap_Latitude",
                InputLongitude = "TxtSubMap_FullAddress_GoogleMap_Longitude",
                MapURL = "MAP/Draw"
            });
            html.AddCustomFields = new Dictionary<string, string>
            {
                { "GoogleMap_Latitude", "$(\"#TxtGoogleMap_FullAddress_GoogleMap_Latitude\").val()" },
                { "GoogleMap_Longitude", "$(\"#TxtGoogleMap_FullAddress_GoogleMap_Longitude\").val()" },
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}