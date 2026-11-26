using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddUOMController : Controller
    {
        // GET: AddUOM
        [Authorize(Roles = "Add UOM, Edit UOM, Delete UOM")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Unit of measure"
            };
            if (html.HasPermission("Add UOM"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit UOM"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete UOM"))
                html.AllowDelete = true;

            html.AddButton = "Add Unit of measure";
            html.AddURL = "AddUOM/CreateUOM";
            html.TableName = "TblUom";
            html.PrimaryField = "UomID";
            html.OrderBy = "UomID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Name",
                SQL = "select UomID, Name from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Name",
                SelectValue = "UomID",
                DisplayField = "Name",
                RadioValue = EncryAndDecry.Encryp("UomID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "UoM Commercial Unit Format", FieldName = "Name",Width="100%" },
				//new GridField() { Header = "UoM Commercial Unit Format", FieldName = "UoMCommercialUnitFormat" },
				new GridField() { Header = "Action", FieldName = "UomID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add UOM, Edit UOM")]
        public ActionResult CreateUom(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateUom",
                FieldIDFocus = "TxtUom",
                TableName = "TblUom",
                PrimaryField = "UomID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            if (Type == "AddNewUnitofmeasure")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "UM:",
                ID = "TxtUom",
                DBField = "Name",
                Required = true,
                ISUniqueField = true
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Label = "UoM Format:",
            //	ID = "TxtUoMCommercialUnitFormat",
            //	DBField = "UoMCommercialUnitFormat",
            //	Required = true,
            //	ISUniqueFiled = true
            //});
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}