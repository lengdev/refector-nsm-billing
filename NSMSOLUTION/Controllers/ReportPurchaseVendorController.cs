using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;


public class ReportPurchaseVendorController : Controller
{
    public ActionResult Index()
    {
        HTML html = new HTML();


        //html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
        //{
        //    ID = "TxtFromDate",
        //    DefaultValueDate = false,
        //    DateRangePicker = true,
        //    LabelFromDate = "PO Date:",

        //});

        html.HtmlAddControls.Add(new HtmlAddControls()
        {
            ID = "txtVendorType",
            Label = "Vendor Type:",
            DBField = "VendorType",
            SQL_CBO = "select distinct VendorType from dbo.TblVendor where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorType",
            DisplayText = "VendorType",
            Value = "VendorType",
            ComboBox = true
        });
        List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
        RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
        {
            OPTLabel = "Vendor Code",
            SQL = "select distinct top 10 VendorCode from dbo.TblVendor where lower(VendorCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorCode",
            //SelectValue = "VendorID",
            DisplayField = "VendorName",
            ISAutoComplete = true,
            RadioValue = EncryAndDecry.Encryp("VendorCode") + "," + EncryAndDecry.Encryp("VendorName"),
            DefaultCheckBok = true
        };
        htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
        radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
        {

            OPTLabel = "Vendor Name",
            SQL = "select distinct top 10 VendorName from dbo.TblVendor where lower(VendorName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorName",
            ISAutoComplete = true,
            RadioValue = EncryAndDecry.Encryp("VendorCode") + "," + EncryAndDecry.Encryp("VendorName"),
        };
        htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);


        html.ReportTitle = "Purchase Order Detail";
        KendoGridSetting kendoGridSetting = new KendoGridSetting
        {
            ServerPaging = true,
            ProcedureName = "SP_VENDOR_INFO",
            ProcedureName2 = "SP_PURCHASE_ORDER_DETAIL",
        };
        html.IsHaveWhereCondition = true;
        html.GH.GridTable = new GridField[]
        {

                new GridField() { Header = "No.", FieldName = "No",  },
                new GridField() { Header = "Vendor ID", FieldName = "VendorID",  IsHiddenField=true,  },
                new GridField() { Header = "Vendor Code", FieldName = "VendorCode" },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName",  Width="300px", ClassName="truncate-cell"},
                new GridField() { Header = "Company Name", FieldName = "CompanyName" },
                new GridField() { Header = "Vendor Type", FieldName = "VendorType" },
                new GridField() { Header = "Contact Phone", FieldName = "ContactPhoneNumber" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
        };

        html.GH.GridTable2 = new GridField[]
        {
                new GridField() { Header = "No.", FieldName = "No" },
                //new GridField() { Header = "Purchase Order ID", FieldName = "PurchaseOrderID" },
                new GridField() { Header = "PO Number.", FieldName = "PONumber", Width="170px", ClassName="truncate-cell" },
                new GridField() { Header = "PO Date", FieldName = "PODate" , Width="170px", ClassName="truncate-cell"},
                new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate", Width="170px", ClassName="truncate-cell" },
                new GridField() { Header = "Deliver To", FieldName = "DeliverTo" , Width="300px", ClassName="truncate-cell"},              
                //new GridField() { Header = "Customer Notes", FieldName = "CustomerNotes" , Width="300px", ClassName="truncate-cell"},
                new GridField() { Header = "Order Qty", FieldName = "TotalOrderQty" },
                new GridField() { Header = "Received Qty", FieldName = "TotalReceivedQty" },
                new GridField() { Header = "GrandTotal", FieldName = "GrandTotal" },
                new GridField() { Header = "Deposit", FieldName = "Deposit" },
                new GridField() { Header = "CreateBy", FieldName = "CreateBy"    , Width="150px", ClassName="truncate-cell"},

        };

        html.OrderBy = "[No]";
        html.ShowPaging = false;
        ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString(),
            html.GH.GridTable2.ToList().ToJsonString(), LinkField: "VendorID");

        return View();
    }



}
