using MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers.Router
{
    public class RouterController : Controller
    {
        // GET: Router
        [Authorize(Roles = "Add Router, Edit Router, Delete Router")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Router"
            };
            if (html.HasPermission("Add Router"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Router"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Router"))
                html.AllowDelete = true;

            html.AddButton = "Add Router";
            html.AddURL = "Router/AddRouter";
            html.TableName = "TblRouter";
            html.PrimaryField = "RouterID";
            html.OrderBy = "RouterID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Router Name",
                SQL = "select RouterID, RouterName from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RouterName desc",
                SelectValue = "RouterID",
                DisplayField = "RouterName",
                RadioValue = EncryAndDecry.Encryp("RouterID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.SQL = @"select *, case when isnull(StatusID, 3) = 3 then '<span class=""badge badge-primary"">Enable</span>' else '<span class=""badge badge-danger"">Disable</span>' end status from dbo.TblRouter";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Router Name", FieldName = "RouterName" },
                new GridField() { Header = "IP Address", FieldName = "IPAddress" },
                new GridField() { Header = "Port", FieldName = "Port" },
                new GridField() { Header = "User Name", FieldName = "UserName" },
                new GridField() { Header = "Version", FieldName = "MikroTikVersion" },
                new GridField() { Header = "Status", FieldName = "status" },
                new GridField() { Header = "Router Description", FieldName = "RouterDescription" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", TextAlign="center" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "RouterID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Router, Edit Router")]
        public ActionResult AddRouter(string PrimaryFieldValue)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML
            {
                FormID = "AddRouter",
                FieldIDFocus = "TxtRouterName",
                TableName = "TblRouter",
                PrimaryField = "RouterID",
                DialogBoxWidth = "1100",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Router Name:",
                ID = "TxtRouterName",
                DBField = "RouterName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "IP Address:",
                ID = "TxtIPAddress",
                DBField = "IPAddress",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Port:",
                ID = "TxtPort",
                DBField = "Port",
                Required = true,
                ISNumeric = true,
                ISInteger = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Name:",
                ID = "TxtUserName",
                DBField = "UserName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "TxtPassword",
                DBField = "Password",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Owner:",
                ID = "TxtOwner",
                DBField = "Owner",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Mikro Tik Version:",
                ID = "TxtMikroTikVersion",
                DBField = "MikroTikVersion",
                SQL = "select 1 MikroTikVersionID, 'Old Version' MikroTikVersion union all select 2 MikroTikVersionID, '7.11.2' MikroTikVersion",
                isTypeahead = true,
                DisplayOnFields = "MikroTikVersionID",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Mikro Tik Version ID:",
                ID = "MikroTikVersionID",
                DBField = "MikroTikVersionID",
                Required = true,
                Hide = true
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatusID",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in (3,4)",
                Value = "StatusID",
                DisplayText = "Status",
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Description:",
                ID = "TxtRouterDescription",
                DBField = "RouterDescription",
                Required = true
            });
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}