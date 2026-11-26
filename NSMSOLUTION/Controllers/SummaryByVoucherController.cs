using MVC;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class SummaryByVoucherController : Controller
    {
        public ActionResult Index()
        {

            HTML html = new HTML();

            //html.TableName = "TblPurchaseOrder";
            //html.PrimaryField = "PurchaseOrderID";
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "PO Date:",
                DBField = "po.PODate"

            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "PO Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPONumber",
                ISAutoComplete = true,
                Label = "PO Number:",
                DBField = "PONumber",
                SQLAutoComplete = "select distinct top 10 PONumber from dbo.TblPurchaseOrder where lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PONumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorCode",
                ISAutoComplete = true,
                Label = "Vendor Code:",
                DBField = "v.VendorCode",
                SQLAutoComplete = "select distinct top 10 v.VendorCode from dbo.TblPurchaseOrder po inner join dbo.TblVendor v on v.VendorID = po.VendorID where lower(v.VendorCode) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + "%' order by v.VendorCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorName",
                ISAutoComplete = true,
                Label = "Vendor Name:",
                DBField = "v.VendorName",
                SQLAutoComplete = "select distinct top 10 v.VendorName from dbo.TblPurchaseOrder po inner join dbo.TblVendor v on v.VendorID = po.VendorID where lower(v.VendorName) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + "%' order by v.VendorName",
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct po.ClassID, c.ClassName from dbo.TblPurchaseOrder po
inner join dbo.TblClass c on c.ClassID = po.ClassID where po.SystemID = " + html.SystemID + " and po.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("po.ClassID"),
                DefaultCheckBok = true,
            });

            string sql = @"select ROW_NUMBER() Over(order by po.ClassID, po.PONumber desc) as [No], po.PurchaseOrderID, format(po.PODate, 'dd-MM-yyyy') [PODate], '<a class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowPOInvoice('+convert(varchar, v.VendorID)+', '+convert(varchar, po.PurchaseOrderID)+', '''+po.PONumber +''')"">'+po.PONumber +'</a>' [PONumber] , po.Reference,
                            v.VendorID, 
                            '<a href=""#!/Profile?ID='+convert(varchar, v.VendorID)+'"" class=""text-primary"" target=""_blank"">' + v.VendorName + '</a>' VendorName, 

                            po.DeliveryDate,
                             po.DeliverTo, 
                             po.SubTotal,
                             po.VAT,
                             po.DiscountUSD,
                             po.GrandTotal,
                            case when (po.StatusID = 60 or po.StatusID = 91) then '<span class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] + '</span>' when (po.StatusID = 15 or po.StatusID = 66) then '<span class=""badge badge-pill badge-warning text-white text-uppercase"">' + s.[Status] + '</span>' when po.StatusID = 92 then '<span class=""badge badge-pill text-uppercase badge-success"">' + s.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-primary"">' + s.[Status] + '</span>' + case when isnull(po.IsClose, 0) = 0 then '' else '<br><span class=""badge badge-pill text-uppercase badge-danger"">Closed</span>' end + case when isnull(po.IsCancel, 0) = 0 then '' else '<br><span class=""badge badge-pill text-uppercase badge-warning text-white"">Cancel</span>' end end [Status], 
                            c.ClassName
                            from dbo.TblPurchaseOrder po
                            inner join dbo.TblVendor v on v.VendorID = po.VendorID
                            inner join dbo.TblStatus s on s.StatusID = po.StatusID
                            inner join dbo.TblClass c on c.ClassID = po.ClassID
                            where po.Type = 'PurchaseOrder'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="po.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="po.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="po.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            //html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "PO. Date", FieldName = "PODate", TextAlign="center" },
                new GridField() { Header = "PO. #", FieldName = "PONumber", },
                new GridField() { Header = "Vendor", FieldName = "VendorName", TotalLabel="Total:"},
                 new GridField() { Header = "SubTotal", FieldName = "SubTotal", ClassName="text-right text-danger", Format="#,#0.00" , IsTotalFooter = true},
                new GridField() { Header = "Discount", FieldName = "DiscountUSD", ClassName="text-right text-primary", Format="#,#0.00" , IsTotalFooter = true},
                new GridField() { Header = "Vat", FieldName = "VAT", ClassName="text-right text-danger", Format="#,#0.00" , IsTotalFooter = true},
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right text-primary", Format="#,#0.00" , IsTotalFooter = true},

            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}