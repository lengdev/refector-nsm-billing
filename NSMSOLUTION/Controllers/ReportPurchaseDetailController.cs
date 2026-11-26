using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;


public class ReportPurchaseDetailController : Controller
{
    public ActionResult Index()
    {
        HTML html = new HTML
        {
            ReportTitle = "Purchase  Detail"
        };

        //html.OrderBy = "PurchaseOrderID desc";
        html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
        {
            DefaultValueDate = false,
            DateRangePicker = true,
            LabelFromDate = "PO Date:",
            //DBField = "po.PODate"
        });
        html.HtmlAddControls.Add(new HtmlAddControls()
        {
            ID = "txtPONumber",
            ISAutoComplete = true,
            Label = "PO Number:",
            DBField = "PONumber",
            SQLAutoComplete = "select distinct top 10 PONumber from dbo.TblPurchaseOrder where lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PONumber"
        });
        //html.HtmlAddControls.Add(new HtmlAddControls()
        //{
        //    ID = "txtStatusID",
        //    Label = "PO Status:",
        //    DBField = "s.StatusID",
        //    SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
        //    DisplayText = "Status",
        //    Value = "StatusID",
        //    ComboBox = true
        //});
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


        KendoGridSetting kendoGridSetting = new KendoGridSetting
        {
            ServerPaging = true,
            ProcedureName = "SP_PURCHASE_ORDER_INFO",
            ProcedureName2 = "SP_ITEM_PURCHASE_DETAIL",
        };
        html.IsHaveWhereCondition = true;
        html.GH.GridTable = new GridField[]
        {

                new GridField() { Header = "No.", FieldName = "No",  },
                new GridField() { Header = "Purchase Order ID", FieldName = "PurchaseOrderID",  IsHiddenField=true,  },
                 new GridField() { Header = "PO Date", FieldName = "PODate" },
                  new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate" },
                   new GridField() { Header = "Deliver To", FieldName = "DeliverTo" },
                new GridField() { Header = "PO Number", FieldName = "PONumber" },
                 new GridField() { Header = "Vendor Name", FieldName = "VendorName" },
                  new GridField() { Header = "Amount", FieldName = "Amount" },
                   new GridField() { Header = "Total Order", FieldName = "TotalOrderQty" },
                    new GridField() { Header = "Total Received", FieldName = "TotalReceivedQty" },
        };

        html.GH.GridTable2 = new GridField[]
        {
                new GridField() { Header = "No.", FieldName = "No" },
                //new GridField() { Header = "PO Number.", FieldName = "PONumber", Width="170px", ClassName="truncate-cell" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" , Width="350px", ClassName="truncate-cell"},
                new GridField() { Header = "Item Description", FieldName = "ItemDescription", Width="300px", ClassName="truncate-cell" },
                new GridField() { Header = "UM", FieldName = "UM" },
                new GridField() { Header = "QTY", FieldName = "QTY" },
                new GridField() { Header = "Sale Price", FieldName = "SalePrice" },
                new GridField() { Header = "Amount", FieldName = "Amount" },
                new GridField() { Header = "UM", FieldName = "UM" },
                //new GridField() { Header = "DeliverTo", FieldName = "DeliverTo",Width="230px", ClassName="truncate-cell" },
                //new GridField() { Header = "Request By", FieldName = "RequestBy"  ,Width="170px", ClassName="truncate-cell" },              
            

        };

        html.OrderBy = "[No]";
        html.ShowPaging = false;
        ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString(),
            html.GH.GridTable2.ToList().ToJsonString(), LinkField: "PurchaseOrderID");

        return View();
    }
}
