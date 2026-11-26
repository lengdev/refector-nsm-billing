using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class IspLinkController : Controller
    {
        // GET: AddCustomerGroup
        [Authorize(Roles = "Add ISP Link, Edit ISP Link, Delete ISP Link")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List ISP Link",
                DialogBoxWidth = "800"
            };
            if (html.HasPermission("Add ISP Link"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit ISP Link"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete ISP Link"))
                html.AllowDelete = true;
            html.AddButton = "Add ISP Link";
            html.AddURL = "IspLink/CreateIspLink";
            html.TableName = "TblIspLink";
            html.PrimaryField = "IspLinkID";
            html.OrderBy = "IspLinkID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "IspLink Name",
                SQL = "select IspLinkID, IspLinkName from TblIspLink ",
                SelectValue = "IspLinkID",
                DisplayField = "IspLinkName",
                RadioValue = EncryAndDecry.Encryp("IspLinkID"),
                DefaultCheckBok = true
            };

            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
              new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
              new GridField() { Header = "ISP Link Name", FieldName = "IspLinkName" },
              new GridField() { Header = "Description", FieldName = "IspLinkDescription", Width="100%"  },
              new GridField() { Header = "Create By", FieldName = "CreateBy" },
              new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yyyy", ClassName="text-center" },
              new GridField() { Header = "Action", FieldName = "IspLinkID", TextAlign = "Center",  IsModifyField = true }
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add ISP Link, Edit ISP Link")]
        public ActionResult CreateIspLink(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateIspLink",
                FieldIDFocus = "TxtIspLinkName",
                TableName = "TblIspLink",
                PrimaryField = "IspLinkID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ISP Link Name:",
                ID = "TxtIspLinkName",
                DBField = "IspLinkName",
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Description:",
                ID = "TxtIspDescription",
                DBField = "IspLinkDescription",
            });

            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }

}