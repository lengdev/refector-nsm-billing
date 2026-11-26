using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class ManufacturerController : Controller
    {
        // GET: Manufacturer
        [Authorize(Roles = "Add Manufacturer, Edit Manufacturer, Delete Manufacturer")]
        public string Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Manufacturer"
            };
            if (html.HasPermission("Add Manufacturer"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Manufacturer"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Manufacturer"))
                html.AllowDelete = true;

            html.AddButton = "Add Manufacturer";
            html.AddURL = "Manufacturer/CreateManufacturer";
            html.TableName = "TblManufacturer";
            html.PrimaryField = "ManufacturerID";
            html.OrderBy = "ManufacturerID desc";


            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Manufacturer",
                SQL = "select ManufacturerID, Manufacturer from dbo.TblManufacturer where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Manufacturer",
                SelectValue = "ManufacturerID",
                DisplayField = "Manufacturer",
                RadioValue = EncryAndDecry.Encryp("ManufacturerID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Manufacturer", FieldName = "Manufacturer" },
                new GridField() { Header = "Action", FieldName = "ManufacturerID", TextAlign = "Center",  IsModifyField = true }
            };
            return html.DrawHTMLSearchForm();
        }

        [Authorize(Roles = "Add Manufacturer, Edit Manufacturer")]
        public string CreateManufacturer(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.FormID = "AddManufacturer";
            html.FieldIDFocus = "TxtManufacturer";
            html.TableName = "TblManufacturer";
            html.PrimaryField = "ManufacturerID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Manufacturer:",
                ID = "TxtManufacturer",
                DBField = "Manufacturer",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            return html.DrawHTMLForm();
        }
    }
}