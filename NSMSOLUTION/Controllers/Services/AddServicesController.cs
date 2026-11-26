using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddServicesController : Controller
    {
        // GET: AddServices
        [Authorize(Roles = "Add Service, Edit Service, Delete Service, Add Item Type, Edit Item Type, Delete Item Type")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Services"
            };
            if (html.HasPermission("Add Service") || html.HasPermission("Add Item Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Service") || html.HasPermission("Edit Item Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Service") || html.HasPermission("Delete Item Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Service";
            html.AddURL = "AddServices/CreateService";
            html.TableName = "TblServices";
            html.PrimaryField = "ServiceID";
            html.OrderBy = "ServiceID desc";
            html.DialogBoxHeight = 300;
            html.DialogBoxWidth = "500";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Service Name",
                SQL = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ServiceName",
                SelectValue = "ServiceID",
                DisplayField = "ServiceName",
                RadioValue = EncryAndDecry.Encryp("ServiceID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Service Code", FieldName = "ServiceCode" },
                new GridField() { Header = "Service Name", FieldName = "ServiceName",Width="100%" },
                new GridField() { Header = "Action", FieldName = "ServiceID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Service, Edit Service, Add Item Type, Edit Item Type")]
        public ActionResult CreateService(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateServices",
                FieldIDFocus = "TxtServiceCode",
                TableName = "TblServices",
                PrimaryField = "ServiceID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Service Code:",
                ID = "TxtServiceCode",
                DBField = "ServiceCode",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Service Name:",
                ID = "TxtServiceName",
                DBField = "ServiceName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "Connection-{00}",
                Label = "CO. Non VAT:",
                ID = "TxtCOIDNonVAT",
                DBField = "COIDNonVAT",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "CON{000000}",
                Label = "CO. VAT:",
                ID = "TxtCOIDVAT",
                DBField = "COIDVAT",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "SID{000000}",
                Label = "SID Non VAT:",
                ID = "TxtSIDNonVAT",
                DBField = "SIDNonVAT",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "SID{000000}",
                Label = "SID VAT:",
                ID = "TxtSIDVAT",
                DBField = "SIDVAT",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}