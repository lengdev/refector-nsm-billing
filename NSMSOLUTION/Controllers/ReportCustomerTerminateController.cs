using MVC;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ReportCustomerTerminateController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML();

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });


            string sql = @"SP_CUSTOMER_TERMINATE_DEVICE";
            html.GH.GridProcedureName = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign="center" },
                new GridField() { Header = "CID", FieldName = "CustomerCode",Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true,  TextAlign = "center" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true, TextAlign="left" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID", TextAlign="center"},
                new GridField() { Header = "SID", FieldName = "SID", TextAlign="center" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo"},
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-center text-danger"},
                new GridField() { Header = "Router", FieldName = "Router" ,TextAlign="center"},
                new GridField() { Header = "Device Name", FieldName = "DeviceName" ,TextAlign="left"},
                new GridField() { Header = "Serial No", FieldName = "SerialNo"},
                new GridField() { Header = "Return Date", FieldName = "ReturnDate" },
                new GridField() { Header = "Received By", FieldName = "CreateBy" },
                new GridField() { Header = "Remark", FieldName = "Reason" },
            };
            html.ShowPaging = false;

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}