using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddOrderTypeController : Controller
    {
        // GET: AddOrderType
        [Authorize(Roles = "Add Order Type, Edit Order Type, Delete Order Type")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Order Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Order Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Order Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Order Type";
            html.AddURL = "AddOrderType/CreateOrderType";
            html.TableName = "TblOrderType";
            html.PrimaryField = "OrderTypeID";
            html.OrderBy = "OrderType";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Order Type",
                SQL = "select OrderTypeID, OrderType from dbo.TblOrderType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by OrderType",
                SelectValue = "OrderTypeID",
                DisplayField = "OrderType",
                RadioValue = EncryAndDecry.Encryp("OrderTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "OrderType", FieldName = "OrderType" },
                new GridField() { Header = "Action", FieldName = "OrderTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Order Type, Edit Order Type")]
        public ActionResult CreateOrderType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateOrderType",
                FieldIDFocus = "TxtOrderType",
                TableName = "TblOrderType",
                PrimaryField = "OrderTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order Type:",
                ID = "TxtOrderType",
                DBField = "OrderType",
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