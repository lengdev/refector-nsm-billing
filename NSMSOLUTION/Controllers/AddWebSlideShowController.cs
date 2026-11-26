using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddWebSlideShowController : Controller
    {
        // GET: AddWebSlideShow
        [Authorize(Roles = "Add Slide Show, Edit Slide Show, Delete Slide Show")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Slide Show"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Slide Show"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Slide Show"))
                html.AllowDelete = true;

            html.AddButton = "Add Slide Show";
            html.AddURL = "AddWebSlideShow/CreateWebSlideShow";
            html.TableName = "TblWebSlideShow";
            html.PrimaryField = "SlideShowID";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Title",
                SQL = "select SlideShowID, Title from dbo.TblWebSlideShow where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Title",
                SelectValue = "SlideShowID",
                DisplayField = "Title",
                RadioValue = EncryAndDecry.Encryp("SlideShowID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Title", FieldName = "Title" },
                new GridField() { Header = "Description", FieldName = "Descriptions" },
                new GridField() { Header = "Slide Show", FieldName = "SlideShow", ImagePath="Content/SlideShow" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "SlideShowID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Slide Show, Edit Slide Show")]
        public ActionResult CreateWebSlideShow(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateSlideShow",
                FieldIDFocus = "TxtTitle",
                TableName = "TblWebSlideShow",
                PrimaryField = "SlideShowID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            string sql = "select MenuID, MenuName from dbo.TblWebMenu where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by OrderBy";
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Label = "Menu:",
            //	ID = "TxtMenu",
            //	DBField = "MenuID",
            //	SQL_AutoComplete = sql,
            //	ISAutoComplete = true,
            //});

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Title:",
                ID = "TxtTitle",
                DBField = "Title"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Description:",
                ID = "TxtDesc",
                DBField = "Descriptions"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order:",
                ID = "TxtOrderBy",
                DBField = "OrderBy",
                Required = true
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Slide Show:",
                ID = "TxtSlideShow",
                DBField = "SlideShow",
                DestinationFile = "SlideShow",
                Multiple = true,
                AcceptImageOnly = true,
                ISFile = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}