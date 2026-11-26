using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.TelegramBot
{
    [Authorize]
    public class TelegramBotController : Controller
    {
        // GET: TelegramBot
        public ActionResult Index()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Telegram Bot";
            if (html.HasPermission("Add Telegram Bot"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Telegram Bot"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Telegram Bot"))
                html.AllowDelete = true;

            html.TableName = "TblTelegramBots";
            html.AddButton = "Add New Bot";
            html.AddURL = "TelegramBot/CreateTelegramBot";
            html.PrimaryField = "BotID";
            html.OrderBy = "BotID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Bot Name",
                SQL = "select BotID, BotName from dbo.TblTelegramBots where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by BotName",
                SelectValue = "BotID",
                DisplayField = "BotName",
                RadioValue = EncryAndDecry.Encryp("BotID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select row_number() over(order by BotID desc) as [No], BotID, BotName, BotUsername, BotToken, case when isnull(IsActive, 0) = 0 then '<span class=""text-danger"">Inactive</span>' else '<span class=""text-primary"">Active</span>' end as Status, 
                            format(CreateDate, 'dd-MM-yyyy') as CreateDate from dbo.TblTelegramBots";
            //html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="SystemID", Value =html.SystemID },
              new GridPara() { ParaName="WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="DomainID", Value =html.DomainID },
            };
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Bot Name", FieldName = "BotName", TextAlign="center" },
                new GridField() { Header = "Bot Username", FieldName = "BotUsername"},
                new GridField() { Header = "Bot Token", FieldName = "BotToken"},
                new GridField() { Header = "Status", FieldName = "Status"},
                new GridField() { Header = "CreateDate", FieldName = "CreateDate"},
                new GridField() { Header = "Action", FieldName = "BotID", TextAlign = "Center",  IsModifyField = true,
                    HideEditOnActionNameNotMatch = AdminEdit==false,
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public ActionResult CreateTelegramBot(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateTelegramBot",
                FieldIDFocus = "TxtBotName",
                TableName = "TblTelegramBots",
                PrimaryField = "BotID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bot Name:",
                ID = "TxtBotName",
                DBField = "BotName",
                ISUniqueField = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bot Token:",
                ID = "TxtBotToken",
                DBField = "BotToken",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bot Username:",
                ID = "TxtBotUsername",
                DBField = "BotUsername",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtBotStatus",
                DBField = "IsActive",
                Required = true,
                SQL_CBO = "select 'Acitve' Status, 1 as IsActive union all select 'Inactive' Status, 0 as IsActive",
                Value = "IsActive",
                DisplayText = "Status"
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}