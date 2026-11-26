using MVC;
using System.Linq;
using System.Web.Mvc;

namespace NewInventorySystem.Controllers
{
    public class ReportBalanceSheetController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML();
            //html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            //{
            //    ID = "TxtFromDate",
            //    DefaultValueDate = false,
            //    DateRangePicker = true,
            //    LabelFromDate = "Date Range:",
            //});
            //ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();




            html.GH.GridProcedureName = "SP_GetBalanceSheetData";
            //html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign="center" },
                new GridField() { Header = "Account Name", FieldName = "AccountName", TextAlign="left", Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Account Types", FieldName = "AccountTypes", TextAlign="left" },
                new GridField() { Header = "Total Debit", FieldName = "TotalDebit", TextAlign="center" },
                new GridField() { Header = "Total Credit", FieldName = "TotalCredit", TextAlign="Center" },
                new GridField() { Header = "Balance", FieldName = "Balance", TextAlign="Center" },

            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}