using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    public class AddExchangeRateController : Controller
    {
        // GET: AddExchangeRate
        [Authorize(Roles = "Add Exchange Rate, Edit Exchange Rate, Delete Exchange Rate")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Exchange Rate"
            };
            if (html.HasPermission("Add Exchange Rate"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Exchange Rate"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Exchange Rate"))
                html.AllowDelete = true;

            html.AddButton = "Add Exchange Rate";
            html.AddURL = "AddExchangeRate/CreateExchangeRate";
            html.TableName = "TblExchangeRate";
            html.PrimaryField = "ExchangeRateID";
            html.OrderBy = "ExchangeRateID desc";
            string sql = @"select ROW_NUMBER() over(order by e.ExchangeRateID desc) as [No], e.ExchangeRateID,  c.CurrencyName + ' (' + c.CurrencySymbol +')' CurrencyName,
            e.ExchangeRate, format(e.DateEntry,'dd-MM-yyyy') as DateEntry, e.AddBy, e.Memo
            from dbo.TblExchangeRate e
            inner join dbo.TblCurrency c on c.CurrencyID = e.CurrencyID where e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID + " and e.DomainID like '%" + html.DomainID + "%'";
            html.GH.SQL = sql;
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCurrency",
                ComboBox = true,
                Label = "Currency:",
                DBField = "e.CurrencyID",
                Value = "CurrencyID",
                DisplayText = "CurrencyName",
                SQL_CBO = "select CurrencyID, CurrencyName + ' (' + CurrencySymbol +')' CurrencyName from dbo.TblCurrency where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'"
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Date",
                SQL = "select DateEntry, format(DateEntry, 'dd-MM-yyyy') [Date] from dbo.TblExchangeRate where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by format(DateEntry, 'dd-MM-yyyy')",
                SelectValue = "DateEntry",
                DisplayField = "Date",
                RadioValue = EncryAndDecry.Encryp("DateEntry"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Currency", FieldName = "CurrencyName" },
                new GridField() { Header = "Exchange Rate", FieldName = "ExchangeRate", TextAlign = "right", Format="#,##" },
                new GridField() { Header = "Date", FieldName = "DateEntry", TextAlign="center" },
                new GridField() { Header = "Add By", FieldName = "AddBy", TextAlign="center" },
                new GridField() { Header = "Memo", FieldName = "Memo",Width = "100%" },
                new GridField() { Header = "Action", FieldName = "ExchangeRateID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Exchange Rate, Edit Exchange Rate")]
        public ActionResult CreateExchangeRate(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateExchangeRate",
                FieldIDFocus = "TxtExchangeRate",
                TableName = "TblExchangeRate",
                PrimaryField = "ExchangeRateID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Currency:",
                ID = "TxtCurrency",
                DBField = "CurrencyID",
                Required = true,
                SQL_CBO = "select CurrencyID, CurrencyName + ' (' + CurrencySymbol +')' CurrencyName from dbo.TblCurrency where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CurrencyName",
                DisplayText = "CurrencyName",
                Value = "CurrencyID"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Exchange Rate:",
                ID = "TxtExchangeRate",
                DBField = "ExchangeRate",
                Required = true,
                ISNumeric = true,
            });
            html.HTMLControls.Add(new HtmlTextBox
            {
                Label = "Date:",
                ID = "TxtDate",
                DBField = "DateEntry",
                Required = true,
                SingleDatePicker = true,
                ISDateTime = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Add By:",
                ID = "TxtAddBy",
                DBField = "AddBy",
                Required = true,
                Value = html.UserName,
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo",
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}