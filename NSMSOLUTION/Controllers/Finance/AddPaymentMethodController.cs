using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddPaymentMethodController : Controller
    {
        // GET: AddPaymentMethod
        [Authorize(Roles = "Add Payment Method, Edit Payment Method, Delete Payment Method")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Payment Method"
            };
            if (html.HasPermission("Add Payment Method"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Payment Method"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Payment Method"))
                html.AllowDelete = true;

            html.AddButton = "Add Payment Method";
            html.AddURL = "AddPaymentMethod/CreatePaymentMethod";
            html.TableName = "TblPaymentMethod";
            html.PrimaryField = "PaymentCategoryID";
            html.OrderBy = "PaymentMethod";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Payment Method",
                SQL = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PaymentMethod",
                SelectValue = "PaymentMethodID",
                DisplayField = "PaymentMethod",
                RadioValue = EncryAndDecry.Encryp("PaymentMethodID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Payment Method", FieldName = "PaymentMethod",Width = "100%" },
                new GridField() { Header = "Action", FieldName = "PaymentMethodID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Payment Method, Edit Payment Method")]
        public ActionResult CreatePaymentMethod(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewPaymentMethod")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewPaymentMethod();";
            }
            html.FormID = "CreatePaymentMethod";
            html.FieldIDFocus = "TxtPaymentMethod";
            html.TableName = "TblPaymentMethod";
            html.PrimaryField = "PaymentMethodID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Payment Method:",
                ID = "TxtPaymentMethod",
                DBField = "PaymentMethod",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Prefix:",
            //    ID = "TxtPrefix",
            //    DBField = "Prefix",
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Prefix VAT:",
            //    ID = "TxtPrefixVAT",
            //    DBField = "PrefixVAT",
            //});
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}