using MVC;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class ReportCustomerAttachmentController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "Report Customer Attachment"
            };
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerType",
                Label = "Block Code:",
                DBField = "CustomerType",
                SQL_CBO = "select CustomerType from dbo.TblCustomerType where [Status] = 'Enable' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerType",
                ComboBox = true
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerType"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and ci.SystemID = " + html.SystemID + " and ci.WarehouseID=" + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%' order by c.CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerType"),
                ISAutoComplete = true,
            });

            string sql = @"
                WITH CustomerData AS (
                    SELECT distinct
                        c.CustomerID,
                        c.CustomerCode,
                        CASE 
                            WHEN c.CustomerNameLatin IS NULL THEN c.CustomerName 
                            ELSE c.CustomerNameLatin 
                        END AS CustomerName, 
                        c.CustomerAddress + ', ' + c.CustomerCommune + ', ' 
                        + c.CustomerDistrict + ', ' + c.CustomerProvinceCity AS [Address], 
                        c.CustomerMobile, 
                        c.CustomerIdentification, 
                        s.value as AttachmentFileName,
                        cf.TypeFor,
                        cf.DestinationFolder,
		                ct.CustomerType
                    FROM dbo.TblCustomer c
                    INNER JOIN dbo.TblCustomerInvoice ci ON ci.CustomerID = c.CustomerID
                    INNER JOIN dbo.TblActivateCustomer ac ON ac.CustomerID = c.CustomerID
		                INNER JOIN dbo.TblCustomerType ct ON ct.CustomerTypeID = c.CustomerTypeID
                    LEFT JOIN TblCustomerAttachmentFile cf ON cf.CustomerID = c.CustomerID AND cf.AttachmentFileName <> ''
		                OUTER APPLY STRING_SPLIT(cf.AttachmentFileName, '~') s
                    WHERE ci.StatusID IN (10,11) 
                        AND ci.[Type] = 'Official Invoice'
                        AND ac.ConnectivityStatus = 40
                ), a as (
                    SELECT
                    CustomerID,
                    CustomerCode,
                    CustomerName,
                    [Address],
                    CustomerMobile,
                    CustomerIdentification,
		                CustomerType,
                    MIN(DestinationFolder) AS DestinationFolder,
                    ISNULL(
                        STRING_AGG(
                            CASE WHEN TypeFor <> 'ServiceForm'
                        THEN RIGHT(DestinationFolder, CHARINDEX('/', REVERSE(DestinationFolder)) - 1) 
                            + '/' + AttachmentFileName 
                            END, ','
                        ), ''
                    ) AS FullAttachmentPath,
                    ISNULL(
                        STRING_AGG(
                            CASE WHEN TypeFor = 'ServiceForm' 
                                    THEN RIGHT(DestinationFolder, CHARINDEX('/', REVERSE(DestinationFolder)) - 1) 
                                        + '/' + AttachmentFileName 
                            END, ','
                        ), ''
                    ) AS FullServiceFormPaths

                FROM CustomerData
                GROUP BY 
                    CustomerID,
                    CustomerCode,
                    CustomerName,
                    [Address],
                    CustomerMobile,
                    CustomerIdentification,
		                CustomerType	
                )
                select row_number() over(order by CustomerID desc) as [No], * from a";
            html.OrderBy = "[No]";
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign="center" },
                new GridField() { Header = "CustomerCode", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true, TextAlign = "center" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true, TextAlign="left" },
                new GridField() { Header = "Block", FieldName = "CustomerType", TextAlign="left",  ClassName="truncate-cell"},
                new GridField() { Header = "Address", FieldName = "Address", TextAlign="left", Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Customer Mobile", FieldName = "CustomerMobile", TextAlign="center" },
                new GridField() { Header = "Customer Identification", FieldName = "CustomerIdentification",TextAlign="center"},
                new GridField() { Header = "Attachment", FieldName = "FullAttachmentPath", ImagePath="Uploads/CustomerAttFiles/" ,TextAlign="center"},
                new GridField() { Header = "Service Form", FieldName = "FullServiceFormPaths" ,ImagePath="Uploads/CustomerAttFiles/",TextAlign="center"},

            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}