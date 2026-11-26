using AllReportAPi.Services;
using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Utilities;

namespace NSMSOLUTION.Controllers.Report
{
    [Authorize]
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConnectionType()
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() OVER(ORDER BY c.CustomerID desc, bi.ConnectionID desc) AS [No], 
            c.CustomerCode, c.CustomerName, bf.CompanyContactMobileNumber ContactPhone, 
            format(c.RegisterDate, 'dd-MM-yyyy') RegisterDate, c.RegisterBy,
            bi.ConnectionID, 
            isnull(bi.[SID], 'Pending') [SID],
            p.PackageCode, case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.PackageName end
            PackageName, pd.Bandwidth, pd.DowSpeed, pd.UpSpeed, bi.UM,
            format(bi.StartDate, 'dd-MM-yyyy') StartDate, 
            format(bi.EndDate, 'dd-MM-yyyy') EndDate,
            [Status],
            (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
            inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            inner join dbo.TblServices se on se.ServiceID = bi.ServiceID
            where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.ShowPaging = false;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.UsingJqueryDataTable = true;
            html.OrderBy = "c.CustomerID desc, bi.ConnectionID desc";
            //html.JqueryDataTableResponsive = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone", FieldName = "ContactPhone" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "ConnectionID", FieldName = "ConnectionID" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Package Code", FieldName = "PackageCode" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                //new GridField() { Header = "Bandwidth", FieldName = "Bandwidth" },
                //new GridField() { Header = "DowSpeed", FieldName = "DowSpeed" },
                //new GridField() { Header = "UpSpeed", FieldName = "UpSpeed" },
                //new GridField() { Header = "UM", FieldName = "UM" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "InstallAddress", FieldName = "InstallAddress" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult PackageList()
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY p.PackageID desc, pt.PackageTypeName, pd.Bandwidth, pd.Duration) AS[No], s.ServiceName, ";
            SQL += "pt.PackageTypeName, p.PackageCode, p.PackageName, p.PackageID, convert(varchar, p.[Date], 103) Lastupdate, ";
            SQL += "(convert(varchar, pd.Bandwidth) + ' Mbps') Bandwidth, ";
            SQL += "(convert(varchar, pd.Duration) + ' ' + pd.UM) Duration, pd.Price, pd.DepositFee, pd.FreeDuration, ";
            if (html.AllowEdit)
                SQL += "'<a href=\"javascript:void(0);\" onclick=\"ShowDialogBox(''Edit Package'', ''Auto'', ''500'', ''" + html.ServerRoot + "AddPackages/CreatePackage/'', '+convert(varchar, p.PackageID)+');\">Edit</a>' Edit, ";
            else
                SQL += "'<a href=\"javascript:void(0);\" onclick=\"DialogSweetAlert(''nopermission'', ''Sorry, you dont have permission for accessing this page. Please contact administrator.'');\">Edit</a>' Edit, ";
            if (html.AllowDelete)
                SQL += "'<a href=\"javascript:void(0);\" onclick=\"deletePackage(''" + html.ServerRoot + "AddPackages/DeletePackage/'', '+convert(varchar, p.PackageID)+');\">Delete</a>' [Del] ";
            else
                SQL += "'<a href=\"javascript:void(0);\" onclick=\"DialogSweetAlert(''nopermission'', ''Sorry, you dont have permission for accessing this page. Please contact administrator.'');\">Delete</a>' [Del] ";

