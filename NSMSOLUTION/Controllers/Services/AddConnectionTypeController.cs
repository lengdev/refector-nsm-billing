using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddConnectionTypeController : Controller
    {
        // GET: AddConnectionType
        [Authorize(Roles = "Add Connection Type, Edit Connection Type, Delete Connection Type")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Connection Type"
            };
            if (html.HasPermission("Add Connection Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Connection Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Connection Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Connection Type";
            html.AddURL = "AddConnectionType/CreatePackageType";
            html.TableName = "TblPackageType";
            html.PrimaryField = "PackageTypeID";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Connection Type",
                SQL = "select PackageTypeID, PackageTypeName from dbo.TblPackageType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PackageTypeName",
                SelectValue = "PackageTypeID",
                DisplayField = "PackageTypeName",
                RadioValue = EncryAndDecry.Encryp("PackageTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            //string SQL = "select ROW_NUMBER() OVER(ORDER BY pt.PackageTypeName) AS[No], pt.PackageTypeID, s.ServiceName, pt.PackageTypeName ";
            //SQL += "from dbo.TblPackageType pt inner join dbo.TblServices s on s.ServiceID = pt.ServiceID ";
            //SQL += " where pt.SystemID = " + html.SystemID + " and pt.WarehouseID = " + html.WarehouseID;
            //html.GH.SQL = SQL;
            html.OrderBy = "PackageTypeID desc";

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Connection Type Code", FieldName = "ConnectionTypeCode" },
                new GridField() { Header = "Connection Type", FieldName = "PackageTypeName" ,Width = "100%"},
                new GridField() { Header = "Action", FieldName = "PackageTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Connection Type, Edit Connection Type")]
        public ActionResult CreatePackageType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreatePackageType",
                FieldIDFocus = "TxtConnectionTypeCode",
                TableName = "TblPackageType",
                PrimaryField = "PackageTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Service Name:",
            //    ID = "TxtServiceName",
            //    DBField = "ServiceID",
            //    SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ServiceName",
            //    Value = "ServiceID",
            //    DisplayText = "ServiceName"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Connection Type Code:",
                ID = "TxtConnectionTypeCode",
                DBField = "ConnectionTypeCode",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Connection Type:",
                ID = "TxtPackageTypeName",
                DBField = "PackageTypeName",
                Required = true,
                //ISUniqueFiled = true,
                //CheckExsitingWithField = "WarehouseID",
                //CheckExsitingWithDefaultValue = html.WarehouseID
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}