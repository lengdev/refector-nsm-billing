using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.Purchases
{
    [Authorize]
    public class PurchaseController : Controller
    {
        // GET: Purchase
        public ActionResult PurchaseRequest()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Purchase Request";
            if (html.HasPermission("Add Purchase Request"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Purchase Request"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Purchase Request"))
                html.AllowDelete = true;

            string sql = @"select ROW_NUMBER() Over(order by po.PurchaseOrderID desc) as [No], po.PurchaseOrderID, Format(po.PRDate, 'dd-MM-yyyy') PRDate,'<a class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowPRInvoice('+convert(varchar, v.VendorID)+', '+convert(varchar, po.PurchaseOrderID)+', '''+po.PRNumber+''')"">'+po.PRNumber+'</a>' PRNumber, po.Reference,
v.VendorID, '<a class=""text-primary"" href=""#!/Profile?ID='+convert(varchar, v.VendorID)+'"" target=""_blank"">' + v.VendorName + '</a>' VendorName, format(po.GrandTotal, '#,#.00') Amount, po.DeliveryDate, po.DeliverTo, 
--case when po.StatusID = 66 then '<a>Request Revide</a> ' else '' end + 
case when (po.StatusID = 60 or po.StatusID = 91) then '<span class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] + '</span>' when (po.StatusID = 15 or po.StatusID = 66) then '<span class=""badge badge-pill badge-warning text-white text-uppercase"">' + s.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-primary"">' + s.[Status] + '</span>' end [Status], 
po.StatusID,
po.Deposit, po.Balance, isnull(po.RemainAmount, 0) RemainAmount, '<span class=""badge badge-danger"">' + isnull(po.PaymentStatus, 'Pending') + '</span>' PaymentStatus
from dbo.TblPurchaseOrder po
inner join dbo.TblVendor v on v.VendorID = po.VendorID
inner join dbo.TblStatus s on s.StatusID = po.StatusID
where po.Type = 'PurchaseRequest'";
            html.OrderBy = "po.PurchaseOrderID desc";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="po.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="po.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="po.DomainID", Value =html.DomainID },
            };
            html.GridEventClick = "onGridClick(this);";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                //new GridField() { Header = "PurchaseOrderID", FieldName = "PurchaseOrderID", IsHiddenField= true },
                new GridField() { Header = "PR. Date", FieldName = "PRDate", TextAlign="center" },
                new GridField() { Header = "PR. Number#", FieldName = "PRNumber", },
				//new GridField() { Header = "DeliverTo", FieldName = "DeliverTo", GroupLabel="Deliver To: ", IsGroupByField = true, DisplayOneRow= true, Colspan = 10 },
				new GridField() { Header = "Vendor", FieldName = "VendorName", },
                new GridField() { Header = "Purchase Status", FieldName = "Status", TextAlign ="center" },
				//new GridField() { Header = "Reference#", FieldName = "Reference", },
                //new GridField() { Header = "Vendor Code", FieldName = "VendorCode", },
				new GridField() { Header = "Deposit", FieldName = "Deposit", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Total", FieldName = "Amount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Balance", FieldName = "RemainAmount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                //new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign = "center" },
				//new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate", Format="dd-MM-yy", TextAlign="center" },				
				new GridField() { Header = "Action", FieldName = "PurchaseOrderID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblPurchaseOrderDetail, TblBills",
                    HideEditOnActionNameNotMatch = AdminEdit==false,
                    ShowActionNameOnConditionField ="StatusID",
                    ShowActionNameOnConditionValue="60",
                    HideDeteleOnNotMatch=true
                }
            };

            //html.ShowGridOnly= true;
            html.AddButton = "Add New";
            html.AddURL = "Purchase/CreatePurchaseRequest";
            html.ShowSearchButton = false;
            html.DialogBoxWidth = "auto";
            KendoGridSetting kendoGridSetting = new KendoGridSetting() { filterable = true };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());

            return View();
        }
        public ActionResult AuthorizePurchaseRequest()
        {
            HTML html = new HTML
            {
                //html.DialogBoxWidth = "Auto";
                ShowPaging = false
            };
            string sql = @"select ROW_NUMBER() Over(order by po.PurchaseOrderID desc) as [No], po.PurchaseOrderID, format(po.PRDate, 'dd-MM-yyyy') [PRDate], '<a class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowPRInvoice('+convert(varchar, v.VendorID)+', '+convert(varchar, po.PurchaseOrderID)+', '''+po.PRNumber +''')"">'+po.PRNumber +'</a>' [PRNumber] , po.Reference,
v.VendorID, '<a href=""#!/Profile?ID='+convert(varchar, v.VendorID)+'"" class=""text-primary"" target=""_blank"">' + v.VendorName + '</a>' VendorName, format(po.GrandTotal, '#,#.00') Amount, po.DeliveryDate, po.DeliverTo, 
--case when po.StatusID = 66 then '<a>Request Revide</a> ' else '' end + 
case when (po.StatusID = 60 or po.StatusID = 91) then '<span class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] + '</span>' when (po.StatusID = 15 or po.StatusID = 66) then '<span class=""badge badge-pill badge-warning text-white text-uppercase"">' + s.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-primary"">' + s.[Status] + '</span>' end [Status], 
po.StatusID,
po.Deposit, po.Balance, isnull(po.RemainAmount, 0) RemainAmount, '<span class=""badge badge-danger"">' + isnull(po.PaymentStatus, 'Pending') + '</span>' PaymentStatus
from dbo.TblPurchaseOrder po
inner join dbo.TblVendor v on v.VendorID = po.VendorID
inner join dbo.TblStatus s on s.StatusID = po.StatusID
where po.Type = 'PurchaseRequest'";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
                new GridPara() { ParaName="po.SystemID", Value =html.SystemID },
                new GridPara() { ParaName="po.WarehouseID", Value =html.WarehouseID },
                new GridPara() { ParaName="po.DomainID", Value =html.DomainID },
            };
            html.OrderBy = "po.PurchaseOrderID desc";
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                //new GridField() { Header = "PurchaseOrderID", FieldName = "PurchaseOrderID", IsHiddenField= true },
                new GridField() { Header = "PR. Date", FieldName = "PRDate", TextAlign="center" },
                new GridField() { Header = "PR. Number#", FieldName = "PRNumber", },
				//new GridField() { Header = "DeliverTo", FieldName = "DeliverTo", GroupLabel="Deliver To: ", IsGroupByField = true, DisplayOneRow= true, Colspan = 10 },
				new GridField() { Header = "Vendor", FieldName = "VendorName", },
                new GridField() { Header = "Purchase Status", FieldName = "Status", TextAlign ="center" },
				//new GridField() { Header = "Reference#", FieldName = "Reference", },
                //new GridField() { Header = "Vendor Code", FieldName = "VendorCode", },
				new GridField() { Header = "Deposit", FieldName = "Deposit", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Total", FieldName = "Amount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Balance", FieldName = "RemainAmount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                //new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign = "center" },
				//new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate", Format="dd-MM-yy", TextAlign="center" },				
				//new GridField() { Header = "Action", FieldName = "PurchaseOrderID", TextAlign = "Center",  IsModifyField = true,
    //                DeleteWithTableName="TblPurchaseOrderDetail, TblBills",
    //                HideEditOnActionNameNotMatch = (AdminEdit==false? true:false),
    //                ShowActionNameOnConditionField ="StatusID",
    //                ShowActionNameOnConditionValue="60",
    //                HideDeteleOnNotMatch=true
    //            }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult AuthorizePurchaseOrder()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            //html.ReportTitle = "List Purchase Order";
            //if (html.HasPermission("Add Purchase Order"))
            //    html.AllowAdd = true;
            //if (html.HasPermission("Edit Purchase Order"))
            //    html.AllowEdit = true;
            //if (html.HasPermission("Delete Purchase Order"))
            //    html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            //html.AddButton = "Add New";
            html.AddURL = "PurchaseOrder/CreatePurchaseOrder";
            string sql = @"select ROW_NUMBER() Over(order by po.PurchaseOrderID desc) as [No], po.PurchaseOrderID, format(po.PODate, 'dd-MM-yyyy') [PODate], '<a class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowPOInvoice('+convert(varchar, v.VendorID)+', '+convert(varchar, po.PurchaseOrderID)+', '''+po.PONumber +''')"">'+po.PONumber +'</a>' [PONumber] , po.Reference,
v.VendorID, '<a href=""#!/Profile?ID='+convert(varchar, v.VendorID)+'"" class=""text-primary"" target=""_blank"">' + v.VendorName + '</a>' VendorName, format(po.GrandTotal, '#,#.00') Amount, po.DeliveryDate, po.DeliverTo, 
--case when po.StatusID = 66 then '<a>Request Revide</a> ' else '' end + 
case when (po.StatusID = 60 or po.StatusID = 91) then '<span class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] + '</span>' when (po.StatusID = 15 or po.StatusID = 66) then '<span class=""badge badge-pill badge-warning text-white text-uppercase"">' + s.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-primary"">' + s.[Status] + '</span>' end [Status], 
po.StatusID, '<a class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowPRInvoice('+convert(varchar, v.VendorID)+', '+convert(varchar, po.RefID)+', '''+po.RefNumber+''')"">'+po.RefNumber+'</a>' RefNumber,
po.Deposit, po.Balance, isnull(po.RemainAmount, 0) RemainAmount, '<span class=""badge badge-danger"">' + isnull(po.PaymentStatus, 'Pending') + '</span>' PaymentStatus
from dbo.TblPurchaseOrder po
inner join dbo.TblVendor v on v.VendorID = po.VendorID
inner join dbo.TblStatus s on s.StatusID = po.StatusID
where po.Type = 'PurchaseOrder'";
            html.OrderBy = "po.PurchaseOrderID desc";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="po.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="po.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="po.DomainID", Value =html.DomainID },
            };
            html.ShowSearchButton = false;
            html.GH.SQL = sql;
            html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
				//new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
				//new GridField() { Header = "PurchaseOrderID", FieldName = "PurchaseOrderID", IsHiddenField= true },
				new GridField() { Header = "PO. Date", FieldName = "PODate", TextAlign="center" },
                new GridField() { Header = "PO. #", FieldName = "PONumber", },
                //new GridField() { Header = "Reference", FieldName = "RefNumber", },
				//new GridField() { Header = "DeliverTo", FieldName = "DeliverTo", GroupLabel="Deliver To: ", IsGroupByField = true, DisplayOneRow= true, Colspan = 10 },
				new GridField() { Header = "Vendor", FieldName = "VendorName", },
                new GridField() { Header = "Purchase Status", FieldName = "Status", TextAlign ="center" },
				//new GridField() { Header = "Reference#", FieldName = "Reference", },
                //new GridField() { Header = "Vendor Code", FieldName = "VendorCode", },
				new GridField() { Header = "Deposit", FieldName = "Deposit", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Total", FieldName = "Amount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Balance", FieldName = "RemainAmount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                //new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign = "center" },
				//new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate", Format="dd-MM-yy", TextAlign="center" },				
				//new GridField() { Header = "Action", FieldName = "PurchaseOrderID", TextAlign = "Center",  IsModifyField = true,
    //                DeleteWithTableName="TblPurchaseOrderDetail, TblBills",
    //                HideEditOnActionNameNotMatch = (AdminEdit==false? true:false),
    //                ShowActionNameOnConditionField ="StatusID",
    //                ShowActionNameOnConditionValue="60",
    //                HideDeteleOnNotMatch=true
    //            }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult EnterBill()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Bills"
            };
            if (html.HasPermission("Add Bills"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Bills"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Bills"))
                html.AllowDelete = true;

            string sql = @"SELECT ROW_NUMBER() Over(order by po.BillID desc) as [No],
    po.BillID,
    format(po.BillDate, 'dd-MM-yyyy') BillDate,
    '<a class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowBillInvoice('+convert(varchar, v.VendorID)+', '+convert(varchar, po.BillID)+', '''+po.BillNumber+''')"">'+po.BillNumber+'</a>' BillNumber,
    po.PONumber,
    v.VendorID,
    '<a class=""text-primary"" href=""#!/Profile?ID=' + CONVERT(VARCHAR, v.VendorID) + '"" target=""_blank"">' + (v.VendorCode + ' - ' + v.VendorName) + '</a>' [VendorName],
    po.GrandTotal AS Amount,
    po.Warehouse,
    CASE
        WHEN po.StatusID <> 10 THEN
            CASE
                WHEN DATEDIFF(d, po.PaymentDueDate, GETDATE()) > 0 THEN
                    '<span class=""badge badge-pill badge-danger text-uppercase"">Overdue by ' + CONVERT(VARCHAR, DATEDIFF(d, po.PaymentDueDate, GETDATE())) + ' Days </span>'
                WHEN po.StatusID = 11 THEN
                    '<div class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] + '</div>'
                WHEN po.StatusID = 14 THEN
                    '<div class=""badge badge-pill badge-primary text-uppercase"">' + s.[Status] + '</div>'
                WHEN po.StatusID = 15 THEN
                    '<div class=""badge badge-pill badge-warning text-white text-uppercase"">' + s.[Status] + '</div>'
            END
        ELSE
            '<div class=""badge badge-pill badge-primary text-uppercase"">' + s.[Status] + '</div>'
    END AS [Status],
		format(po.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate,
        po.VendorDONumber,
    po.VendorInvoiceNumber,
    po.StatusID,
    ISNULL(po.AmountPaid, 0) AS AmountPaid,
    ISNULL(po.AmountDue, 0) AS AmountDue,
    po.CreditNoteNumber
FROM dbo.TblBills po
INNER JOIN dbo.TblVendor v ON v.VendorID = po.VendorID
INNER JOIN dbo.TblStatus s ON s.StatusID = po.StatusID
";
            html.GH.SQL = sql;
            html.OrderBy = "po.BillID desc";
            //html.GH.GridProcedureName = "SP_SHOW_VENDOR_BILL";
            //html.TblID = "TblVendorBillBill";
            //html.TableName = "TblBills";
            //html.GridEventClick = "onGridVendorBill_Click(this);";
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "VendorName", FieldName = "VendorName", IsGroupByField = true, Colspan = 14 },
                new GridField() { Header = "Bill Number#", FieldName = "BillNumber", },
                new GridField() { Header = "Purchase Order#", FieldName = "PONumber", },
                new GridField() { Header = "VendorName", FieldName = "VendorName" },
                //new GridField() { Header = "BillID", FieldName = "BillID", IsHiddenField= true },
                //new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                //new GridField() { Header = "Vendor Code", FieldName = "VendorCode", },
                //new GridField() { Header = "Vendor Name", FieldName = "VendorName", },
                new GridField() { Header = "Vendor DO#", FieldName = "VendorDONumber", },
                new GridField() { Header = "Vendor Invoice#", FieldName = "VendorInvoiceNumber", },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Amount Paid", FieldName = "AmountPaid", TextAlign = "right", ClassName="text-info", Format="#,#0.00" },
                new GridField() { Header = "Amount Due", FieldName = "AmountDue", TextAlign = "right", ClassName="text-danger", Format="#,#0.00" },
                new GridField() { Header = "Bill Date", FieldName = "BillDate"},
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate"},
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
                new GridField() { Header = "Warehouse", FieldName = "Warehouse" },
                new GridField() { Header = "Action", FieldName = "BillID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblBillsDetail" }
            };
            html.AddButton = "Add New";
            html.AddURL = "Bills/CreateBill";
            html.ShowSearchButton = false;
            html.DialogBoxWidth = "auto";
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult GoodsReceived()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Goods Received";
            if (html.HasPermission("Add Goods Received"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Goods Received"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Goods Received"))
                html.AllowDelete = true;
            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "AddStockIn/StockIn";
            html.TableName = "TblStockIn";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Received Date:",
                DBField = "ReceiveDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPONumber",
                //ComboBox = true,
                Label = "PO Number:",
                DBField = "PONumber",
                ISAutoComplete = true,
                //SQL_CBO = "select distinct PONumber from dbo.TblStockIn where PONumber is not null and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PONumber",
                SQLAutoComplete = "select distinct top 10 PONumber from dbo.TblStockIn where lower(PONumber) like @SearchText and PONumber is not null and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PONumber",
                //DisplayText = "PONumber",
                //Value="PONumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Type:",
                DBField = "si.StatusID",
                SQL_CBO = "select distinct st.StatusID, st.Status from dbo.TblStockIn si inner join dbo.TblStatus st on st.StatusID = si.StatusID where st.StatusID in (70, 79, 83)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendor",
                Label = "Vendor:",
                DBField = "v.VendorName",
                //DBField = "si.VendorID",
                //SQL_CBO = "select distinct v.VendorID, v.VendorName from dbo.TblStockIn si left join dbo.TblVendor v on v.VendorID = si.VendorID where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + "%' order by v.VendorName",
                SQLAutoComplete = "select distinct top 10 v.VendorName from dbo.TblStockIn si left join dbo.TblVendor v on v.VendorID = si.VendorID where lower(v.VendorName) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + "%' order by v.VendorName",
                ISAutoComplete = true,
                //DisplayText = "VendorName",
                //Value = "VendorID",
                //ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Received Number",
                //SQL = @"select distinct StockInID, ReceiveNoteNumber from dbo.TblStockIn where ReceiveNoteNumber is not null and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                SQL = @"select distinct top 10 ReceiveNoteNumber from dbo.TblStockIn where lower(ReceiveNoteNumber) like @SearchText and ReceiveNoteNumber is not null and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                //DisplayField = "ReceiveNoteNumber",
                //SelectValue = "StockInID",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ReceiveNoteNumber"),
                DefaultCheckBok = true,
            });


            string sql = @"select ROW_NUMBER() Over(order by si.StockInID desc) as [No], si.StockInID, si.ReceiveDate, format(si.CreateDate, 'dd-MM-yyyy') CreateDate,
'<a href=""javascript:void(0);"" style=""cursor: pointer;"" onclick=""ShowInvoiceStockReceive('+convert(varchar, si.StockInID)+', '''+si.ReceiveNoteNumber +''')"">'+si.ReceiveNoteNumber +'</a>' ReceiveNoteNumber, si.PODate, 
'<a href=""javascript:void(0);"" style=""cursor: pointer;"" onclick=""ShowPOInvoice('+convert(varchar, si.VendorID)+', '+convert(varchar, si.PurchaseOrderID)+', '''+si.PONumber +''')"">'+ si.PONumber +'</a>' PONumber, 
'<a href=""#!/Profile?ID='+convert(varchar, si.VendorID)+'"" target=""_blank"">' + case when isnull(v.VendorCode, '') <> '' then v.VendorCode + '-' + v.VendorName else v.VendorName end + '</a>' VendorName, isnull((select sum(OrderQTY) from dbo.TblStockInDetail where StockInID = si.StockInID), 0) as TotalOrderQty, isnull((select sum(QTY) from dbo.TblStockInDetail where StockInID = si.StockInID), 0) as TotalReceivedQty, st.Status, isnull((select sum(QTYRemain) from dbo.TblStockInDetail where StockInID = si.StockInID), 0) as TotalQtyRemain
from dbo.TblStockIn si 
left join dbo.TblStatus st on st.StatusID = si.StatusID
left join dbo.TblVendor v on v.VendorID = si.VendorID
where si.Type in ('Opening Stock','Stock In')";
            html.OrderBy = "si.StockInID desc";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="si.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="si.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="si.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date In", FieldName = "ReceiveDate", TextAlign="center", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "Good Received No.", FieldName = "ReceiveNoteNumber", TextAlign="center" },
				//new GridField() { Header = "PO. Date", FieldName = "PODate", TextAlign="center", FormatDateTime="dd-MM-yyyy"  },
                new GridField() { Header = "PO Number", FieldName = "PONumber"},
                new GridField() { Header = "Vendor", FieldName = "VendorName", },
                new GridField() { Header = "Type", FieldName = "Status", TextAlign = "center", TotalLabel="Total:"},
                new GridField() { Header = "Order QTY", FieldName = "TotalOrderQty", ClassName="text-right text-danger", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Received QTY", FieldName = "TotalReceivedQty",  ClassName="text-right text-primary", Format="n0", IsTotalFooter=true},
                new GridField() {Header = "QTY Remaining", FieldName = "TotalQtyRemain", ClassName = "text-right", Format = "n0", IsTotalFooter = true},
                new GridField() { Header = "Create Date", FieldName = "CreateDate", ClassName="text-center" },
                new GridField() { Header = "Action", FieldName = "StockInID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblStockInDetail",
                    //HideEditOnActionNameNotMatch = (AdminEdit==false? true:false),
                    //ShowActionNameOnConditionField ="StatusID",
                    //ShowActionNameOnConditionValue="60",
                    //HideDeteleOnNotMatch=true
                }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreatePurchaseRequest(string PrimaryFieldValue, string Type = "", bool Clone = false, int ItemID = 0, string ItemName = "")
        {
            ViewBag.ItemName = ItemName;
            ViewBag.ItemID = ItemID;
            ViewBag.Clone = Clone;
            ViewBag.Type = Type;
            ViewBag.VendorName = "";
            HTML html = new HTML
            {
                isCloned = Clone,
                FormID = "CreatePurchaseRequest",
                //html.FieldIDFocus = "VendorName";
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);"
            };
            string vendornamevalue = "";
            string vendorID = "0";
            string StatusID = "60";
            string sqlv;
            if (Type == "NewPO")
            {
                sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                //sqlv += "inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID ";
                sqlv += "where v.VendorID = " + PrimaryFieldValue;
                sqlv += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
                html.PrimaryFieldValue = "0";
                DataSet dsv = html.LoadData(sqlv);
                vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                ViewBag.VendorName = vendornamevalue;
            }
            else if (PrimaryFieldValue != "0")
            {
                html.PrimaryFieldValue = PrimaryFieldValue;
                sqlv = "select p.StatusID, v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                sqlv += "inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID ";
                sqlv += "where p.PurchaseOrderID = " + html.PrimaryFieldValue.ToString();
                sqlv += " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
                DataSet dsv = html.LoadData(sqlv);
                vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                StatusID = dsv.Tables[0].Rows[0]["StatusID"].ToString() == "91" ? "60" : dsv.Tables[0].Rows[0]["StatusID"].ToString();
            }
            if (Clone) StatusID = "60";
            ViewBag.StatusID = StatusID;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="Vendor_Name", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            HiddenField="VendorID,Reference_VendorName",
                            Value=vendornamevalue,
                            HiddenFieldValue=vendorID+",0",
                            SQL="select VendorID VendorID, (VendorCode + ' - ' + VendorName) VendorName, VendorName Reference_VendorName from dbo.TblVendor where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true,
                            FieldName="VendorName",
                            DivClassCol="col-6",
                            AddButtonFunction="AddNewVendor()",
                            TypeAheadAddNewFunctionName="AddNewVendor()"
                        },
                    }
                },
                new  FormData() {
                    TableName="TblPurchaseOrder",
                    FormTitle="Purchase Request Information",
                    GetIDENTITYByFiled="PurchaseOrderID",
                    PrimaryFieldName="PurchaseOrderID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Vendors/PurchaseOrder",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="NeedToCheckUserID", Value="NeedToCheckBy_NeedToCheckUserID", GetValueFromInputName= true,
                        },
                        new DBField()
                        {
                            DBFieldName="IssuedBy", SetValue=""+html.UserFullName+""
                        },
                        //new DBField()
                        //{
                        //    DBFieldName="IssuedByPosition", SetValue=""+html.UserPosition+""
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="CheckVerifyByPosition", SetValue="$(\"#NeedToCheckBy_TelegramUserName\").val().toString()"
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="ApprovedByPosition", SetValue="$(\"#NeedToApproval_ApprovedByPosition\").val().toString()"
                        //},
						new DBField()
                        {
                            DBFieldName="IssuedByUserID", SetValue=""+html.UserID+""
                        },
                        new DBField()
                        {
                            DBFieldName="VendorID", Value="Vendor_Name_VendorID", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VendorName", Value="Vendor_Name_Reference_VendorName", GetValueFromInputName= true
                        },new DBField()
                        {
                            DBFieldName="WarehouseName", Value="TxtContactName_WarehouseName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VAT", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="CustomerNotes", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermsConditions", Value="TermsConditions", GetValueFromTextAreaName= true
                        }
                    },
                    SQL="update dbo.TblPurchaseOrder set CheckVerifyDate = NULL, ApprovalDate = NULL, CheckVerifyBy = '', CheckVerifyByUserID=0, ApproveBy='', ApproveByUserID=0 where PurchaseOrderID = @PurOrderID",
                    UpdateFiledName="PurOrderID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PR Status", ID="PRStatus", Required=true,
                            DBField="PRStatus", Value="0", IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PurchaseOrderID", ID="PurOrderID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PR Number", ID="PRNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PRNumber", IsReadOnly = true,
                            isCloned = Clone,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'PR'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'PR' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'PR-{yy}{MM}{dd}-{0000}'",
                            CallBackToControlID="PRNumber, PrefixPR", UpdatePrefixModule="PR", Prefix="PrefixPR", SetPrefixToDbFieldName="PRNumber"
                        },
						//new HtmlTextBox()
						//{
						//	Label="Reference#", ID="Reference",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="Reference",
						//	HiddenFeild="VendorName",
						//},
						new HtmlTextBox()
                        {
                            Label="Request Date", ID="PRDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PRDate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Estimated arrival", ID="DeliveryDate", Required=true,
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="DeliveryDate", DivClassCol="col-sm-auto",
                        //    SingleDatePicker = true,
                        //},
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request By", ID="RequestBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestBy", isTypeahead = true,
                            SQL="select distinct isnull(RequestBy, '') RequestBy from dbo.TblPurchaseOrder \r\nwhere isnull(RequestBy, '') <> '' and SystemID = "+html.SystemID+" and WarehouseID = "+ html.WarehouseID + " order by isnull(RequestBy, '')",
                            //HiddenFeild="TelegramUserName",
                            DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedToCheckBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedToCheckUserID, u.TelegramUserName, u.FullName NeedToCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="TelegramUserName, NeedToCheckUserID",
                            HiddenFieldValue="0,0",
                            DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Approval By", ID="NeedToApproval", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToApproval", isTypeahead = true,
                            SQL="select u.UserLoginID ApproveByUserID, u.TelegramUserName, u.FullName NeedToApproval, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="TelegramUserName, ApproveByUserID",
                            HiddenFieldValue="0,0",
                            DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Deliver To", ID="DeliverTo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliverTo", isTypeahead = true,
                            //SQL="select BranchID, BranchName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end DeliverTo from dbo.TblBranch where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select WID BranchID, WarehouseName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end DeliverTo from dbo.TblUserWarehouse where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,
                            SQL="select uw.WID, uw.WarehouseName DeliverTo, uw.WarehouseName WName, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                            HiddenField="WID",//, TypeaheadGroupByField="WarehouseName",
                            AddButtonFunction="AddNewWarehouse()", TypeAheadAddNewFunctionName="AddNewWarehouse()",
                            DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone",TxtContactName_WarehouseName="WName"})
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Contact Name", ID="TxtContactName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactName", HiddenField="WarehouseName"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone", ID="TxtContactPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactPhone"
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Issued By", ID="TxtIssuedBy", Required=true,
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="IssuedBy", Hide = true, Value=html.UserFullName,                             
                        //},
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Payment Terms", ID="PaymentTerms", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentTerms", isTypeahead = true,
                            SQL="select convert(varchar, NumberofDays) NumberofDays, TermName PaymentTerms from dbo.TblPaymentTerms where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select '0' NumberofDays, 'Due on Receipt' PaymentTerms order by PaymentTerms",
                            HiddenField="NumberofDays", Value="Due on Receipt", HiddenFieldValue = "0",
                            DivClassCol="col-sm",TypeAheadAddNewFunctionName="AddNewPaymentTerms()",
                        },
                        new HtmlTextBox()
                        {
                            Label="Payment Due Date", ID="PaymentDueDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentDueDate",
                            SingleDatePicker = true,DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Tax", ID="Tax",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Tax", isTypeahead = true,
                            SQL="select Rate, TaxName + ' [' + convert(varchar, format(Rate, '#')) + '%]' Tax from dbo.TblTax where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by TaxName",
                            HiddenField="Rate", HiddenFieldValue="0", DivClassCol="col-sm"
                        },
                        
                        //new HtmlTextBox()
                        //{
                        //    Label="CurrencyName", ID="Currency",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="CurrencyName", isTypeahead = true,
                        //    SQL="select CurrencyID, CurrencyName + ' (' + CurrencySymbol + ')' CurrencyName from dbo.TblCurrency where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by CurrencyName",
                        //    HiddenFeild="CurrencyID",
                        //    SQLCallBack="select format(ExchangeRate, '#,#.##') ExchangeRate, format(DateEntry,'dd-MM-yyyy') [Date] from TblExchangeRate where CurrencyID = @CurrencyID and SystemID = "+html.SystemID+" and WarehouseID = " + html.WarehouseID + " order by DateEntry desc",
                        //    CallBackToControlID="TxtExchangeRate, TxtLastUpdate",
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Exchange Rate", ID="TxtExchangeRate", IsallowInputOnlyNumber = true,
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="ExchangeRate", IsReadOnly=true, DivClassCol="col-sm-auto",
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Last Update", ID="TxtLastUpdate",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="LastUpdate", IsReadOnly=true,DivClassCol="col-sm-auto",
                        //},
                        new HtmlTextBox()
                        {
                            Label="Status", ID="TxtStatusID", Value = StatusID, Hide = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StatusID", IsReadOnly=true,DivClassCol="col-sm-auto",
                        },
                        new HtmlTextBox()
                        {
                            Label="Type", ID="Type",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Type", DivClassCol="col-sm-auto",
                            Value="PurchaseRequest",
                            Hide=true
                        },
                    }
                },
                new  FormData() {
                    TableName="TblPurchaseOrderDetail",
                    GetIDENTITYByFiled="PurchaseOrderID",
                    PrimaryFieldName="PurchaseOrderID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll()",//,ApplySummernote('summernote'),Reload_UM,Reload_AccountName
                            FooterColspan=2,
                            SubTotalColspan=5,
                            GetIDENTITYByFiled="PurchaseOrderID",
                            PrimaryFieldName="PurchaseOrderID",
                            TableName="TblPurchaseOrderDetail",
                            OrderBy="PurchaseOrderDetailID",
                            ReturnFieldNameValue="PurchaseOrderID, VendorID, PRNumber, VendorName, NeedToApproval, TelegramUserName",
                            ReturnFieldNameValueFromTblName = "TblPurchaseOrder",
                            
                            //UpdateValueFiledName="ItemDescription",
                            //UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            //UpdateValueFiledNameConditionFiledName="ItemID",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="PRNumber", SetValue="$(\"#PRNumber\").val()"
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="PRDate", SetValue="StringToDate($(\"#PRDate\").val())"
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="DeliveryDate", SetValue="StringToDate($(\"#DeliveryDate\").val())"
                                //},
                                new DBField()
                                {
                                    DBFieldName="StatusID", SetValue="60"
                                },
                                new DBField()
                                {
                                    DBFieldName="DeliverTo", SetValue="$(\"#DeliverTo\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="RequestBy", SetValue="$(\"#RequestBy\").val()"
                                }
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="បរិយាយ"
                                },
                                //new TblTHead()
                                //{
                                //     Class="col-3", Label="គណនេយ្យ"
                                //},
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ បញ្ចាទិញ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា"
                                },
                                new TblTHead()
                                {
                                     Label="តម្លៃលក់"
                                },
                                new TblTHead()
                                {
                                     Class="col-2", Label="បញ្ចុះតម្លៃ"//, colspan=2
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="", Class="col-1",
                                //     CloseTR="</tr>"
                                //},
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Account"
                                //},
                                new TblTHead()
                                {
                                     Label="UM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Order QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Sale Price", Class="text-nowrap"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Dis%"
                                //},
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="",
                                //     CloseTR="</tr>"
                                //}
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select top 10 ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName
from dbo.TblItem i where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                                            SQLCallBack ="select distinct iv.UM,iv.UomID, iv.CostPrice UnitPrice, iv.SellingPrice SalePrice, iv.ItemDescription ItemName_Description from dbo.TblItemVariant iv where iv.ItemID = @SearchText and iv.CostPrice is not null union all select distinct i.UM,i.UomID, i.CostPrice UnitPrice, i.SellingPrice SalePrice, i.ItemDescription ItemName_Description from dbo.TblItem i where i.ItemID = @SearchText and i.CostPrice is not null",
                                            DisplayOnFields="UM, UomID, UnitPrice, SalePrice, ItemName_Description",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, CostPrice UnitPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            DisplayOnFieldsTypeahead="UnitPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM", FieldName = "ItemID",
                                            CallBackDisplayText="UM",
                                            TypeaheadCallBackFunction="TotalAll()",
                                            InputDescription = true, placeholder="Item Name"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="AccountName", Required=true,
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DBField="AccountName", DivClass="wrap-input3",
                                        //    isTypeahead = true, HiddenFeild="ChartOfAccountID",
                                        //    SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                                        //    TypeaheadGroupByField = "AccountType"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            //OnChangeFunction="TotalAll()"
                                            //IsReadOnly=true,
                                            //SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                                            //SQL="select distinct * from(select ItemID, UM, CostPrice from dbo.TblItem union all select ItemID, UM, CostPrice from dbo.TblUMItem where UM <> '') as a where ItemID = 140 order by ItemID",
                                            //isTypeahead=true//,
                                            HiddenField="UomID",
                                            //TypeAheadAddNewFuntionName="AddNewUM();",
                                            
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="TotalAll();", Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true, Digit="0.000",
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SalePrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="SalePrice", IsAllowInputOnlyNumber =true,
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="DisP", DivClass="wrap-input3",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="DisP", Value = "0", IsallowInputOnlyNumber =true,
                                        //    OnKeyupFunction="onChangeDisP(this);"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="onChangeDisUSD(this);",
                                            HiddenField="DisP"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="AmountKeyup(this);"
                                        }
                                    },
                                    //TDAlign="center",
                                    //TDDelete="<div class=\"btn-group\"><a name=\"Delete\" onclick=\"RemoveTableRow('TRNo', 'TDNo', this,'','TxtItemDescription');\" class=\"btn btn-danger btn-sm\" href=\"javascript:void(0);\" title=\"Delete\" alt=\"Delete\"><i class=\"fa fa-trash\"></i></a></div>",
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult ConvertToPurchaseOrder(string PrimaryFieldValue, string BillID = "0", string Type = "", string PR_Number = "", string BillNumber = "0", bool Clone = false, string Status = "Convert To PO", int ItemID = 0, string ItemName = "")
        {
            ViewBag.ItemName = ItemName;
            ViewBag.ItemID = ItemID;
            ViewBag.Clone = Clone;
            ViewBag.Type = Type;
            ViewBag.VendorName = "";
            HTML html = new HTML
            {
                isCloned = Clone,
                FormID = "CreatePurchaseOrder",
                //html.FieldIDFocus = "VendorName";
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);"
            };
            string vendornamevalue = "";
            string vendorID = "0";
            string StatusID = "60";
            if (PR_Number != "")
            {
                string sqlv = @"select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName 
from dbo.TblPurchaseOrder po 
inner join dbo.TblVendor v on v.VendorID = po.VendorID
where po.PRNumber = @PRNumber";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    { "PRNumber", PR_Number }
                };
                DataSet ds = html.LoadData(sqlv, false, para);
                vendornamevalue = ds.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = ds.Tables[0].Rows[0]["VendorID"].ToString();
            }
            //if (Type == "NewPO")
            //{
            //    sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
            //    //sqlv += "inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID ";
            //    sqlv += "where v.VendorID = " + PrimaryFieldValue;
            //    sqlv += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
            //    html.PrimaryFieldValue = "0";
            //    DataSet dsv = html.LoadData(sqlv);
            //    vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
            //    vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
            //    ViewBag.VendorName = vendornamevalue;
            //}
            //else if (PrimaryFieldValue != "0")
            //{
            //    html.PrimaryFieldValue = PrimaryFieldValue;
            //    sqlv = "select p.StatusID, v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
            //    sqlv += "inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID ";
            //    sqlv += "where p.PurchaseOrderID = " + html.PrimaryFieldValue.ToString();
            //    sqlv += " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
            //    DataSet dsv = html.LoadData(sqlv);
            //    vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
            //    vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
            //    StatusID = (dsv.Tables[0].Rows[0]["StatusID"].ToString() == "91" ? "60" : dsv.Tables[0].Rows[0]["StatusID"].ToString());
            //}
            if (Clone) StatusID = "60";
            ViewBag.StatusID = StatusID;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="Vendor_Name", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            HiddenField="VendorID,Reference_VendorName",
                            Value=vendornamevalue,
                            HiddenFieldValue=vendorID+",0",
                            SQL="select VendorID VendorID, (VendorCode + ' - ' + VendorName) VendorName, VendorName Reference_VendorName from dbo.TblVendor where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true,
                            FieldName="VendorName",
                            DivClassCol="col-6",
                            AddButtonFunction="AddNewVendor()",
                            TypeAheadAddNewFunctionName="AddNewVendor()"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PR Number", ID="PRNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 PRNumber from dbo.TblPurchaseOrder where PRStatus = 90 and lower(PRNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            CallBackFunction="OnPRNumberChange",
                            Value = PR_Number,DivClassCol="col-6",
                        },
                    }
                },
                new  FormData() {
                    TableName="TblPurchaseOrder",
                    FormTitle="Purchase Order Information",
                    GetIDENTITYByFiled="PurchaseOrderID",
                    PrimaryFieldName="PurchaseOrderID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Vendors/PurchaseOrder",
                    dbField=new DBField[]
                    {
                        //new DBField()
                        //{
                        //    DBFieldName="NeedToCheckUserID", Value="NeedToCheckBy_NeedToCheckUserID", GetValueFromInputName= true,
                        //},
                        new DBField()
                        {
                            DBFieldName="IssuedBy", SetValue=""+html.UserFullName+""
                        },
                        //new DBField()
                        //{
                        //    DBFieldName="IssuedByPosition", SetValue=""+html.UserPosition+""
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="CheckVerifyByPosition", SetValue="$(\"#NeedToCheckBy_TelegramUserName\").val().toString()"
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="ApprovedByPosition", SetValue="$(\"#NeedToApproval_ApprovedByPosition\").val().toString()"
                        //},
						new DBField()
                        {
                            DBFieldName="IssuedByUserID", SetValue=""+html.UserID+""
                        },
                        new DBField()
                        {
                            DBFieldName="VendorID", Value="Vendor_Name_VendorID", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VendorName", Value="Vendor_Name_Reference_VendorName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="WarehouseName", Value="DeliverTo", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VAT", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="CustomerNotes", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermsConditions", Value="TermsConditions", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="RefNumber", Value="PRNumber",GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="RefID", Value="RefID", GetValueFromInputName = true
                        },
                    },
                    SQL="update dbo.TblPurchaseOrder set CheckVerifyDate = NULL, ApprovalDate = NULL, CheckVerifyBy = '', CheckVerifyByUserID=0, ApproveBy='', ApproveByUserID=0 where PurchaseOrderID = @PurOrderID; update dbo.TblPurchaseOrder set IsConvertedToPO=1 where PRNumber = @PRnumber",
                    UpdateFiledName="PurOrderID, PRnumber",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PO Status", ID="POStatus", Required=true,
                            DBField="POStatus", Value="0", IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PurchaseOrderID", ID="PurOrderID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PRnumber", ID="PRnumber", Required=true,
                           Value=PR_Number.ToString(),Hide = true
                        },
                        new HtmlTextBox()
                        {
                           Label="RefID", ID="RefID", Required=true,
                           Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PO Number", ID="PONumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PONumber", IsReadOnly = true,
                            isCloned = Clone,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'PO'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'PO' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'PO-{yy}{MM}{dd}-{0000}'",
                            CallBackToControlID="PONumber, PrefixPO", UpdatePrefixModule="PO", Prefix="PrefixPO", SetPrefixToDbFieldName="PONumber"
                        },
						//new HtmlTextBox()
						//{
						//	Label="Reference#", ID="Reference",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="Reference",
						//	HiddenFeild="VendorName",
						//},
						new HtmlTextBox()
                        {
                            Label="Order Date", ID="PODate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PODate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Estimated arrival", ID="DeliveryDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliveryDate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        //new HtmlTextBox()
                        //{
                        //    Label="Request By", ID="RequestBy", 
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="RequestBy", isTypeahead = true,
                        //    SQL="select distinct isnull(RequestBy, '') RequestBy from dbo.TblPurchaseOrder \r\nwhere isnull(RequestBy, '') <> '' and SystemID = "+html.SystemID+" and WarehouseID = "+ html.WarehouseID + " order by isnull(RequestBy, '')",
                        //    //HiddenFeild="TelegramUserName",
                        //    DivClassCol="col-sm-auto"
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Check By", ID="NeedToCheckBy", 
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="NeedToCheckBy", isTypeahead = true,
                        //    SQL="select u.UserLoginID NeedToCheckUserID, u.TelegramUserName, u.FullName NeedToCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        //    HiddenFeild="TelegramUserName, NeedToCheckUserID",
                        //    HidenFieldValue="0,0",
                        //    DivClassCol="col-sm-auto"
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Approval By", ID="NeedToApproval", 
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="NeedToApproval", isTypeahead = true,
                        //    SQL="select u.UserLoginID ApproveByUserID, u.TelegramUserName, u.FullName NeedToApproval, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        //    HiddenFeild="TelegramUserName, ApproveByUserID",
                        //    HidenFieldValue="0,0",
                        //    DivClassCol="col-sm-auto"
                        //},
                     
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Deliver To", ID="DeliverTo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliverTo", isTypeahead = true,
                            //SQL="select BranchID, BranchName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end DeliverTo from dbo.TblBranch where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select WID BranchID, WarehouseName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end DeliverTo from dbo.TblUserWarehouse where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,
                            SQL="select uw.WID, uw.WarehouseName DeliverTo, uw.WarehouseName WName, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                            HiddenField="WID",//, TypeaheadGroupByField="WarehouseName",
                            AddButtonFunction="AddNewWarehouse()", TypeAheadAddNewFunctionName="AddNewWarehouse()",
                            DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone",TxtContactName_WarehouseName="WName"})
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Name", ID="TxtContactName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactName"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone", ID="TxtContactPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactPhone"
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Issued By", ID="TxtIssuedBy", Required=true,
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="IssuedBy", Hide = true, Value=html.UserFullName,                             
                        //},
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Payment Terms", ID="PaymentTerms", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentTerms", isTypeahead = true,
                            SQL="select convert(varchar, NumberofDays) NumberofDays, TermName PaymentTerms from dbo.TblPaymentTerms where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select '0' NumberofDays, 'Due on Receipt' PaymentTerms order by PaymentTerms",
                            HiddenField="NumberofDays", Value="Due on Receipt", HiddenFieldValue = "0",
                            DivClassCol="col-sm",TypeAheadAddNewFunctionName="AddNewPaymentTerms()",
                        },
                        new HtmlTextBox()
                        {
                            Label="Payment Due Date", ID="PaymentDueDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentDueDate",
                            SingleDatePicker = true,DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Tax", ID="Tax",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Tax", isTypeahead = true,
                            SQL="select Rate, TaxName + ' [' + convert(varchar, format(Rate, '#')) + '%]' Tax from dbo.TblTax where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by TaxName",
                            HiddenField="Rate", HiddenFieldValue="0", DivClassCol="col-sm"
                        },
                   
                        //new HtmlTextBox()
                        //{
                        //    Label="CurrencyName", ID="Currency",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="CurrencyName", isTypeahead = true,
                        //    SQL="select CurrencyID, CurrencyName + ' (' + CurrencySymbol + ')' CurrencyName from dbo.TblCurrency where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by CurrencyName",
                        //    HiddenFeild="CurrencyID",
                        //    SQLCallBack="select format(ExchangeRate, '#,#.##') ExchangeRate, format(DateEntry,'dd-MM-yyyy') [Date] from TblExchangeRate where CurrencyID = @CurrencyID and SystemID = "+html.SystemID+" and WarehouseID = " + html.WarehouseID + " order by DateEntry desc",
                        //    CallBackToControlID="TxtExchangeRate, TxtLastUpdate",
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Exchange Rate", ID="TxtExchangeRate", IsallowInputOnlyNumber = true,
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="ExchangeRate", IsReadOnly=true, DivClassCol="col-sm-auto",
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Last Update", ID="TxtLastUpdate",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="LastUpdate", IsReadOnly=true,DivClassCol="col-sm-auto",
                        //},
                        new HtmlTextBox()
                        {
                            Label="Status", ID="TxtStatusID", Value = StatusID, Hide = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StatusID", IsReadOnly=true,DivClassCol="col-sm-auto",
                        },
                        new HtmlTextBox()
                        {
                            Label="Type", ID="Type",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Type", DivClassCol="col-sm-auto",
                            Value="PurchaseOrder",
                            Hide=true
                        },
                    }
                },
                new  FormData() {
                    TableName="TblPurchaseOrderDetail",
                    GetIDENTITYByFiled="PurchaseOrderID",
                    PrimaryFieldName="PurchaseOrderID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll()",//,ApplySummernote('summernote'),Reload_UM,Reload_AccountName
                            FooterColspan=2,
                            SubTotalColspan=5,
                            GetIDENTITYByFiled="PurchaseOrderID",
                            PrimaryFieldName="PurchaseOrderID",
                            TableName="TblPurchaseOrderDetail",
                            OrderBy="PurchaseOrderDetailID",
                            ReturnFieldNameValue="PurchaseOrderID, VendorID, PONumber, VendorName, NeedToApproval, TelegramUserName",
                            ReturnFieldNameValueFromTblName = "TblPurchaseOrder",
                            
                            //UpdateValueFiledName="ItemDescription",
                            //UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            //UpdateValueFiledNameConditionFiledName="ItemID",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                new DBField()
                                {
                                    DBFieldName="PONumber", SetValue="$(\"#PONumber\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="PODate", SetValue="StringToDate($(\"#PODate\").val())"
                                },
                                new DBField()
                                {
                                    DBFieldName="DeliveryDate", SetValue="StringToDate($(\"#DeliveryDate\").val())"
                                },
                                new DBField()
                                {
                                    DBFieldName="StatusID", SetValue="60"
                                },
                                new DBField()
                                {
                                    DBFieldName="DeliverTo", SetValue="$(\"#DeliverTo\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="RequestBy", SetValue="$(\"#RequestBy\").val()"
                                },

                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="បរិយាយ"
                                },
                                //new TblTHead()
                                //{
                                //     Class="col-3", Label="គណនេយ្យ"
                                //},
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ បញ្ចាទិញ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា"
                                },
                                new TblTHead()
                                {
                                     Label="តម្លៃលក់"
                                },
                                new TblTHead()
                                {
                                     Class="col-2", Label="បញ្ចុះតម្លៃ"//, colspan=2
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="", Class="col-1",
                                //     CloseTR="</tr>"
                                //},
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Account"
                                //},
                                new TblTHead()
                                {
                                     Label="UM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Order QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Sale Price", Class="text-nowrap"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Dis%"
                                //},
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="",
                                //     CloseTR="</tr>"
                                //}
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select top 10 ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName
from dbo.TblItem i where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID +" and DomainID like '%"+html.DomainID+"%'",
                                            SQLCallBack ="select distinct iv.UM,iv.UomID, iv.CostPrice UnitPrice, iv.SellingPrice SalePrice, iv.ItemDescription ItemName_Description from dbo.TblItemVariant iv where iv.ItemID = @SearchText",
                                            DisplayOnFields="UM, UomID, UnitPrice, SalePrice, ItemName_Description",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, CostPrice UnitPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            DisplayOnFieldsTypeahead="UnitPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM", FieldName = "ItemID",
                                            CallBackDisplayText="UM",
                                            TypeaheadCallBackFunction="TotalAll()",
                                            InputDescription = true, placeholder="Item Name"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="AccountName", Required=true,
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DBField="AccountName", DivClass="wrap-input3",
                                        //    isTypeahead = true, HiddenFeild="ChartOfAccountID",
                                        //    SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                                        //    TypeaheadGroupByField = "AccountType"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            //OnChangeFunction="TotalAll()"
                                            //IsReadOnly=true,
                                            //SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                                            //SQL="select distinct * from(select ItemID, UM, CostPrice from dbo.TblItem union all select ItemID, UM, CostPrice from dbo.TblUMItem where UM <> '') as a where ItemID = 140 order by ItemID",
                                            //isTypeahead=true//, HiddenFeild="UomID",
                                            //TypeAheadAddNewFuntionName="AddNewUM();",
                                            
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="TotalAll();", Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true, Digit="0.000",
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SalePrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="SalePrice", IsAllowInputOnlyNumber =true,
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="DisP", DivClass="wrap-input3",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="DisP", Value = "0", IsallowInputOnlyNumber =true,
                                        //    OnKeyupFunction="onChangeDisP(this);"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="onChangeDisUSD(this);",
                                            HiddenField="DisP"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="AmountKeyup(this);"
                                        }
                                    },
                                    //TDAlign="center",
                                    //TDDelete="<div class=\"btn-group\"><a name=\"Delete\" onclick=\"RemoveTableRow('TRNo', 'TDNo', this,'','TxtItemDescription');\" class=\"btn btn-danger btn-sm\" href=\"javascript:void(0);\" title=\"Delete\" alt=\"Delete\"><i class=\"fa fa-trash\"></i></a></div>",
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public async Task<JsonResult> ApprovePR(int PRID, int PRStatus, string Comment = "", int TelegramGroupID = 0, string FirstName = "", string LastName = "", string UserName = "", string UserFullName = "", string NeedToCheckUserID = "", string ChatID = "", string MessageID = "")
        {
            HTML html = new HTML();
            ProfileController profileController = new ProfileController();
            string sql = @"";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"PRID", PRID.ToString() },
                {"StatusID", PRStatus.ToString()},
                {"PRStatus", PRStatus.ToString()},
            };
            if (PRStatus == 92) // 92 = Checked
            {
                sql = @"update dbo.TblPurchaseOrder set PRStatus = @PRStatus, CheckVerifyBy = @CheckVerifyBy, 
AprrovalFrom = @AprrovalFrom, CheckVerifyByUserID = @CheckVerifyByUserID,
CheckVerifyDate = @CheckVerifyDate, StatusID = @StatusID where PurchaseOrderID = @PRID;
select VendorID, VendorName, PRNumber, isnull(TelegramMessageID,'') TelegramMessageID, TelegramMessage, NeedToApproval, TelegramUserName from dbo.TblPurchaseOrder where PurchaseOrderID = @PRID";
                para.Add("CheckVerifyByUserID", NeedToCheckUserID != "" ? NeedToCheckUserID : html.UserID);
                para.Add("CheckVerifyBy", UserFullName != "" ? UserFullName : html.UserFullName);
                para.Add("CheckVerifyDate", html.GetCurrentDate());
                para.Add("AprrovalFrom", UserName != "" ? "Telegram" : "Billing System");
            }
            else if (PRStatus == 90) // 90 = Approve
            {
                sql = @"declare @IsService bit=0;
update dbo.TblPurchaseOrder set PRStatus = @PRStatus, ApproveBy = @ApproveBy, 
AprrovalFrom = @AprrovalFrom, 
ApprovalDate = @ApprovalDate, StatusID = @StatusID where PurchaseOrderID = @PRID;
update dbo.TblPurchaseOrderDetail set StatusID = 90 where PurchaseOrderID = @PRID;
select VendorID, VendorName, PRNumber, isnull(TelegramMessageID,'') TelegramMessageID, TelegramMessage, NeedToApproval, TelegramUserName from dbo.TblPurchaseOrder where PurchaseOrderID = @PRID;
if(exists (select distinct it.ItemTypeID from dbo.TblPurchaseOrderDetail pd
				inner join dbo.TblItem i on i.ItemID = pd.ItemID
				inner join dbo.TblItemType it on it.ItemTypeID = i.ItemTypeID
				where pd.PurchaseOrderID = @PRID and it.TypeName = 'Inventory'
				and pd.SystemID = " + html.SystemID + " and pd.WarehouseID = " + html.WarehouseID + @"))
	begin
		update dbo.TblPurchaseOrder set StatusID = 90 where PurchaseOrderID = @PRID;
		select @IsService = 0;
	end
	else
	begin
		update dbo.TblPurchaseOrder set StatusID = 90 where PurchaseOrderID = @PRID;
		select @IsService = 1;
	end;
select @IsService IsService
";
                para.Add("ApproveByUserID", NeedToCheckUserID != "" ? NeedToCheckUserID : html.UserID);
                para.Add("ApproveBy", UserFullName != "" ? UserFullName : html.UserFullName);
                para.Add("AprrovalFrom", UserName != "" ? "Telegram" : "Billing System");
                para.Add("ApprovalDate", html.GetCurrentDate());
            }
            else if (PRStatus == 91) // 91 = Disapprove
            {
                sql = @"update dbo.TblPurchaseOrder set PRStatus = @PRStatus, DisapproveBy = @DisapproveBy, 
AprrovalFrom = @AprrovalFrom, DisapproveByUserID = @DisapproveByUserID,ApproveByUserID = @ApproveByUserID,
ApprovalDate = @ApprovalDate, ApproveBy = @ApproveBy, StatusID = @StatusID, Comment = @Comment where PurchaseOrderID = @PRID;
select VendorID, VendorName, PRNumber, isnull(TelegramMessageID,'') TelegramMessageID, TelegramMessage, NeedToApproval, TelegramUserName from dbo.TblPurchaseOrder where PurchaseOrderID = @PRID";
                para.Add("DisapproveByUserID", NeedToCheckUserID != "" ? NeedToCheckUserID : html.UserID);
                para.Add("DisapproveBy", UserName != "" ? UserName : html.UserFullName);
                para.Add("ApproveByUserID", UserName != "" ? "0" : html.UserID);
                para.Add("ApproveBy", UserFullName != "" ? UserFullName : html.UserFullName);
                para.Add("ApprovalDate", html.GetCurrentDate());
                para.Add("AprrovalFrom", UserName != "" ? "Telegram" : "Billing System");
                para.Add("Comment", Comment);
            }
            SqlResult result = html.SaveChangeData(sql, false, para, true, true);
            DataRow dr = result.Data.Tables[0].Rows[0];
            if (result.Data.Tables.Count > 1)
            {
                bool IsService = bool.Parse(result.Data.Tables[1].Rows[0][0].ToString());
            }
            string NeedToApproval = dr["NeedToApproval"].ToString();
            string TelegramUserName = dr["TelegramUserName"].ToString();
            string VendorID = dr["VendorID"].ToString();
            string VendorName = html.ToUrlSlug(dr["VendorName"].ToString());
            string PRNumber = html.ToUrlSlug(dr["PRNumber"].ToString());
            string TelegramMessageID = dr["TelegramMessageID"].ToString();
            string TelegramMessage = dr["TelegramMessage"].ToString();
            if (TelegramMessageID != "")
            {
                sql = "select top 1 TelegramMessage, TelegramGroupID from TblPOTelegramMessage where POID = @PRID and TelegramMessageID = @TelegramMessageID order by TelegramPOMessageID desc";
                para = new Dictionary<string, string>
                {
                    {"PRID", PRID.ToString() },
                    {"TelegramMessageID", TelegramMessageID },
                };
                DataSet ds = html.LoadData(sql, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    dr = ds.Tables[0].Rows[0];
                    TelegramMessage = dr["TelegramMessage"].ToString();
                    TelegramGroupID = TelegramGroupID == 0 ? int.Parse(dr["TelegramGroupID"].ToString()) : TelegramGroupID;
                }
            }

            //RouteData route = new RouteData();
            //route.Values.Add("action", "InvoiceTMP");
            //route.Values.Add("controller", "ProfileController");

            //var sw = new StringWriter();
            //var controller = DependencyResolver.Current.GetService<ProfileController>();
            //ControllerContext newContext = new ControllerContext(new HttpContextWrapper(System.Web.HttpContext.Current), route, profileController);

            //controller.ControllerContext = newContext;//ControllerContext;// DependencyResolver.Current.GetService<ProfileController>().ControllerContext;
            //controller.InvoiceTMP(int.Parse(VendorID), POID, "PO", false, true).ExecuteResult(newContext);

            dynamic jsonResult = profileController.InvoiceTMPR(int.Parse(VendorID), PRID, "PR", false, true);

            string Data = jsonResult.Data.Data;

            result = await html.HTMLToPDF(Data, "Uploads/Vendors/" + VendorName, PRNumber);
            //result = await html.URLtoPDF("Profile/InvoiceTMP?VendorID=" + VendorID + "&POID=" + POID + "&Type=PO&ReturnHtmlData=false&ReturnAllHtmlData=false&RedirectToPOView=true", "", "Uploads/Vendors/" + VendorName, PONumber + "_" + POStatus);

            if (result.Result == ResultType.Successful)
            {
                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        //InlineKeyboardButton.WithCallbackData(text: "Check ✍️", callbackData: "CheckVerifyPO~"+POID),
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅", callbackData: "ApprovePO~"+PRID+"~-967102140"),// -967102140 = approval po// -844276850 = test
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❎", callbackData: "DisapprovePO~"+PRID+"~-967102140"),// -967102140 = approval po// -844276850 = test
                    }
                });
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                string msg = "";
                //(TelegramUserName != "" ? TelegramUserName + "<br><br>" : "") + "Dear Team,<br><br>Now this PO " + (POStatus == 90 ? "Approved By " : POStatus == 91 ? "Disapproved By " : "Checked By ") + (UserName != "" ? UserName : html.UserFullName) + ".";
                if (PRStatus == 90) // 90 = Approve
                    msg = "Dear Team,<br><br>Now this PR Approved By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>";
                else if (PRStatus == 91) // 91 = Disapprove
                    msg = "Dear Team,<br><br>Now this PR Disapproved By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>Please revise this PR.<br><br>";
                else if (PRStatus == 92) // 92 = Checked
                {
                    msg = (TelegramUserName != "" ? TelegramUserName + "<br><br>" : "") + "Dear " + NeedToApproval + ",<br><br>Now this PR Checked By " + (UserName != "" ? UserName : html.UserFullName) + ".";
                    msg += "<br><br>Please check attachment file for detail and APPROVE or DISAPPROVE.<br><br>";
                }
                if (Comment != "")
                    msg += "Comment: " + Comment + "<br><br>";
                msg += "Thank you and best regards,<br><br>";
                msg += "NSM ISP BILLING SYSTEM AUTO SEND PR NOTIFICATION. #" + PRNumber;

                if (PRStatus == 92) // 92 = Checked
                {
                    string msgs = "Dear Team, <br><br>Now this PR checked by " + (UserName != "" ? UserName : html.UserFullName) + ".";
                    msgs += "<br><br>Thank you and best regards,<br><br>";
                    msgs += "NSM ISP BILLING SYSTEM AUTO SEND PR NOTIFICATION. #" + PRNumber;
                    await notificationTelegram.NotificationTelegramBot("", -986646049, "", result.OutPutFile, msgs, false, true, PRID, int.Parse(TelegramMessageID), TelegramMessage, null, true); // check po = -986646049
                    result = await notificationTelegram.NotificationTelegramBot("", -967102140, "", result.OutPutFile, msg, false, true, PRID, 0, TelegramMessage, inlineKeyboard, false); // -967102140 = approval po // -844276850 = test
                    TelegramGroupID = -967102140;
                    para = new Dictionary<string, string>
                    {
                        { "TelegramMessageID", result.Value },
                        { "TelegramMessage", msg },
                        { "TelegramGroupID", TelegramGroupID.ToString() },
                        { "PRID", PRID.ToString() }
                    };
                    sql = @"update dbo.TblPurchaseOrder set TelegramGroupID =@TelegramGroupID, TelegramMessageID=@TelegramMessageID, TelegramMessage=@TelegramMessage  where PurchaseOrderID = @PRID;";
                    html.SaveChangeData(sql, false, para);
                }
                else
                {
                    if (PRStatus == 90)
                    {
                        if (ChatID == "0")
                            ChatID = "-986646049";
                        if (MessageID != "0")
                            TelegramMessageID = MessageID.ToString();
                        await notificationTelegram.NotificationTelegramBot("", long.Parse(ChatID), "", result.OutPutFile, msg, false, true, PRID, int.Parse(TelegramMessageID), TelegramMessage, null, true); // -986646049 = check po // -967102140 = approval po // -844276850 = test
                    }
                    else
                        await notificationTelegram.NotificationTelegramBot("", TelegramGroupID, "", result.OutPutFile, msg, false, true, PRID, int.Parse(TelegramMessageID), TelegramMessage, null, true); // check po = -986646049
                    para = new Dictionary<string, string>
                    {
                        { "TelegramMessageID", "0" },
                        { "PRID", PRID.ToString() }
                    };
                    //html.SaveChangeData(sql, false, para);
                }
                if (result.Value.ToString() != "")
                {
                    sql = "insert into TblPOTelegramMessage (POID, TelegramMessageID, TelegramMessage, StatusID, TelegramGroupID)";
                    sql += "values (@PRID, @TelegramMessageID, @TelegramMessage, @StatusID,@TelegramGroupID)";

                    para = new Dictionary<string, string>
                    {
                        { "PRID", PRID.ToString() },
                        { "TelegramMessageID",result.Value },
                        { "TelegramMessage", msg },
                        { "TelegramGroupID", TelegramGroupID.ToString() },
                        { "StatusID", PRStatus.ToString() },
                    };
                    result = html.SaveChangeData(sql, false, para);
                }

                //if (POStatus == 90)
                //{
                //    sql = "SP_INSERT_BILL";
                //    para = new Dictionary<string, string>
                //    {
                //        { "POID", POID.ToString() },
                //        {"IssuedByID", UserName != "" ? "0" : html.UserID },
                //        {"IssuedBy", UserName != "" ? UserName : html.UserFullName },
                //        {"CreateBy", UserName != "" ? UserName : html.UserFullName }
                //    };
                //    SqlResult rst = html.SaveChangeData(sql, true, para, true, true);
                //    DataSet ds = rst.Data;
                //    string BillNumber = ds.Tables[1].Rows[0]["BillNumber"].ToString();
                //    string BillID = ds.Tables[1].Rows[0]["BillID"].ToString();
                //    html.SystemID = ds.Tables[1].Rows[0]["SystemID"].ToString();
                //    html.WarehouseID = ds.Tables[1].Rows[0]["WarehouseID"].ToString();
                //    SqlResult result2 = new SqlResult();

                //    //result2 = await html.URLtoPDF("Profile/InvoiceTMP?VendorID=" + VendorID + "&POID=" + BillID + "&Type=Bill&ReturnHtmlData=false&ReturnAllHtmlData=false&RedirectToPOView=true", "", "Uploads/Vendors/" + VendorName, PONumber);
                //    jsonResult = profileController.InvoiceTMP(int.Parse(VendorID), int.Parse(BillID), "Bill", false, true);

                //    Data = jsonResult.Data.Data;

                //    result2 = await html.HTMLToPDF(Data, "Uploads/Vendors/" + VendorName, PONumber + "_Bill");

                //    OutPutFile1 = result.OutPutFile;
                //    string pdf = result.OutPutFile + "~" + result2.OutPutFile;
                //    //msg = "Dear Team,<br><br> This is auto generated BILL from PO Number: "+ PONumber;
                //    msg = "Dear Team,<br><br>Now this PO Approved By " + (UserName != "" ? UserName : html.UserFullName) + ".";
                //    msg += "<br><br>Auto generated BILL from PO Number: " + PONumber;
                //    msg += "<br><br>Please check attachment file for detail.<br><br>";
                //    msg += "Thank you and best regards,<br><br>";
                //    msg += "NSM ISP BILLING SYSTEM AUTO SEND BILL NOTIFICATION. #" + BillNumber;
                //    //sql = "select TelegramMessageID, TelegramMessage, TelegramGroupID from dbo.TblPurchaseOrder\r\n where PurchaseOrderID =" + POID;
                //    sql = "select TelegramMessageID, TelegramMessage, isnull(TelegramGroupID,0) TelegramGroupID from dbo.TblPOTelegramMessage\r\n where POID =" + POID + " order by TelegramPOMessageID desc";
                //    ds = html.LoadData(sql);
                //    TelegramMessageID = "0";
                //    TelegramMessage = "";
                //    if (ds.Tables[0].Rows.Count > 0)
                //    {
                //        TelegramMessageID = ds.Tables[0].Rows[0]["TelegramMessageID"].ToString();
                //        TelegramMessage = ds.Tables[0].Rows[0]["TelegramMessage"].ToString();
                //        TelegramGroupID = int.Parse(ds.Tables[0].Rows[0]["TelegramGroupID"].ToString());
                //    }
                //    //result = await notificationTelegram.NotificationTelegramBot("", -967102140, "", result.OutPutFile, msg, false, false, 0, int.Parse(TelegramMessageID), TelegramMessage,null, false); // -967102140 = approval po
                //    if (TelegramGroupID == 0 || TelegramGroupID != -967102140)
                //        result = await notificationTelegram.NotificationTelegramBot("", -967102140, "", pdf, msg, false, false, 0, 0, TelegramMessage, null, false); // -986646049 = check po // -967102140 = approval po // -844276850 = test
                //    else
                //        result = await notificationTelegram.NotificationTelegramBot("", -967102140, "", pdf, msg, false, false, 0, int.Parse(TelegramMessageID), TelegramMessage, null, true); // -986646049 = check po // -967102140 = approval po // -844276850 = test
                //    if (!IsService)
                //    {
                //        msg = "Dear Team,<br><br>Now this PO Approved.";
                //        msg += "<br><br>Please check attachment file for detail and CONFIRM RECEIVED.<br><br>";
                //        msg += "Thank you and best regards,<br><br>";
                //        msg += "NSM ISP BILLING SYSTEM AUTO SEND PO NOTIFICATION. #" + PONumber;
                //        result = await notificationTelegram.NotificationTelegramBot("", -4019202888, "", OutPutFile1, msg, false, false, 0, 0, TelegramMessage, null, false); // -986646049 = check po // -967102140 = approval po // -844276850 = test
                //    }
                //}
            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadPO(string PRNumber, string BillID = "0")
        {
            if (PRNumber == "")
                return Json(new { data = "[]" }, JsonRequestBehavior.AllowGet);
            HTML html = new HTML();
            string SQL = @"select distinct p.PurchaseOrderID, p.VendorID, w.WarehouseName, p.Tax,p.Rate, p.CurrencyID, p.CurrencyName,
pd.ItemID, pd.ItemName, pd.ItemDescription, pd.ChartOfAccountID, pd.AccountName, p.ContactName, p.ContactPhone,
pd.QTY [OrderQTY], pd.QTY, pd.UnitPrice, pd.DisP, pd.DisUSD, p.WID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.ClassID, p.ClassName,
format(p.PODate, 'dd/MM/yyyy') PODate, p.PONumber, iv.SellingPrice [SalePrice], pd.UM, pd.UomID, v.VendorName Vendor_Name_Reference_VendorName
from dbo.TblPurchaseOrder p
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblUserWarehouse w on w.WID = p.WID
inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = p.PurchaseOrderID
inner join dbo.TblItem i on i.ItemID = pd.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
            where p.PRNumber = case when @PRNumber <> '' then @PRNumber else p.PRNumber end and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID and p.DomainID = @DomainID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "PRNumber", PRNumber },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, ValidateInput(false)]
        public async Task<JsonResult> SendNotificationToTelegram(int PurchaseOrderID, int VendorID, string PRNumber, string VendorName, string Data, string NeedToApproval, string TelegramUserName)
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            string sql = @"select distinct ui.TelegramUserName, ui.FullName from dbo.TblPurchaseOrder p
inner join dbo.TblUserLogin ui on ui.UserLoginID = p.NeedToCheckUserID where p.PurchaseOrderID = @PurchaseOrderID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"PurchaseOrderID", PurchaseOrderID.ToString() }
            };
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string Telegram_UserName = ds.Tables[0].Rows[0]["TelegramUserName"].ToString();
                string UserNameFullName = ds.Tables[0].Rows[0]["FullName"].ToString();
                //string msg = (TelegramUserName != "" ? TelegramUserName+ "<br><br>" : "") + "Dear " + NeedToApproval + ",<br><br>Now you have new PO Need To Approve that's created by " + html.UserFullName + ".";
                string msg = Telegram_UserName + "<br><br>Dear " + UserNameFullName + ",<br><br>Now you have new PR Need To Check that's created by " + html.UserFullName + ".";
                msg += "<br><br>Please check attachment file for detail.<br><br>Thank you and best regards,<br><br>";
                msg += "NSM ISP BILLING SYSTEM AUTO GENERATED PR. #" + PRNumber;
                VendorName = html.ToUrlSlug(VendorName);
                PRNumber = html.ToUrlSlug(PRNumber);
                sqlresult = await html.HTMLToPDF(Data, "Uploads/Vendors/" + VendorName, PRNumber);
                string OutPutFile = sqlresult.OutPutFile;

                if (sqlresult.Result == ResultType.Successful)
                {
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Check ✍️", callbackData: "CheckVerifyPO~"+PurchaseOrderID+"~-986646049"),
                        //InlineKeyboardButton.WithCallbackData(text: "Approve ✅", callbackData: "ApprovePO~"+PurchaseOrderID),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❎", callbackData: "DisapprovePO~"+PurchaseOrderID+"~-986646049"),//-986646049 = check po
                    }
                });
                    //sqlresult = await notificationTelegram.NotificationTelegramBot("", -967102140, "", sqlresult.OutPutFile, msg, false, true, PurchaseOrderID, 0, "", inlineKeyboard); // -967102140 = approval po
                    sqlresult = await notificationTelegram.NotificationTelegramBot("", -986646049, "", sqlresult.OutPutFile, msg, false, true, PurchaseOrderID, 0, "", inlineKeyboard); // -986646049 = check po
                    if (System.IO.File.Exists(OutPutFile))
                    {
                        System.IO.File.Delete(OutPutFile);
                    }
                    if (sqlresult.Result == ResultType.Successful)
                    {
                        sql = @"update dbo.TblPurchaseOrder set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage 
where PurchaseOrderID = @PurchaseOrderID";
                        para = new Dictionary<string, string> {
                            {"TelegramMessageID",sqlresult.Value },
                            {"PurchaseOrderID",PurchaseOrderID.ToString()},
                            {"TelegramMessage",msg}
                        };
                        html.SaveChangeData(sql, false, para);
                        sql = "insert into TblPOTelegramMessage (POID, TelegramMessageID, TelegramMessage, StatusID, TelegramGroupID)";
                        sql += "values (@POID, @TelegramMessageID, @TelegramMessage, @StatusID,@TelegramGroupID)";

                        para = new Dictionary<string, string>
                        {
                            { "POID", PurchaseOrderID.ToString() },
                            { "TelegramMessageID",sqlresult.Value },
                            { "TelegramMessage", msg },
                            { "TelegramGroupID","-986646049"},
                            { "StatusID", "60" },
                        };
                        sqlresult = html.SaveChangeData(sql, false, para);
                    }
                }
            }
            return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}