using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class ShowCurrentBlockNumberController : Controller
    {
        // GET: ShowCurrentBlockNumber
        [Authorize(Roles = "Show Current Block Number")]
        public ActionResult Index()
        {
            HTML html = new HTML();

            //sort by customer Type



            string sql = @"select distinct DistrictCode from TblCustomerType t
where t.[Status] = 'Enable' and t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'";
            html.ReportTitle = "Customer Block";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtDistrictCode",
                Label = "District Code:",
                DBField = "t.DistrictCode",
                SQL_CBO = sql,
                DisplayText = "DistrictCode",
                Value = "DistrictCode",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Type",
                SQL = "select CustomerType from dbo.TblCustomerType where [Status] = 'Enable' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerType",
                SelectValue = "CustomerType",
                DisplayField = "CustomerType",
                RadioValue = EncryAndDecry.Encryp("CustomerType"),
                DefaultCheckBok = true
            };
            html.OrderBy = "t.CustomerType";
            html.IsHaveWhereCondition = true;
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select ROW_NUMBER() over (order by t.CustomerType) as [No],
t.CustomerType, case when isnull(p.CurrentPrefix, '') = '' then t.Prefix else p.CurrentPrefix end CurrentPrefix,
case when isnull(p.CurrentNumber, '') = '' then 1 else p.CurrentNumber end CurrentNumber, 
t.DistrictCode from dbo.TblCustomerType t
left join dbo.TblPrefix p on t.Prefix = p.Prefix
where t.[Status] = 'Enable' and t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'";
            //html.OrderBy = "[No]";



            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", Width="12%"},
                new GridField() { Header = "Customer Type", FieldName = "CustomerType",Width="25%"},
                new GridField() { Header = "District Code", FieldName = "DistrictCode" ,Width="25%"},
                new GridField() { Header = "Current Prefix", FieldName = "CurrentPrefix", TextAlign="right" ,Width="25%"  },
                new GridField() { Header = "Current #", FieldName = "CurrentNumber", TextAlign="right" ,Width="18%"},
            };
            html.ShowPaging = false;
            //html.UsingJqueryDataTable = true;
            html.GroupableKendoGrid = false;
            //html.ShowToolbarKendoGrid = false;
            html.FilterableKendoGrid = false;
            html.KendoGridAutoFitColumn = true;

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            // html.DrawHTMLSearchForm();//html.DrawKendoGrid();
            return View();
        }
    }
}