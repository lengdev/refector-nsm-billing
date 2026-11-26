using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class AddCustomerGroupController : Controller
    {
        // GET: AddCustomerGroup
        [Authorize(Roles = "Add Customer Group, Edit Customer Group, Delete Customer Group")]
        public string Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Customer Group"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer Group"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Customer Group"))
                html.AllowDelete = true;

            html.AddButton = "Add Customer Group";
            html.AddURL = "AddCustomerGroup/CreateCustomerGroup";
            html.TableName = "TblCustomerGroup";
            html.PrimaryField = "CustomerGroupID";
            html.OrderBy = "CustomerGroupID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Group",
                SQL = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerGroup",
                SelectValue = "CustomerGroupID",
                DisplayField = "CustomerGroup",
                RadioValue = EncryAndDecry.Encryp("CustomerGroupID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup",Width="100%" },
                new GridField() { Header = "Action", FieldName = "CustomerGroupID", TextAlign = "Center",  IsModifyField = true }
            };

            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());

        }
        [Authorize(Roles = "Add Customer Group, Edit Customer Group")]
        public string CreateCustomerGroup(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateCustomerGroup",
                FieldIDFocus = "TxtCustomerGroup",
                TableName = "TblCustomerGroup",
                PrimaryField = "CustomerGroupID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Group:",
                ID = "TxtCustomerGroup",
                DBField = "CustomerGroup",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            return html.DrawHTMLForm();
        }
    }
}