using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class InfrastructureProviderController : Controller
    {
        // GET: AddCustomerGroup
        [Authorize(Roles = "Add Infrastructure Provider, Edit Infrastructure Provider, Delete Infrastructure Provider")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Infrastructure Provider",
                DialogBoxWidth = "800"
            };
            if (html.HasPermission("Add Infrastructure Provider"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Infrastructure Provider"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Infrastructure Provider"))
                html.AllowDelete = true;
            html.AddButton = "Add New";
            html.AddURL = "InfrastructureProvider/CreateNewInfra";
            html.TableName = "TblInfraProvider";
            html.PrimaryField = "InfraProviderID";
            html.OrderBy = "InfraProviderID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Infra Name",
                SQL = "select InfraProviderID, InfraProviderName from TblInfraProvider ",
                SelectValue = "InfraProviderID",
                DisplayField = "InfraProviderName",
                RadioValue = EncryAndDecry.Encryp("InfraProviderID"),
                DefaultCheckBok = true
            };

            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
              new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
              new GridField() { Header = "Name", FieldName = "InfraProviderName" },
              new GridField() { Header = "Description", FieldName = "InfraProviderDescription", Width="100%"  },
              new GridField() { Header = "Create By", FieldName = "CreateBy" },
              new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yyyy", ClassName="text-center" },
              new GridField() { Header = "Action", FieldName = "InfraProviderID", TextAlign = "Center",  IsModifyField = true }
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Infrastructure Provider, Edit Infrastructure Provider")]
        public ActionResult CreateNewInfra(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateInfraProvider",
                FieldIDFocus = "TxtInfraProviderName",
                TableName = "TblInfraProvider",
                PrimaryField = "InfraProviderID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Infra Provider Name:",
                ID = "TxtInfraProviderName",
                DBField = "InfraProviderName",
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Description:",
                ID = "TxtInfraProviderDescription",
                DBField = "InfraProviderDescription",
            });

            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }

}