using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddWorkingTimeController : Controller
    {
        // GET: AddWorkingTime
        [Authorize(Roles = "Add Working Time, Edit Working Time, Delete Working Time")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Working Time"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Working Time"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Working Time"))
                html.AllowDelete = true;

            html.AddButton = "Add Working Time";
            html.AddURL = "AddWorkingTime/CreateWorkingTime";
            html.TableName = "TblWorkingTime";
            html.PrimaryField = "WorkingID";
            //html.OrderBy = "Name";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Working Name",
                SQL = "select WorkingID, WorkingName from dbo.TblWorkingTime where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by WorkingName",
                SelectValue = "WorkingID",
                DisplayField = "WorkingName",
                RadioValue = EncryAndDecry.Encryp("WorkingID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "WorkingName", FieldName = "WorkingName", IsGroupByField = true, Colspan = 7 },
                new GridField() { Header = "Name", FieldName = "Name" },
                new GridField() { Header = "DayOfWeek", FieldName = "DayOfWeek" },
                new GridField() { Header = "DayPeriod", FieldName = "DayPeriod" },
                new GridField() { Header = "WorkFrom", FieldName = "WorkFrom" },
                new GridField() { Header = "WorkTo", FieldName = "WorkTo" },
                new GridField() { Header = "Action", FieldName = "WorkingID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Working Time, Edit Working Time")]
        public ActionResult CreateWorkingTime(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateWorkingTime",
                FieldIDFocus = "TxtWorkingName",
                TableName = "TblWorkingTime",
                PrimaryField = "WorkingID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Working Name:",
                ID = "TxtWorkingName",
                DBField = "WorkingName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Name:",
                ID = "TxtName",
                DBField = "Name",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Day Of Week:",
                ID = "TxtDayOfWeek",
                DBField = "DayOfWeek",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Day Period:",
                ID = "TxtDayPeriod",
                DBField = "DayPeriod",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Work From:",
                ID = "TxtWorkFrom",
                DBField = "WorkFrom",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Work To:",
                ID = "TxtWorkTo",
                DBField = "WorkTo",
                Required = true
            });
            html.RestFields = "TxtName,TxtWorkTo,TxtWorkFrom,TxtDayPeriod,TxtDayOfWeek";
            html.FocusFieldIDAfterReset = "TxtName";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}