            SQL += ", case when IsApplyRediusProfile = 0 then '<a href=\"javascript:void(0);\" onclick=\"ShowDialogBox(''Add Radius Mikrotik Profile'', ''Auto'', ''500'', ''" + html.ServerRoot + "RADIUS/CreateRediusProfile'', {PrimaryFieldValue:0,PackageDetailID:' + convert(varchar, pd.PackageDetailID) + '});\">Apply Radius Profile</a>' else 'Already Apply' end ApplyRediusProfile ";//PrimaryFieldValue=0&PackageDetailID='', '+convert(varchar, pd.PackageDetailID)+
            SQL += "from dbo.TblPackages p inner join dbo.TblServices s on s.ServiceID = p.ServiceID ";
            SQL += "inner join dbo.TblPackageType pt on pt.PackageTypeID = p.PackageTypeID ";
            SQL += "inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID ";
            SQL += "where p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "p.PackageID desc, pt.PackageTypeName, pd.Bandwidth, pd.Duration";
            html.GH.SQL = SQL;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.ShowPaging = false;
            html.UsingJqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Service Name", FieldName = "ServiceName" },
                new GridField() { Header = "Connection Type", FieldName = "PackageTypeName"},
                new GridField() { Header = "Package Code", FieldName = "PackageCode"},
                new GridField() { Header = "Package Name", FieldName = "PackageName"},
                //new GridField() { Header = "Lastupdate", FieldName = "Lastupdate", IsGroupByField = true, Colspan = 7, GroupLabel= "Date: " },
                //new GridField() { Header = "Edit", FieldName = "Edit", IsGroupByField = true, Colspan = 7 },
                //new GridField() { Header = "Delete", FieldName = "Del", IsGroupByField = true, Colspan = 7 },
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", TextAlign="center" },
                new GridField() { Header = "Duration", FieldName = "Duration", TextAlign="center" },
                new GridField() { Header = "Price", FieldName = "Price", TextAlign="center" },
                new GridField() { Header = "DepositFee", FieldName = "DepositFee", TextAlign="center" }, //ClassName="text-nowrap-pre"
                new GridField() { Header = "FreeDuration", FieldName = "FreeDuration", TextAlign="center" },
                //new GridField() { Header = "Apply Radius Profile", FieldName = "ApplyRediusProfile", TextAlign="center" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Show Report Sales By Item")]
        public ActionResult SalesByItem()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStockName",
                Label = "Stock Name:",
                DBField = "StockNameID",
                SQL_CBO = "select distinct so.StockNameID, so.StockName from dbo.TblItem i inner join dbo.TblStockName so on so.StockNameID = i.StockNameID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "StockName",
                Value = "StockNameID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemGroup",
                Label = "Item Group:",
                DBField = "ItemGroupID",
                SQL_CBO = "select distinct so.ItemGroupID, so.ItemGroup from dbo.TblItem i inner join dbo.TblItemGroup so on so.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroupID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBarcode",
                Label = "Barcode:",
                DBField = "Barcode",
                SQLAutoComplete = "select distinct top 10 i.Barcode from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSerialNumber",
                Label = "Serial#:",
                DBField = "SerialNumber",
                SQLAutoComplete = "select distinct top 10 i.SerialNumber from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.SerialNumber) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_SHOW_SALES_BY_ITEM";
            html.isShowLoadingGrid = false;
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 3;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "StockName", FieldName = "StockName", IsGroupByField = true, GroupLabel="Stock Name: ", Colspan = 7 },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", IsGroupByField = true, GroupLabel="Item Group: ", Colspan = 7 },
                new GridField() { Header = "Date Out", FieldName = "DateOut", Format="dd-MM-yyyy" },
                new GridField() { Header = "Barcode", FieldName = "Barcode" },
                new GridField() { Header = "Serial#", FieldName = "SerialNumber" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "Quantity Sold", FieldName = "QuantitySold", TextAlign="right", Format="#,#" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign="right", Format="#,#.00",
                LinkField = "ItemID", DataTableNumber = 2, DisplayFields="Total Quantity Sold, Total Amount", Colspan = 7}
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Show Report Sales By Sales Person")]
        public ActionResult SalesBySalesPerson()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Sales Person Name",
                SQL = "select distinct top 10 l.FullName from dbo.TblLeads l inner join dbo.TblCustomer c on c.EmployeeID = l.LeadID where lower(l.FullName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("SalesPerson"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_SHOW_SALES_BY_Sales_Person";
            html.isShowLoadingGrid = false;
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Name", FieldName = "FullName" },
                new GridField() { Header = "Customer", FieldName = "Customer", TextAlign="right" },
                new GridField() { Header = "Invoice", FieldName = "Invoice", TextAlign="right" },
                new GridField() { Header = "Invoice Paid", FieldName = "InvoicePaid", TextAlign="right", Format="#,#0.00" },
                new GridField() { Header = "Invoice Unpaid", FieldName = "InvoiceUnpaid", TextAlign="right", Format="#,#0.00"},
                new GridField() { Header = "Invoice Sales", FieldName = "InvoiceSales", TextAlign="right", Format="#,#0.00"}
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Show Report Sales By Customer")]
        public ActionResult SalesByCustomer()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 (c.CustomerCode + ' - ' + c.CustomerName) CustomerName from dbo.TblCustomer c where lower(c.CustomerCode + ' - ' + c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_SHOW_SALES_BY_Customer";
            html.isShowLoadingGrid = false;
            html.GridEventClick = "onGridClickShowCustProfile(this);";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField = true },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Invoice", FieldName = "Invoice", TextAlign="right" },
                new GridField() { Header = "Invoice Paid", FieldName = "InvoicePaid", TextAlign="right", Format="#,#" },
                new GridField() { Header = "Invoice Unpaid", FieldName = "InvoiceUnpaid", TextAlign="right", Format="#,#.00"},
                new GridField() { Header = "Invoice Sales", FieldName = "InvoiceSales", TextAlign="right", Format="#,#.00"}
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Show Report Item List")]
        public ActionResult ItemList()
        {
            HTML html = new HTML();
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 ItemName from dbo.TblItem where lower(ItemName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("i.ItemName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.IsHaveWhereCondition = true;
            string sql = @"select ROW_NUMBER() over (order by sn.StockName, g.ItemGroup, i.ItemID)as [No],
            sn.StockName, g.ItemGroup,
            i.Barcode, i.SerialNumber, i.ItemName, i.SKU, i.UM,
            p.CostPrice, s.SellingPrice, i.ReorderPoint, i.MaximumPoint, i.MaintainStock
            from dbo.TblItem i
            inner join dbo.TblPurchaseInfo p on p.ItemID = i.ItemID
            inner join dbo.TblSaleInfo s on s.ItemID = i.ItemID
            inner join dbo.TblStockName sn on sn.StockNameID = i.StockNameID
            inner join dbo.TblItemGroup g on g.ItemGroupID = i.ItemGroupID
            where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.OrderBy = "sn.StockName, g.ItemGroup, i.ItemID";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Stock Name", FieldName = "StockName", GroupLabel="Stock Name: ", IsGroupByField = true, Colspan = 11 },
                new GridField() { Header = "Item Group", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 11 },
                new GridField() { Header = "Barcode", FieldName = "Barcode" },
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "SKU", FieldName = "SKU" },
                new GridField() { Header = "UM", FieldName = "UM" },
                new GridField() { Header = "Cost Price", FieldName = "CostPrice" },
                new GridField() { Header = "Selling Price", FieldName = "SellingPrice" },
                new GridField() { Header = "Reorder Point", FieldName = "ReorderPoint" },
                new GridField() { Header = "Maximum Point", FieldName = "MaximumPoint" },
                new GridField() { Header = "Maintain Stock", FieldName = "MaintainStock" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Show Report Inventory Summary")]
        public ActionResult InventorySummary()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStockName",
                Label = "Stock Name:",
                DBField = "StockNameID",
                SQL_CBO = "select distinct so.StockNameID, so.StockName from dbo.TblItem i inner join dbo.TblStockName so on so.StockNameID = i.StockNameID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "StockName",
                Value = "StockNameID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemGroup",
                Label = "Item Group:",
                DBField = "ItemGroupID",
                SQL_CBO = "select distinct so.ItemGroupID, so.ItemGroup from dbo.TblItem i inner join dbo.TblItemGroup so on so.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroupID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBarcode",
                Label = "Barcode:",
                DBField = "Barcode",
                SQLAutoComplete = "select distinct top 10 i.Barcode from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSerialNumber",
                Label = "Serial#:",
                DBField = "SerialNumber",
                SQLAutoComplete = "select distinct top 10 i.SerialNumber from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.SerialNumber) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_SHOW_InventorySummary";
            html.isShowLoadingGrid = false;
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 3;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "StockName", FieldName = "StockName", IsGroupByField = true, GroupLabel="Stock Name: ", Colspan = 6 },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", IsGroupByField = true, GroupLabel="Item Group: ", Colspan = 6 },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "Quantity Order", FieldName = "QuantityOrder", TextAlign="right", Format="#,#" },
                new GridField() { Header = "Quantity In", FieldName = "QuantityIn", TextAlign="right", Format="#,#", },
                new GridField() { Header = "Quantity Out", FieldName = "QuantityOut", TextAlign="right", Format="#,#", },
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", TextAlign="right", Format="#,#0", }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Show Report Inventory Valuation Summary")]
        public ActionResult InventoryValuationSummary()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStockName",
                Label = "Stock Name:",
                DBField = "StockNameID",
                SQL_CBO = "select distinct so.StockNameID, so.StockName from dbo.TblItem i inner join dbo.TblStockName so on so.StockNameID = i.StockNameID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "StockName",
                Value = "StockNameID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemGroup",
                Label = "Item Group:",
                DBField = "ItemGroupID",
                SQL_CBO = "select distinct so.ItemGroupID, so.ItemGroup from dbo.TblItem i inner join dbo.TblItemGroup so on so.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroupID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBarcode",
                Label = "Barcode:",
                DBField = "Barcode",
                SQLAutoComplete = "select distinct top 10 i.Barcode from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSerialNumber",
                Label = "Serial#:",
                DBField = "SerialNumber",
                SQLAutoComplete = "select distinct top 10 i.SerialNumber from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.SerialNumber) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockOut so on so.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_SHOW_InventoryValuationSummary";
            html.isShowLoadingGrid = false;
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "StockName", FieldName = "StockName", IsGroupByField = true, GroupLabel="Stock Name: ", Colspan = 8 },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", IsGroupByField = true, GroupLabel="Item Group: ", Colspan = 8 },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "Quantity Order", FieldName = "QuantityOrder", TextAlign="right", Format="#,#" },
                new GridField() { Header = "Quantity In", FieldName = "QuantityIn", TextAlign="right", Format="#,#", },
                new GridField() { Header = "Quantity Out", FieldName = "QuantityOut", TextAlign="right", Format="#,#", },
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", TextAlign="right", Format="#,#0", },
                new GridField() { Header = "Cost Price", FieldName = "CostPrice", TextAlign="right", Format="#,#.00", },
                new GridField() { Header = "Inventory Asset Value", FieldName = "InventoryAssetValue", TextAlign="right", Format="#,#.00", }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult CustomerNearlyExpired()
        {
            // --- Tab 1: Customer Nearly Expired ---
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Expire Date:",
                DateTimePickerWithRanges = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (40, 41, 42, 43)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
                Multiple = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClass",
                Label = "Class Name:",
                DBField = "ClassNameID",
                SQL_CBO = "select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            html.GH.GridPara = new GridPara[]
            {
                new GridPara() { ParaName="Type", Value="ShowAll" },
            };

            html.ReportTitle = "Customer lists < 7 Days Expired";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Class Name", FieldName = "ClassName", IsHiddenField=true},
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo", PrimaryKeyFieldName = true, TotalLabel="Total:", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);"},
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format = "#,#0", IsTotalFooter=true},
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", ClassName="text-center", FormatDateTime = "dd-MM-yyyy"},
                new GridField() { Header = "Expire Date", FieldName = "ExpireDate", ClassName="text-center text-danger", FormatDateTime = "dd-MM-yyyy"},
                new GridField() { Header = "Days Left", FieldName = "DaysLeft", ClassName="text-center"},
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-center", DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Actived", "badge-success" },
                        { "Locked", "badge-danger" },
                        { "Terminate", "badge-danger" },
                        { "Inactive", "badge-warning" },
                    }
                },
                new GridField() { Header = "Remark", FieldName = "Comment", Editable=true, Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Sale Remark", FieldName = "PostComment", Width="500px", ClassName="truncate-cell"},
            };

            html.ShowPaging = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SEARCH_CUSTOMER_NEARLY_EXPIRRE",
                IsEditable = true,
                UrlApiUpdateData = "ApiGetMasterData/UpdateInvoiceRemark"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());

            // --- Tab 2: Customer Nearly Expired Second ---
            HTML html2 = new HTML();
            List<HtmlAddControls> htmlAddControls2 = html2.HtmlAddControls;
            htmlAddControls2.Add(new HtmlAddControls()
            {
                ID = "txtStatusID2",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (40, 41, 42, 43)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
                Multiple = true,
            });
            htmlAddControls2.Add(new HtmlAddControls()
            {
                ID = "txtClass2",
                Label = "Class Name:",
                DBField = "ClassNameID",
                SQL_CBO = "select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html2.SystemID + " and c.WarehouseID=" + html2.WarehouseID + " order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox2 = html2.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox2.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox2.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            html2.GH.GridPara = new GridPara[]
            {
                new GridPara() { ParaName="Type", Value="" },
            };

            html2.ReportTitle = "Customer lists < 7 Days Expired";
            html2.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Class Name", FieldName = "ClassName", IsHiddenField=true},
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo", PrimaryKeyFieldName = true, TotalLabel="Total:", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);"},
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format = "#,#0", IsTotalFooter=true},
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", ClassName="text-center", FormatDateTime = "dd-MM-yyyy"},
                new GridField() { Header = "Expire Date", FieldName = "ExpireDate", ClassName="text-center text-danger", FormatDateTime = "dd-MM-yyyy"},
                new GridField() { Header = "Days Left", FieldName = "DaysLeft", ClassName="text-center"},
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-center", DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Actived", "badge-success" },
                        { "Locked", "badge-danger" },
                        { "Terminate", "badge-danger" },
                        { "Inactive", "badge-warning" },
                    }
                },
                new GridField() { Header = "Remark", FieldName = "Comment", Editable=true, Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Sale Remark", FieldName = "PostComment", Width="500px", ClassName="truncate-cell"},
            };
            html2.ShowPaging = false;
            KendoGridSetting kendoGridSetting2 = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SEARCH_CUSTOMER_NEARLY_EXPIRRE",
                IsEditable = true,
                ShowBellButton = true,
                OnClickBellButton = "AlertToSale(dataItem);",
                UrlApiUpdateData = "ApiGetMasterData/UpdateInvoiceRemark"
            };
            ViewBag.DrawHTMLSearchForm2 = html2.DrawHTMLSearchForm(1, "", true, html2.GH.GridTable.ToList().ToJsonString(), kendoGridSetting2.ToJsonString());
            return View();
        }

        public ActionResult ShowGeneratedInvoice()
        {
            HTML html = new HTML
            {
                ShowGridOnly = true,
                TblID = "TblShowGeneratedInv"
            };
            string sql = "select ROW_NUMBER() OVER(ORDER BY bi.CustomerID, p.PackageName) AS[No], ";
            sql += "'<a href=\"javascript:void(0);\" onclick=\"OpenNewTap(''#!/CustomerProfile?ID='+convert(varchar, bi.CustomerID)+''')\">' + (c.CustomerCode + ' - ' + c.CustomerName) + '</a>' CustomerName, ";
            //sql += "(c.CustomerCode + ' - ' + c.CustomerName) CustomerName, ";
            sql += "s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, bi.UM) PackageName, ";
            sql += "format(bi.InvoiceDate, 'dd-MM-yy') InvoiceDate, format(bi.StartDate, 'dd-MM-yy') StartDate, format(bi.EndDate, 'dd-MM-yy') EndDate, format(bi.NextInvoiceDate, 'dd-MM-yy') NextInvoiceDate, bi.Amount, ";
            sql += "(case when format(bi.EndDate, 'yyMMdd') < format(getdate(), 'yyMMdd') then 'Expired' else 'Remain: ' + convert(varchar, DATEDIFF(d, getdate(), bi.EndDate)) + ' days' end) InvoiceStatus, ";
            sql += "bi.GeneratedBy, '<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+convert(varchar, bi.CustomerID) +', ''' + bi.InvoiceNo +''','+ convert(varchar, bi.IsIncludedVAT) +','+ convert(varchar, bi.StatusID) +','''',false);\">' + bi.InvoiceNo + '</a>' InvoiceNo ";
            sql += ", bi.GeneratedDate from dbo.TblCustomerBillingInvoice bi ";
            sql += "inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID ";
            sql += "inner join dbo.TblPackages p on p.PackageID = bi.PackageID ";
            sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
            sql += "inner join dbo.TblServices s on s.ServiceID = bi.ServiceID ";
            sql += "where bi.StatusID not in (12,13) and bi.IsNextInvoice = 1 and isnull(bi.IsGenerated,0) = 1 ";
            sql += "and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID;
            sql += " and format(bi.GeneratedDate, 'yyyyMMdd') = " + DateTime.Parse(html.GetCurrentDate()).ToString("yyyyMMdd");

            html.GH.SQL = sql;
            html.OrderBy = "bi.CustomerID, p.PackageName";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerName", FieldName = "CustomerName" },
                new GridField() { Header = "PackageName", FieldName = "PackageName", IsGroupByField = true, Colspan = 9 },
                new GridField() { Header = "Generated Date", FieldName = "GeneratedDate", TextAlign="center", Format="dd-MM-yy" },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right" },
                new GridField() { Header = "InvoiceDate", FieldName = "InvoiceDate", IsGroupByField = true, Colspan = 9, GroupLabel="Inv.Date: " },
                new GridField() { Header = "StartDate", FieldName = "StartDate", IsGroupByField = true, Colspan = 9, GroupLabel="Start Date: " },
                new GridField() { Header = "EndDate", FieldName = "EndDate", IsGroupByField = true, Colspan = 9, GroupLabel="End Date: " },
                new GridField() { Header = "NextInvoiceDate", FieldName = "NextInvoiceDate", TextAlign="center" },
                new GridField() { Header = "Status", FieldName = "InvoiceStatus", ClassName="text-danger" },
                new GridField() { Header = "GeneratedBy", FieldName = "GeneratedBy" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadInvGenerated");
            return View();
        }

        [Authorize(Roles = "Customer Services")]
        public ActionResult CustomerService()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "End Date:",
                DefaultValueDate = false,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Payment Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblCustomerBalance cb\r\ninner join dbo.TblStatus s on s.StatusID = cb.StatusID\r\nwhere cb.StatusID not in (12,13) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClass",
                Label = "Class Name:",
                DBField = "ClassNameID",
                SQL_CBO = "select distinct c.ClassID, c.ClassName from dbo.TblClass c\r\ninner join dbo.TblCustomerBalance cb on cb.ClassNameID = c.ClassID\r\nwhere c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            //htmlAddControls.Add(new HtmlAddControls()
            //{
            //    ID = "txtIsIncludedVAT",
            //    Label = "Is Included VAT:",
            //    DBField = "IsIncludedVAT",
            //    SQL_CBO = "select 1 VATID, 'VAT' [Status]\r\nunion \r\nselect 0 VATID, 'None VAT' [Status]",
            //    DisplayText = "Status",
            //    Value = "VATID",
            //    ComboBox = true
            //});
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSalesRepresentative",
                Label = "Sales Representative:",
                DBField = "SalesRepresentative",
                SQL_CBO = "select distinct l.LeadID, l.FullName\r\nfrom dbo.TblCustomerBalance cb\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\nwhere cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " order by l.FullName",
                DisplayText = "FullName",
                Value = "LeadID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroup",
                SQL_CBO = "select distinct cg.CustomerGroupID, cg.CustomerGroup\r\nfrom dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\ninner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID\r\n where cg.SystemID = " + html.SystemID + " and cg.WarehouseID=" + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode\r\nfrom dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID where lower(c.CustomerCode) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_REPORT_CUSTOMER_SERVICES";
            html.ReportTitle = "Report Customer Services";
            html.ReportTitle = "Customer Services";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Class Name", FieldName = "ClassName", IsGroupByField= true, GroupLabel="Class Name: ", Colspan = 16 },
                new GridField() { Header = "SaleName", FieldName = "SaleName" , IsGroupByField= true, GroupLabel="Sale's Name: ", Colspan = 16 },
                new GridField() { Header = "Cust.Group", FieldName = "CustomerGroup", IsGroupByField= true, GroupLabel="Customer Group: ", Colspan = 16 },
                new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" }, 
                //new GridField() { Header = "Tel", FieldName = "CustomerContactMobileNumber" }, 
                new GridField() { Header = "SID", FieldName = "SID" },
                //new GridField() { Header = "Conn.ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Conn.Status", FieldName = "ConnectivityStatus" },
                //new GridField() { Header = "Inv.No", FieldName = "InvoiceNo" },
                //new GridField() { Header = "Conn.Type", FieldName = "ConntionType", ClassName="text-danger" },
                //new GridField() { Header = "Start Date", FieldName = "StartDate", Format ="dd/MM/yy" },
                //new GridField() { Header = "End Date", FieldName = "EndDate", Format ="dd/MM/yy" },
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-info" },
                new GridField() { Header = "Package", FieldName = "PackageDate" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", Format ="dd/MM/yy" },
                
                //new GridField() { Header = "PaidBy", FieldName = "PaidBy" }
                //new GridField() { Header = "PaidBy", FieldName = "PaidBy", DataTableNumber=2, LinkField="ClassNameID, EmployeeID, CustomerGroupID", DisplayFields="GrandTotal, Paid, Unpaid", Colspan=15 }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Show Invoice List")]
        public ActionResult CustomerInvoiceList()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                DateTimePickerWithRanges = true,
            });

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroup",
                SQL_CBO = "select distinct cg.CustomerGroup from dbo.TblCustomerGroup cg where cg.SystemID = " + html.SystemID + " and cg.WarehouseID = " + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroup",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct where ct.SystemID = " + html.SystemID + " and ct.WarehouseID=" + html.WarehouseID,
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = "select distinct c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSaleName",
                Label = "Sale's Name:",
                DBField = "SaleID",
                SQL_CBO = "select distinct l.FullName, c.EmployeeID from dbo.TblLeads l inner join dbo.TblCustomer c on c.EmployeeID = l.LeadID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by l.FullName",
                DisplayText = "FullName",
                Value = "EmployeeID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Payment Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (10, 11)",
                DisplayText = "Status",
                Value = "StatusID",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoice",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 InvoiceNo from dbo.TblCustomerBalance where lower(InvoiceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and StatusID not in (12, 13, 14) and isnull(InvoiceNo, '') <> '' order by InvoiceNo desc",
                ISAutoComplete = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtATTName",
                Label = "ATT Name:",
                DBField = "ATTName",
                SQLAutoComplete = @"select distinct top 10 bi.ATTName from dbo.TblCustomerBillInformation bi where lower(bi.ATTName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + "  order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Identification/Passport",
                SQL = "select distinct top 10 c.CustomerIdentification from dbo.TblCustomer c where lower(c.CustomerIdentification) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by c.CustomerIdentification",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);"},
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", ClassName="text-center", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", ClassName="text-center", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "Amount", FieldName = "GrandTotal", ClassName = "text-danger text-right", Format = "#,#0" },
                new GridField() { Header = "Exchange Rate", FieldName = "ExchangeRate", ClassName="text-right"},
                new GridField() { Header = "Invoice Status", FieldName = "InvoiceStatus", ClassName="text-center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Approve", "badge-primary" },
                        { "Pending", "badge-danger" },
                        { "Checked", "badge-warning" },
                    }
                },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", ClassName="text-uppercase", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-center", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "Block", FieldName = "CustomerType", ClassName="text-center"},
                new GridField() { Header = "VAT Number", FieldName = "VATNumber" },
                new GridField() { Header = "ATT Name", FieldName = "ATTName" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
                new GridField() { Header = "Remark", FieldName = "CommentUnpaid" },
            };

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_REPORT_CUSTOMER_INVOICE_LIST",
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }


        [Authorize(Roles = "Customer Invoice Unpaid")]
        public ActionResult CustomerInvoiceUnpaid()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                DateTimePickerWithRanges = true,
            });

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroup",
                SQL_CBO = "select distinct cg.CustomerGroup from dbo.TblCustomerGroup cg inner join dbo.TblCustomer c on c.CustomerGroupID = cg.CustomerGroupID inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID where ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%' order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroup",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID where ci.SystemID = " + html.SystemID + " and ci.WarehouseID=" + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = "select distinct c.ClassName from dbo.TblClass c inner join dbo.TblCustomerBalance cb on cb.ClassNameID = c.ClassID where cb.StatusID not in(12,13,14,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%' order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSaleName",
                Label = "Sale's Name:",
                DBField = "SaleName",
                SQL_CBO = "select distinct l.FullName from dbo.TblLeads l inner join dbo.TblCustomer c on c.EmployeeID = l.LeadID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by l.FullName",
                DisplayText = "FullName",
                Value = "FullName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoice",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 InvoiceNo from dbo.TblCustomerBalance where lower(InvoiceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' and StatusID not in (12, 13, 14) and isnull(InvoiceNo, '') <> '' order by InvoiceNo",
                ISAutoComplete = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtATTName",
                Label = "ATT Name:",
                DBField = "ATTName",
                SQLAutoComplete = @"select distinct top 10 bi.ATTName from dbo.TblCustomerBillInformation bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower(bi.ATTName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " union all select top 10 CustomerNameLatin from dbo.TblCustomer where lower(CustomerNameLatin) like @SearchText and SystemID = " + html.SystemID + "",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Identification/Passport",
                SQL = "select distinct top 10 c.CustomerIdentification from dbo.TblCustomer where lower(c.CustomerIdentification) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by c.CustomerIdentification",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                Hide = true
            });
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNoLink"},
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", ClassName="text-center" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", ClassName="text-center" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link = "#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                new GridField() { Header = "Amount", FieldName = "GrandTotal", ClassName="text-danger text-right" },
                new GridField() { Header = "Exchange Rate", FieldName = "ExchangeRate", ClassName="text-right"},
                new GridField() { Header = "Package", FieldName = "Package",  },
                new GridField() { Header = "Remark", FieldName = "CommentUnpaid", ClassName="text-left", Editable = true,},
            };

            html.GH.GridPara = new GridPara[]
            {
                new GridPara { ParaName="StatusID", Value = "11" },
                new GridPara { ParaName="Type", Value = "Report" }
            };

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_REPORT_CUSTOMER_INVOICE_UNPAID",
                IsEditable = true,
                UrlApiUpdateData = "ApiGetMasterData/UpdateInvoiceRemarkUnpaid"
            };
            html.DefaultSearch = true;
            html.OrderBy = "[InvoiceNo] ASC";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "CustomerInvoiceUnpaid", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        [Authorize(Roles = "Customer Invoice")]
        public ActionResult CustomerInvoice()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                DateTimePickerWithRanges = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroup",
                SQL_CBO = "select distinct cg.CustomerGroup from dbo.TblCustomerGroup cg where cg.SystemID = " + html.SystemID + " and cg.WarehouseID = " + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroup",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct where ct.SystemID = " + html.SystemID + " and ct.WarehouseID=" + html.WarehouseID,
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = "select distinct c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSaleName",
                Label = "Sale's Name:",
                DBField = "SaleName",
                SQL_CBO = "select distinct l.FullName from dbo.TblLeads l inner join dbo.TblCustomer c on c.EmployeeID = l.LeadID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by l.FullName",
                DisplayText = "FullName",
                Value = "FullName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Payment Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (10, 11)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoice",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 InvoiceNo from dbo.TblCustomerBalance where lower(InvoiceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and StatusID not in (12, 13, 14) and isnull(InvoiceNo, '') <> '' order by InvoiceNo desc",
                ISAutoComplete = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceType",
                Label = "Invoice Type:",
                DBField = "TypeOfInvoice",
                SQL_CBO = "select 'CM' as TypeOfInvoice union all select 'VAT' as TypeOfInvoice",
                DisplayText = "TypeOfInvoice",
                Value = "TypeOfInvoice",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtATTName",
                Label = "ATT Name:",
                DBField = "ATTName",
                SQLAutoComplete = @"select distinct top 10 bi.ATTName from dbo.TblCustomerBillInformation bi where lower(bi.ATTName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + "  order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Identification/Passport",
                SQL = "select distinct top 10 c.CustomerIdentification from dbo.TblCustomer c where lower(c.CustomerIdentification) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by c.CustomerIdentification",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });
            html.ReportTitle = "Report Customer Invoice";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", FormatDateTime = "dd-MMM-yyyy", ClassName="text-center"},
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", FormatDateTime = "dd-MMM-yyyy", ClassName="text-danger text-center"},
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo", PrimaryKeyFieldName=true, OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);"},
                new GridField() { Header = "Invoice Status", FieldName = "InvoiceStatus", ClassName="text-center", DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Approve", "badge-primary" },
                        { "Pending", "badge-danger" },
                        { "Checked", "badge-warning" },
                    }
                },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", ClassName="text-uppercase", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                new GridField() { Header = "Customer Phone", FieldName = "CustomerMobile"},
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", FormatDateTime="dd-MMM-yyyy", ClassName="text-center" },
                new GridField() { Header = "Block", FieldName = "CustomerType"},
                new GridField() { Header = "Identification/Passport", FieldName = "CustomerIdentification"},
                new GridField() { Header = "ATT Name", FieldName = "ATTName"},
                new GridField() { Header = "VAT Number", FieldName = "VATNumber"},
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center", DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Paid", "badge-success" },
                        { "Unpaid", "badge-danger" },
                    }
                },
                new GridField() { Header = "Age", FieldName = "OverDayPayment", TextAlign="center", TotalLabel="Total:"},
                new GridField() { Header = "Amount", FieldName = "SubTotal", Format="#,#0.00", TextAlign="right", ClassName="text-danger text-right", IsTotalFooter=true},
                new GridField() { Header = "VAT(%)", FieldName = "VATPercent", TextAlign="right", ClassName="text-danger text-right"},
                new GridField() { Header = "VAT($)", FieldName = "VAT", Format="#,#0.00", TextAlign="right", ClassName="text-danger text-right", IsTotalFooter=true},
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", Format="#,#0.00", TextAlign="right", ClassName="text-primary text-right", IsTotalFooter= true },
                new GridField() { Header = "Paid", FieldName = "Paid", Format="#,#0.00", TextAlign="right", ClassName="text-primary text-right", IsTotalFooter= true },
                new GridField() { Header = "Unpaid", FieldName = "Unpaid", Format="#,#0.00", TextAlign="right", ClassName="text-danger text-right", IsTotalFooter= true},
                new GridField() { Header = "Exchange Rate", FieldName = "ExchangeRate", ClassName="text-right"},
                new GridField() { Header = "Package", FieldName = "Package" },
                new GridField() { Header = "Start Date", FieldName = "StartDate", FormatDateTime="dd-MMM-yyyy", ClassName="text-center"},
                new GridField() { Header = "End Date", FieldName = "EndDate", FormatDateTime="dd-MMM-yyyy", ClassName="text-center text-danger"},
            };

            html.ShowPaging = false;
            GridField[] gridTable = html.GH.GridTable;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_REPORT_CUSTOMER_INVOICE",
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, gridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }


        [Authorize(Roles = "List CM & Tax Invoice")]
        public ActionResult CMTaxInvoice()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                DBField = "InvoiceDate"
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoice",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 InvoiceNo from dbo.TblCustomerBalance where lower(InvoiceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and StatusID not in (12, 13, 14) and isnull(InvoiceNo, '') <> '' order by InvoiceNo",
                ISAutoComplete = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceType",
                Label = "Invoice Type:",
                DBField = "cb.TypeOfInvoice",
                SQL_CBO = "select 'CM' as TypeOfInvoice union all select 'VAT' as TypeOfInvoice",
                DisplayText = "TypeOfInvoice",
                Value = "TypeOfInvoice",
                ComboBox = true
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
            });

            string sql = $@"select ROW_NUMBER() over(order by format(cb.InvoiceDate, 'yyyyMMdd') desc, cb.InvoiceNo desc) as [No], cb.InvoiceNo,
