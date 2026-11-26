using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class AddPaymentTermsController : Controller
    {
        // GET: AddPaymentTerms
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Payment Terms",
                //if (html.HasPermission("Add Expense Type"))
                AllowAdd = true,
                //if (html.HasPermission("Edit System"))
                AllowEdit = true,
                //if (html.HasPermission("Delete System"))
                AllowDelete = true,

                AddButton = "Add Payment Terms",
                AddURL = "AddPaymentTerms/CreatePaymentTerms",
                TableName = "TblPaymentTerms",
                PrimaryField = "PaymentTermsID",
                OrderBy = "TermName"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Payment Terms",
                SQL = "select PaymentTermsID, TermName from dbo.TblPaymentTerms where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by TermName",
                SelectValue = "PaymentTermsID",
                DisplayField = "TermName",
                RadioValue = EncryAndDecry.Encryp("PaymentTermsID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Term Name", FieldName = "TermName", Width="100%" },
                new GridField() { Header = "Number of Days", FieldName = "NumberofDays", ClassName="text-center" },
                new GridField() { Header = "Action", FieldName = "PaymentTermsID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreatePaymentTerms(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
                html.BtnSaveChangeDialog = "SaveChange2";
            html.FormID = "CreatePaymentTerms";
            html.FieldIDFocus = "TxtTermName";
            html.TableName = "TblPaymentTerms";
            html.PrimaryField = "PaymentTermsID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Term Name:",
                ID = "TxtTermName",
                DBField = "TermName",
                Required = true,
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Number of Days:",
                ID = "TxtNumberofDays",
                DBField = "NumberofDays",
                Required = true,
                Type = "number"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}