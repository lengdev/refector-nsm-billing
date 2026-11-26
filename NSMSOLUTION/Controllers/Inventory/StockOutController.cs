using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockOutController : Controller
    {
        // GET: StockOut
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Out"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Stock Out Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtWarehouseName",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                SQL_CBO = "select distinct uw.WID, uw.WarehouseName\r\nfrom dbo.TblStockOut b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.LocationID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Group_StockIN",
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQL_CBO = "select distinct ig.ItemGroupID, ig.ItemGroup from dbo.TblItemGroup ig \r\ninner join dbo.TblItem i on i.ItemGroupID = ig.ItemGroupID\r\ninner join dbo.TblStockOut so on so.ItemID = i.ItemID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Invoice_Number",
                ISAutoComplete = true,
                Label = "Reference Number:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 so.InvoiceNumber from dbo.TblStockOut so where lower(so.InvoiceNumber) like @SearchText and so.SystemID = " + html.SystemID + " and so.WarehouseID=" + html.WarehouseID + " order by so.InvoiceNumber desc",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Customer_Name",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "CustomerName",
                SQLAutoComplete = "select distinct top 10 (c.CustomerCode + ' - ' + c.CustomerName) CustomerName from dbo.TblStockOut so inner join dbo.TblStockIn si on si.StockInID = so.StockInID inner join dbo.TblVendor v on v.VendorID = si.VendorID inner join dbo.TblCustomer c on c.CustomerID = so.CustomerID where lower((c.CustomerCode + ' - ' + c.CustomerName)) like @SearchText and so.SystemID = " + html.SystemID + " and so.WarehouseID=" + html.WarehouseID + " order by (c.CustomerCode + ' - ' + c.CustomerName)",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBlock",
                Label = "Block:",
                DBField = "CustomerType",
                SQL_CBO = "select distinct t.CustomerTypeID, t.CustomerType from dbo.TblCustomerType t where t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%' order by t.CustomerTypeID",
                DisplayText = "CustomerType",
                Value = "CustomerType",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemCode",
                Label = "Item Code:",
                DBField = "ItemCode",
                SQL_CBO = "select distinct i.ItemCode from TblItem i where isnull(i.ItemCode, '') <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayText = "ItemCode",
                Value = "ItemCode",
                ComboBox = true,
                Multiple = true,
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i left join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Code",
                SQL = "select distinct top 10 i.ItemCode from TblItem i where isnull(i.ItemCode, '') <> '' and lower(i.ItemCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Serial Number",
                SQL = @"SELECT distinct top 10 SerialNumber
FROM (
    SELECT SerialNumber 
    FROM TblItem
    UNION
    SELECT SerialNumber 
    FROM TblItemVariant
    UNION
    SELECT SerialNumber 
    FROM TblItemSerialDetail
) AS CombinedResults
WHERE ISNULL(SerialNumber, '') <> '' 
AND LOWER(SerialNumber) LIKE  @SearchText
ORDER BY SerialNumber;",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridProcedureName = "SP_SHOW_STOCK_OUT";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date Out", FieldName = "StockOutDate", ClassName="text-center"},
                new GridField() { Header = "Reference Number", FieldName = "InvoiceNo",ClassName="text-center" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Block", FieldName = "Block" },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center" },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true },
                new GridField() { Header = "Item Size", FieldName = "ItemSize", ClassName="text-center" },
                new GridField() { Header = "Expiry Date", FieldName = "ExpireDate", ClassName = "text-center text-red",IsHiddenField= html.SystemID!="25",},
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber", ClassName="text-center" },
                new GridField() { Header = "Item Group", FieldName = "ItemGroup" },
                new GridField() { Header = "Category", FieldName = "Category" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", TotalLabel="Total Qty OUT:"},
                new GridField() { Header = "QTY OUT", FieldName = "QTYOUT", ClassName="text-right text-danger", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "UOM", FieldName = "UM", TextAlign="center"},
                new GridField() { Header = "Stock Out Type", FieldName = "Type", IsGroupByField=true},
                new GridField() { Header = "Remark", FieldName = "Remark"},
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}