l.FullName, cc.ClassName, cb.TypeOfInvoice, c.CustomerCode, case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end as CustomerName, cb.InvoiceDate, cb.PaymentDueDate, cb.GrandTotal, isnull(cb.CommentInvoice,'') Memo, isnull(c.CustomerNoted, '') Remark,
c.CustomerID, format(cb.InvoiceDate, 'yyyyMMdd') + '-'+ c.CustomerCode +'_'+ REPLACE(cb.InvoiceNo,'/','') + '.pdf' InvoiceFileName,
case when cb.IsUploadToFTP = 1 then '<span class=""badge badge-primary"">Uploaded</span>' else '<span class=""badge badge-danger"">Pending</span>' end UploadStatus,
'<button onclick=""UploadInvoiceTODMC('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''', '''+ format(cb.InvoiceDate, 'yyyyMMdd') + '-'+ c.CustomerCode +'_'+ REPLACE(cb.InvoiceNo,'/','') + '.pdf'', '''+ cb.TypeOfInvoice +''', '+convert(varchar, isnull(cb.ReviseCount, 0))+');"" class=""btn btn-danger btn-sm"">Upload</button>' Upload,
case when '" + html.DomainID + @"' = '001,002' then case when isnull(c.NewRegisterDate,'') <> '' then isnull(c.NewRegisterDate,'') else c.RegisterDate end else c.RegisterDate end RegisterDate, cb.CreateBy
from dbo.TblCustomer c
inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID
inner join dbo.TblLeads l on l.LeadID = cb.EmployeeID
inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID 
where cb.StatusID not in(12,13,14,20) and not (cb.InvoiceNo like '%PI-%' or cb.InvoiceNo like '%CISP%' or cb.InvoiceNo like '%Q/%') and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and cb.DomainID like '%001,002%' ";
            html.GH.SQL = sql;
            html.ReportTitle = "Report CM & TAX Invoice";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
           {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", TextAlign="center", FormatDateTime="dd-MMM-yy" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this,'','','','','','',true);" },
                new GridField() { Header = "Sale​​", FieldName = "FullName" },
                new GridField() { Header = "Class", FieldName = "ClassName" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID="  },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", TextAlign="center", FormatDateTime="dd-MMM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate",  ClassName="text-danger text-right", FormatDateTime="dd-MMM-yy", TotalLabel = "Total" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", Format="#,#0.00", TextAlign = "right", ClassName="text-danger text-right", IsTotalFooter = true },
                new GridField() { Header = "Invoice File Name", FieldName = "InvoiceFileName" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
           };
            html.ShowPaging = false;
            GridField[] gridTable = html.GH.GridTable;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                filterable = false,
                ShowHeaderInfoExcel = true,
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, gridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        [Authorize(Roles = "Customer Connection List")]
        public ActionResult CustomerConnectionList()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DateTimePickerWithRanges = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRouterName",
                Label = "Router Name:",
                DBField = "RouterName",
                SQL_CBO = "select distinct r.RouterName from dbo.TblRouter r inner join dbo.TblActivateCustomer ac on ac.RouterID = r.RouterID where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " order by r.RouterName",
                DisplayText = "RouterName",
                Value = "RouterName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Connectivity Status:",
                DBField = "Status",
                SQL_CBO = "select distinct s.[Status] from dbo.TblStatus s inner join dbo.TblActivateCustomer ac on ac.ConnectivityStatus = s.StatusID where ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " order by s.[Status]",
                DisplayText = "Status",
                Value = "Status",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode\r\nfrom dbo.TblActivateCustomer cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID where lower(c.CustomerCode) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            //html.isShowLoadingGrid = false;
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridProcedureName = "SP_REPORT_CUSTOMER_CONNECTION_LIST";
            html.ReportTitle = "Report Customer Connection";
            //html.GH.SQL = sql;
            //html.OrderBy = "ConntionTypeID, InvoiceNo, ClassName, CustomerGroup, CID";
            //html.GridEventClick = "ShowPreviewCustInv(this)";
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Router Name", FieldName = "RouterName", IsGroupByField = true, GroupLabel="Router Name: ", Colspan=8 },
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", IsGroupByField = true, GroupLabel="Group: ", Colspan=8 },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "ConnectionID", FieldName = "ConnectionID" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
                //new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", },
                new GridField() { Header = "Status", FieldName = "Status", },
                new GridField() { Header = "Date", FieldName = "Date", },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Customer Connectivity List")]
        public ActionResult CustomerConnectivityList()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DateTimePickerWithRanges = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPackageName",
                Label = "Package Name:",
                DBField = "PackageName",
                SQL_CBO = "select distinct p.PackageID, p.PackageName from dbo.TblPackageDetail pd inner join dbo.TblPackages p on p.PackageID = pd.PackageID inner join dbo.TblActivateCustomer ac on ac.PackageDetailID = pd.PackageDetailID where ac.SystemID=" + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " order by p.PackageName",
                DisplayText = "PackageName",
                Value = "PackageID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBandwidth",
                Label = "Bandwidth:",
                DBField = "Bandwidth",
                SQL_CBO = "select distinct pd.Bandwidth from dbo.TblPackageDetail pd inner join dbo.TblPackages p on p.PackageID = pd.PackageID inner join dbo.TblActivateCustomer ac on ac.PackageDetailID = pd.PackageDetailID where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " order by pd.Bandwidth",
                DisplayText = "Bandwidth",
                Value = "Bandwidth",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroupID",
                SQL_CBO = "select distinct cg.CustomerGroupID, cg.CustomerGroup from dbo.TblCustomerGroup cg inner join dbo.TblCustomer c on c.CustomerGroupID = cg.CustomerGroupID inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID where c.SystemID = " + html.SystemID + " and c.WarehouseID =" + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtConnectivityStatus",
                Label = "Connectivity Status:",
                DBField = "ConnectivityStatus",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblActivateCustomer ac inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " order by s.[Status]",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtType",
                Label = "Type:",
                DBField = "InvoiceType",
                SQL_CBO = "select distinct case when bi.InvoiceType = 'Renew Invoice' then 'Renew' else 'New Connection' end[Type], bi.InvoiceType from dbo.TblActivateCustomer ac inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID where ac.SystemID=" + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " order by bi.InvoiceType",
                DisplayText = "Type",
                Value = "InvoiceType",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode\r\nfrom dbo.TblActivateCustomer cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID where lower(c.CustomerCode) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_REPORT_CUSTOMER_CONNETIVITY_LIST";
            html.ReportTitle = "Report Customer Connectivity List";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", IsGroupByField = true, Colspan = 12, GroupLabel="Customer Group: " },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone", FieldName = "CustomerContactMobileNumber" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Connection", FieldName = "ConnectionID" },
                //new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", TextAlign="right" },
                new GridField() { Header = "Duration", FieldName = "Duration", TextAlign="right", },
                new GridField() { Header = "Date", FieldName = "Date", },
                //new GridField() { Header = "End Date", FieldName = "EndDate", Format="dd/MM/yyyy" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
                new GridField() { Header = "Equipment", FieldName = "ItemName", Colspan=12,
                DataTableNumber = 3, DisplayFields="Customer, Bandwidth, Actived, Locked, Terminate", LinkField = "CustomerGroup"
                },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Customer Info List")]
        public ActionResult CustomerInfoList()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DateTimePickerWithRanges = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInstallCommune",
                Label = "Commune:",
                DBField = "InstallCommune",
                SQL_CBO = "select distinct InstallCommune from dbo.TblCustomerBillInformation where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by InstallCommune",
                DisplayText = "InstallCommune",
                Value = "InstallCommune",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInstallDistrict",
                Label = "District:",
                DBField = "InstallDistrict",
                SQL_CBO = "select distinct InstallDistrict from dbo.TblCustomerBillInformation where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by InstallDistrict",
                DisplayText = "InstallDistrict",
                Value = "InstallDistrict",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInstallProvinceCity",
                Label = "Province/City:",
                DBField = "InstallProvinceCity",
                SQL_CBO = "select distinct InstallProvinceCity from dbo.TblCustomerBillInformation where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by InstallProvinceCity",
                DisplayText = "InstallProvinceCity",
                Value = "InstallProvinceCity",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroupID",
                SQL_CBO = "select distinct cg.CustomerGroupID, cg.CustomerGroup from dbo.TblCustomerGroup cg inner join dbo.TblCustomer c on c.CustomerGroupID = cg.CustomerGroupID inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID where c.SystemID = " + html.SystemID + " and c.WarehouseID =" + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSaleName",
                Label = "Sale's Name:",
                DBField = "SaleName",
                SQL_CBO = "select distinct l.FullName from dbo.TblLeads l inner join dbo.TblCustomer c on c.EmployeeID = l.LeadID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by l.FullName",
                DisplayText = "FullName",
                Value = "FullName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode\r\nfrom dbo.TblActivateCustomer cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID where lower(c.CustomerCode) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridProcedureName = "SP_REPORT_CUSTOMER_INFO_LIST";
            html.ReportTitle = "Report Customer Information List";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", IsGroupByField= true, GroupLabel="Customer Group: ", DisplayOneRow = true, Colspan=12 },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", Format="dd/MM/yyyy" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "Address", FieldName = "InstallAddress" },
                new GridField() { Header = "Commune", FieldName = "InstallCommune", IsGroupByField = true, Colspan = 15, },
                new GridField() { Header = "District", FieldName = "InstallDistrict", IsGroupByField = true, Colspan = 15, },
                new GridField() { Header = "Province/City", FieldName = "InstallProvinceCity", IsGroupByField = true, Colspan = 15, },
                new GridField() { Header = "Sale's Name", FieldName = "SaleName" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", TextAlign="right" },
                new GridField() { Header = "Duration", FieldName = "Duration", TextAlign="right" },
                new GridField() { Header = "Commune", FieldName = "InstallCommune", IsHiddenField = true },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center",
                DataTableNumber = 3, DisplayFields="Customer", LinkField="InstallCommune"
                },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Report Customer Active Bandwidth")]
        public ActionResult ReportCustomerActiveBandwidth()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "Status",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, Status from dbo.TblStatus where StatusID in (40,41,42,43)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = @"select distinct top 10 ci.CustomerCode
from dbo.TblCustomer c where c.ClassName = 'NSM';",
                RadioValue = EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.ReportTitle = "Report Customer Active Bandwidth";
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-center"},
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-primary text-center"},
                new GridField() { Header = "Package Name", FieldName = "PackageName", TotalLabel="Total:"},
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", ClassName="text-right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Unit", FieldName = "Unit" },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType" },
                new GridField() { Header = "Province/City", FieldName = "Location" },
                new GridField() { Header = "Data Usage", FieldName = "DataUsage" },
            };
            html.ShowPaging = false;
            var kendoSetting = new KendoGridSetting
            {
                ProcedureName = "SP_NSM_CUSTOMER_INFO_LIST",
                ServerPaging = true,
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List Report Customer Active")]
        public ActionResult ReportCustomerActive()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "Status",
                Label = "Status:",
                DBField = "[Status (Active or Deactive)]",
                SQL_CBO = "select 'Actived' [Status (Active or Deactive)] union all select 'Terminate' [Status (Active or Deactive)] union all select 'Locked' [Status (Active or Deactive)]",
                DisplayText = "Status (Active or Deactive)",
                Value = "Status (Active or Deactive)",
                ComboBox = true,
                Hide = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "ConnectionType",
                Label = "Connection Type:",
                DBField = "[Connection Type]",
                SQL_CBO = "select 'FTTH' [Connection Type] union all select 'FTTB' [Connection Type]",
                DisplayText = "Connection Type",
                Value = "Connection Type",
                ComboBox = true,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = @"select distinct top 10 ci.CustomerCode [Customer ID]
from dbo.TblCustomer c 
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
where lower(ci.CustomerCode) like @SearchText and isnull(ci.[SID], '') <> '' and ci.StatusID not in(12,13,14,20) and c.DomainID = '001,002' and ci.DomainID = '001,002' and ci.[Type] ='Official Invoice' and format(ci.InvoiceDate,'yyyy-MM-dd') >= '2024-01-01'
order by ci.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("[Customer ID]") + "," + EncryAndDecry.Encryp("Name"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = @"with FilterCustName as (
	select distinct top 10 upper(replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then 'Mr.' + isnull(c.CustomerNameLatin, '')
	when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then 'Ms.' + isnull(c.CustomerNameLatin, '')
	when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(ci.CustomerName, 'Mr. ', ''), 'Mr.','')
	when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
	when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Other' then isnull(c.CustomerNameLatin, '')
	else ci.CustomerName end, ' ', '')),',',' ')) Name
from dbo.TblCustomer c 
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
where isnull(ci.[SID], '') <> '' and ci.StatusID not in(12,13,14,20) and c.DomainID = '001,002' and ci.DomainID = '001,002' and ci.[Type] ='Official Invoice' and format(ci.InvoiceDate,'yyyy-MM-dd') >= '2024-01-01'
)
select * from FilterCustName
where lower(Name) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("[Customer ID]") + "," + EncryAndDecry.Encryp("Name"),
                ISAutoComplete = true,
            });
            //html.GH.GridProcedureName = "SP_REPORT_CUSTOMER_INFO_LIST";
            string sql = @"
with TotalBandwidthCustActive as (
	select ROW_NUMBER() over (order by ci.[SID]) as [No],
	case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end 'Register Date (DD-MMM-YYYY)'
	, isnull((select top 1 format(ac.CreateDate, 'dd-MMM-yyyy') from dbo.TblActivateCustomer ac where ac.CustomerID = ci.CustomerID and ac.SID = ci.SID),
	case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end) 'Register Account IDs',
	(select top 1 format(cci.StartDate, 'dd-MMM-yyyy') from dbo.TblCustomerInvoice cci where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.DomainID = '001,002' order by cci.CustomerInvoiceID asc) 'Bill Date (DD-MMM-YYYY)',
	ci.CustomerCode 'Customer ID', ci.[SID] 'Service ID/Account ID'
	, replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then 'Mr.' + isnull(c.CustomerNameLatin, '')
	when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then 'Ms.' + isnull(c.CustomerNameLatin, '')
	when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(ci.CustomerName, 'Mr. ', ''), 'Mr.','')
	when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
	when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Other' then isnull(c.CustomerNameLatin, '')
	else ci.CustomerName end, ' ', '')),',',' ') [Name], replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then N'លោក ' + replace(c.CustomerName, N'លោក','')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then N'អ្នកស្រី ' + replace(replace(c.CustomerName, N'លោកស្រី​',''), N'អ្នកស្រី','')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(c.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(c.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
else c.CustomerName end, ' ', '')),',',' ') CustomerName,
	--case when ci.TypeOfInvoice = 'CM' then 'Individual' else 'Firm' end 'Customer Type (Firm or Individual)',
	it.IndustryType 'Customer Type (Firm or Individual)',
	replace((select top 1 i.InstallCommune + ', ' + i.InstallDistrict + ', ' + i.InstallProvinceCity from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc),',',' ') 'Installation Address',
	(select top 1 replace(i.InstallProvinceCity, N'រាជធានីភ្នំពេញ', N'ភ្នំពេញ') from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) [Location]
	, isnull((select top 1 convert(varchar, isnull(cci.Bandwidth, 0)) from dbo.TblCustomerInvoice cci where cci.ServiceID = ci.ServiceID and cci.DomainID = '001,002' and  isnull(cci.Bandwidth, 0) <> 0 and cci.CustomerID = ci.CustomerID and cci.ConnectionID = ci.ConnectionID order by cci.CustomerInvoiceID desc),0) Bandwidth, 'Mbps' Unit
	, isnull((select top 1 (pd.PackageName + ' - ' + cci.UM) from dbo.TblCustomerInvoice cci inner join dbo.TblPackageDetail pd on pd.PackageDetailID = cci.PackageDetailID where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.[Type] ='Official Invoice' order by cci.CustomerInvoiceID desc), (select top 1 s.ServiceName from dbo.TblServices s where s.ServiceID = ci.ServiceID)) PackageName
	, case when ci.ServiceID = 1 then case when c.CustomerGroup = 'SME' then 'FTTB' else 'FTTH' end else '' end 'Connection Type'
	, 'Unlimited' 'Data Usage (Limited or Unlimited)', ci.CustomerID
	, isnull((select top 1 s.[Status] from dbo.TblActivateCustomer ac inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus where ac.CustomerID = ci.CustomerID and ac.[SID] = ci.[SID]), 'Actived') 'Status (Active or Deactive)'
	, isnull((select top 1 case when isnull(ac.LockedDate,'') <> '' and ac.ConnectivityStatus <> 40 then format(ac.LockedDate, 'dd-MMM-yyyy') else '' end from dbo.TblActivateCustomer ac where ac.CustomerID = ci.CustomerID and ac.[SID] = ci.[SID]), '') 'Deactivate Date (DD-MMM-YYYY)', (select top 1 format(cci.EndDate, 'dd-MMM-yyyy') from dbo.TblCustomerInvoice cci where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.[Type] ='Official Invoice' order by cci.CustomerInvoiceID desc) as ExpireDate, 
    (SELECT TOP 1 
    CASE 
        WHEN DATEDIFF(day, GETDATE(), cci.EndDate) < 0 THEN 'Expired'
        WHEN DATEDIFF(day, GETDATE(), cci.EndDate) = 0 THEN 'Today'
        WHEN DATEDIFF(day, GETDATE(), cci.EndDate) < 30 THEN 
            CAST(DATEDIFF(day, GETDATE(), cci.EndDate) AS VARCHAR) + ' Day(s)'
        ELSE 
            -- Calculate full months
            CAST(DATEDIFF(MONTH, GETDATE(), cci.EndDate) - 
                 CASE WHEN DAY(cci.EndDate) < DAY(GETDATE()) THEN 1 ELSE 0 END AS VARCHAR) + ' Month(s) ' +
            -- Then calculate remaining days
            CAST(DATEDIFF(DAY, DATEADD(MONTH, DATEDIFF(MONTH, GETDATE(), cci.EndDate) - 
                 CASE WHEN DAY(cci.EndDate) < DAY(GETDATE()) THEN 1 ELSE 0 END, GETDATE()), cci.EndDate) AS VARCHAR) + ' Day(s)'
    END
 FROM dbo.TblCustomerInvoice cci 
 WHERE cci.CustomerID = ci.CustomerID AND cci.SID = ci.SID AND cci.[Type] = 'Official Invoice'
 ORDER BY cci.CustomerInvoiceID DESC) AS ExpireDaysPeriod
	from dbo.TblCustomerInvoice ci
	inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
	inner join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
	where isnull(ci.[SID], '') <> '' and ci.StatusID not in(12,13,14,20) and c.DomainID = '001,002' and ci.DomainID = '001,002' and ci.[Type] ='Official Invoice' and format(ci.InvoiceDate,'yyyy-MM-dd') >= '2024-01-01'
	group by ci.CustomerID, 
	case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end
	, ci.CustomerCode, ci.[SID]
	, replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then 'Mr.' + isnull(c.CustomerNameLatin, '')
	when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then 'Ms.' + isnull(c.CustomerNameLatin, '')
	when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(ci.CustomerName, 'Mr. ', ''), 'Mr.','')
	when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
	when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Other' then isnull(c.CustomerNameLatin, '')
	else ci.CustomerName end, ' ', '')),',',' '), replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then N'លោក ' + replace(c.CustomerName, N'លោក','')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then N'អ្នកស្រី ' + replace(replace(c.CustomerName, N'លោកស្រី​',''), N'អ្នកស្រី','')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(c.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(c.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
else c.CustomerName end, ' ', '')),',',' '), ci.TypeOfInvoice, ci.ConnectionID, ci.ServiceID, c.CustomerGroup, it.IndustryType
)
select [No],CustomerID, [Register Date (DD-MMM-YYYY)] RegisterDate,  [Customer ID] CID, [Service ID/Account ID] SID, Name, Bandwidth, Unit, PackageName, [Connection Type] ConnectionType, CustomerName, ExpireDate,ExpireDaysPeriod,
[Data Usage (Limited or Unlimited)] DataUsage, case when [Status (Active or Deactive)] = 'Actived' then '<span class=""text-primary"">Actived</span>' when [Status (Active or Deactive)] = 'Terminate' then '<span class=""text-danger"">Terminate</span>' else '<span class=""text-warning"">Locked</span>' end Status,
[Installation Address] Address
from TotalBandwidthCustActive
where [Status (Active or Deactive)] = 'Actived'
";
            html.ReportTitle = "Report Customer Active Bandwidth";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            //html.GH.GridPara = new GridPara[] {
            //  new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
            //  new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
            //  new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            //};
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-center"},
                new GridField() { Header = "CID", FieldName = "CID", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Customer Name KH", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Customer Name Latin", FieldName = "Name", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-primary text-center"},
                new GridField() { Header = "Package Name", FieldName = "PackageName", TotalLabel="Total:"},
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", ClassName="text-right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Unit", FieldName = "Unit" },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType", ClassName="text-center"},
                new GridField() { Header = "Expire Date", FieldName = "ExpireDate" },
                new GridField() { Header = "Expire Days", FieldName = "ExpireDaysPeriod" },
                new GridField() { Header = "Address", FieldName = "Address" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "List Report Customer Identification")]
        public ActionResult ReportCustomerIdentification()
        {
            HTML html = new HTML();
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Expire Date:",
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = @"select distinct top 10 c.CustomerCode
from dbo.TblCustomer c where lower(c.CustomerCode) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Name KH",
                SQL = @"select distinct top 10 c.CustomerName
from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Name Latin",
                SQL = @"select distinct top 10 c.CustomerNameLatin
from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });

            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Identification Number",
                SQL = @"select distinct top 10 c.CustomerIdentification
from dbo.TblCustomer c​​​ where lower(c.CustomerIdentification) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
            });

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "Customer Name KH", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Customer Name Latin", FieldName = "CustomerNameLatin", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "Customer Phone", FieldName = "CustomerMobile" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-center text-primary", FormatDateTime="dd-MM-yyyy"},
                new GridField() { Header = "Customer Identification", FieldName = "CustomerIdentification", ClassName="text-center" },
                new GridField() { Header = "Identification Expire Date", FieldName = "IdentificationExpireDate", ClassName="text-center" },
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-center text-danger" },
            };
            html.ShowPaging = false;
            html.TblID = "tblCustomerIdentification";
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                HeaderReportNameExcel = "Report Customer Identification",
                ServerPaging = true,
                ProcedureName = "SP_CUSTOMER_IDENTIFICATION"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        // ───────────── form action (the data) ─────────────
        public ActionResult ReportARDailyForm(int ClassID, DateTime? from = null, DateTime? to = null, string title = "Daily")
        {
            var html = new HTML();

            int systemId, warehouseId;
            if (!int.TryParse(html.SystemID, out systemId) ||
                !int.TryParse(html.WarehouseID, out warehouseId))
                return new HttpStatusCodeResult(400, "The current system or warehouse could not be resolved.");

            var scope = new ReportScope { SystemId = systemId, WarehouseId = warehouseId };

            var fromDate = (from ?? DateTime.Today).Date;
            var toDate = (to ?? fromDate).Date;

            using (var db = new DBContext())
            using (var services = new ReportServices(scope))
            {
                var classInfo = db.TblClass.AsNoTracking().FirstOrDefault(c => c.ClassID == ClassID);
                var className = classInfo == null ? null : classInfo.ClassName;
                  
                var company = db.TblCompany.AsNoTracking()
                    .FirstOrDefault(c => c.SystemID == systemId && c.ISDefault == true);

                var filter = new ReportFilter { Class = className, From = fromDate, To = toDate };

                ViewBag.ARData = services.Receivable.DailyIncome(filter).ToList();
                ViewBag.CompanyInfo = company;
                ViewBag.ClassInfo = classInfo;
                ViewBag.ReportTitle = title;        // Daily | Weekly | Monthly
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
            }

            return View();
        }

        // ───────────── render to PDF ─────────────
        public async Task<SqlResult> RenderReportAR(int ClassID, string ClassName,
                                                    DateTime from, DateTime to, string title)
        {
            var pdfResult = new SqlResult();
            try
            {
                // fills ViewBag; stop if the scope could not be resolved
                var formResult = ReportARDailyForm(ClassID, from, to, title) as HttpStatusCodeResult;
                if (formResult != null)
                {
                    pdfResult.Result = ResultType.Failed;
                    pdfResult.Message = formResult.StatusDescription;
                    return pdfResult;
                }

                string htmlContent = "";
                var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
                using (var sw = new StringWriter())
                {
                    var routeData = new RouteData();
                    routeData.Values.Add("controller", "Report");
                    routeData.Values.Add("action", "ReportARDailyForm");
                    routeData.Values.Add("ClassID", ClassID);
                    this.ControllerContext = new ControllerContext(httpContext, routeData, this);
                    var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, "ReportARDailyForm");
                    var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    htmlContent = sw.ToString();
                }

                string templatePath = httpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
                string template = System.IO.File.ReadAllText(templatePath);
                string finalHtml = template
                    .Replace("{Title}", $"Report Income {title} {ClassName}")
                    .Replace("{BodyMessage}", htmlContent);

                var htmlHelper = new HTML();
                pdfResult = await htmlHelper.HTMLToPDF(finalHtml, "Uploads/Reports/",
                    $"{(ClassName == "NSM" ? ClassName : "ISP")}_ReportIncome{title}.pdf");

                return pdfResult;
            }
            catch (Exception ex)
            {
                pdfResult.Result = ResultType.Failed;
                pdfResult.Message = ex.Message;
                return pdfResult;
            }
        }

        public ActionResult CustomerInvoiceDailyAlertForm()
        {
            var html = new HTML();
            var parameters = new Dictionary<string, string>
            {
                { "@SystemID", "13" },
                { "@WarehouseID", "16" },
                { "@DomainID", "001" },
                { "@StatusID", "11" },
                { "@FromDate", "2020-01-01" },
                // { "@FromDate", "2026-05-12" },
                { "@ToDate", DateTime.Now.ToString("yyyy-MM-dd") },
                { "@Page", "1" },
                { "@RecsPerPage", "500" }
            };

            string sql = "SP_REPORT_CUSTOMER_INVOICE_UNPAID";
            var ds = html.LoadData(sql, true, parameters);
            ViewBag.ARData = ds != null && ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();
            ViewBag.TargetClassName = "";
            return View();
        }

        public async Task<SqlResult> RenderCustomerInvoiceDailyAlert()
        {
            var pdfResult = new SqlResult();
            try
            {
                CustomerInvoiceDailyAlertForm();

                var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
                string templatePath = httpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
                string template = System.IO.File.ReadAllText(templatePath);
                var htmlHelper = new HTML();

                var sourceData = ViewBag.ARData as System.Data.DataTable ?? new System.Data.DataTable();

                var outputFiles = new List<string>();
                var captions = new List<string>();

                // Always generate 2 reports: NSM (Non-ISP) first, NSM ISP second
                var categories = new[] { "NSM", "NSM ISP" };

                foreach (var cat in categories)
                {
                    bool isISP = cat == "NSM ISP";
                    ViewBag.TargetClassName = cat;

                    var classData = sourceData.Clone();
                    foreach (var row in sourceData.AsEnumerable())
                    {
                        var rowClass = (row.Table.Columns.Contains("ClassName") && row["ClassName"] != DBNull.Value)
                            ? row["ClassName"].ToString().Trim()
                            : "";
                        bool rowIsISP = rowClass.IndexOf("ISP", StringComparison.OrdinalIgnoreCase) >= 0;

                        if ((isISP && rowIsISP) || (!isISP && !rowIsISP))
                        {
                            classData.ImportRow(row);
                        }
                    }
                    ViewBag.ARData = classData;

                    string htmlContent;
                    using (var sw = new StringWriter())
                    {
                        var routeData = new RouteData();
                        routeData.Values.Add("controller", "Report");
                        routeData.Values.Add("action", "CustomerInvoiceDailyAlertForm");
                        this.ControllerContext = new ControllerContext(httpContext, routeData, this);
                        var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, "CustomerInvoiceDailyAlertForm");
                        var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
                        viewResult.View.Render(viewContext, sw);
                        htmlContent = sw.ToString();
                    }

                    string reportClassName = isISP ? "NSM ISP" : "NSM";
                    string title = "Receivables Report of " + reportClassName;
                    string finalHtml = template.Replace("{Title}", title).Replace("{BodyMessage}", htmlContent);
                    string fileName = isISP ? "ReportReceivableNSMISP" : "ReportReceivableNSM";

                    var classPdfResult = await htmlHelper.HTMLToPDF(finalHtml, "Uploads/Reports/", fileName, pageSize: "A4Landscape");
                    if (classPdfResult.Result != ResultType.Successful)
                    {
                        return classPdfResult;
                    }

                    outputFiles.Add(classPdfResult.OutPutFile);
                    captions.Add(title);
                }

                pdfResult.Result = ResultType.Successful;
                pdfResult.Message = "Html to PDF file successful.";
                pdfResult.OutPutFile = string.Join("~", outputFiles);
                pdfResult.Caption = string.Join("~", captions);

                return pdfResult;
            }
            catch (Exception ex)
            {
                pdfResult.Result = ResultType.Failed;
                pdfResult.Message = ex.Message;
                return pdfResult;
            }
        }


    }
}
