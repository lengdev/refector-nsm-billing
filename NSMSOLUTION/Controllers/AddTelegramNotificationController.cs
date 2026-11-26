using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddTelegramNotificationController : Controller
    {
        // GET: AddTelegramNotification
        [Authorize(Roles = "Add Telegram Notification, Edit Telegram Notification, Delete Telegram Notification")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Telegram Notification"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Telegram Notification"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Telegram Notification"))
                html.AllowDelete = true;

            html.AddButton = "Add Telegram Group";
            html.AddURL = "AddTelegramNotification/TelegramNotification";
            html.TableName = "TblTelegramGroupNotification";
            html.PrimaryField = "TID";
            html.OrderBy = "TID desc";

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "TxtGroupTyp",
                Label = "Group Type:",
                DBField = "TelegramGroupType",
                SQL_CBO = "select distinct TelegramGroupType from dbo.TblTelegramGroupNotification",
                DisplayText = "TelegramGroupType",
                Value = "TelegramGroupType",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Telegram Group Name",
                SQL = "select TelegramGroupID, TelegramGroupName from dbo.TblTelegramGroupNotification where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by TelegramGroupName",
                SelectValue = "TelegramGroupID",
                DisplayField = "TelegramGroupName",
                RadioValue = EncryAndDecry.Encryp("TelegramGroupID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Telegram Group ID", FieldName = "TelegramGroupID" },
                new GridField() { Header = "Telegram Group Name", FieldName = "TelegramGroupName" },
                new GridField() { Header = "Telegram Group Type", FieldName = "TelegramGroupType" , ClassName="text-center"},
                new GridField() { Header = "Telegram Topic ID", FieldName = "TelegramGroupTopicID" , ClassName="text-center"},
                new GridField() { Header = "Module", FieldName = "Module"},
                new GridField() { Header = "Action", FieldName = "TID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Telegram Notification, Edit Telegram Notification")]
        public ActionResult TelegramNotification(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateTelegramGroupNotification",
                FieldIDFocus = "TxtTelegramGroupID",
                TableName = "TblTelegramGroupNotification",
                PrimaryField = "TID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Telegram Bot:",
                ID = "TxtTelegramBot",
                DBField = "BotID",
                Required = true,
                SQL_CBO = "select BotID, BotName from TblTelegramBots where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                Value = "BotID",
                DisplayText = "BotName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Group ID:",
                ID = "TxtTelegramGroupID",
                DBField = "TelegramGroupID",
                Required = true,
                ISUniqueField = true,
                //CheckExsitingWithField = "WarehouseID",
                //CheckExsitingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Group Name:",
                ID = "TxtTelegramGroupName",
                DBField = "TelegramGroupName",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Module:",
                ID = "TxtModule",
                DBField = "Module",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Group Type:",
                ID = "TxtGroupType",
                DBField = "TelegramGroupType",
                Required = true,
                SQL_CBO = "select distinct TelegramGroupType from dbo.TblTelegramGroupNotification",
                Value = "TelegramGroupType",
                DisplayText = "TelegramGroupType"
            });
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Status For:",
            //    ID = "TxtStatus",
            //    DBField = "StatusID",
            //    SQL_CBO = "select distinct StatusID, Status from dbo.TblStatus where StatusID in (60, 90, 91, 92)",
            //    Value = "StatusID",
            //    DisplayText = "Status",
            //    Hide = true
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Topic ID :",
                ID = "TxtTopicID",
                DBField = "TelegramGroupTopicID",
                ISNumeric = true,
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}