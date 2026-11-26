using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ShowCustCompanyController : Controller
    {
        // GET: ShowCustCompany        
        public ActionResult Index()
        {
            HTML html = new HTML();

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Company Name",
                SQL = "select CompanyName from dbo.TblCustomerCompany where lower(CompanyName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("c.CompanyName"),
                DefaultCheckBok = true,
                ISAutoComplete = true,
            };
            html.GridEventClick = "onGridClick(this);";
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over(order by trim(cu.CustomerCode)) as [No], cu.CustomerID,
            cu.CustomerCode CID, cu.CustomerName,
            c.CompanyName, c.CompanyNameKH, c.CompanyAddress + 
            case when c.CompanyCommune <> '' then ', ' + CompanyCommune else '' end +
            case when c.CompanyDistrict <> '' then ', ' + c.CompanyDistrict else '' end + 
            case when c.CompanyProvinceCity <> '' then ', ' + c.CompanyProvinceCity else '' end CompanyAddress, 
            c.CompanyContactName, c.CompanyContactPosition, c.CompanyContactMobileNumber, c.CompanyEmail
            from dbo.TblCustomerCompany c
            inner join dbo.TblCustomer cu on cu.CustomerID = c.CustomerID
            where (c.CompanyName <> '' OR c.CompanyNameKH <> '')
            and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "trim(cu.CustomerCode)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Company Name", FieldName = "CompanyName" },
                new GridField() { Header = "Company Name KH", FieldName = "CompanyNameKH" },
                new GridField() { Header = "Address", FieldName = "CompanyAddress" },
                new GridField() { Header = "Contact Name", FieldName = "CompanyContactName" },
                new GridField() { Header = "Contact Position", FieldName = "CompanyContactPosition" },
                new GridField() { Header = "Contact Number", FieldName = "CompanyContactMobileNumber" },
                new GridField() { Header = "Company Email", FieldName = "CompanyEmail" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
    }
}