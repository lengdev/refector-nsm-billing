using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddDeviceNameController : Controller
    {
        // GET: AddDeviceName
        [Authorize(Roles = "Add Device Name, Edit Device Name, Delete Device Name")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Device Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Device Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Device Name"))
                html.AllowDelete = true;

            html.AddButton = "Add Device Name";
            html.AddURL = "AddDeviceName/CreateDeviceName";
            html.TableName = "TblDeviceName";
            html.PrimaryField = "DeviceNameID";
            html.OrderBy = "DeviceNameID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Device Name",
                SQL = "select DeviceNameID, DeviceName from dbo.TblDeviceName where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by DeviceName",
                SelectValue = "DeviceNameID",
                DisplayField = "DeviceName",
                RadioValue = EncryAndDecry.Encryp("DeviceNameID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Device Name", FieldName = "DeviceName",Width="100%"},
                new GridField() { Header = "Action", FieldName = "DeviceNameID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Device Name, Edit Device Name")]
        public ActionResult CreateDeviceName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateDeviceName",
                FieldIDFocus = "TxtDeviceName",
                TableName = "TblDeviceName",
                PrimaryField = "DeviceNameID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Device Name:",
                ID = "TxtDeviceName",
                DBField = "DeviceName",
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