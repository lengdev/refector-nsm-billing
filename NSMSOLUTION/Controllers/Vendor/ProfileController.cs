using HtmlAgilityPack;
using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        [Authorize]
        public ActionResult Index(int ID)
        {
            ViewBag.ID = ID.ToString();
            return View();
        }
        [Authorize]
        public JsonResult LoadVendorInfo(int ID)
        {
            HTML html = new HTML();
            string SQL = @"select v.VendorName, v.VendorType, v.[Address],
v.ContactPersonName, v.ContactPhoneNumber, v.Position, '' GoogleMap_FullAddress,
v.Email BillingEmail, '' GoogleMap_Latitude, '' GoogleMap_Longitude, v.Website,
'' CompanyName, '' CompanyNameKH, '' DeskPhone, '' HendPhone, v.Phone, 
v.[Address] VendorAddress, v.Email,
v.BankerName, v.BankerAddress, v.SwiftCode, v.AccountNo, v.AccountName, isnull(v.VendorPicture, '') VendorPicture
from dbo.TblVendor v where v.VendorID = " + ID + " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadVendorProduct(int ID)
        {
            HTML html = new HTML
            {
                ReportTitle = "List Products"
            };
            string SQL = @"select ROW_NUMBER() OVER(order by i.ItemGroup) as [No],
            i.ItemName, pd.ItemDescription, pd.UnitPrice CostPrice, i.SellingPrice,
            i.StockName, i.ItemGroup from dbo.TblVendor v
            inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID
            inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = p.PurchaseOrderID
            inner join dbo.TblItem i on i.ItemID = pd.ItemID
            --inner join dbo.TblSaleInfo s on s.ItemID = pd.ItemID
            where v.VendorID = " + ID + " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " group by i.ItemName, pd.ItemDescription, pd.UnitPrice, i.SellingPrice, i.StockName, i.ItemGroup";
            html.GH.SQL = SQL;
            html.TblID = "tblVendorProduct";
            html.isShowLoadingGrid = false;
            html.OrderBy = "i.ItemGroup";
            html.IsHaveWhereCondition = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName  from dbo.TblVendor v inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = p.PurchaseOrderID inner join dbo.TblItem i on i.ItemID = pd.ItemID where v.VendorID = " + ID + " and lower(i.ItemName) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("i.ItemName"),
                DefaultCheckBok = true,
                Name = "lblVendorPO_Product",
                ID = "TXTVendorPO_Product"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                //new GridField() { Header = "Description", FieldName = "ItemDescription" },
                new GridField() { Header = "Cost Price", FieldName = "CostPrice", TextAlign ="right" },
                new GridField() { Header = "Selling Price", FieldName = "SellingPrice", TextAlign ="right" },
                new GridField() { Header = "Stock Name", FieldName = "StockName", IsGroupByField= true, GroupLabel ="Stock Name: ", Colspan = 4 },
                new GridField() { Header = "Item Group", FieldName = "ItemGroup", IsGroupByField= true, GroupLabel ="Group Name: ", Colspan = 4 },
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadVendorProduct") }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadPurchaseOrder(int ID)
        {
            HTML html = new HTML
            {
                ReportTitle = "List Purchase Order",
                GridEventClick = "onGridPOClick(this);",
                BunSearch = "BtnSearchVendorPO"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "PO Date:",
                DBField = "po.PODate",
                FromDateID = "txtPODate",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Purchase Order#",
                ISAutoComplete = true,
                SQL = "select distinct top 10 p.PONumber from dbo.TblPurchaseOrder p inner join dbo.TblVendor v on v.VendorID = p.VendorID where v.VendorID = " + ID + " and lower(p.PONumber) like @SearchText and p.SystemID = " + html.SystemID + " and p.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("po.PONumber"),
                DefaultCheckBok = true,
                Name = "lblPO",
                ID = "TXTSearchPO"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.TblID = "tblVendorPO";
            html.FormID = "frmVendorPO";
            string sql = @"select ROW_NUMBER() Over(order by po.PurchaseOrderID desc) as [No], po.PurchaseOrderID, po.PODate, po.PONumber, po.Reference,
            v.VendorID, v.VendorName, v.VendorCode, format(po.GrandTotal, '#,#.00') Amount, 
            po.DeliveryDate, po.DeliverTo, case when (po.StatusID = 60 or po.StatusID = 91) then '<div class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] +'</div>' when po.StatusID = 64 then '<div class=""badge badge-pill badge-primary text-uppercase"">' + s.[Status] +'</div>' when (po.StatusID = 15 or po.StatusID = 66) then '<div class=""badge badge-pill badge-warning text-white text-uppercase"">' + s.[Status] +'</div>' else '<div class=""badge badge-pill badge-primary text-uppercase"">' + s.[Status] +'</div>' end [Status],
            isnull(po.Deposit, 0) Deposit, isnull(po.Balance, 0) Balance
            from dbo.TblPurchaseOrder po
            inner join dbo.TblVendor v on v.VendorID = po.VendorID
            inner join dbo.TblStatus s on s.StatusID = po.StatusID
            where v.VendorID = " + ID + " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "po.PurchaseOrderID desc";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            if (html.HasPermission("Add Purchase Order"))
                html.AllowAdd = true;
            html.DialogBoxWidth = "Auto";
            html.AddButton = "New PO";
            //html.AddURL = "PurchaseOrder/CreatePurchaseOrder";
            html.AddFUNCTION = "NewPurchaseOrder(" + ID + ");";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "PurchaseOrderID", FieldName = "PurchaseOrderID", IsHiddenField= true },
                new GridField() { Header = "Purchase Order#", FieldName = "PONumber", },
                new GridField() { Header = "Reference#", FieldName = "Reference", },
                new GridField() { Header = "Date", FieldName = "PODate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Deposit", FieldName = "Deposit", TextAlign = "right", Format="#,#0.00" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right", ClassName="text-red", Format="#,#0.00" },
                new GridField() { Header = "Balance", FieldName = "Balance", TextAlign = "right", ClassName="text-red", Format="#,#0.00" },
                new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Deliver To", FieldName = "DeliverTo", }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadVendorPurchaseOrder") }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadBills(int ID, string Type = "Bill")
        {
            HTML html = new HTML
            {
                ReportTitle = Type == "CreditNote" ? "List Credit Note" : "List Bills",
                GridEventClick = "onGridVendorBill_Click(this, '" + Type + "');",
                BunSearch = "BtnSearchVendorBills" + Type
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtDate_" + Type
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID" + Type,
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblBills b inner join dbo.TblStatus s on s.StatusID = b.StatusID where b.VendorID = " + ID + " and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + (Type == "Bill" ? " and b.StatusID not in (14)" : " and b.StatusID = 14"),
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBillNumber" + Type,
                ISAutoComplete = true,
                Label = "Bill#:",
                DBField = "BillNumber",
                SQLAutoComplete = "select distinct top 10 BillNumber from dbo.TblBills where VendorID = " + ID + " and lower(BillNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and StatusID " + (Type == "CreditNote" ? " = 14" : " not in (14)"),
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtType" + Type,
                Label = "Type:",
                DBField = "Type",
                UseText = true,
                Hide = true,
                Value = Type
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorID" + Type,
                UseText = true,
                Hide = true,
                Value = ID.ToString(),
                Label = "VendorID:",
                DBField = "VendorID"
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = Type == "CreditNote" ? "Credit Note Number" : "Purchase Number",
                ISAutoComplete = true,
                SQL = Type == "CreditNote" ? "select distinct top 10 CreditNoteNumber from dbo.TblBills where VendorID = " + ID + " and lower(CreditNoteNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + (Type == "Bill" ? " and StatusID not in (14)" : " and StatusID = 14") : "select distinct top 10 PONumber from dbo.TblBills where VendorID = " + ID + " and lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + (Type == "Bill" ? " and StatusID not in (14)" : " and StatusID = 14"),
                RadioValue = EncryAndDecry.Encryp(Type == "CreditNote" ? "CreditNoteNumber" : "PONumber") + "," + EncryAndDecry.Encryp("VendorDONumber") + "," + EncryAndDecry.Encryp("VendorInvoiceNumber"),
                DefaultCheckBok = true,
                Name = "lblVendorBill" + Type,
                ID = "TxtVendorBill" + Type
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                Name = "lblVendorBill" + Type,
                ID = "TxtVendorBill" + Type,
                OPTLabel = "Vendor DO Number",
                ISAutoComplete = true,
                SQL = "select distinct top 10 VendorDONumber from dbo.TblBills where VendorID = " + ID + " and lower(VendorDONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + (Type == "Bill" ? " and StatusID not in (14)" : " and StatusID = 14"),
                RadioValue = EncryAndDecry.Encryp(Type == "CreditNote" ? "CreditNoteNumber" : "PONumber") + "," + EncryAndDecry.Encryp("VendorDONumber") + "," + EncryAndDecry.Encryp("VendorInvoiceNumber")
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                Name = "lblVendorBill" + Type,
                ID = "TxtVendorBill" + Type,
                OPTLabel = "VendorInvoiceNumber",
                ISAutoComplete = true,
                SQL = "select distinct top 10 VendorInvoiceNumber from dbo.TblBills where VendorID = " + ID + " and lower(VendorInvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + (Type == "Bill" ? " and StatusID not in (14)" : " and StatusID = 14"),
                RadioValue = EncryAndDecry.Encryp(Type == "CreditNote" ? "CreditNoteNumber" : "PONumber") + "," + EncryAndDecry.Encryp("VendorDONumber") + "," + EncryAndDecry.Encryp("VendorInvoiceNumber")
            });
            html.GH.GridProcedureName = "SP_SHOW_VENDOR_BILL";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.TblID = "TblVendorBill" + Type;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "BillID", FieldName = "BillID", IsHiddenField= true },
                new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                new GridField() { Header = Type=="CreditNote"?"Credit Note#": "Bill#", FieldName = Type=="CreditNote"?"CreditNoteNumber":"BillNumber", },
                new GridField() { Header = Type=="CreditNote"?"Bill#":"Purchase Order#", FieldName = Type=="CreditNote"?"BillNumber":"PONumber", },
                //new GridField() { Header = "Vendor DO#", FieldName = "VendorDONumber", },
                new GridField() { Header = "Vendor Invoice#", FieldName = "VendorInvoiceNumber", },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right", ClassName="text-primary", Format="#,#.00" },
                new GridField() { Header = "Amount Paid", FieldName = "AmountPaid", TextAlign = "right", ClassName="text-info", Format="#,#.00" },
                new GridField() { Header = "Amount Due", FieldName = "AmountDue", TextAlign = "right", ClassName="text-danger", Format="#,#.00" },
                new GridField() { Header = "Bill Date", FieldName = "BillDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
                new GridField() { Header = "Warehouse", FieldName = "Warehouse" },
                new GridField() { Header = "Action", FieldName = "BillID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblBillsDetail" }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadVendorBill" + Type) }, JsonRequestBehavior.AllowGet);
        }
        //[Authorize]
        [ValidateInput(false)]
        public ActionResult InvoiceTMP(int VendorID, int POID, string Type = "PO", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = html.httpContext.Server.MapPath("~/Templates/Invoice.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            ViewBag.POID = POID;
            ViewBag.VendorID = VendorID;
            ViewBag.Type = Type;
            ViewBag.StatusID = "";
            ViewBag.PaymentStatus = "";
            ViewBag.OfficialReceipt = "";
            ViewBag.PurchaseOrderID = "";
            ViewBag.BillType = "";
            ViewBag.BillID = "";
            ViewBag.BillNumber = "";
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            //			SQL = @"SELECT VendorID, BankerName, BankerAddress, SwiftCode,
            //AccountNo, AccountName, Remark
            //from dbo.TblVendor where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID";
            //			para.Clear();
            //			para = new Dictionary<string, string>
            //			{
            //				{"VendorID",VendorID.ToString() },
            //				{"SystemID", html.SystemID },
            //				{"WarehouseID", html.WarehouseID }
            //			};
            //			ds = html.LoadData(SQL, false, para);
            //			if (ds.Tables.Count > 0)
            //			{
            //				if (ds.Tables[0].Rows.Count > 0)
            //				{
            //					DataRow dr = ds.Tables[0].Rows[0];
            //					if (dr["SwiftCode"].ToString() != "")
            //					{
            //						sb.AppendLine("<div class=\"row\">");
            //						sb.AppendLine("<div class=\"col\"><label>Swift Code: " + dr["SwiftCode"].ToString() + "</label></div>");
            //						sb.AppendLine("</div>");
            //					}
            //					if (dr["BankerName"].ToString() != "")
            //					{
            //						sb.AppendLine("<div class=\"row\">");
            //						sb.AppendLine("<div class=\"col\"><label>Banker's Name: " + dr["BankerName"].ToString() + "</label></div>");
            //						sb.AppendLine("</div>");
            //					}
            //					if (dr["AccountNo"].ToString() != "")
            //					{
            //						sb.AppendLine("<div class=\"row\">");
            //						sb.AppendLine("<div class=\"col\"><label>Account No: " + dr["AccountNo"].ToString() + "</label></div>");
            //						sb.AppendLine("</div>");
            //					}
            //					if (dr["AccountName"].ToString() != "")
            //					{
            //						sb.AppendLine("<div class=\"row\">");
            //						sb.AppendLine("<div class=\"col\"><label>Account Name: " + dr["AccountName"].ToString() + "</label></div>");
            //						sb.AppendLine("</div>");
            //					}
            //					if (dr["Remark"].ToString() != "")
            //					{
            //						sb.AppendLine("<div class=\"row\">");
            //						sb.AppendLine("<div class=\"col\"><label>Remark: " + dr["Remark"].ToString() + "</label></div>");
            //						sb.AppendLine("</div>");
            //					}
            //					Text = Text.Replace("{BankInformation}", sb.ToString());
            //				}
            //				else
            //					Text = Text.Replace("{BankInformation}", "");
            //			}
            //			else
            //				Text = Text.Replace("{BankInformation}", "");

            sb.Clear();
            para.Clear();
            if (Type != "ReceivingNoteFromStockInOnly")
                Text = Text.Replace("{showsubtotal}", "");
            string SQL;
            if (Type == "PO")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblPurchaseOrder where PurchaseOrderID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{ReceiveNote}", "");
                Text = Text.Replace("{BillNumber}", "");
                Text = Text.Replace("{checkverify}", "style=\"\"");
                Text = Text.Replace("{IssueByCaption}", "Issued By");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.PONumber, format(p.PODate, 'dd-MM-yyyy') PODate, format(p.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, v.VendorCode, format(p.GrandTotal, '#,#.00') GrandTotal,
format(p.SubTotal, '#,#0.00') SubTotal, format(p.VAT, '#,#0.00') VAT, format(p.Total, '#,#0.00') Total, format(p.DiscountP, '#,#0.00') DiscountP, format(p.DiscountUSD, '#,#0.00') DiscountUSD, 
p.Tax, format(p.Rate, '#') Rate, case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone, p.StatusID,p.StockInStatusID,
case when (p.StatusID = 15 or p.StatusID = 66) then '<div class=""badge badge-pill badge-warning text-uppercase text-white text-lg"">' + s.[Status] + '</div>'
when p.StatusID in(60,91) then '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.ClassName, p.CreateBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate, p.WarehouseName, p.POStatus,
isnull(p.ApproveBy, '') ApproveBy,
--case when isnull(p.POStatus, 0) = 90 then (select Job from dbo.TblUserLogin where UserLoginID = p.ApproveByUserID) else p.ApproveBy end ApproveBy,
p.IssuedBy, format(p.ApprovalDate, 'dd-MM-yyyy HH:mm:ss tt') ApprovalDate, p.DisapproveBy, isnull(p.Deposit, 0) Deposit, isnull(p.Balance, 0) Balance, isnull(p.CheckVerifyBy, '') CheckVerifyBy, format(p.CheckVerifyDate, 'dd-MM-yy HH:mm:ss tt') CheckVerifyDate, p.UserID,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, p.RequestBy, p.PaymentTerms, ul.Job IssuedByPosition, ul1.Job CheckVerifyByPosition, ul2.Job ApprovedByPosition, p.BankInformation
from dbo.TblPurchaseOrder p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
left join dbo.TblUserLogin ul1 on ul1.UserLoginID = p.CheckVerifyByUserID
left join dbo.TblUserLogin ul2 on ul2.UserLoginID = p.ApproveByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
left join dbo.TblUserWarehouse uw on uw.WID = p.WID
where p.PurchaseOrderID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end ItemName, isnull(i.SerialNumber, '') SerialNumber, pd.ItemDescription, format(pd.UnitPrice, '#,#0.000') UnitPrice, format(pd.DisP, '#,#.00') DisP, format(pd.DisUSD, '#,#0.00') DisUSD, format(pd.Amount, '#,#0.00') Amount, pd.UM, format(pd.SalePrice, '#,#0.00') SalePrice, 
						isnull((select top 1 value from TblItemVariant iv
						CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
						from dbo.TblPurchaseOrderDetail pd
						left join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.PurchaseOrderID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                        order by pd.PurchaseOrderDetailID;";
                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
                ViewBag.StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
                ViewBag.POStatus = ds.Tables[2].Rows[0]["POStatus"].ToString();
                ViewBag.StockInStatusID = ds.Tables[2].Rows[0]["StockInStatusID"].ToString();
                Text = Text.Replace("{InvoiceName}", "PO - " + ds.Tables[2].Rows[0]["ClassName"].ToString());
                Text = Text.Replace("{CheckVerifyByCaption}", "Checked By");
                Text = Text.Replace("{CheckVerifyBy}", ds.Tables[2].Rows[0]["CheckVerifyBy"].ToString());
                Text = ds.Tables[2].Rows[0]["CheckVerifyBy"].ToString() != ""
                    ? Text.Replace("{CheckVerifyByPosition}", ds.Tables[2].Rows[0]["CheckVerifyByPosition"].ToString())
                    : Text.Replace("{CheckVerifyByPosition}", "");
                Text = Text.Replace("{CheckVerifyDate}", ds.Tables[2].Rows[0]["CheckVerifyDate"].ToString());
                Text = Text.Replace("{ApprovedBy}", ds.Tables[2].Rows[0]["ApproveBy"].ToString());
                Text = Text.Replace("{ApprovedDate}", ds.Tables[2].Rows[0]["ApprovalDate"].ToString());
                //Text = Text.Replace("{ApprovedByPosition}", "");
                Text = ds.Tables[2].Rows[0]["ApproveBy"].ToString() != ""
                    ? Text.Replace("{ApprovedByPosition}", ds.Tables[2].Rows[0]["ApprovedByPosition"].ToString())
                    : Text.Replace("{ApprovedByPosition}", "");

                //UserID = ds.Tables[2].Rows[0]["UserID"].ToString();
                Text = ViewBag.POStatus == "0" || ViewBag.POStatus == "90" || ViewBag.POStatus == "92"
                    ? Text.Replace("{ApprovedByCaption}", "Approved By")
                    : ViewBag.POStatus == "91"
                    ? Text.Replace("{ApprovedByCaption}", "Dispproved By")
                    : Text.Replace("{ApprovedByCaption}", "Approved By");
            }
            else if (Type == "Bill" || Type == "CreditNote")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblBills where BillID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{ApprovedByCaption}", "Approved By");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
                Text = Text.Replace("{ApprovedByPosition}", "");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.BillNumber, p.CreditNoteNumber, p.PONumber, format(p.BillDate, 'dd-MM-yyyy') PODate, format(p.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, v.VendorCode, format(p.GrandTotal, '#,#.00') GrandTotal,
format(p.SubTotal, '#,#0.00') SubTotal, format(p.VAT, '#,#0.00') VAT, format(p.Total, '#,#0.00') Total, format(p.DiscountP, '#,#0.00') DiscountP, format(p.DiscountUSD, '#,#0.00') DiscountUSD, 
p.Tax, format(p.Rate, '#') Rate, case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone, p.StatusID, p.OfficalReceiptNo, p.[Type], p.ReceiveNoteNumber, format(p.ReceiveDate, 'dd-MM-yyyy') ReceiveDate,
case when p.StatusID = 15 then '<div class=""badge badge-pill badge-warning text-uppercase text-white text-lg"">' + s.[Status] + '</div>'
when p.StatusID = 11 then '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.ClassName, w.WarehouseName, format(isnull(p.AmountPaid,0), '#,#0.00') AmountPaid, p.AmountDue,
p.CreateBy, p.IssuedBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, p.PaymentTerms,
ul.Job IssuedByPosition, '' BankInformation
from dbo.TblBills p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse w on w.WID = p.WID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
                        where p.BillID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end + case when isnull(pd.ItemExpirationDate, '') <> '' then '<br />Expire Date: ' + format(pd.ItemExpirationDate, 'dd-MM-yy') else '' end  ItemName, i.SerialNumber, pd.ItemDescription, format(pd.UnitPrice, '#,#0.000') UnitPrice, format(pd.DisP, '#,#.00') DisP, format(pd.DisUSD, '#,#0.00') DisUSD, format(pd.Amount, '#,#0.00') Amount,
                        pd.ItemExpirationDate, pd.UM, format(pd.SalePrice, '#,#0.00') SalePrice,
						isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
                        from dbo.TblBillsDetail pd
                        left join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.BillID = @POID
                        order by pd.BillDetailID;";

                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
                Text = Text.Replace("{InvoiceName}", Type == "Bill" ? "Vendor Bill - " + ds.Tables[2].Rows[0]["ClassName"].ToString() : " Purchase Return");
                ViewBag.StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
                ViewBag.OfficialReceipt = ds.Tables[2].Rows[0]["OfficalReceiptNo"].ToString();
                ViewBag.BillType = ds.Tables[2].Rows[0]["Type"].ToString();
                ViewBag.BillNumber = Type == "Bill" ? ds.Tables[2].Rows[0]["BillNumber"].ToString() : ds.Tables[2].Rows[0]["CreditNoteNumber"].ToString();
            }
            else if (Type == "ReceivingNote")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblBills where BillID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{ApprovedByCaption}", "Approved By");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
                Text = Text.Replace("{InvoiceName}", "GSRN (Goods Service Receive Note) ");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.BillID, p.BillNumber, p.PONumber, v.VendorCode, 
case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, 
p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone,
p.ReceiveNoteNumber, format(p.ReceiveDate, 'dd-MM-yyyy') ReceiveDate,
case when p.StatusID = 70 then '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.VendorDONumber, p.VendorInvoiceNumber, format(p.BillDate, 'dd-MM-yyyy') BillDate, p.ReferenceNo,
p.CreateBy, p.IssuedBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate,  p.Warehouse WarehouseName,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, '' BankInformation
from dbo.TblStockIn p
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
                        where p.BillID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end + case when isnull(pd.ItemExpirationDate, '') <> '' then '<br />Expire Date: ' + format(pd.ItemExpirationDate, 'dd-MM-yy') else '' end  ItemName, 
						i.SerialNumber, pd.ItemDescription, 
						format(pd.CostPrice, '#,#0.000') UnitPrice, 
						format(pd.Amount, '#,#0.00') Amount,
						pd.ItemExpirationDate, pd.OrderQTY, pd.QTYUnusable, pd.Reason, pd.UM,
						isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
						from dbo.TblStockInDetail pd
						left join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.BillID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                        order by pd.StockInDetailID;";
                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
                ViewBag.BillID = ds.Tables[2].Rows[0]["BillID"].ToString();
            }
            else if (Type == "ReceivingNoteFromStockInOnly")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblStockIn where StockInID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{IssueByCaption}", "Received By");
                Text = Text.Replace("{showsubtotal}", " style=\"visibility: hidden;\"");
                Text = Text.Replace("{ApprovedByCaption}", "Approved By");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
                Text = Text.Replace("{ApprovedByPosition}", "");
                Text = Text.Replace("{InvoiceName}", "GSRN (Goods Service Receive Note) ");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.BillNumber, p.PONumber, v.VendorCode, 
case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, 
p.TermsConditions, uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone,
p.ReceiveNoteNumber, format(p.ReceiveDate, 'dd-MM-yyyy') ReceiveDate,
case when p.StatusID = 70 then '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.VendorDONumber, p.VendorInvoiceNumber, format(p.BillDate, 'dd-MM-yyyy') BillDate, p.ReferenceNo,
p.CreateBy, p.IssuedBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate, p.Warehouse WarehouseName,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, ul.Job IssuedByPosition, '' BankInformation
from dbo.TblStockIn p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
                        where p.StockInID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end + case when isnull(pd.ItemExpirationDate, '') <> '' then '<br />Expire Date: ' + case when isnull(pd.ItemExpirationDate, '') <> '' then format(CAST(pd.ItemExpirationDate as Date), 'dd-MM-yyyy') else '' end else '' end  ItemName,
i.SerialNumber, pd.ItemDescription, 
format(pd.CostPrice, '#,#0.000') UnitPrice, 
format(pd.Amount, '#,#0.00') Amount,
pd.ItemExpirationDate, pd.OrderQTY, pd.QTYUnusable, pd.Reason, pd.UM,
isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
from dbo.TblStockInDetail pd
inner join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.StockInID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                        order by pd.StockInDetailID;";
                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
            }
            DataRow drcomInfo = ds.Tables[0].Rows[0];
            DataRow drvendorInfo = ds.Tables[1].Rows[0];
            DataRow drPOInfo = ds.Tables[2].Rows[0];
            if (Type == "Bill" || Type == "CreditNote")
            {
                Text = Text.Replace("{BankInformation}", "");
                Text = Text.Replace("{From}", "From");
                Text = Text.Replace("{To}", "To");
                Text = Text.Replace("{VendorName}", drPOInfo["WarehouseName"].ToString());//drcomInfo["CompanyName"].ToString());                
                Text = Text.Replace("{VendorAddress}", drPOInfo["DeliverTo"].ToString());
                Text = Text.Replace("{VendorPhone}", drcomInfo["Phone"].ToString());
                Text = Text.Replace("{ContactName}", drPOInfo["ContactName"].ToString());
                Text = Text.Replace("{ContactPhone}", drPOInfo["ContactPhone"].ToString());
                Text = Text.Replace("{VendorEmail}", drcomInfo["Email"].ToString());

                Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
                Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
                Text = Text.Replace("{CompanyPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
                Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());

                Text = Text.Replace("{PODate}", "<b>Bill Date: " + drPOInfo["PODate"].ToString() + "</b>");
                Text = Text.Replace("{BillNumber}", "<b>Bill Number#:</b> " + drPOInfo["BillNumber"].ToString() + "<br />");
                Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");
                if (drPOInfo["Type"].ToString() == "Stock In")
                {
                    Text = Text.Replace("{ReceiveNoteNumber}", "<br /><br /><b>Receive Note Number#:</b> " + drPOInfo["ReceiveNoteNumber"].ToString() + "<br />");
                    Text = Text.Replace("{ReceiveDate}", "<b>ReceiveDate:</b> " + drPOInfo["ReceiveDate"].ToString() + "<br />");
                }
                else
                {
                    Text = Text.Replace("{ReceiveNoteNumber}", "");
                    Text = Text.Replace("{ReceiveDate}", "");
                }
                Text = Text.Replace("{ReceiveNote}", "");
            }
            else if (Type == "PO")
            {
                Text = Text.Replace("{BankInformation}", drPOInfo["BankInformation"].ToString());
                //Text = Text.Replace("{Status}", "");
                Text = Text.Replace("{ReceiveNoteNumber}", "");
                Text = Text.Replace("{ReceiveDate}", "");
                Text = Text.Replace("{From}", "Vendor / Vendor Code: " + drPOInfo["VendorCode"].ToString());
                Text = Text.Replace("{To}", "Delivery Address ");
                Text = Text.Replace("{VendorName}", drPOInfo["WarehouseName"].ToString());//drcomInfo["CompanyName"].ToString());
                Text = Text.Replace("{VendorAddress}", drPOInfo["DeliverTo"].ToString());
                Text = Text.Replace("{VendorPhone}", drcomInfo["Phone"].ToString());
                Text = Text.Replace("{ContactName}", drPOInfo["ContactName"].ToString());
                Text = Text.Replace("{ContactPhone}", drPOInfo["ContactPhone"].ToString());
                Text = Text.Replace("{VendorEmail}", drcomInfo["Email"].ToString());

                Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
                Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
                Text = Text.Replace("{CompanyPhone}", drvendorInfo["HendPhone"].ToString());
                Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());
                Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
                Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());

                Text = Text.Replace("{PODate}", "<b>PO Date: " + drPOInfo["PODate"].ToString() + "</b>");
                Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");
            }
            else if (Type == "ReceivingNote" || Type == "ReceivingNoteFromStockInOnly")
            {
                Text = Text.Replace("{BankInformation}", "");
                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{showsubtotal}", " style=\"visibility: hidden;\"");
                Text = Text.Replace("{RequestBy}", "");
                Text = Text.Replace("{CheckVerifyByCaption}", "Checked By");
                Text = Text.Replace("{CheckVerifyBy}", "");
                Text = Text.Replace("{CheckVerifyDate}", "");
                Text = Text.Replace("{ReceiveNoteNumber}", "");
                Text = Text.Replace("{ReceiveDate}", "");
                Text = Text.Replace("{AmountDue}", "");

                Text = Text.Replace("{From}", "Receiver / Location");
                Text = Text.Replace("{To}", "From Vendor ");
                Text = Text.Replace("{Grand_Total}", "");
                Text = Text.Replace("{CompnayName}", drPOInfo["WarehouseName"].ToString());
                Text = Text.Replace("{CompanyAddress}", drPOInfo["DeliverTo"].ToString());//drcomInfo["CompanyAddress"].ToString());
                Text = Text.Replace("{CompanyPhone}", drcomInfo["Phone"].ToString());
                Text = Text.Replace("{CompanyContactName}", drPOInfo["ContactName"].ToString());
                Text = Text.Replace("{CompanyContactPhone}", drPOInfo["ContactPhone"].ToString());
                Text = Text.Replace("{CompanyEmail}", drcomInfo["Email"].ToString());

                Text = Text.Replace("{VendorName}", drvendorInfo["VendorName"].ToString());
                Text = Text.Replace("{VendorAddress}", drvendorInfo["Address"].ToString());
                Text = Text.Replace("{VendorPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{ContactName}", drvendorInfo["ContactPersonName"].ToString());
                Text = Text.Replace("{ContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{VendorEmail}", drvendorInfo["BillingEmail"].ToString());

                Text = Text.Replace("{PODate}", "<b>Bill Date: " + drPOInfo["BillDate"].ToString() + "</b>");
                Text = Text.Replace("{BillNumber}", "<b>Bill Number#: " + drPOInfo["BillNumber"].ToString() + "<br /></b>");
                Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");
                Text = Text.Replace("{ReceiveNote}",
                    "<b>Receive Note Number#:</b> " + drPOInfo["ReceiveNoteNumber"].ToString() + "<br />" +
                    "<b>Receive Date:</b> " + drPOInfo["ReceiveDate"].ToString() + "<br />" +
                    "<b>Vendor DO Number#:</b> " + drPOInfo["VendorDONumber"].ToString() + "<br />" +
                    "<b>Vendor Invoice Number#:</b> " + drPOInfo["VendorInvoiceNumber"].ToString() + "<br /><br />"
                );

            }
            Text = Text.Replace("{PONumber}", "<b>Order Number#:</b> " + drPOInfo["PONumber"].ToString());
            if (Type != "ReceivingNote" && Type != "ReceivingNoteFromStockInOnly")
            {
                string strp = "<b>Payment Terms:</b> " + drPOInfo["PaymentTerms"].ToString();
                strp += "<br /><b>Payment Due:</b> " + drPOInfo["PaymentDueDate"].ToString();
                Text = Text.Replace("{AmountDue}", "Amount Due " + drPOInfo["PaymentDueDate"].ToString());
                Text = Text.Replace("{PODueDate}", strp);
                //Text = Text.Replace("{VendorCode}", "<b>Vendor Code:</b> " + drPOInfo["VendorCode"].ToString());
                //Text = Text.Replace("{Grand_Total}", "<b>Amount Invoice:</b> " + drPOInfo["GrandTotal"].ToString());
                Text = Type == "PO"
                    ? Text.Replace("{RequestBy}", "<b>Requested By:</b> " + drPOInfo["RequestBy"].ToString())
                    : Text.Replace("{RequestBy}", "");
                Text = Text.Replace("{Grand_Total}", "");
                Text = Text.Replace("{GrandTotal}", drPOInfo["GrandTotal"].ToString());

                Text = Text.Replace("{SubTotal}", drPOInfo["SubTotal"].ToString());
                Text = Text.Replace("{Tax}", drPOInfo["Tax"].ToString());
                Text = Text.Replace("{Rate}", drPOInfo["Rate"].ToString());
                Text = Text.Replace("{VAT}", drPOInfo["VAT"].ToString());
                Text = Text.Replace("{DiscountP}", drPOInfo["DiscountP"].ToString());
                Text = Text.Replace("{DiscountUSD}", drPOInfo["DiscountUSD"].ToString());
                Text = Text.Replace("{TRDisplay}", "style=\"\"");
                if (Type == "PO")
                {
                    Text = Text.Replace("{DepositLabel}", "Deposit:");
                    Text = Text.Replace("{Deposit}", float.Parse(drPOInfo["Deposit"].ToString()).ToString("N2"));
                    Text = Text.Replace("{Balance}", float.Parse(drPOInfo["Balance"].ToString()).ToString("N2"));
                }
                else
                {
                    Text = Text.Replace("{DepositLabel}", "Amount Paid:");
                    Text = Text.Replace("{Deposit}", float.Parse(drPOInfo["AmountPaid"].ToString()).ToString("N2"));
                    Text = Text.Replace("{Balance}", float.Parse(drPOInfo["AmountDue"].ToString()).ToString("N2"));
                }
            }
            else
            {
                Text = Text.Replace("{TRDisplay}", "style=\"display:none\"");
                Text = Text.Replace("{PODueDate}", "");
                Text = Text.Replace("{VendorCode}", "<b>Vendor Code:</b> " + drPOInfo["VendorCode"].ToString());
                Text = Text.Replace("{GrandTotal}", "");
            }

            Text = Text.Replace("{IssuedByName}", drPOInfo["IssuedBy"].ToString());
            Text = Text.Replace("{IssuedByPosition}", drPOInfo["IssuedByPosition"].ToString());
            Text = Text.Replace("{IssuedByDate}", drPOInfo["CreateDate"].ToString());

            Text = Text.Replace("{CustomerNotes}", drPOInfo["CustomerNotes"].ToString());
            Text = Text.Replace("{TermsConditions}", drPOInfo["TermsConditions"].ToString() != "" ? "Terms & Conditions:<br>" + drPOInfo["TermsConditions"].ToString() : "");
            ViewBag.PONumber = drPOInfo["PONumber"].ToString();
            string TblHead = @"
                    <tr><th>#</th><th>Image</th><th>Item</th>
                    <th>QTY</th><th>UM</th>
                    <th class=""text-nowrap"">Unit Price</th>
                    <th class=""text-nowrap"">Sale Price</th>
                    <th class=""text-nowrap"">Discount $</th>
                    <th>Total</th></tr>
                ";//<th class=""text-nowrap"">Discount %</th>
            if (Type == "ReceivingNote" || Type == "ReceivingNoteFromStockInOnly")
            {
                TblHead = @"
                    <tr><th>#</th><th>Image</th><th>Item</th>
                    <th>Order QTY</th>                    
                    <th>Received QTY</th>
                    <th>UM</th>
                    <th>Unusable QTY</th>
                    <th>Reason</th>
                    </tr>
                ";
            }
            Text = Text.Replace("{TblHead}", TblHead);

            int i = 0;
            foreach (DataRow dr in ds.Tables[3].Rows)
            {
                i++;
                sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
                if (dr["ItemImage"].ToString() != "")
                    sb.AppendLine("<td><img src=\"../Uploads/Items/" + dr["ItemImage"].ToString() + "\" class=\"rounded m-2\" height=\"80\"></td>");
                else
                    sb.AppendLine("<td></td>");
                sb.AppendLine("<td>" + dr["ItemName"] + (dr["ItemDescription"].ToString() != "" ? "<br /><div class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</div>" : "") + "</td>");
                //sb.AppendLine("<td>" + dr["ItemDescription"] + "</td>");

                if (Type != "ReceivingNote" && Type != "ReceivingNoteFromStockInOnly")
                {
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UM"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UnitPrice"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["SalePrice"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["DisUSD"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["Amount"] + "</td>");
                }
                else
                {
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["OrderQTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UM"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTYUnusable"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["Reason"] + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();

            string IssuedBySignature = "";
            SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
            para.Clear();
            para.Add("UserID", drPOInfo["IssuedByUserID"].ToString());
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            else if (drPOInfo["IssuedBy"].ToString() != "")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                para.Clear();
                para.Add("UserName", drPOInfo["IssuedBy"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            Text = Text.Replace("{IssuedBySignature}", IssuedBySignature);

            IssuedBySignature = "";
            if (Type == "PO")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drPOInfo["CheckVerifyByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drPOInfo["CheckVerifyBy"].ToString() != "")
                {
                    SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    para.Add("UserName", drPOInfo["CheckVerifyBy"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                        IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                    }
                }
            }
            Text = Text.Replace("{CheckVerifyBySignature}", IssuedBySignature);
            IssuedBySignature = "";
            if ((Type == "PO" && ViewBag.POStatus == "90") || ViewBag.POStatus == "91")
            {
                IssuedBySignature = "";
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drPOInfo["ApproveByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drPOInfo["ApproveBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drPOInfo["ApproveBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }
            }
            Text = Text.Replace("{ApprovedBySignature}", IssuedBySignature);

            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//section")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();

            SQL = Type == "PO"
                ? "select PurchaseOrderID, BillID, [Type], StatusID, OfficalReceiptNo,BillNumber from dbo.TblBills where PurchaseOrderID = @POID"
                : "select PurchaseOrderID, BillID, [Type], StatusID, OfficalReceiptNo,BillNumber from dbo.TblBills where BillID = @POID";

            para.Clear();
            para.Add("POID", POID.ToString());
            DataSet ds1 = html.LoadData(SQL, false, para);
            if (ds1.Tables.Count > 0)
            {
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    ViewBag.BillID = ds1.Tables[0].Rows[0]["BillID"].ToString();
                    ViewBag.BillType = ds1.Tables[0].Rows[0]["Type"].ToString();
                    ViewBag.PaymentStatus = ds1.Tables[0].Rows[0]["StatusID"].ToString();
                    ViewBag.OfficialReceipt = ds1.Tables[0].Rows[0]["OfficalReceiptNo"].ToString();
                    ViewBag.PurchaseOrderID = ds1.Tables[0].Rows[0]["PurchaseOrderID"].ToString();
                    ViewBag.BillNumber = ds1.Tables[0].Rows[0]["BillNumber"].ToString();
                }
            }


            if (Type == "ReceivingNote" || Type == "ReceivingNoteFromStockInOnly")
            {
                return Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return ReturnHtmlData ? Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet) : (ActionResult)View();
        }
        public ActionResult InvoiceEnterBill(int VendorID, int POID, string Type = "PO", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = html.httpContext.Server.MapPath("~/Templates/Invoice.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            ViewBag.POID = POID;
            ViewBag.VendorID = VendorID;
            ViewBag.Type = Type;
            ViewBag.StatusID = "";
            ViewBag.PaymentStatus = "";
            ViewBag.OfficialReceipt = "";
            ViewBag.PurchaseOrderID = "";
            ViewBag.BillType = "";
            ViewBag.BillID = "";
            ViewBag.BillNumber = "";
            ViewBag.PrintDate = DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt");
            // Replace the {PrintDate} placeholder in your text
            Text = Text.Replace("{PrintDate}", ViewBag.PrintDate);


            //Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));

            sb.Clear();
            para.Clear();
            if (Type != "ReceivingNoteFromStockInOnly")
                Text = Text.Replace("{showsubtotal}", "");
            string SQL;
            if (Type == "PO")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblPurchaseOrder where PurchaseOrderID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{ReceiveNote}", "");
                Text = Text.Replace("{BillNumber}", "");
                Text = Text.Replace("{checkverify}", "style=\"\"");
                Text = Text.Replace("{IssueByCaption}", "Issued By");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.PONumber, format(p.PODate, 'dd-MM-yyyy') PODate, format(p.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, v.VendorCode, format(p.GrandTotal, '#,#.00') GrandTotal,
format(p.SubTotal, '#,#0.00') SubTotal, format(p.VAT, '#,#0.00') VAT, format(p.Total, '#,#0.00') Total, format(p.DiscountP, '#,#0.00') DiscountP, format(p.DiscountUSD, '#,#0.00') DiscountUSD, 
p.Tax, format(p.Rate, '#') Rate, case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone, p.StatusID,p.StockInStatusID,
case when (p.StatusID = 15 or p.StatusID = 66) then '<div class=""badge badge-pill badge-warning text-uppercase text-white text-lg"">' + s.[Status] + '</div>'
when p.StatusID in(60,91) then '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.ClassName, p.CreateBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate, p.WarehouseName, p.POStatus,
isnull(p.ApproveBy, '') ApproveBy,
--case when isnull(p.POStatus, 0) = 90 then (select Job from dbo.TblUserLogin where UserLoginID = p.ApproveByUserID) else p.ApproveBy end ApproveBy,
p.IssuedBy, format(p.ApprovalDate, 'dd-MM-yyyy HH:mm:ss tt') ApprovalDate, p.DisapproveBy, isnull(p.Deposit, 0) Deposit, isnull(p.Balance, 0) Balance, isnull(p.CheckVerifyBy, '') CheckVerifyBy, format(p.CheckVerifyDate, 'dd-MM-yy HH:mm:ss tt') CheckVerifyDate, p.UserID,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, p.RequestBy, p.PaymentTerms, ul.Job IssuedByPosition, ul1.Job CheckVerifyByPosition, ul2.Job ApprovedByPosition, p.BankInformation
from dbo.TblPurchaseOrder p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
left join dbo.TblUserLogin ul1 on ul1.UserLoginID = p.CheckVerifyByUserID
left join dbo.TblUserLogin ul2 on ul2.UserLoginID = p.ApproveByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
left join dbo.TblUserWarehouse uw on uw.WID = p.WID
where p.PurchaseOrderID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end ItemName, isnull(i.SerialNumber, '') SerialNumber, pd.ItemDescription, format(pd.UnitPrice, '#,#0.000') UnitPrice, format(pd.DisP, '#,#.00') DisP, format(pd.DisUSD, '#,#0.00') DisUSD, format(pd.Amount, '#,#0.00') Amount, pd.UM, format(pd.SalePrice, '#,#0.00') SalePrice, 
						isnull((select top 1 value from TblItemVariant iv
						CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
						from dbo.TblPurchaseOrderDetail pd
						left join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.PurchaseOrderID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                        order by pd.PurchaseOrderDetailID;";
                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
                ViewBag.StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
                ViewBag.POStatus = ds.Tables[2].Rows[0]["POStatus"].ToString();
                ViewBag.StockInStatusID = ds.Tables[2].Rows[0]["StockInStatusID"].ToString();
                Text = Text.Replace("{InvoiceName}", "PO - " + ds.Tables[2].Rows[0]["ClassName"].ToString());
                Text = Text.Replace("{CheckVerifyByCaption}", "Checked By");
                Text = Text.Replace("{CheckVerifyBy}", ds.Tables[2].Rows[0]["CheckVerifyBy"].ToString());
                Text = ds.Tables[2].Rows[0]["CheckVerifyBy"].ToString() != ""
                    ? Text.Replace("{CheckVerifyByPosition}", ds.Tables[2].Rows[0]["CheckVerifyByPosition"].ToString())
                    : Text.Replace("{CheckVerifyByPosition}", "");
                Text = Text.Replace("{CheckVerifyDate}", ds.Tables[2].Rows[0]["CheckVerifyDate"].ToString());
                Text = Text.Replace("{ApprovedBy}", ds.Tables[2].Rows[0]["ApproveBy"].ToString());
                Text = Text.Replace("{ApprovedDate}", ds.Tables[2].Rows[0]["ApprovalDate"].ToString());
                //Text = Text.Replace("{ApprovedByPosition}", "");
                Text = ds.Tables[2].Rows[0]["ApproveBy"].ToString() != ""
                    ? Text.Replace("{ApprovedByPosition}", ds.Tables[2].Rows[0]["ApprovedByPosition"].ToString())
                    : Text.Replace("{ApprovedByPosition}", "");

                //UserID = ds.Tables[2].Rows[0]["UserID"].ToString();
                Text = ViewBag.POStatus == "0" || ViewBag.POStatus == "90" || ViewBag.POStatus == "92"
                    ? Text.Replace("{ApprovedByCaption}", "Approved By")
                    : ViewBag.POStatus == "91"
                    ? Text.Replace("{ApprovedByCaption}", "Dispproved By")
                    : Text.Replace("{ApprovedByCaption}", "Approved By");
            }
            else if (Type == "Bill" || Type == "CreditNote")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblBills where BillID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{ApprovedByCaption}", "Approved By");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
                Text = Text.Replace("{ApprovedByPosition}", "");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.BillNumber, p.CreditNoteNumber, p.PONumber, format(p.BillDate, 'dd-MM-yyyy') PODate, format(p.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, v.VendorCode, format(p.GrandTotal, '#,#.00') GrandTotal,
format(p.SubTotal, '#,#0.00') SubTotal, format(p.VAT, '#,#0.00') VAT, format(p.Total, '#,#0.00') Total, format(p.DiscountP, '#,#0.00') DiscountP, format(p.DiscountUSD, '#,#0.00') DiscountUSD, 
p.Tax, format(p.Rate, '#') Rate, case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone, p.StatusID, p.OfficalReceiptNo, p.[Type], p.ReceiveNoteNumber, format(p.ReceiveDate, 'dd-MM-yyyy') ReceiveDate,
case when p.StatusID = 15 then '<div class=""badge badge-pill badge-warning text-uppercase text-white text-lg"">' + s.[Status] + '</div>'
when p.StatusID = 11 then '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.ClassName, w.WarehouseName, format(isnull(p.AmountPaid,0), '#,#0.00') AmountPaid, p.AmountDue,
p.CreateBy, p.IssuedBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, p.PaymentTerms,
ul.Job IssuedByPosition, '' BankInformation
from dbo.TblBills p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse w on w.WID = p.WID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
                        where p.BillID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end + case when isnull(pd.ItemExpirationDate, '') <> '' then '<br />Expire Date: ' + format(pd.ItemExpirationDate, 'dd-MM-yy') else '' end  ItemName, i.SerialNumber, pd.ItemDescription, format(pd.UnitPrice, '#,#0.000') UnitPrice, format(pd.DisP, '#,#.00') DisP, format(pd.DisUSD, '#,#0.00') DisUSD, format(pd.Amount, '#,#0.00') Amount,
                        pd.ItemExpirationDate, pd.UM, format(pd.SalePrice, '#,#0.00') SalePrice,
						isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
                        from dbo.TblBillsDetail pd
                        left join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.BillID = @POID
                        order by pd.BillDetailID;";

                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
                Text = Text.Replace("{InvoiceName}", Type == "Bill" ? "Vendor Bill - " + ds.Tables[2].Rows[0]["ClassName"].ToString() : " Purchase Return");
                ViewBag.StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
                ViewBag.OfficialReceipt = ds.Tables[2].Rows[0]["OfficalReceiptNo"].ToString();
                ViewBag.BillType = ds.Tables[2].Rows[0]["Type"].ToString();
                ViewBag.BillNumber = Type == "Bill" ? ds.Tables[2].Rows[0]["BillNumber"].ToString() : ds.Tables[2].Rows[0]["CreditNoteNumber"].ToString();
            }
            else if (Type == "ReceivingNote")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblBills where BillID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{ApprovedByCaption}", "Approved By");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
                Text = Text.Replace("{InvoiceName}", "GSRN (Goods Service Receive Note) ");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.BillID, p.BillNumber, p.PONumber, v.VendorCode, 
case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, 
p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone,
p.ReceiveNoteNumber, format(p.ReceiveDate, 'dd-MM-yyyy') ReceiveDate,
case when p.StatusID = 70 then '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.VendorDONumber, p.VendorInvoiceNumber, format(p.BillDate, 'dd-MM-yyyy') BillDate, p.ReferenceNo,
p.CreateBy, p.IssuedBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate,  p.Warehouse WarehouseName,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, '' BankInformation
from dbo.TblStockIn p
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
                        where p.BillID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end + case when isnull(pd.ItemExpirationDate, '') <> '' then '<br />Expire Date: ' + format(pd.ItemExpirationDate, 'dd-MM-yy') else '' end  ItemName, 
						i.SerialNumber, pd.ItemDescription, 
						format(pd.CostPrice, '#,#0.000') UnitPrice, 
						format(pd.Amount, '#,#0.00') Amount,
						pd.ItemExpirationDate, pd.OrderQTY, pd.QTYUnusable, pd.Reason, pd.UM,
						isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
						from dbo.TblStockInDetail pd
						left join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.BillID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                        order by pd.StockInDetailID;";
                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
                ViewBag.BillID = ds.Tables[2].Rows[0]["BillID"].ToString();
            }
            else if (Type == "ReceivingNoteFromStockInOnly")
            {
                SQL = "select SystemID, WarehouseID from dbo.TblStockIn where StockInID = " + POID;
                ds = html.LoadData(SQL);
                html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{IssueByCaption}", "Received By");
                Text = Text.Replace("{showsubtotal}", " style=\"visibility: hidden;\"");
                Text = Text.Replace("{ApprovedByCaption}", "Approved By");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
                Text = Text.Replace("{ApprovedByPosition}", "");
                Text = Text.Replace("{InvoiceName}", "GSRN (Goods Service Receive Note) ");
                SQL = @"select CompanyName, CompanyAddress, Phone, Email
                        from dbo.TblCompany
                        where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                        select p.BillNumber, p.PONumber, v.VendorCode, 
case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, 
p.TermsConditions, uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone,
p.ReceiveNoteNumber, format(p.ReceiveDate, 'dd-MM-yyyy') ReceiveDate,
case when p.StatusID = 70 then '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.VendorDONumber, p.VendorInvoiceNumber, format(p.BillDate, 'dd-MM-yyyy') BillDate, p.ReferenceNo,
p.CreateBy, p.IssuedBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate, p.Warehouse WarehouseName,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, ul.Job IssuedByPosition, '' BankInformation
from dbo.TblStockIn p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
                        where p.StockInID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                        select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end + case when isnull(pd.ItemExpirationDate, '') <> '' then '<br />Expire Date: ' + case when isnull(pd.ItemExpirationDate, '') <> '' then format(CAST(pd.ItemExpirationDate as Date), 'dd-MM-yyyy') else '' end else '' end  ItemName,
i.SerialNumber, pd.ItemDescription, 
format(pd.CostPrice, '#,#0.000') UnitPrice, 
format(pd.Amount, '#,#0.00') Amount,
pd.ItemExpirationDate, pd.OrderQTY, pd.QTYUnusable, pd.Reason, pd.UM,
isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
from dbo.TblStockInDetail pd
inner join dbo.TblItem i on i.ItemID = pd.ItemID
                        where pd.StockInID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                        order by pd.StockInDetailID;";
                para.Add("VendorID", VendorID.ToString());
                para.Add("POID", POID.ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                ds = html.LoadData(SQL, false, para);
            }
            DataRow drcomInfo = null;
            DataRow drvendorInfo = null;
            DataRow drPOInfo = null;

            // Check table 0
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                drcomInfo = ds.Tables[0].Rows[0];

            // Check table 1
            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                drvendorInfo = ds.Tables[1].Rows[0];

            // Check table 2
            if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                drPOInfo = ds.Tables[2].Rows[0];

            if (Type == "Bill" || Type == "CreditNote")
            {
                Text = Text.Replace("{BankInformation}", "");
                Text = Text.Replace("{From}", "From");
                Text = Text.Replace("{To}", "To");
                Text = Text.Replace("{VendorName}", drPOInfo["WarehouseName"].ToString());//drcomInfo["CompanyName"].ToString());                
                Text = Text.Replace("{VendorAddress}", drPOInfo["DeliverTo"].ToString());
                Text = Text.Replace("{VendorPhone}", drcomInfo["Phone"].ToString());
                Text = Text.Replace("{ContactName}", drPOInfo["ContactName"].ToString());
                Text = Text.Replace("{ContactPhone}", drPOInfo["ContactPhone"].ToString());
                Text = Text.Replace("{VendorEmail}", drcomInfo["Email"].ToString());




                // view bag sending vendor
                ViewBag.VendorName = drPOInfo["WarehouseName"].ToString();
                ViewBag.VendorAddress = drPOInfo["DeliverTo"].ToString();
                //ViewBag.VendorPhone = drcomInfo["Phone"].ToString();
                ViewBag.VendorCompany = drvendorInfo["VendorName"].ToString();
                //ViewBag.VendorEmail = drcomInfo["Email"].ToString();



                // view our company
                ViewBag.CompanyName = drvendorInfo["VendorName"].ToString();
                ViewBag.CompanyAddress = drvendorInfo["Address"].ToString();
                //ViewBag.CompanyPhone =  drvendorInfo["HendPhone"].ToString();
                ViewBag.CompanyContactName = drvendorInfo["ContactPersonName"].ToString();

                // view bag billing

                ViewBag.BillNumber = drPOInfo["BillNumber"].ToString();
                ViewBag.BillDate = drPOInfo["PODate"].ToString();
                ViewBag.OrderNumber = drPOInfo["BillNumber"].ToString();




                Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
                Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
                Text = Text.Replace("{CompanyPhone}", drvendorInfo["ContactPhoneNumber"].ToString());

                Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
                Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());

                // viewbag 
                ViewBag.AttVendor = drvendorInfo["ContactPersonName"].ToString();
                ViewBag.ContactPhone = drvendorInfo["ContactPhoneNumber"].ToString();
                ViewBag.BillingEmail = drvendorInfo["BillingEmail"].ToString();
                ViewBag.AttOurCom = drPOInfo["ContactName"].ToString();

                Text = Text.Replace("{PODate}", "<b>Bill Date: " + drPOInfo["PODate"].ToString() + "</b>");
                Text = Text.Replace("{BillNumber}", "<b>Bill Number#:</b> " + drPOInfo["BillNumber"].ToString() + "<br />");
                Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");
                if (drPOInfo["Type"].ToString() == "Stock In")
                {
                    Text = Text.Replace("{ReceiveNoteNumber}", "<br /><br /><b>Receive Note Number#:</b> " + drPOInfo["ReceiveNoteNumber"].ToString() + "<br />");
                    Text = Text.Replace("{ReceiveDate}", "<b>ReceiveDate:</b> " + drPOInfo["ReceiveDate"].ToString() + "<br />");
                }
                else
                {
                    Text = Text.Replace("{ReceiveNoteNumber}", "");
                    Text = Text.Replace("{ReceiveDate}", "");
                }
                Text = Text.Replace("{ReceiveNote}", "");
            }
            else if (Type == "PO")
            {
                Text = Text.Replace("{BankInformation}", drPOInfo["BankInformation"].ToString());
                //Text = Text.Replace("{Status}", "");
                Text = Text.Replace("{ReceiveNoteNumber}", "");
                Text = Text.Replace("{ReceiveDate}", "");
                Text = Text.Replace("{From}", "Vendor / Vendor Code: " + drPOInfo["VendorCode"].ToString());
                Text = Text.Replace("{To}", "Delivery Address ");
                Text = Text.Replace("{VendorName}", drPOInfo["WarehouseName"].ToString());//drcomInfo["CompanyName"].ToString());
                Text = Text.Replace("{VendorAddress}", drPOInfo["DeliverTo"].ToString());
                Text = Text.Replace("{VendorPhone}", drcomInfo["Phone"].ToString());
                Text = Text.Replace("{ContactName}", drPOInfo["ContactName"].ToString());


                Text = Text.Replace("{ContactPhone}", drPOInfo["ContactPhone"].ToString());
                Text = Text.Replace("{VendorEmail}", drcomInfo["Email"].ToString());

                Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
                Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
                Text = Text.Replace("{CompanyPhone}", drvendorInfo["HendPhone"].ToString());
                Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());
                Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
                Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());

                //view bag
                ViewBag.AttVendor = drvendorInfo["ContactPersonName"].ToString();
                ViewBag.ContactPhone = drvendorInfo["ContactPhoneNumber"].ToString();

                ViewBag.AttOurCom = drPOInfo["ContactName"].ToString();

                Text = Text.Replace("{PODate}", "<b>PO Date: " + drPOInfo["PODate"].ToString() + "</b>");
                Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");
            }
            else if (Type == "ReceivingNote" || Type == "ReceivingNoteFromStockInOnly")
            {
                Text = Text.Replace("{BankInformation}", "");
                Text = Text.Replace("{checkverify}", "style=\"visibility: hidden;\"");
                Text = Text.Replace("{showsubtotal}", " style=\"visibility: hidden;\"");
                Text = Text.Replace("{RequestBy}", "");
                Text = Text.Replace("{CheckVerifyByCaption}", "Checked By");
                Text = Text.Replace("{CheckVerifyBy}", "");
                Text = Text.Replace("{CheckVerifyDate}", "");
                Text = Text.Replace("{ReceiveNoteNumber}", "");
                Text = Text.Replace("{ReceiveDate}", "");
                Text = Text.Replace("{AmountDue}", "");

                Text = Text.Replace("{From}", "Receiver / Location");
                Text = Text.Replace("{To}", "From Vendor ");
                Text = Text.Replace("{Grand_Total}", "");
                Text = Text.Replace("{CompnayName}", drPOInfo["WarehouseName"].ToString());
                Text = Text.Replace("{CompanyAddress}", drPOInfo["DeliverTo"].ToString());//drcomInfo["CompanyAddress"].ToString());
                Text = Text.Replace("{CompanyPhone}", drcomInfo["Phone"].ToString());
                Text = Text.Replace("{CompanyContactName}", drPOInfo["ContactName"].ToString());
                Text = Text.Replace("{CompanyContactPhone}", drPOInfo["ContactPhone"].ToString());
                Text = Text.Replace("{CompanyEmail}", drcomInfo["Email"].ToString());

                Text = Text.Replace("{VendorName}", drvendorInfo["VendorName"].ToString());
                Text = Text.Replace("{VendorAddress}", drvendorInfo["Address"].ToString());
                Text = Text.Replace("{VendorPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{ContactName}", drvendorInfo["ContactPersonName"].ToString());

                Text = Text.Replace("{ContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
                Text = Text.Replace("{VendorEmail}", drvendorInfo["BillingEmail"].ToString());
                //view bag

                ViewBag.AttVendor = drvendorInfo["ContactPersonName"].ToString();
                ViewBag.ContactPhone = drvendorInfo["ContactPhoneNumber"].ToString();
                ViewBag.AttOurCom = drPOInfo["ContactName"].ToString();

                Text = Text.Replace("{PODate}", "<b>Bill Date: " + drPOInfo["BillDate"].ToString() + "</b>");
                Text = Text.Replace("{BillNumber}", "<b>Bill Number#: " + drPOInfo["BillNumber"].ToString() + "<br /></b>");
                Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");
                Text = Text.Replace("{ReceiveNote}",
                    "<b>Receive Note Number#:</b> " + drPOInfo["ReceiveNoteNumber"].ToString() + "<br />" +
                    "<b>Receive Date:</b> " + drPOInfo["ReceiveDate"].ToString() + "<br />" +
                    "<b>Vendor DO Number#:</b> " + drPOInfo["VendorDONumber"].ToString() + "<br />" +
                    "<b>Vendor Invoice Number#:</b> " + drPOInfo["VendorInvoiceNumber"].ToString() + "<br /><br />"
                );

            }
            Text = Text.Replace("{PONumber}", "<b>Order Number#:</b> " + drPOInfo["PONumber"].ToString());

            //viewbag
            ViewBag.orderNumber = drPOInfo["PONumber"].ToString();
            if (Type != "ReceivingNote" && Type != "ReceivingNoteFromStockInOnly")
            {
                string strp = "<b>Payment Terms:</b> " + drPOInfo["PaymentTerms"].ToString();
                strp += "<br /><b>Payment Due:</b> " + drPOInfo["PaymentDueDate"].ToString();

                // view bag
                ViewBag.paymentTerm = drPOInfo["PaymentTerms"].ToString();
                ViewBag.paymentDue = drPOInfo["PaymentDueDate"].ToString();

                Text = Text.Replace("{AmountDue}", "Amount Due " + drPOInfo["PaymentDueDate"].ToString());
                Text = Text.Replace("{PODueDate}", strp);
                //Text = Text.Replace("{VendorCode}", "<b>Vendor Code:</b> " + drPOInfo["VendorCode"].ToString());
                //Text = Text.Replace("{Grand_Total}", "<b>Amount Invoice:</b> " + drPOInfo["GrandTotal"].ToString());
                Text = Type == "PO"
                    ? Text.Replace("{RequestBy}", "<b>Requested By:</b> " + drPOInfo["RequestBy"].ToString())
                    : Text.Replace("{RequestBy}", "");
                Text = Text.Replace("{Grand_Total}", "");
                Text = Text.Replace("{GrandTotal}", drPOInfo["GrandTotal"].ToString());

                Text = Text.Replace("{SubTotal}", drPOInfo["SubTotal"].ToString());
                Text = Text.Replace("{Tax}", drPOInfo["Tax"].ToString());
                Text = Text.Replace("{Rate}", drPOInfo["Rate"].ToString());
                Text = Text.Replace("{VAT}", drPOInfo["VAT"].ToString());
                Text = Text.Replace("{DiscountP}", drPOInfo["DiscountP"].ToString());
                Text = Text.Replace("{DiscountUSD}", drPOInfo["DiscountUSD"].ToString());
                Text = Text.Replace("{TRDisplay}", "style=\"\"");


                ViewBag.SubTotal = drPOInfo["SubTotal"].ToString();
                ViewBag.Tax = drPOInfo["Tax"].ToString();
                ViewBag.DiscuntUsa = drPOInfo["DiscountUSD"].ToString();
                ViewBag.VAT = drPOInfo["VAT"].ToString();
                ViewBag.GrandTotal = drPOInfo["GrandTotal"].ToString();

                if (Type == "PO")
                {
                    Text = Text.Replace("{DepositLabel}", "Deposit:");
                    Text = Text.Replace("{Deposit}", float.Parse(drPOInfo["Deposit"].ToString()).ToString("N2"));
                    Text = Text.Replace("{Balance}", float.Parse(drPOInfo["Balance"].ToString()).ToString("N2"));
                }
                else
                {
                    Text = Text.Replace("{DepositLabel}", "Amount Paid:");
                    Text = Text.Replace("{Deposit}", float.Parse(drPOInfo["AmountPaid"].ToString()).ToString("N2"));
                    ViewBag.AmountPaid = drPOInfo["AmountPaid"].ToString();
                    ViewBag.Balance = drPOInfo["AmountDue"].ToString();
                    Text = Text.Replace("{Balance}", float.Parse(drPOInfo["AmountDue"].ToString()).ToString("N2"));
                }
            }
            else
            {
                Text = Text.Replace("{TRDisplay}", "style=\"display:none\"");
                Text = Text.Replace("{PODueDate}", "");
                Text = Text.Replace("{VendorCode}", "<b>Vendor Code:</b> " + drPOInfo["VendorCode"].ToString());
                Text = Text.Replace("{GrandTotal}", "");
            }

            Text = Text.Replace("{IssuedByName}", drPOInfo["IssuedBy"].ToString());
            Text = Text.Replace("{IssuedByPosition}", drPOInfo["IssuedByPosition"].ToString());
            Text = Text.Replace("{IssuedByDate}", drPOInfo["CreateDate"].ToString());

            Text = Text.Replace("{CustomerNotes}", drPOInfo["CustomerNotes"].ToString());

            //view bag
            ViewBag.IssuedBy = drPOInfo["IssuedBy"].ToString();
            ViewBag.IssuedByPosition = drPOInfo["IssuedByPosition"].ToString();
            ViewBag.IssuedByDate = drPOInfo["CreateDate"].ToString();
            ViewBag.CustomerNote = drPOInfo["CustomerNotes"].ToString();
            Text = Text.Replace("{TermsConditions}", drPOInfo["TermsConditions"].ToString() != "" ? "Terms & Conditions:<br>" + drPOInfo["TermsConditions"].ToString() : "");
            ViewBag.PONumber = drPOInfo["PONumber"].ToString();
            string TblHead = @"
                    <tr><th>#</th><th>Image</th><th>Item</th>
                    <th>QTY</th><th>UM</th>
                    <th class=""text-nowrap"">Unit Price</th>
                    <th class=""text-nowrap"">Sale Price</th>
                    <th class=""text-nowrap"">Discount $</th>
                    <th>Total</th></tr>
                ";//<th class=""text-nowrap"">Discount %</th>
            if (Type == "ReceivingNote" || Type == "ReceivingNoteFromStockInOnly")
            {
                TblHead = @"
                    <tr><th>#</th><th>Image</th><th>Item</th>
                    <th>Order QTY</th>                    
                    <th>Received QTY</th>
                    <th>UM</th>
                    <th>Unusable QTY</th>
                    <th>Reason</th>
                    </tr>
                ";
            }
            Text = Text.Replace("{TblHead}", TblHead);

            ViewBag.TableData = ds.Tables[3];

            int i = 0;
            foreach (DataRow dr in ds.Tables[3].Rows)
            {
                i++;
                sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
                if (dr["ItemImage"].ToString() != "")
                    sb.AppendLine("<td><img src=\"../Uploads/Items/" + dr["ItemImage"].ToString() + "\" class=\"rounded m-2\" height=\"80\"></td>");
                else
                    sb.AppendLine("<td></td>");
                sb.AppendLine("<td>" + dr["ItemName"] + (dr["ItemDescription"].ToString() != "" ? "<br /><div class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</div>" : "") + "</td>");
                //sb.AppendLine("<td>" + dr["ItemDescription"] + "</td>");

                if (Type != "ReceivingNote" && Type != "ReceivingNoteFromStockInOnly")
                {
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UM"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UnitPrice"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["SalePrice"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["DisUSD"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["Amount"] + "</td>");
                }
                else
                {
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["OrderQTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UM"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTYUnusable"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["Reason"] + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();
            string IssuedBySignature = "";
            SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
            para.Clear();
            para.Add("UserID", drPOInfo["IssuedByUserID"].ToString());
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                    ViewBag.IssuedBySignature = aa[0];
                }
            }
            else if (drPOInfo["IssuedBy"].ToString() != "")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                para.Clear();
                para.Add("UserName", drPOInfo["IssuedBy"].ToString());

                // viewbag

                ViewBag.IssuedBy = drPOInfo["IssuedBy"].ToString();

                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                    ViewBag.IssuedBySignature = aa[0];
                }
            }
            Text = Text.Replace("{IssuedBySignature}", IssuedBySignature);

            IssuedBySignature = "";
            if (Type == "PO")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drPOInfo["CheckVerifyByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drPOInfo["CheckVerifyBy"].ToString() != "")
                {
                    SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    para.Add("UserName", drPOInfo["CheckVerifyBy"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                        IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                    }
                }
            }
            Text = Text.Replace("{CheckVerifyBySignature}", IssuedBySignature);

            IssuedBySignature = "";
            if ((Type == "PO" && ViewBag.POStatus == "90") || ViewBag.POStatus == "91")
            {
                IssuedBySignature = "";
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drPOInfo["ApproveByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drPOInfo["ApproveBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drPOInfo["ApproveBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }
            }
            Text = Text.Replace("{ApprovedBySignature}", IssuedBySignature);

            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//section")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();

            SQL = Type == "PO"
                ? "select PurchaseOrderID, BillID, [Type], StatusID, OfficalReceiptNo,BillNumber from dbo.TblBills where PurchaseOrderID = @POID"
                : "select PurchaseOrderID, BillID, [Type], StatusID, OfficalReceiptNo,BillNumber from dbo.TblBills where BillID = @POID";

            para.Clear();
            para.Add("POID", POID.ToString());
            DataSet ds1 = html.LoadData(SQL, false, para);
            if (ds1.Tables.Count > 0)
            {
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    ViewBag.BillID = ds1.Tables[0].Rows[0]["BillID"].ToString();
                    ViewBag.BillType = ds1.Tables[0].Rows[0]["Type"].ToString();
                    ViewBag.PaymentStatus = ds1.Tables[0].Rows[0]["StatusID"].ToString();
                    ViewBag.OfficialReceipt = ds1.Tables[0].Rows[0]["OfficalReceiptNo"].ToString();
                    ViewBag.PurchaseOrderID = ds1.Tables[0].Rows[0]["PurchaseOrderID"].ToString();
                    ViewBag.BillNumber = ds1.Tables[0].Rows[0]["BillNumber"].ToString();
                }
            }


            if (Type == "ReceivingNote" || Type == "ReceivingNoteFromStockInOnly")
            {
                return Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return ReturnHtmlData ? Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet) : (ActionResult)View();
        }

        [ValidateInput(false)]
        public ActionResult InvoiceTMPR(int VendorID, int PRID, string Type = "PR", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = html.httpContext.Server.MapPath("~/Templates/InvoicePR.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            ViewBag.PRID = PRID;
            ViewBag.VendorID = VendorID;
            ViewBag.Type = Type;
            ViewBag.StatusID = "";
            ViewBag.PaymentStatus = "";
            ViewBag.OfficialReceipt = "";
            ViewBag.PurchaseOrderID = "";
            ViewBag.BillType = "";
            ViewBag.BillID = "";
            ViewBag.BillNumber = "";
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            string SQL = @"SELECT VendorID, BankerName, BankerAddress, SwiftCode,
AccountNo, AccountName, Remark
from dbo.TblVendor where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID";
            para.Clear();
            para = new Dictionary<string, string>
            {
                {"VendorID",VendorID.ToString() },
                {"SystemID", html.SystemID },
                {"WarehouseID", html.WarehouseID }
            };
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr["SwiftCode"].ToString() != "")
                    {
                        sb.AppendLine("<div class=\"row\">");
                        sb.AppendLine("<div class=\"col\"><label>Swift Code: " + dr["SwiftCode"].ToString() + "</label></div>");
                        sb.AppendLine("</div>");
                    }
                    if (dr["BankerName"].ToString() != "")
                    {
                        sb.AppendLine("<div class=\"row\">");
                        sb.AppendLine("<div class=\"col\"><label>Banker's Name: " + dr["BankerName"].ToString() + "</label></div>");
                        sb.AppendLine("</div>");
                    }
                    if (dr["AccountNo"].ToString() != "")
                    {
                        sb.AppendLine("<div class=\"row\">");
                        sb.AppendLine("<div class=\"col\"><label>Account No: " + dr["AccountNo"].ToString() + "</label></div>");
                        sb.AppendLine("</div>");
                    }
                    if (dr["AccountName"].ToString() != "")
                    {
                        sb.AppendLine("<div class=\"row\">");
                        sb.AppendLine("<div class=\"col\"><label>Account Name: " + dr["AccountName"].ToString() + "</label></div>");
                        sb.AppendLine("</div>");
                    }
                    if (dr["Remark"].ToString() != "")
                    {
                        sb.AppendLine("<div class=\"row\">");
                        sb.AppendLine("<div class=\"col\"><label>Remark: " + dr["Remark"].ToString() + "</label></div>");
                        sb.AppendLine("</div>");
                    }
                    Text = Text.Replace("{BankInformation}", sb.ToString());
                }
                else
                    Text = Text.Replace("{BankInformation}", "");
            }
            else
                Text = Text.Replace("{BankInformation}", "");
            sb.Clear();
            para.Clear();

            SQL = "select SystemID, WarehouseID from dbo.TblPurchaseOrder where PurchaseOrderID = " + PRID;
            ds = html.LoadData(SQL);
            html.SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
            html.WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

            Text = Text.Replace("{ReceiveNote}", "");
            Text = Text.Replace("{BillNumber}", "");
            Text = Text.Replace("{checkverify}", "style=\"\"");
            SQL = @"select CompanyName, CompanyAddress, Phone, Email
                    from dbo.TblCompany
                    where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                    select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                    select p.PRNumber, format(p.PRDate, 'dd-MM-yyyy') PRDate, format(p.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, v.VendorCode, format(p.GrandTotal, '#,#.00') GrandTotal,
format(p.SubTotal, '#,#0.00') SubTotal, format(p.VAT, '#,#0.00') VAT, format(p.Total, '#,#0.00') Total, format(p.DiscountP, '#,#0.00') DiscountP, format(p.DiscountUSD, '#,#0.00') DiscountUSD, 
p.Tax, format(p.Rate, '#') Rate, case when CustomerNotes <> '' then 'Remark:<br />' + p.CustomerNotes else '' end CustomerNotes, p.TermsConditions, 
uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
, p.ContactName, p.ContactPhone, p.StatusID, p.IsConvertedToPO,
case when (p.StatusID = 15 or p.StatusID = 66) then '<div class=""badge badge-pill badge-warning text-uppercase text-white text-lg"">' + s.[Status] + '</div>'
when p.StatusID in(60,91) then '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
else '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
end [Status], p.ClassName, p.CreateBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate, p.WarehouseName, p.PRStatus,
isnull(p.ApproveBy, '') ApproveBy,
--case when isnull(p.PRStatus, 0) = 90 then (select Job from dbo.TblUserLogin where UserLoginID = p.ApproveByUserID) else p.ApproveBy end ApproveBy,
p.IssuedBy, format(p.ApprovalDate, 'dd-MM-yyyy HH:mm:ss tt') ApprovalDate, p.DisapproveBy, isnull(p.Deposit, 0) Deposit, isnull(p.Balance, 0) Balance, isnull(p.CheckVerifyBy, '') CheckVerifyBy, format(p.CheckVerifyDate, 'dd-MM-yy HH:mm:ss tt') CheckVerifyDate, p.UserID,
p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID, p.CheckVerifyByUserID, p.RequestBy, p.PaymentTerms, ul.Job IssuedByPosition, ul1.Job CheckVerifyByPosition, ul2.Job ApprovedByPosition
from dbo.TblPurchaseOrder p
inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
left join dbo.TblUserLogin ul1 on ul1.UserLoginID = p.CheckVerifyByUserID
left join dbo.TblUserLogin ul2 on ul2.UserLoginID = p.ApproveByUserID
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblStatus s on s.StatusID = p.StatusID
inner join dbo.TblUserWarehouse uw on uw.WID = p.WID
where p.PurchaseOrderID = @PRID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;

                    select pd.QTY, pd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end ItemName, isnull(i.SerialNumber, '') SerialNumber, pd.ItemDescription, format(pd.UnitPrice, '#,#0.000') UnitPrice, format(pd.DisP, '#,#.00') DisP, format(pd.DisUSD, '#,#0.00') DisUSD, format(pd.Amount, '#,#0.00') Amount, pd.UM, format(pd.SalePrice, '#,#0.00') SalePrice,
					isnull((select top 1 value from TblItemVariant iv CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
                    from dbo.TblPurchaseOrderDetail pd
                    left join dbo.TblItem i on i.ItemID = pd.ItemID
                    where pd.PurchaseOrderID = @PRID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
                    order by pd.PurchaseOrderDetailID;";
            para.Add("VendorID", VendorID.ToString());
            para.Add("PRID", PRID.ToString());
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            ds = html.LoadData(SQL, false, para);
            ViewBag.StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
            ViewBag.PRStatus = ds.Tables[2].Rows[0]["PRStatus"].ToString();
            ViewBag.IsConvertedToPO = ds.Tables[2].Rows[0]["IsConvertedToPO"].ToString();
            Text = Text.Replace("{InvoiceName}", "PR - " + ds.Tables[2].Rows[0]["ClassName"].ToString());
            Text = Text.Replace("{CheckVerifyByCaption}", "Checked By");
            Text = Text.Replace("{CheckVerifyBy}", ds.Tables[2].Rows[0]["CheckVerifyBy"].ToString());
            Text = ds.Tables[2].Rows[0]["CheckVerifyBy"].ToString() != ""
                ? Text.Replace("{CheckVerifyByPosition}", ds.Tables[2].Rows[0]["CheckVerifyByPosition"].ToString())
                : Text.Replace("{CheckVerifyByPosition}", "");
            Text = Text.Replace("{CheckVerifyDate}", ds.Tables[2].Rows[0]["CheckVerifyDate"].ToString());
            Text = Text.Replace("{ApprovedBy}", ds.Tables[2].Rows[0]["ApproveBy"].ToString());
            Text = Text.Replace("{ApprovedDate}", ds.Tables[2].Rows[0]["ApprovalDate"].ToString());
            //Text = Text.Replace("{ApprovedByPosition}", "");
            Text = ds.Tables[2].Rows[0]["ApproveBy"].ToString() != ""
                ? Text.Replace("{ApprovedByPosition}", ds.Tables[2].Rows[0]["ApprovedByPosition"].ToString())
                : Text.Replace("{ApprovedByPosition}", "");

            //UserID = ds.Tables[2].Rows[0]["UserID"].ToString();
            Text = ViewBag.PRStatus == "0" || ViewBag.PRStatus == "90" || ViewBag.PRStatus == "92"
                ? Text.Replace("{ApprovedByCaption}", "Approved By")
                : ViewBag.PRStatus == "91"
                ? Text.Replace("{ApprovedByCaption}", "Dispproved By")
                : Text.Replace("{ApprovedByCaption}", "Approved By");

            DataRow drcomInfo = ds.Tables[0].Rows[0];
            DataRow drvendorInfo = ds.Tables[1].Rows[0];
            DataRow drPOInfo = ds.Tables[2].Rows[0];

            //Text = Text.Replace("{Status}", "");
            Text = Text.Replace("{ReceiveNoteNumber}", "");
            Text = Text.Replace("{ReceiveDate}", "");
            Text = Text.Replace("{From}", "Vendor / Vendor Code: " + drPOInfo["VendorCode"].ToString());
            Text = Text.Replace("{To}", "Delivery Address ");
            Text = Text.Replace("{VendorName}", drPOInfo["WarehouseName"].ToString());//drcomInfo["CompanyName"].ToString());
            Text = Text.Replace("{VendorAddress}", drPOInfo["DeliverTo"].ToString());
            Text = Text.Replace("{VendorPhone}", drcomInfo["Phone"].ToString());


            // view bag sending vendor
            ViewBag.VendorName = drPOInfo["WarehouseName"].ToString();
            ViewBag.VendorAddress = drPOInfo["DeliverTo"].ToString();
            ViewBag.VendorPhone = drcomInfo["Phone"].ToString();
            //ViewBag.VendorCompany = drvendorInfo["VendorName"].ToString();
            ViewBag.VendorEmail = drcomInfo["Email"].ToString();


            // view our company
            ViewBag.CompanyName = drvendorInfo["VendorName"].ToString();
            ViewBag.CompanyAddress = drvendorInfo["Address"].ToString();
            //ViewBag.CompanyPhone =  drvendorInfo["HendPhone"].ToString();
            ViewBag.AttnNamePerson = drvendorInfo["ContactPersonName"].ToString();

            // view bag billing

            //ViewBag.BillNumber = drPOInfo["BillNumber"].ToString();
            //ViewBag.BillDate = drPOInfo["PODate"].ToString();
            //ViewBag.OrderNumber = drPOInfo["BillNumber"].ToString();
            //ViewBag.paymentTerm  = drPOInfo["PaymentTerms"].ToString();
            //ViewBag.paymentDue = drPOInfo["PaymentDueDate"].ToString();







            //Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
            //Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
            //Text = Text.Replace("{CompanyPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
            //Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
            //Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());
            //Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());

            //Text = Text.Replace("{PODate}", "<b>Bill Date: " + drPOInfo["PODate"].ToString() + "</b>");
            //Text = Text.Replace("{BillNumber}", "<b>Bill Number#:</b> " + drPOInfo["BillNumber"].ToString() + "<br />");
            //Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");




            Text = Text.Replace("{ContactName}", drPOInfo["ContactName"].ToString());
            Text = Text.Replace("{ContactPhone}", drPOInfo["ContactPhone"].ToString());
            Text = Text.Replace("{VendorEmail}", drcomInfo["Email"].ToString());

            Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
            Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
            Text = Text.Replace("{CompanyPhone}", drvendorInfo["HendPhone"].ToString());
            Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());
            Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
            Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());

            Text = Text.Replace("{PRDate}", "<b>PR Date: " + drPOInfo["PRDate"].ToString() + "</b>");
            Text = Text.Replace("{Status}", "<br />" + drPOInfo["Status"].ToString() + "<br /><br />");

            Text = Text.Replace("{PRNumber}", "<b>Request Number#:</b> " + drPOInfo["PRNumber"].ToString());
            Text = Text.Replace("{RequestBy}", "<b>Requested By:</b> " + drPOInfo["RequestBy"].ToString());

            Text = Text.Replace("{SubTotal}", drPOInfo["SubTotal"].ToString());
            Text = Text.Replace("{Tax}", drPOInfo["Tax"].ToString());
            Text = Text.Replace("{Rate}", drPOInfo["Rate"].ToString());
            Text = Text.Replace("{VAT}", drPOInfo["VAT"].ToString());
            Text = Text.Replace("{DiscountP}", drPOInfo["DiscountP"].ToString());
            Text = Text.Replace("{DiscountUSD}", drPOInfo["DiscountUSD"].ToString());
            Text = Text.Replace("{GrandTotal}", drPOInfo["GrandTotal"].ToString());

            Text = Text.Replace("{TRDisplay}", "style=\"display:none\"");
            Text = Text.Replace("{VendorCode}", "<b>Vendor Code:</b> " + drPOInfo["VendorCode"].ToString());

            Text = Text.Replace("{IssuedByName}", drPOInfo["IssuedBy"].ToString());
            Text = Text.Replace("{IssuedByPosition}", drPOInfo["IssuedByPosition"].ToString());
            // viewbag

            ViewBag.IssuedBy = drPOInfo["IssuedBy"].ToString();
            ViewBag.IssuedByPosition = drPOInfo["IssuedByPosition"].ToString();
            ViewBag.IssuedByDate = drPOInfo["CreateDate"].ToString();
            Text = Text.Replace("{IssuedByDate}", drPOInfo["CreateDate"].ToString());


            Text = Text.Replace("{CustomerNotes}", drPOInfo["CustomerNotes"].ToString());
            Text = Text.Replace("{TermsConditions}", drPOInfo["TermsConditions"].ToString() != "" ? "Terms & Conditions:<br>" + drPOInfo["TermsConditions"].ToString() : "");
            ViewBag.PRNumber = drPOInfo["PRNumber"].ToString();
            string TblHead = @"
                    <tr><th>#</th><th>Item</th>
                    <th>QTY</th><th>UM</th>
                    <th class=""text-nowrap"">Unit Price</th>
                    <th class=""text-nowrap"">Sale Price</th>
                    <th class=""text-nowrap"">Discount $</th>
                    <th>Total</th></tr>
                ";//<th class=""text-nowrap"">Discount %</th>

            Text = Text.Replace("{TblHead}", TblHead);

            int i = 0;
            foreach (DataRow dr in ds.Tables[3].Rows)
            {
                i++;
                sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
                sb.AppendLine("<td>" + dr["ItemName"] + (dr["ItemDescription"].ToString() != "" ? "<br /><div class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</div>" : "") + "</td>");
                //sb.AppendLine("<td>" + dr["ItemDescription"] + "</td>");

                if (Type != "ReceivingNote" && Type != "ReceivingNoteFromStockInOnly")
                {
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UM"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UnitPrice"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["SalePrice"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["DisUSD"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["Amount"] + "</td>");
                }
                else
                {
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["OrderQTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["UM"] + "</td>");
                    sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTYUnusable"].ToString()).ToString("#,#0.000") + "</td>");
                    sb.AppendLine("<td align=\"center\">" + dr["Reason"] + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();
            string IssuedBySignature = "";
            SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
            para.Clear();
            para.Add("UserID", drPOInfo["IssuedByUserID"].ToString());
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            else if (drPOInfo["IssuedBy"].ToString() != "")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                para.Clear();
                para.Add("UserName", drPOInfo["IssuedBy"].ToString());
                ViewBag.IssuedBy = drPOInfo["IssuedBy"].ToString();
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            Text = Text.Replace("{IssuedBySignature}", IssuedBySignature);

            IssuedBySignature = "";
            if (Type == "PR")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drPOInfo["CheckVerifyByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drPOInfo["CheckVerifyBy"].ToString() != "")
                {
                    SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    para.Add("UserName", drPOInfo["CheckVerifyBy"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                        IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                    }
                }
            }
            Text = Text.Replace("{CheckVerifyBySignature}", IssuedBySignature);
            IssuedBySignature = "";
            if ((Type == "PR" && ViewBag.PRStatus == "90") || ViewBag.PRStatus == "91")
            {
                IssuedBySignature = "";
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drPOInfo["ApproveByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drPOInfo["ApproveBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drPOInfo["ApproveBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            IssuedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }
            }
            Text = Text.Replace("{ApprovedBySignature}", IssuedBySignature);

            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//section")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();
            return View();
        }
        [ValidateInput(false)]
        public ActionResult InvoiceTMPRCA(int RequestFormID, string Type = "RequestForm", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = html.httpContext.Server.MapPath("~/Templates/InvoiceRA.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            ViewBag.RequestFormID = RequestFormID;
            //ViewBag.VendorID = VendorID;
            ViewBag.Type = Type;
            ViewBag.StatusID = "";
            ViewBag.ReqeustStatusID = "";
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            string SQL = @"select r.RequestFormID, ur.FullName [RequestBy], r.RequestByID,r.Remark, r.SubTotal, r.TotalEstimate, r.BankInformation, r.DepartmentName, r.ReferenceNumber, format(r.RequestDate, 'dd-MMM-yyyy HH:mm:ss tt') [RequestDate], format(r.RequirementDate, 'dd-MMM-yyyy') [RequirementDate], r.PaymentMethod,
r.StatusID,r.RequestStatusID, r.ApprovedByUserID,ur.Job [RequestedByPosition], ua.Job [ApprovedByPosition], r.ApprovedBy [ApprovedBy], format(r.ApprovalDate, 'dd-MMM-yyyy HH:mm:ss tt') [ApprovalDate], r.PhoneNumber Phone, r.ReceivedBy, format(r.ReceivedDate, 'dd-MMM-yyyy HH:mm:ss tt') [ReceivedDate], r.ReceivedByID, r.ExchangeRate, r.ImagePath, r.PaymentMean,r.ClassID, r.ClassName, r.DeliverTo,
r.SystemID, isnull(r.CheckedByUserID, 0) CheckedByUserID, uc.FullName [CheckedBy], uc.Job [CheckedByPosition], format(r.CheckedDate, 'dd-MMM-yyyy HH:mm:ss tt') [CheckedDate], case when r.UserID is null then r.CreateBy else u.FullName end [CreateBy], u.Job [CreatedByPosition], format(r.CreateDate, 'dd-MMM-yyyy HH:mm:ss tt') [CreatedDate], r.UserID, isnull(r.NeedNextApprovalByUserID, 0) NeedNextApprovalByUserID,
isnull(r.NextApproveStatusID, 0) NextApproveStatusID, isnull(r.NeedToCheckUserID, 0) NeedToCheckUserID
from dbo.TblRequestForm r
left join dbo.TblUserLogin u on u.UserLoginID = r.UserID
inner join dbo.TblUserLogin ur on ur.UserLoginID = r.RequestByID
left join dbo.TblUserLogin ua on ua.UserLoginID = r.ApprovedByUserID
left join dbo.TblUserLogin uc on uc.UserLoginID = r.CheckedByUserID
--left join dbo.TblUserLogin ur on ur.UserLoginID = r.ReceivedByID
where r.RequestFormID = @RequestFormID;

select rd.QTY, rd.RequestFormID, rd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end ItemName, isnull(i.SerialNumber, '') SerialNumber, rd.ItemDescription, format(rd.UnitPrice, '#,#0.000') UnitPrice,  format(rd.Amount, '#,#0.00') Amount, format(rd.SalePrice, '#,#0.00') SalePrice,isnull(rd.UM, '') UOM,
rd.EstimateAmount
from dbo.TblRequestFormDetail rd
left join dbo.TblItem i on i.ItemID = rd.ItemID
where rd.RequestFormID = @RequestFormID
order by rd.RequestFormDetailID;";
            para = new Dictionary<string, string>
            {
                {"RequestFormID", RequestFormID.ToString() },
                //{"SystemID", html.SystemID },
                //{"WarehouseID", html.WarehouseID },
                //{"DomainID","%" + html.DomainID + "%"}
            };
            ds = html.LoadData(SQL, false, para);
            DataRow drRq = ds.Tables[0].Rows[0];
            //DataRow drRqDetail = ds.Tables[1].Rows[0];
            int ClassID = int.Parse(drRq["ClassID"].ToString());
            ViewBag.StatusID = drRq["StatusID"].ToString();
            ViewBag.NeedToCheckUserID = drRq["NeedToCheckUserID"].ToString();
            ViewBag.NextApprovalByUserID = drRq["NeedNextApprovalByUserID"].ToString();
            ViewBag.NextApproveStatusID = drRq["NextApproveStatusID"].ToString();
            ViewBag.RequestStatusID = drRq["RequestStatusID"].ToString();
            Text = Text.Replace("{Title}", "Request Form");
            Text = Text.Replace("{InvoiceName}", "Request Form");
            Text = Text.Replace("{RequestBy}", drRq["RequestBy"].ToString());
            Text = Text.Replace("{Department}", drRq["DepartmentName"].ToString());
            Text = Text.Replace("{Tel}", drRq["Phone"].ToString());
            Text = Text.Replace("{PaymentMean}", drRq["PaymentMean"].ToString());
            Text = Text.Replace("{ReferenceNo}", drRq["ReferenceNumber"].ToString());
            Text = Text.Replace("{RequestDate}", drRq["RequestDate"].ToString());
            Text = Text.Replace("{RequirementDate}", drRq["RequirementDate"].ToString());
            Text = Text.Replace("{PaymentMethod}", drRq["PaymentMethod"].ToString());
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));

            //Logo 
            SQL = "select * from dbo.TblClass where ClassID = @ClassID";
            para = new Dictionary<string, string> { { "ClassID", ClassID.ToString() } };
            DataSet dsLogo = html.LoadData(SQL, false, para);


            string SystemLogo = html.SystemLogo;
            if (dsLogo.Tables[0].Rows.Count > 0)
            {
                DataRow drLogo = dsLogo.Tables[0].Rows[0];
                ViewBag.ClassLogo = "../Uploads/WarehouseLogo/" + drLogo["FileLogo"];
            }
            else if (!string.IsNullOrEmpty(SystemLogo))
            {
                ViewBag.ClassLogo = "../Uploads/WarehouseLogo/" + SystemLogo;
            }
            else
            {
                //ViewBag.ClassLogo = "../Uploads/WarehouseLogo/2PNG-169344668472033.png";
                ViewBag.ClassLogo = "../Uploads/WarehouseLogo/NSM-ISPLOGOPNG-169319650995441.png";
            }

            //=====================

            //Text = Text.Replace("{LOGO}", LOGO/);

            //Attach file
            if (drRq["ImagePath"].ToString() != "")
            {
                string[] paths = drRq["ImagePath"].ToString().Split('~');
                string FileUpload = "";
                foreach (string path in paths)
                {
                    FileUpload += "<img height=\"200\" style=\"margin: 10px 10px 0 0;\" src=\"../Uploads/RequestForm/CashAdvance/" + path + "\">";
                }
                Text = Text.Replace("{FileUpload}", FileUpload);
            }
            else
            {
                Text = Text.Replace("{FileUpload}", "");
            }

            int SystemID = int.Parse(drRq["SystemID"].ToString());
            string bgcolor = SystemID == 13 ? "#f89c39" : (SystemID == 25 ? "#c5d9f1" : (SystemID == 27 && ClassID == 11 ? "#ffde16" : (SystemID == 27 && ClassID == 12 ? "#ffccff" : "#2275B9")));
            string txtcolor = SystemID == 13 || SystemID == 28 ? "#fff" : "#000";

            //Body content
            string TblHead = $@"
				<tr style=""background: {bgcolor} ; color: {txtcolor};"">
					<th class=""text-center"" width=""10%"">No</th>
					<th class=""text-center"" width=""45%"">Description</th>
					<th class=""text-center"" width=""5%"">Quantity</th>
					<th class=""text-center"" width=""5%"">UOM</th>
					<th class=""text-center"" width=""10%"">Estimate</th>
					<th class=""text-nowrap text-center"" width=""10%"">Unit Price</th>
					<th class=""text-center"" width=""15%"">Amount</th>
				</tr>
			";
            Text = Text.Replace("{TblHead}", TblHead);

            int i = 0;
            foreach (DataRow dr in ds.Tables[1].Rows)
            {
                i++;
                sb.AppendLine("<tr><td align=\"center\">" + i + "</td>");
                //sb.AppendLine("<td>" + dr["ItemName"] + (dr["ItemDescription"].ToString() != "" ? "<br /><div class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</div>" : "") + "</td>");
                sb.AppendLine("<td class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">" + dr["UOM"].ToString().ToString() + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">$ " + float.Parse(dr["EstimateAmount"].ToString() == "" ? "0" : dr["EstimateAmount"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">$ " + float.Parse(dr["UnitPrice"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">$ " + float.Parse(dr["Amount"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("</tr>");
            }


            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();
            Text = ds.Tables[0].Rows[0]["Remark"].ToString() != "" ? Text.Replace("{RemarkTitle}", "Remark") : Text.Replace("{RemarkTitle}", "");
            Text = ds.Tables[0].Rows[0]["BankInformation"].ToString() != ""
                ? Text.Replace("{BankInformationTitle}", "Payment Information")
                : Text.Replace("{BankInformationTitle}", "");

            Text = ds.Tables[0].Rows[0]["PaymentMethod"].ToString() != ""
                ? Text.Replace("{PaymentMethodTitle}", "Payment Method:")
                : Text.Replace("{PaymentMethodTitle}", "");

            float TotalAmtKHR = float.Parse(drRq["SubTotal"].ToString()) * float.Parse(drRq["ExchangeRate"].ToString());
            float TotalEstimateAmtKHR = float.Parse(drRq["TotalEstimate"].ToString()) * float.Parse(drRq["ExchangeRate"].ToString());
            Text = Text.Replace("{Remark}", drRq["Remark"].ToString());
            Text = Text.Replace("{BankInformation}", drRq["BankInformation"].ToString());
            Text = Text.Replace("{TotalAmountUSD}", "$ " + float.Parse(drRq["SubTotal"].ToString()).ToString("#,#0.00"));
            Text = Text.Replace("{EstimateAmountUSD}", "$ " + float.Parse(drRq["TotalEstimate"].ToString()).ToString("#,#0.00"));
            Text = Text.Replace("{TotalAmountKHR}", "៛ " + float.Parse(TotalAmtKHR.ToString()).ToString("#,#0.00"));
            Text = Text.Replace("{EstimateAmountKHR}", "៛ " + float.Parse(TotalEstimateAmtKHR.ToString()).ToString("#,#0.00"));

            //Text = Text.Replace("{RequestByName}", drRq["RequestBy"].ToString());
            //Text = Text.Replace("{RequestedByPosition}", drRq["RequestedByPosition"].ToString());
            //Text = Text.Replace("{RequestByDate}", drRq["RequestDate"].ToString());

            //Text = Text.Replace("{ReceievedBy}", drRq["ReceivedBy"].ToString());
            //Text = Text.Replace("{ReceievedByPosition}", "");
            //Text = Text.Replace("{ReceievedDate}", drRq["ReceivedDate"].ToString());

            //Signature
            Text = Text.Replace("{CreatedByName}", drRq["CreateBy"].ToString());
            Text = Text.Replace("{CreatedByPosition}", drRq["CreatedByPosition"].ToString());
            Text = Text.Replace("{CreatedByDate}", drRq["CreatedDate"].ToString());

            string CreatedBySignature = "";
            SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
            para.Clear();
            para.Add("UserID", drRq["UserID"].ToString());
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    CreatedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            else if (drRq["CreateBy"].ToString() != "")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                para.Clear();
                para.Add("UserName", drRq["CreateBy"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    CreatedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            Text = Text.Replace("{CreatedBySignature}", CreatedBySignature);

            string CheckedBySignature = "";

            if (ViewBag.StatusID == "92" || ViewBag.StatusID == "90" || ViewBag.StatusID == "94")
            {
                Text = Text.Replace("{CheckedBy}", drRq["CheckedBy"].ToString());
                Text = Text.Replace("{CheckedByPosition}", drRq["CheckedByPosition"].ToString());
                Text = Text.Replace("{CheckedDate}", drRq["CheckedDate"].ToString());

                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drRq["CheckedByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    CheckedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["CheckedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ApprovedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            CheckedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{CheckedBySignature}", CheckedBySignature);
            }
            else
            {
                Text = Text.Replace("{CheckedBySignature}", "");
                Text = Text.Replace("{CheckedBy}", "");
                Text = Text.Replace("{CheckedByPosition}", "");
                Text = Text.Replace("{CheckedDate}", "");
            }

            string ApprovedBySignature = "";

            if (ViewBag.RequestStatusID == "90")
            {
                Text = Text.Replace("{ApprovedBy}", drRq["ApprovedBy"].ToString());
                Text = Text.Replace("{ApprovedByPosition}", drRq["ApprovedByPosition"].ToString());
                Text = Text.Replace("{ApprovedDate}", drRq["ApprovalDate"].ToString());

                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drRq["ApprovedByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    ApprovedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["ApprovedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ApprovedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            ApprovedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{ApprovedBySignature}", ApprovedBySignature);
            }
            else
            {
                Text = Text.Replace("{ApprovedBySignature}", "");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedByPosition}", "");
                Text = Text.Replace("{ApprovedDate}", "");
            }

            Text = Text.Replace("{ReceievedBy}", "");
            Text = Text.Replace("{ReceievedByPosition}", "");
            Text = Text.Replace("{ReceievedDate}", "");

            string ReceievedBySignature = "";
            string ReceivedID = drRq["ReceivedByID"].ToString();
            if (ViewBag.RequestStatusID == "67")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", ReceivedID);
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    ReceievedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["ReceivedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ReceivedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            ReceievedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{ReceievedBySignature}", ReceievedBySignature);
            }
            else
            {
                Text = Text.Replace("{ReceievedBySignature}", "");
            }

            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//body")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();
            return View();
        }
        [ValidateInput(false)]
        public ActionResult InvoiceTMPRF(int VendorID, int RequestFormID, string Type = "RequestForm", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = html.httpContext.Server.MapPath("~/Templates/InvoiceRF.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            ViewBag.RequestFormID = RequestFormID;
            ViewBag.VendorID = VendorID;
            ViewBag.Type = Type;
            ViewBag.StatusID = "";
            ViewBag.ReqeustStatusID = "";
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            string SQL = @"select r.RequestFormID, ur.FullName [RequestBy], r.UserID, case when r.UserID is null then r.CreateBy else u.FullName end [CreateBy], u.Job [CreatedByPosition], format(r.CreateDate, 'dd-MMM-yyyy HH:mm:ss tt') [CreatedDate], r.RequestByID,r.Remark, r.SubTotal, r.TotalEstimate, r.BankInformation, r.DepartmentName, r.ReferenceNumber, format(r.RequestDate, 'dd-MMM-yyyy') [RequestDate], format(r.RequirementDate, 'dd-MMM-yyyy') [RequirementDate], r.PaymentMethod,
r.StatusID,r.RequestStatusID, r.ApprovedByUserID,ur.Job [RequestedByPosition],uc.Job [CheckedByPosition], ua.Job [ApprovedByPosition], r.ApprovedBy [ApprovedBy], format(r.ApprovalDate, 'dd-MMM-yyyy HH:mm:ss tt') [ApprovalDate], r.ReceivedBy, format(r.ReceivedDate, 'dd-MMM-yyyy HH:mm:ss tt') [ReceivedDate], r.ReceivedByID,
v.VendorName, v.Address, v.Phone, uc.FullName [CheckedBy], r.CheckedByUserID, format(r.CheckedDate, 'dd-MMM-yyyy HH:mm:ss tt') [CheckedDate], r.ImagePath, r.PaymentMean,r.ClassID, r.ClassName, r.ExchangeRate, r.DeliverTo, r.SystemID
from dbo.TblRequestForm r
left join dbo.TblUserLogin u on u.UserLoginID = r.UserID
inner join dbo.TblUserLogin ur on ur.UserLoginID = r.RequestByID
left join dbo.TblUserLogin uc on uc.UserLoginID = r.CheckedByUserID
left join dbo.TblUserLogin ua on ua.UserLoginID = r.ApprovedByUserID
--left join dbo.TblUserLogin ur on ur.UserLoginID = r.ReceivedByID
left join dbo.TblVendor v on v.VendorID = r.VendorID
where r.RequestFormID = @RequestFormID;

select rd.QTY, rd.RequestFormID, rd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end ItemName, isnull(i.SerialNumber, '') SerialNumber, rd.ItemDescription, format(rd.UnitPrice, '#,#0.000') UnitPrice,  format(rd.Amount, '#,#0.00') Amount, format(rd.SalePrice, '#,#0.00') SalePrice,isnull(rd.UM, '') UOM,
rd.EstimateAmount
from dbo.TblRequestFormDetail rd
left join dbo.TblItem i on i.ItemID = rd.ItemID
where rd.RequestFormID = @RequestFormID
order by rd.RequestFormDetailID;";
            para = new Dictionary<string, string>
            {
                {"RequestFormID", RequestFormID.ToString() },
				//{"SystemID", html.SystemID },
				//{"WarehouseID", html.WarehouseID },
				//{"DomainID","%" + html.DomainID + "%"}
			};
            ds = html.LoadData(SQL, false, para);
            DataRow drRq = ds.Tables[0].Rows[0];
            //DataRow drRqDetail = ds.Tables[1].Rows[0];
            int ClassID = int.Parse(drRq["ClassID"].ToString());
            ViewBag.StatusID = drRq["StatusID"].ToString();
            ViewBag.RequestStatusID = drRq["RequestStatusID"].ToString();
            Text = Text.Replace("{Title}", "Request Form");
            Text = Text.Replace("{InvoiceName}", "Request Form");
            Text = Text.Replace("{VendorName}", drRq["VendorName"].ToString());
            Text = Text.Replace("{VendorAddress}", drRq["Address"].ToString());
            Text = Text.Replace("{VendorAttend}", "");
            Text = Text.Replace("{VendorContact}", drRq["Phone"].ToString());
            Text = Text.Replace("{RequestBy}", drRq["RequestBy"].ToString());
            Text = Text.Replace("{Department}", drRq["DepartmentName"].ToString());
            Text = Text.Replace("{ReferenceNo}", drRq["ReferenceNumber"].ToString());
            Text = Text.Replace("{RequestDate}", drRq["RequestDate"].ToString());
            Text = Text.Replace("{RequirementDate}", drRq["RequirementDate"].ToString());
            Text = Text.Replace("{PaymentMean}", drRq["PaymentMean"].ToString());
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            Text = drRq["DeliverTo"].ToString() != ""
                ? Text.Replace("{DeliveryTo}", drRq["DeliverTo"].ToString())
                : Text.Replace("{DeliveryTo}", "");

            //Logo 
            SQL = "select * from dbo.TblClass where ClassID = @ClassID";
            para = new Dictionary<string, string>
            {
                {"ClassID", ClassID.ToString() }
            };
            DataSet dsLogo = html.LoadData(SQL, false, para);
            string SystemLogo = html.SystemLogo;
            string LOGO = "";
            if (dsLogo.Tables[0].Rows.Count > 0)
            {
                DataRow drLogo = dsLogo.Tables[0].Rows[0];
                LOGO += "<img src=\"../Uploads/ClassLogo/" + drLogo["FileLogo"] + "\" alt=\"Not found\" width=\"200\" />";
            }
            else
            {
                if (SystemLogo != "")
                    LOGO += "<img src=\"" + SystemLogo + "\" alt=\"Not found\" width=\"200\" />";
                else
                    LOGO += "<img src=\"../Uploads/WarehouseLogo/2PNG-169344668472033.png\" alt=\"Not found\" width=\"200\" />";
            }

            Text = Text.Replace("{LOGO}", LOGO);

            if (drRq["ImagePath"].ToString() != "")
            {
                string[] paths = drRq["ImagePath"].ToString().Split('~');
                string FileUpload = "";
                foreach (string path in paths)
                {
                    FileUpload += "<img height=\"200\" style=\"margin: 10px 10px 0 0;\" src=\"../Uploads/RequestForm/CashAdvance/" + path + "\" onerror=\"this.onerror=null;this.src='Content/images/placeholder.jpg';>";
                }
                Text = Text.Replace("{FileUpload}", FileUpload);
            }
            else
            {
                Text = Text.Replace("{FileUpload}", "");
            }

            int SystemID = int.Parse(drRq["SystemID"].ToString());
            string bgcolor = SystemID == 13 ? "#f89c39" : (SystemID == 25 ? "#c5d9f1" : (SystemID == 27 && ClassID == 11 ? "#ffde16" : (SystemID == 27 && ClassID == 12 ? "#ffccff" : "#2275B9")));
            string txtcolor = SystemID == 13 || SystemID == 28 ? "#fff" : "#000";

            //Table header
            string TblHead = $@"
			    <tr style=""background:{bgcolor}; color:{txtcolor};"">
					<th class=""text-center"" width=""10%"">No</th>
					<th class=""text-center"" width=""45%"">Description</th>
					<th class=""text-center"" width=""5%"">Quantity</th>
					<th class=""text-center"" width=""5%"">UOM</th>
					<th class=""text-center"" width=""10%"">Estimate</th>
					<th class=""text-nowrap text-center"" width=""10%"">Unit Price</th>
					<th class=""text-center"" width=""15%"">Amount</th>
				</tr>
			";

            Text = Text.Replace("{TblHead}", TblHead);

            int i = 0;
            foreach (DataRow dr in ds.Tables[1].Rows)
            {
                i++;
                sb.AppendLine("<tr><td align=\"center\">" + i + "</td>");
                //sb.AppendLine("<td>" + dr["ItemName"] + (dr["ItemDescription"].ToString() != "" ? "<br /><div class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</div>" : "") + "</td>");
                sb.AppendLine("<td class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">" + dr["UOM"].ToString().ToString() + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">$ " + float.Parse(dr["EstimateAmount"].ToString() == "" ? "0" : dr["EstimateAmount"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">$ " + float.Parse(dr["UnitPrice"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("<td align=\"center\" class=\"text-nowrap-pre\">$ " + float.Parse(dr["Amount"].ToString()).ToString("#,#0.00") + "</td>");
                sb.AppendLine("</tr>");
            }


            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();


            Text = ds.Tables[0].Rows[0]["Remark"].ToString() != "" ? Text.Replace("{RemarkTitle}", "Remark") : Text.Replace("{RemarkTitle}", "");

            Text = ds.Tables[0].Rows[0]["BankInformation"].ToString() != ""
                ? Text.Replace("{BankInformationTitle}", "Payment Information")
                : Text.Replace("{BankInformationTitle}", "");

            Text = ds.Tables[0].Rows[0]["PaymentMethod"].ToString() != ""
                ? Text.Replace("{PaymentMethodTitle}", "Payment Method:")
                : Text.Replace("{PaymentMethodTitle}", "");

            Text = Text.Replace("{Remark}", drRq["Remark"].ToString());
            Text = Text.Replace("{BankInformation}", drRq["BankInformation"].ToString());
            Text = Text.Replace("{PaymentMethod}", drRq["PaymentMethod"].ToString());

            float TotalAmtKHR = float.Parse(drRq["SubTotal"].ToString()) * float.Parse(drRq["ExchangeRate"].ToString());
            float TotalEstimateAmtKHR = float.Parse(drRq["TotalEstimate"].ToString()) * float.Parse(drRq["ExchangeRate"].ToString());
            Text = Text.Replace("{Remark}", drRq["Remark"].ToString());
            Text = Text.Replace("{BankInformation}", drRq["BankInformation"].ToString());
            Text = Text.Replace("{TotalAmountUSD}", "$ " + float.Parse(drRq["SubTotal"].ToString()).ToString("#,#0.00"));
            Text = Text.Replace("{EstimateAmountUSD}", "$ " + float.Parse(drRq["TotalEstimate"].ToString()).ToString("#,#0.00"));
            Text = Text.Replace("{TotalAmountKHR}", "៛ " + float.Parse(TotalAmtKHR.ToString()).ToString("#,#0.00"));
            Text = Text.Replace("{EstimateAmountKHR}", "៛ " + float.Parse(TotalEstimateAmtKHR.ToString()).ToString("#,#0.00"));

            Text = Text.Replace("{CreatedByName}", drRq["CreateBy"].ToString());
            Text = Text.Replace("{CreatedByPosition}", drRq["CreatedByPosition"].ToString());
            Text = Text.Replace("{CreatedByDate}", drRq["CreatedDate"].ToString());

            string CreatedBySignature = "";
            SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
            para.Clear();
            para.Add("UserID", drRq["UserID"].ToString());
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    CreatedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            else if (drRq["CreateBy"].ToString() != "")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                para.Clear();
                para.Add("UserName", drRq["CreateBy"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    CreatedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            Text = Text.Replace("{CreatedBySignature}", CreatedBySignature);

            string CheckedBySignature = "";

            if (ViewBag.StatusID == "92" || ViewBag.StatusID == "90" || ViewBag.StatusID == "94")
            {
                Text = Text.Replace("{CheckedBy}", drRq["CheckedBy"].ToString());
                Text = Text.Replace("{CheckedByPosition}", drRq["CheckedByPosition"].ToString());
                Text = Text.Replace("{CheckedDate}", drRq["CheckedDate"].ToString());

                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drRq["CheckedByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    CheckedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["CheckedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ApprovedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            CheckedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{CheckedBySignature}", CheckedBySignature);
            }
            else
            {
                Text = Text.Replace("{CheckedBySignature}", "");
                Text = Text.Replace("{CheckedBy}", "");
                Text = Text.Replace("{CheckedByPosition}", "");
                Text = Text.Replace("{CheckedDate}", "");
            }

            string ApprovedBySignature = "";

            if (ViewBag.RequestStatusID == "90")
            {
                Text = Text.Replace("{ApprovedBy}", drRq["ApprovedBy"].ToString());
                Text = Text.Replace("{ApprovedByPosition}", drRq["ApprovedByPosition"].ToString());
                Text = Text.Replace("{ApprovedDate}", drRq["ApprovalDate"].ToString());

                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drRq["ApprovedByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    ApprovedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["ApprovedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ApprovedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            ApprovedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{ApprovedBySignature}", ApprovedBySignature);
            }
            else
            {
                Text = Text.Replace("{ApprovedBySignature}", "");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedByPosition}", "");
                Text = Text.Replace("{ApprovedDate}", "");
            }

            string ReceievedBySignature = "";
            string ReceivedID = drRq["ReceivedByID"].ToString();
            if (ViewBag.RequestStatusID == "67")
            {
                Text = Text.Replace("{ReceievedBy}", drRq["ReceivedBy"].ToString());
                Text = Text.Replace("{ReceievedByByPosition}", "");
                Text = Text.Replace("{ReceievedDate}", drRq["ReceivedDate"].ToString());

                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", ReceivedID);
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    ReceievedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["ReceivedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ReceivedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            ReceievedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{ReceievedBySignature}", ReceievedBySignature);
            }
            else
            {
                Text = Text.Replace("{ReceievedBySignature}", "");
                Text = Text.Replace("{ReceievedBy}", "");
                Text = Text.Replace("{ReceievedByByPosition}", "");
                Text = Text.Replace("{ReceievedDate}", "");
            }

            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//body")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();
            return View();
        }
        [ValidateInput(false)]
        public ActionResult InvoiceTMPRI(int RequestFormID, string Type = "RequestForm", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = html.httpContext.Server.MapPath("~/Templates/InvoiceRI.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            ViewBag.RequestFormID = RequestFormID;
            //ViewBag.VendorID = VendorID;
            ViewBag.Type = Type;
            ViewBag.StatusID = "";
            ViewBag.ReqeustStatusID = "";
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            string SQL = @"select r.RequestFormID, r.RequestBy, r.RequestByID,r.Remark, r.SubTotal, r.TotalEstimate, r.BankInformation, r.DepartmentName, r.ReferenceNumber, format(r.RequestDate, 'dd-MMM-yyyy') [RequestDate], format(r.RequirementDate, 'dd-MMM-yyyy') [RequirementDate], r.PaymentMethod,
r.StatusID,r.RequestStatusID, r.ApprovedByUserID, ua.Job [ApprovedByPosition], r.ApprovedBy, format(r.ApprovalDate, 'dd-MMM-yyyy') [ApprovalDate], r.ReceivedBy, format(r.ReceivedDate, 'dd-MMM-yyyy') [ReceivedDate], r.ReceivedByID,
r.CheckedBy, r.CheckedByUserID, format(r.CheckedDate, 'dd-MMM-yyyy') [CheckedDate], r.ImagePath
from dbo.TblRequestForm r
--inner join dbo.TblUserLogin ur on ur.UserLoginID = r.RequestByID
left join dbo.TblUserLogin ua on ua.UserLoginID = r.ApprovedByUserID
left join dbo.TblUserLogin ur on ur.UserLoginID = r.ReceivedByID
where r.RequestFormID = @RequestFormID and r.SystemID = @SystemID and r.WarehouseID = @WarehouseID and r.DomainID like @DomainID;

select rd.QTY, rd.RequestFormID, rd.ItemName + case when isnull(i.ItemSize, '') <> '' then '<br>'+i.ItemSize else '' end ItemName, isnull(i.SerialNumber, '') SerialNumber, rd.ItemDescription, format(rd.UnitPrice, '#,#0.000') UnitPrice,  format(rd.Amount, '#,#0.00') Amount, format(rd.SalePrice, '#,#0.00') SalePrice,
rd.EstimateAmount, rd.Remark, iv.SerialNumber, rd.UM
from dbo.TblRequestFormDetail rd
left join dbo.TblItem i on i.ItemID = rd.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = rd.ItemID
where rd.RequestFormID = @RequestFormID and rd.SystemID = @SystemID and rd.WarehouseID = @WarehouseID and rd.DomainID like @DomainID
order by rd.RequestFormDetailID;";
            para = new Dictionary<string, string>
            {
                {"RequestFormID", RequestFormID.ToString() },
                {"SystemID", html.SystemID },
                {"WarehouseID", html.WarehouseID },
                {"DomainID","%" + html.DomainID + "%"}
            };
            ds = html.LoadData(SQL, false, para);
            DataRow drRq = ds.Tables[0].Rows[0];
            //DataRow drRqDetail = ds.Tables[1].Rows[0];

            ViewBag.StatusID = drRq["StatusID"].ToString();
            ViewBag.RequestStatusID = drRq["RequestStatusID"].ToString();
            Text = Text.Replace("{InvoiceName}", "Inventory Request");
            Text = Text.Replace("{RequestBy}", drRq["RequestBy"].ToString());
            Text = Text.Replace("{Department}", drRq["DepartmentName"].ToString());
            Text = Text.Replace("{ReferenceNo}", drRq["ReferenceNumber"].ToString());
            Text = Text.Replace("{RequestDate}", drRq["RequestDate"].ToString());
            Text = Text.Replace("{Address}", "");

            string TblHead = @"
			        <tr style=""background:#f89c39; color:#fff;"">
						<th class=""text-center"">No</th>
						<th class=""text-center"">Item Description</th>
						<th class=""text-center"">Serial Number</th>
						<th class=""text-center"">Qty</th>
						<th class=""text-nowrap"">Remark</th>
			    ";

            Text = Text.Replace("{TblHead}", TblHead);

            int i = 0;
            foreach (DataRow dr in ds.Tables[1].Rows)
            {
                i++;
                sb.AppendLine("<tr><td align=\"center\">" + i + "</td>");
                sb.AppendLine("<td>" + dr["ItemName"] + (dr["ItemDescription"].ToString() != "" ? "<br /><div class=\"text-nowrap-pre\">" + dr["ItemDescription"] + "</div>" : "") + "</td>");
                //sb.AppendLine("<td>" + dr["ItemDescription"] + "</td>");
                sb.AppendLine("<td align=\"center\">" + dr["SerialNumber"].ToString() + "</td>");
                sb.AppendLine("<td align=\"center\">" + float.Parse(dr["QTY"].ToString()).ToString("#,#0") + " " + dr["UM"].ToString() + "</td>");
                sb.AppendLine("<td align=\"left\">" + dr["Remark"].ToString() + "</td>");
                sb.AppendLine("</tr>");
            }
            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();

            Text = Text.Replace("{RequestByName}", drRq["RequestBy"].ToString());
            Text = Text.Replace("{RequestByDate}", drRq["RequestDate"].ToString());

            string RequestBySignature = "";
            SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
            para.Clear();
            para.Add("UserID", drRq["RequestByID"].ToString());
            ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    RequestBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            else if (drRq["RequestBy"].ToString() != "")
            {
                SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                para.Clear();
                para.Add("UserName", drRq["RequestBy"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    RequestBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
            }
            Text = Text.Replace("{RequestBySignature}", RequestBySignature);


            string ApprovedBySignature = "";

            if (ViewBag.StatusID == "90")
            {
                Text = Text.Replace("{ApprovedBy}", drRq["ApprovedBy"].ToString());
                Text = Text.Replace("{ApprovedDate}", drRq["ApprovalDate"].ToString());

                SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                para.Clear();
                para.Add("UserID", drRq["ApprovedByUserID"].ToString());
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    ApprovedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                }
                else if (drRq["ApprovedBy"].ToString() != "")
                {
                    SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                    para.Clear();
                    string app = drRq["ApprovedBy"].ToString();
                    if (app != "")
                    {
                        para.Add("UserName", app);
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            ApprovedBySignature = "<img height=\"60\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }
                }

                Text = Text.Replace("{ApprovedBySignature}", ApprovedBySignature);
            }
            else
            {
                Text = Text.Replace("{ApprovedBySignature}", "");
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
            }


            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//section")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();
            return View();
        }
        [ValidateInput(false)]
        public ActionResult InvoiceSaleOrderTMP(int ID, string Type = "", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false, string approvalFrom = "System")
        {
            HTML html = new HTML();
            string SQL = "";
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            SQL = $@"select ClassID from dbo.TblSales where SalesID = {ID} and Type = '{Type}';";
            if (approvalFrom == "System")
                SQL += $"select * from dbo.TblUserLogin where UserLoginID = {html.UserID};";

            ds = html.LoadData(SQL, false);
            string ClassID = ds.Tables[0].Rows[0]["ClassID"].ToString();
            ViewBag.TelegramUsername = approvalFrom == "System" && ds.Tables[1].Rows.Count > 0
                ? ds.Tables[1].Rows[0]["TelegramUsername"].ToString() : "";

            string InvoiceTMPName = "";

            if (ClassID == "11" || ClassID == "12")
            {
                InvoiceTMPName = Type == "Quotation" && ClassID == "11"
                    ? "Quotation" : Type == "SalesOrder" && ClassID == "11"
                    ? "SaleOrder"
                    : "Invoices";
            }
            else
            {
                InvoiceTMPName = Type == "Quotation"
                    ? "Quotation" : Type == "SalesOrder" && ClassID == "10"
                    ? "SaleOrder" : "Invoices";

                if (Type == "DeliveryNote")
                {
                    InvoiceTMPName = "DeliveryNote";
                }
            }
            string FilePath = html.httpContext.Server.MapPath($"~/Templates/Sales/{InvoiceTMPName}.html");
            StreamReader str = new StreamReader(FilePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            if (Type == "SalesOrder")
            {
                Text = Text.Replace("{Title}", "Sales Order");
            }
            else if (Type == "Quotation")
            {
                Text = Text.Replace("{Title}", "Quotation");
            }
            else if (Type == "Invoices" || Type == "ConvertInvoices")
            {
                Text = Text.Replace("{Title}", "Invoices");
            }
            else if (Type == "DeliveryNote")
            {
                Text = Text.Replace("{Title}", "Delivery Note");
            }

            //Logo
            para.Add("SalesID", ID.ToString());
            para.Add("Type", Type);

            SQL = @"select SystemID, WarehouseID from dbo.TblSales where SalesID = @SalesID and Type = @Type;";
            ds = html.LoadData(SQL, false, para);
            int SystemID = int.Parse(ds.Tables[0].Rows[0]["SystemID"].ToString());
            int warehouseId = int.Parse(ds.Tables[0].Rows[0]["WarehouseID"].ToString());

            SQL = @"select format(s.TranDate, 'dd-MMM-yyyy') TranDate, isnull(s.IsCommercialINV, 0) as IsCommercialINV, s.InvoiceNumber, s.NewInvoiceNumber,s.SONumber, s.QuoteNumber,s.DNNumber, s.ClassID, c.FileLogo, cs.CustomerName, cs.CustomerMobile, (l.Salutation + ' ' + l.FullName) SellerName, s.Remark, isnull(s.Comment, '') as Comment, s.UserID, 
s.CreateBy, s.StatusID,s.CreateBy IssuedBy, format(s.CreateDate, 'dd-MM-yyyy HH:mm:ss') IssuedDate,s.CheckByID, s.CheckBy CheckedBy,format(s.CheckDate, 'dd-MM-yyyy HH:mm:ss') CheckedDate,s.ApproveByID, s.ApproveBy ApprovedBy, s.NeedCheckByID, s.NeedApproveByID, s.SystemID, s.WarehouseID, s.DomainID,​cb.CompanyNameKH,
format(s.ApproveDate, 'dd-MM-yyyy HH:mm:ss') ApprovedDate,s.TermCondition, isnull(s.IsCheck, 0) IsCheck, isnull(s.IsApprove, 0) IsApprove, s.SalesID, s.ExchangeRate, cs.CustomerAddress, cb.VATNumber, cs.ATTName, format(s.DueDate, 'dd-MMM-yyyy') DueDate, s.IsToInvoice, cs.CustomerEmail, isnull(s.INVID, 0) INVID, isnull(s.DNNumber, '') DNNumber,
isnull(s.PreparedByID, 0) PreparedByID, isnull(s.PreparedBy, '') PreparedBy, (select Telephone from dbo.TblUserLogin where UserLoginID = s.PreparedByID) PreparedPhone,
isnull(s.DeliveryByID, 0) DeliveryByID, isnull(s.DeliveryBy, '') DeliveryBy, (select Telephone from dbo.TblUserLogin where UserLoginID = s.DeliveryByID) DeliveryPhone,
isnull(s.NeedCheckBy, '') NeedCheckBy, (select Telephone from dbo.TblUserLogin where UserLoginID = s.NeedCheckByID) NeedCheckPhone, format(s.CreateDate, 'dd-MM-yyyy') CreateDate, s.WarehouseName
from dbo.TblSales s 
inner join dbo.TblClass c on c.ClassID = s.ClassID 
left join dbo.TblLeads l on l.LeadID = s.SellerID 
inner join dbo.TblCustomer cs on cs.CustomerID = s.CustomerID 
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = cs.CustomerID 
where s.SalesID = @SalesID and s.Type = @Type;

with tempTotalAMT as(
	select s.SalesID, sum((ci.QTY * ci.UnitPrice)) as TotalNoDis, sum(ci.DiscountUSD) + s.DiscountUSD as tempDis
	from dbo.TblSales s
	inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
	where s.SalesID = @SalesID and s.Type = @Type
	group by s.SalesID, s.DiscountUSD
)
select ci.ItemName,format(ci.QTY, 'N3') as QTY, u.Name UOM, format(ci.UnitPrice, 'N2') as UnitPrice, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(ci.DiscountUSD, 0) end DisUSD, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(ci.Amount, 0) end Amount,ci.ItemDescription, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(s.GrandTotal, 0) end as GrandTotal, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(s.SubTotal, 0) end SubTotal, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(s.Total, 0) end Total, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else tmp.TotalNoDis end TotalNoDis,
case when s.SystemID = 27 and s.StatusID = 94 then 0 else s.DiscountUSD end DiscountUSD, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(s.VAT, 0) end VAT, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(s.Deposit, 0) end Deposit, 
case when s.SystemID = 27 and s.StatusID = 94 then 0 else isnull(s.Balance, 0) end Balance, ci.ItemStatus, 
case when ci.ItemStatus = 'NonOtherCharge' then (select ItemCode from dbo.TblItem where ItemID = ci.ItemID) else '' end ItemCode, 
case when ci.ItemStatus = 'NonOtherCharge' then (select ItemSize from dbo.TblItem where ItemID = ci.ItemID) else '' end ItemSize
from tempTotalAMT tmp
inner join dbo.TblSales s on s.SalesID = tmp.SalesID
inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
left join dbo.TblUom u on u.UomID = ci.UomID
order by ci.CustomerInvoiceID
";

            ds = html.LoadData(SQL, false, para);
            string SystemLogo = html.SystemLogo;
            string LOGO = "";
            ViewBag.PrimaryFieldValue = ID;
            ViewBag.Type = Type;
            if (ds.Tables[0].Rows.Count > 0)
            {
                //Dynamic logo
                var SalesDetail = ds.Tables[1].Rows;
                DataRow dr = ds.Tables[0].Rows[0];
                DataRow dr2 = ds.Tables[1].Rows[0];

                //ClassID = dr["ClassID"].ToString();
                string systemId = dr["SystemID"].ToString();
                string wid = dr["WarehouseID"].ToString();
                string domainId = dr["DomainID"].ToString();
                string usd = "<span style=\"font-family:'Times New Roman'\">$</span>";
                string khr = "៛";
                string exchangeRate = dr["ExchangeRate"].ToString();
                bool isCM = bool.Parse(dr["IsCommercialINV"].ToString().ToLower());
                ViewBag.StatusID = dr["StatusID"].ToString();
                ViewBag.IsCheck = dr["IsCheck"].ToString();
                ViewBag.IsApprove = dr["IsApprove"].ToString();
                ViewBag.SalesID = dr["SalesID"].ToString();
                ViewBag.ClassID = ClassID;
                ViewBag.InvoiceNumber = dr["InvoiceNumber"].ToString();
                ViewBag.SONumber = dr["SONumber"].ToString();
                ViewBag.QuoteNumber = dr["QuoteNumber"].ToString();
                ViewBag.IsToInvoice = dr["IsToInvoice"].ToString();
                ViewBag.NeedCheckByID = dr["NeedCheckByID"].ToString();
                ViewBag.NeedApproveByID = dr["NeedApproveByID"].ToString();
                LOGO = !string.IsNullOrEmpty(ClassID)
                    ? "<img src=\"../Uploads/ClassLogo/" + dr["FileLogo"] + "\" alt=\"Not found\" width=\"240\" height=\"120\" class=\"{defaultHideLogo}\"/>"
                    : SystemLogo != ""
                        ? "<img src=\"" + SystemLogo + "\" alt=\"Not found\" width=\"240\" height=\"120\" class=\"{defaultHideLogo}\"/>"
                        : "<img src=\"../Uploads/WarehouseLogo/2PNG-169344668472033.png\" alt=\"Not found\" width=\"240\" height=\"120\" class=\"{defaultHideLogo}\"/>";
                Text = Text.Replace("{LOGO}", LOGO);


                /*** 
                 * Form data 
                 * ==============
                 * classId 10: BSM
                 * classId 11: BS
                 * classId 12: BSCoLtd
                 * systemId 25: BSM
                 * systemId 27: BS
                 * ***/

                if (Type == "SalesOrder")
                {
                    Text = Text.Replace("{defaultHideLogo}", ClassID == "10" || ClassID == "11" ? "d-none" : "");

                    if (ClassID == "11" || ClassID == "10")
                    {
                        //Title 
                        Text = Text.Replace("{MainTitle}", " ប័ណ្ណបញ្ជាលក់");
                        Text = Text.Replace("{SubTitle}", "Sale Order");
                        //=========Header Info=========//
                        string customerName = dr["CustomerName"].ToString();
                        string customerPhone = dr["CustomerMobile"].ToString();
                        string customerAddress = dr["CustomerAddress"].ToString();
                        string sellerName = dr["SellerName"].ToString();
                        string soNumber = dr["SONumber"].ToString();
                        string soDate = dr["TranDate"].ToString();
                        string quoteNumber = dr["QuoteNumber"].ToString();
                        if (systemId == "27" || systemId == "25")
                        {
                            string HeaderInfo = "";
                            HeaderInfo = $@"<div style=""max-width: 50%;"">
								<table>
									<tr><td valign=""top"">Customer:</td><td class=""pl-4"" valign=""top""> {customerName}</td></tr>
									<tr><td valign=""top"">Telephone:</td><td class=""pl-4"" valign=""top""> {customerPhone}</td></tr>
									<tr><td valign=""top"" nowrap>Delivery To:</td><td class=""pl-4"" valign=""top""> {customerAddress}</td></tr>
								</table>
								</div>
								<div style=""max-width: 50%;"">
									<table>
										<tr><td valign=""top"" nowrap>SO No:</td><td class=""pl-4"" valign=""top""> {soNumber}</td></tr>
										<tr><td valign=""top"" nowrap>Quotation No:</td><td class=""pl-4"" valign=""top""> {quoteNumber}</td></tr>
										<tr><td valign=""top"" nowrap>SO Date:</td><td class=""pl-4"" valign=""top""> {soDate}</td></tr>
										<tr><td valign=""top"" nowrap>Exchange Rate:</td><td class=""pl-4"" valign=""top""> {float.Parse(dr["ExchangeRate"].ToString()).ToString("#,#0") + khr}</td></tr>
									</table>
								</div>";

                            Text = Text.Replace("{HeaderInfo}", HeaderInfo);
                        }

                        //Remark
                        string remark = "";
                        string remarkText = dr["StatusID"]?.ToString() == "94"
                            ? (dr.Table.Columns.Contains("Comment") ? dr["Comment"]?.ToString() ?? "" : "")
                            : (dr["Remark"]?.ToString() ?? "");
                        remark = $@"
							<div class=""row remark"">
								<div class=""col-12"">
									<strong>Remark:</strong>
									<p class=""text-nowrap-pre well well-sm shadow-none"">{remarkText}</p>
								</div>
							</div>
						";
                        Text = Text.Replace("{Remark}", remark);
                    }
                    else if (ClassID == "12")
                    {
                        string tmpInvoice = ClassID == "12" ? "BSCoLtd" : "BSM";
                        string CompanyNameTitleKH = "";
                        string CompanyNameTitleEN = "";
                        string CompanyInfo = "";
                        SQL = @"select * from dbo.TblCompany 
where SystemID = " + systemId + " and WarehouseID = " + wid + " and InvoiceTemplateName = '" + tmpInvoice + "'";
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow drc = ds.Tables[0].Rows[0];
                                CompanyNameTitleKH = drc["CompanyNameKH"].ToString();
                                CompanyNameTitleEN = drc["CompanyName"].ToString();

                                string VATNumber = drc["VATNumber"].ToString();
                                string Address = drc["CompanyAddress"].ToString();
                                string AddressKH = drc["AddressKH"].ToString();
                                string Phone = drc["Phone"].ToString();
                                string PhoneKH = drc["PhoneKH"].ToString();
                                string txtLabel = "លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN)៖";

                                CompanyInfo = $@"<div>
								<div><span>{txtLabel}</span><span> {VATNumber}</span></div>
								<div><span>អាសយដ្ឋាន៖​</span><span> {AddressKH}</span></div>
								<div><span>Head Office:</span><span> {Address}</span></div>
								<div><span>ទូរស័ព្ទលេខ៖</span><span> {PhoneKH}</span></div>
								<div><span>HP:</span><span> {Phone}</span></div>
							";
                            }
                        }

                        //Title & company info
                        string InvoiceTitle = "Sales Order";
                        string InvoiceTitleKH = "ការបញ្ជាទិញ";
                        Text = Text.Replace("{MainTitle}", CompanyNameTitleKH);
                        Text = Text.Replace("{SubTitle}", CompanyNameTitleEN);
                        Text = Text.Replace("{CompanyInfo}", CompanyInfo);
                        Text = Text.Replace("{InvoiceTitle}", InvoiceTitle);
                        Text = Text.Replace("{InvoiceTitleKH}", InvoiceTitleKH);

                        //=========Header Info=========//
                        string customerName = dr["CustomerName"].ToString();
                        string customerPhone = dr["CustomerMobile"].ToString();
                        string customerAddress = dr["CustomerAddress"].ToString();
                        string customerCompanyNameKH = dr["CompanyNameKH"].ToString();
                        string customerVATNumber = dr["VATNumber"].ToString();
                        string attName = dr["ATTName"].ToString();
                        string sellerName = dr["SellerName"].ToString();
                        string invoiceNumber = dr["InvoiceNumber"].ToString();
                        string soNumber = dr["SONumber"].ToString();
                        string invoiceDate = dr["TranDate"].ToString();
                        string dueDate = dr["DueDate"].ToString();
                        string quoteNumber = dr["QuoteNumber"].ToString();

                        if (systemId == "27" || systemId == "25")
                        {
                            string InvoiceHeaderInfo = "";
                            InvoiceHeaderInfo = $@"
							<table class=""table mytable""><tr>
								<td width=""50%"">
									<div>
										<div><span>ឈ្មោះក្រុមហ៊ុន ឬអតិថិជន៖</span><span> {customerCompanyNameKH}</span></div>
										<div><span>Company Name/Customer:</span><span> {customerName}</span></div>
										<div><span>អាសយដ្ឋាន៖</span><span> {(Regex.IsMatch(customerAddress, @"[\u1780-\u17FF]") ? customerAddress : "")}</span></div>
										<div><span>Address:</span><span> {(!Regex.IsMatch(customerAddress, @"[\u1780-\u17FF]") ? customerAddress : "")}</span></div>
										<div><span>ទូរស័ព្ទលេខ៖</span><span> {(Regex.IsMatch(customerPhone, @"[\u1780-\u17FF]") ? customerPhone : "")}</span></div>
										<div><span>Phone:</span><span> {(!Regex.IsMatch(customerPhone, @"[\u1780-\u17FF]") ? customerPhone : "")}</span></div>
										<div><span>Attention:</span><span> {attName}</span></div>
										<div><span>លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN):</span><span> {customerVATNumber}</span></div>
									</div>
								</td>
								<td width=""50%"">
									<div>
										<div><span>លេខរៀងបញ្ជាទិញ៖</span><span></span></div>
										<div><span>Sales Order Nº:</span><span> {soNumber}</span></div>
										<div><span>កាលបរិច្ឆេទ៖</span><span></span></div>
										<div><span>Date:</span><span> {invoiceDate}</span></div>
										<div><span>REF:</span><span> {sellerName}</span></div>
										<div><span>Due Date:</span><span> {dueDate}</span></div>
										<div><span>DN:</span><span></span></div>
										<div><span>Exchange Rate:</span><span> {float.Parse(dr["ExchangeRate"].ToString()).ToString("#,#0") + khr}</span></div>
									</div>
								</td>
							</tr></table>
						";

                            Text = Text.Replace("{InvoiceHeaderInfo}", InvoiceHeaderInfo);
                        }
                    }

                }
                else if (Type == "Invoices" || Type == "ConvertInvoices")
                {
                    string tmpInvoice = ClassID == "11" ? "BS" : (ClassID == "12" ? "BSCoLtd" : "BSM");
                    string dnone = ClassID == "11" || ClassID == "10" ? "d-none" : "";
                    string marginTop = ClassID == "11" || ClassID == "10" ? "mt-4" : "mt-0";
                    string fontSize18 = ClassID == "11" ? "font-size: 18px!important;" : "";

                    string CompanyNameTitleKH = "";
                    string CompanyNameTitleEN = "";
                    string CompanyInfo = "";
                    SQL = @"select * from dbo.TblCompany 
where SystemID = " + systemId + " and WarehouseID = " + wid + " and InvoiceTemplateName = '" + tmpInvoice + "'";
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow drc = ds.Tables[0].Rows[0];
                            CompanyNameTitleKH = drc["CompanyNameKH"].ToString();
                            CompanyNameTitleEN = drc["CompanyName"].ToString();

                            string VATNumber = drc["VATNumber"].ToString();
                            string Address = drc["CompanyAddress"].ToString();
                            string AddressKH = drc["AddressKH"].ToString();
                            string Phone = drc["Phone"].ToString();
                            string PhoneKH = drc["PhoneKH"].ToString();
                            //string txtLabel = ClassID == "11" ? "លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN)៖" : "លេខអត្តសញ្ញាណកម្ម អតប (VAT TIN)៖";
                            string txtLabel = "លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN)៖";

                            CompanyInfo = $@"<div class=""company-info {(Type == "Invoices" ? dnone : "")}"">
								<div><span>{txtLabel}</span><span> {VATNumber}</span></div>
								<div><span>អាសយដ្ឋាន៖​</span><span> {AddressKH}</span></div>
								<div><span>Head Office:</span><span> {Address}</span></div>
								<div><span>ទូរស័ព្ទលេខ៖</span><span> {PhoneKH}</span></div>
								<div><span>HP:</span><span> {Phone}</span></div>
							";
                        }
                    }

                    //Title & company info
                    string InvoiceTitle = ClassID == "11" || ClassID == "10"
                        ? "Invoice" : isCM
                        ? "Commercial Invoice"
                        : "Tax Invoice";
                    string InvoiceTitleKH = ClassID == "11" || ClassID == "10"
                        ? "វិក្កយបត្រ" : isCM
                        ? "វិក្កយបត្រ"
                        : "វិក្កយបត្រអាករ";
                    //string showQr = ClassID == "12" ? "d-flex" : "d-none";

                    //Text = Text.Replace("{show-only-bscoltd}", showQr);
                    Text = Text.Replace("{company-info}", Type == "Invoices" ? dnone : "");
                    Text = Text.Replace("{mg-top}", Type == "Invoices" ? marginTop : "");
                    Text = Text.Replace("{MainTitle}", CompanyNameTitleKH);
                    Text = Text.Replace("{SubTitle}", CompanyNameTitleEN);
                    Text = Text.Replace("{CompanyInfo}", CompanyInfo);
                    Text = Text.Replace("{InvoiceTitle}", InvoiceTitle);
                    Text = Text.Replace("{InvoiceTitleKH}", InvoiceTitleKH);

                    //=========Header Info=========//
                    string customerName = dr["CustomerName"].ToString();
                    string customerPhone = dr["CustomerMobile"].ToString();
                    string customerAddress = dr["CustomerAddress"].ToString();
                    string customerVATNumber = dr["VATNumber"].ToString();
                    string customerCompanyNameKH = dr["CompanyNameKH"].ToString();
                    string attName = dr["ATTName"].ToString();
                    string sellerName = dr["SellerName"].ToString();
                    string invoiceNumber = dr["InvoiceNumber"].ToString();
                    string newInvoiceNumber = dr["NewInvoiceNumber"].ToString();
                    string dnNumber = dr["DNNumber"].ToString();
                    string soNumber = Type == "ConvertInvoices" ? "" : dr["SONumber"].ToString();
                    string invoiceDate = dr["TranDate"].ToString();
                    string dueDate = dr["DueDate"].ToString();
                    string quoteNumber = dr["QuoteNumber"].ToString();

                    // Split by comma, trim, sort ascending, rejoin with line breaks
                    if (!string.IsNullOrWhiteSpace(soNumber))
                    {
                        var sorted = soNumber.Split(',')
                                             .Select(s => s.Trim())
                                             .Where(s => !string.IsNullOrEmpty(s))
                                             .OrderBy(s => s)
                                             .ToArray();

                        var chunks = sorted.Select((s, i) => new { s, i })
                                           .GroupBy(x => x.i / 2)
                                           .Select(g => string.Join(", ", g.Select(x => x.s)));

                        soNumber = string.Join(",<br/>", chunks);
                    }
                    if (systemId == "27" || systemId == "25")
                    {
                        string InvoiceHeaderInfo = "";
                        InvoiceHeaderInfo = $@"<div style=""max-width: 50%; {fontSize18}"" >
							<div><span>ឈ្មោះក្រុមហ៊ុន ឬអតិថិជន៖</span><span> {customerCompanyNameKH}</span></div>
							<div><span>Company Name/Customer:</span><span> {customerName}</span></div>
							<div><span>អាសយដ្ឋាន៖</span><span> {(Regex.IsMatch(customerAddress, @"[\u1780-\u17FF]") ? customerAddress : "")}</span></div>
							<div><span>Address:</span><span> {(!Regex.IsMatch(customerAddress, @"[\u1780-\u17FF]") ? customerAddress : "")}</span></div>
							<div><span>ទូរស័ព្ទលេខ៖</span><span> {(Regex.IsMatch(customerPhone, @"[\u1780-\u17FF]") ? customerPhone : "")}</span></div>
							<div><span>Phone:</span><span> {(!Regex.IsMatch(customerPhone, @"[\u1780-\u17FF]") ? customerPhone : "")}</span></div>
							<div><span>Attention:</span><span> {attName}</span></div>
							<div class=""{dnone} mb-4""><span>លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN):</span><span> {customerVATNumber}</span></div>
						</div>
						<div style=""max-width: 50%; {fontSize18}"" >
							<div><span>លេខរៀងវិក្កយបត្រ៖</span><span></span></div>
							<div><span>INVOICE:</span><span> {(Type == "Invoices" ? invoiceNumber : newInvoiceNumber)}</span></div>
							<div><span>កាលបរិច្ឆេទ៖</span><span></span></div>
							<div><span>Date:</span><span> {invoiceDate}</span></div>
							<div class=""hidden""><span>SEF:</span><span> {sellerName}</span></div>
							<div><span>SO:</span><span> {soNumber}</span></div>
							<div><span>Due Date:</span><span> {dueDate}</span></div>
							<div class=""hidden""><span>DN:</span><span></span> {dnNumber}</div>
							<div><span>Exchange Rate:</span><span> {float.Parse(dr["ExchangeRate"].ToString()).ToString("#,#0") + khr}</span></div>
						</div>";

                        Text = Text.Replace("{InvoiceHeaderInfo}", InvoiceHeaderInfo);
                    }
                }
                else if (Type == "Quotation")
                {
                    if (ClassID == "11" || ClassID == "10")
                    {
                        string tmpInvoice = ClassID == "11" ? "BS" : "BSM";
                        string dnone = ClassID == "11" || ClassID == "10" ? "d-none" : "";
                        string marginTop = ClassID == "11" || ClassID == "10" ? "mt-4" : "mt-0";
                        string fontSize18 = ClassID == "11" ? "font-size: 18px!important;" : "";

                        string CompanyNameTitleKH = "";
                        string CompanyNameTitleEN = "";
                        string CompanyInfo = "";
                        SQL = @"select * from dbo.TblCompany 
where SystemID = " + systemId + " and WarehouseID = " + wid + " and InvoiceTemplateName = '" + tmpInvoice + "'";
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow drc = ds.Tables[0].Rows[0];
                                CompanyNameTitleKH = drc["CompanyNameKH"].ToString();
                                CompanyNameTitleEN = drc["CompanyName"].ToString();

                                string Address = drc["CompanyAddress"].ToString();
                                string Email = drc["Email"].ToString();
                                string Website = drc["Website"].ToString();
                                string Phone = drc["Phone"].ToString();

                                string bsOnlyDnoneInfo = ClassID == "11" ? "d-none" : "";
                                CompanyInfo = $@"<div class=""company-info {bsOnlyDnoneInfo}"">
								<div><span>Head Office:</span><span> {Address}</span></div>
								<div><span>HP:</span><span> {Phone}</span></div>
								<div><span>Email:</span><span> {Email}</span></div>
								<div><span>Website:</span><span> {Website}</span></div>
							";
                            }
                        }

                        //Title & company info
                        string QuoteTitle = "Quotation";
                        string QuoteTitleKH = "សម្រង់តម្លៃ";

                        string bsOnlyDnone = ClassID == "11" ? "d-none" : "";
                        string bsOnlyShow = ClassID == "11" ? "" : "d-none";
                        string hideForbs = ClassID == "11" ? "d-none" : "";
                        string titleCol = ClassID == "11" ? "col-7" : "col-12";
                        Text = Text.Replace("{defaultHideLogo}", bsOnlyDnone);
                        Text = Text.Replace("{company-info}", bsOnlyDnone);
                        Text = Text.Replace("{show-for-bs}", bsOnlyShow);
                        Text = Text.Replace("{hide-for-bs}", hideForbs);
                        Text = Text.Replace("{title-col}", titleCol);
                        Text = Text.Replace("{mg-top}", marginTop);
                        Text = Text.Replace("{MainTitle}", CompanyNameTitleKH);
                        Text = Text.Replace("{SubTitle}", CompanyNameTitleEN);
                        Text = Text.Replace("{CompanyInfo}", CompanyInfo);
                        Text = Text.Replace("{QuoteTitle}", QuoteTitle);
                        Text = Text.Replace("{QuoteTitleKH}", QuoteTitleKH);

                        //=========Header Info=========//
                        string customerName = dr["CustomerName"].ToString();
                        string customerPhone = dr["CustomerMobile"].ToString();
                        string customerAddress = dr["CustomerAddress"].ToString();
                        string email = dr["CustomerEmail"].ToString();
                        string attName = dr["ATTName"].ToString();
                        string sellerName = dr["SellerName"].ToString();
                        string quoteDate = dr["TranDate"].ToString();
                        string dueDate = dr["DueDate"].ToString();
                        string quoteNumber = dr["QuoteNumber"].ToString();

                        if (systemId == "27" || systemId == "25")
                        {
                            string QuoteHeaderInfo = "";
                            QuoteHeaderInfo = $@"
								<table class=""table mytable""><tr>
									<td width=""50%"">
										<div>
											<div><span>Customer:</span><span> {customerName}</span></div>
											<div><span>Address:</span><span> {customerAddress}</span></div>
											<div><span>Phone:</span><span> {customerPhone}</span></div>
											<div><span>Email:</span><span> {email}</span></div>
											<div><span>Attention:</span><span> {attName}</span></div>
										</div>
									</td>
									<td width=""50%"">
										<div>
											<div><span>Quotation Nº:</span><span> {quoteNumber}</span></div>
											<div><span>SEF:</span><span> {sellerName}</span></div>
											<div><span>Date:</span><span> {quoteDate}</span></div>
											<div><span>Due Date:</span><span> {dueDate}</span></div>
											<div><span>Exchange Rate:</span><span> {float.Parse(dr["ExchangeRate"].ToString()).ToString("#,#0") + khr}</span></div>
										</div>
									</td>
								</tr></table>
							";

                            Text = Text.Replace("{QuoteHeaderInfo}", QuoteHeaderInfo);
                        }
                    }
                    else if (ClassID == "12")
                    {
                        string tmpInvoice = ClassID == "12" ? "BSCoLtd" : "BSM";
                        string CompanyNameTitleKH = "";
                        string CompanyNameTitleEN = "";
                        string CompanyInfo = "";
                        SQL = @"select * from dbo.TblCompany 
where SystemID = " + systemId + " and WarehouseID = " + wid + " and InvoiceTemplateName = '" + tmpInvoice + "'";
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow drc = ds.Tables[0].Rows[0];
                                CompanyNameTitleKH = drc["CompanyNameKH"].ToString();
                                CompanyNameTitleEN = drc["CompanyName"].ToString();

                                string VATNumber = drc["VATNumber"].ToString();
                                string Address = drc["CompanyAddress"].ToString();
                                string AddressKH = drc["AddressKH"].ToString();
                                string Phone = drc["Phone"].ToString();
                                string PhoneKH = drc["PhoneKH"].ToString();
                                string txtLabel = "លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN)៖";

                                CompanyInfo = $@"<div>
								<div><span>{txtLabel}</span><span> {VATNumber}</span></div>
								<div><span>អាសយដ្ឋាន៖​</span><span> {AddressKH}</span></div>
								<div><span>Head Office:</span><span> {Address}</span></div>
								<div><span>ទូរស័ព្ទលេខ៖</span><span> {PhoneKH}</span></div>
								<div><span>HP:</span><span> {Phone}</span></div>
							";
                            }
                        }

                        //Title & company info
                        string InvoiceTitle = "Quotation";
                        string InvoiceTitleKH = "សម្រង់តម្លៃ";
                        Text = Text.Replace("{MainTitle}", CompanyNameTitleKH);
                        Text = Text.Replace("{SubTitle}", CompanyNameTitleEN);
                        Text = Text.Replace("{CompanyInfo}", CompanyInfo);
                        Text = Text.Replace("{InvoiceTitle}", InvoiceTitle);
                        Text = Text.Replace("{InvoiceTitleKH}", InvoiceTitleKH);
                        Text = Text.Replace("{defaultHideLogo}", "");
                        Text = Text.Replace("{company-info}", "");
                        Text = Text.Replace("{show-for-bs}", "d-none");
                        Text = Text.Replace("{hide-for-bs}", "");
                        Text = Text.Replace("{title-col}", "col-12");
                        Text = Text.Replace("{mg-top}", "mt-0");

                        //=========Header Info=========//
                        string customerName = dr["CustomerName"].ToString();
                        string customerPhone = dr["CustomerMobile"].ToString();
                        string customerAddress = dr["CustomerAddress"].ToString();
                        string customerVATNumber = dr["VATNumber"].ToString();
                        string attName = dr["ATTName"].ToString();
                        string sellerName = dr["SellerName"].ToString();
                        string quoteDate = dr["TranDate"].ToString();
                        string dueDate = dr["DueDate"].ToString();
                        string quoteNumber = dr["QuoteNumber"].ToString();

                        if (systemId == "27" || systemId == "25")
                        {
                            string InvoiceHeaderInfo = "";
                            InvoiceHeaderInfo = $@"
							<table class=""table mytable""><tr>
								<td width=""50%"">
									<div>
										<div><span>ឈ្មោះក្រុមហ៊ុន ឬអតិថិជន៖</span><span></span></div>
										<div><span>Company Name/Customer:</span><span> {customerName}</span></div>
										<div><span>អាសយដ្ឋាន៖</span><span></span></div>
										<div><span>Address:</span><span> {customerAddress}</span></div>
										<div><span>ទូរស័ព្ទលេខ៖</span><span></span></div>
										<div><span>Phone:</span><span> {customerPhone}</span></div>
										<div><span>Attention:</span><span> {attName}</span></div>
										<div><span>លេខអត្តសញ្ញាណកម្មសារពើពន្ធ (TIN):</span><span> {customerVATNumber}</span></div>
									</div>
								</td>
								<td width=""50%"">
									<div>
										<div><span>លេខរៀងសម្រង់តម្លៃ៖</span><span></span></div>
										<div><span>Quotation Nº:</span><span> {quoteNumber}</span></div>
										<div><span>កាលបរិច្ឆេទ៖</span><span></span></div>
										<div><span>Date:</span><span> {quoteDate}</span></div>
										<div><span>REF:</span><span> {sellerName}</span></div>
										<div><span>Due Date:</span><span> {dueDate}</span></div>
										<div><span>DN:</span><span></span></div>
										<div><span>Exchange Rate:</span><span> {float.Parse(dr["ExchangeRate"].ToString()).ToString("#,#0") + khr}</span></div>
									</div>
								</td>
							</tr></table>
						";

                            Text = Text.Replace("{InvoiceHeaderInfo}", InvoiceHeaderInfo);
                        }
                    }
                }
                else if (Type == "DeliveryNote")
                {
                    if (ClassID == "10")
                    {
                        string tmpInvoice = ClassID == "12" ? "BSCoLtd" : "BSM";
                        string CompanyNameTitleKH = "";
                        string CompanyNameTitleEN = "";
                        string CompanyInfo = "";
                        SQL = @"select * from dbo.TblCompany 
where SystemID = " + systemId + " and WarehouseID = " + wid + " and InvoiceTemplateName = '" + tmpInvoice + "'";
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow drc = ds.Tables[0].Rows[0];
                                CompanyNameTitleKH = drc["CompanyNameKH"].ToString();
                                CompanyNameTitleEN = drc["CompanyName"].ToString();

                                string Address = drc["CompanyAddress"].ToString();
                                string Phone = drc["Phone"].ToString();

                                CompanyInfo = $@"<div style=""width:50%;"">
										<div><span>Head Office:</span><span> {Address}</span></div>
										<div><span>HP:</span><span> {Phone}</span></div>
									</div>
								";
                            }
                        }

                        //Title 
                        Text = Text.Replace("{CompanyInfo}", CompanyInfo);
                        Text = Text.Replace("{MainTitle}", "");
                        Text = Text.Replace("{SubTitle}", "Delivery Note");
                        //=========Header Info=========//
                        string customerName = dr["CustomerName"].ToString();
                        string customerPhone = dr["CustomerMobile"].ToString();
                        string customerAddress = dr["CustomerAddress"].ToString();
                        string soNumber = dr["SONumber"].ToString();
                        string dnNumber = dr["DNNumber"].ToString();
                        string dnDate = dr["TranDate"].ToString();
                        string warehouse = dr["WarehouseName"].ToString();
                        string HeaderInfo = "";

                        Text = Text.Replace("{DNNumber}", dnNumber);

                        HeaderInfo = $@"
						<table class=""table mytable mb-2""><tr>
							<td width=""70%"">
								<div>
									<div><span>To Customer Name:</span><span> {customerName}</span></div>
									<div><span>Phone:</span><span> {customerPhone}</span></div>
									<div><span>Address:</span><span> {customerAddress}</span></div>
								</div>
							</td>
							<td width=""30%"">
								<div>
									<div><span>Date:</span><span> {dnDate}</span></div>
									<div><span>SO:</span><span> {soNumber}</span></div>
									<div><span>WH:</span><span> {warehouse}</span></div>
								</div>
							</td>
						</tr></table>";

                        Text = Text.Replace("{HeaderInfo}", HeaderInfo);

                        //Remark
                        string remark = "";
                        string remarkText = dr["StatusID"]?.ToString() == "94"
                            ? (dr.Table.Columns.Contains("Comment") ? dr["Comment"]?.ToString() ?? "" : "")
                            : (dr["Remark"]?.ToString() ?? "");
                        remark = $@"
							<div class=""row remark"">
								<div class=""col-12"">
									<span>✮ Remark:</span>
									<span class=""text-nowrap-pre well well-sm shadow-none"">{remarkText}</span>
								</div>
							</div>
						";
                        Text = Text.Replace("{Remark}", remark);
                    }
                }

                //=========Table data=========//
                string TblHead = "", TblBody = "", TblFooter = "";
                string bgcolor = systemId == "13" ? "#f89c39" : (systemId == "25" ? "#c5d9f1" : (dr["ClassID"].ToString() == "11" ? "#ffde16" : "#ffccff"));
                string txtcolor = systemId == "13" ? "#fff" : (systemId == "25" ? "#000" : "#000");
                string tdItemCodeKh = systemId == "25" & Type == "SalesOrder" ? "<th class=\"text-center nowrap\" width=\"10%\">កូដ</th>" : "";
                string tdItemCode = systemId == "25" && Type == "SalesOrder" ? "<th class=\"text-center\" width=\"10%\">Code</th>" : "";
                //Talbe header
                if (Type != "DeliveryNote")
                {
                    if (Type == "Invoices" && ClassID == "12")
                    {
                        TblHead = $@"
						    <thead style=""background:{bgcolor}; color:{txtcolor};"">
							    <tr>
			            		    <th class=""text-center"" width=""5%"">ល.រ</th>
								    {tdItemCodeKh}
			            		    <th class=""text-center"" width=""45%"">បរិយាយ</th>
			            		    <th class=""text-center"" width=""5%"">ខ្នាត</th>
			            		    <th class=""text-center"" width=""5%"">បរិមាណ</th>
			            		    <th class=""text-nowrap text-center"" width=""10%"">ថ្លៃឯកតា</th>
			            		    <th colspan=""2"" class=""text-center"" width=""15%"" nowrap>ថ្លៃទំនិញ/សេវា</th>
							    </tr>
							    <tr>
			            		    <th class=""text-center"" width=""5%"">No</th>
								    {tdItemCode}
			            		    <th class=""text-center"" width=""45%"">Description</th>
			            		    <th class=""text-center"" width=""5%"">UOM</th>
			            		    <th class=""text-center"" width=""5%"">QTY</th>
			            		    <th class=""text-nowrap text-center"" width=""10%"">Unit Price</th>
			            		    <th class=""text-center"" width=""8%"">(Dollar)</th>
			            		    <th class=""text-center"" width=""8%"">(Riel)</th>
							    </tr>
						    </thead>
					    ";

                        //Table body
                        int i = 0;
                        foreach (DataRow dr3 in SalesDetail)
                        {
                            i++;
                            string tdItemCodeData = systemId == "25" & Type == "SalesOrder" ? "<td valign=\"top\">" + dr3["ItemCode"].ToString() + " </td>" : "";
                            string tdDiscount = "<td valign=\"top\"><div class=\"d-flex justify-content-between\"><div>" + usd + "</div><div>" + float.Parse(dr3["DisUSD"].ToString()).ToString("#,#0.00") + "</div></div></td>";
                            TblBody += $@"
                                <tr style=""text-align:center"">
                                    <td valign=""top"">{i}</td>
							        {tdItemCodeData}
                                    <td valign=""top"" align=""left"">{"<b>" + dr3["ItemName"] + "</b>" +
                                            (!string.IsNullOrEmpty(dr3["ItemDescription"].ToString())
                                            ? "<br /><div class=\"text-nowrap-pre\" style=\"font-size: 0.9rem;\">" + dr3["ItemDescription"] + "</div>" : "")}</td>
                                    <td valign=""top"">{dr3["UOM"].ToString()}</td>
                                    <td valign=""top"">{dr3["QTY"].ToString()}</td>
                                    <td valign=""top"">
								        <div class=""d-flex justify-content-between"">
									        <div>{usd}</div>
									        <div>{dr3["UnitPrice"].ToString()}<div/>
								        </div>
							        </td>
                                    <td valign=""top"">
								        <div class=""d-flex justify-content-between"">
									        <div>{usd}</div>
									        <div>{float.Parse(dr3["Amount"].ToString()).ToString("#,#0.00")}<div/>
								        </div>
							        </td>
                                    <td valign=""top"" class=""text-right"">
									    <div>{(float.Parse(dr3["Amount"].ToString()) * int.Parse(exchangeRate)).ToString("#,#0")}{khr}<div/>
							        </td>
                                </tr>
                            ";
                        }
                    }
                    else
                    {
                        TblHead = $@"
						    <thead style=""background:{bgcolor}; color:{txtcolor};"">
							    <tr>
			            		    <th class=""text-center"" width=""5%"">ល.រ</th>
								    {tdItemCodeKh}
			            		    <th class=""text-center"" width=""45%"">បរិយាយ</th>
			            		    <th class=""text-center"" width=""5%"">ខ្នាត</th>
			            		    <th class=""text-center"" width=""5%"">បរិមាណ</th>
			            		    <th class=""text-nowrap text-center"" width=""10%"">ថ្លៃឯកតា</th>
			            		    <th class=""text-center"" width=""10%"" nowrap>បញ្ចុះតម្លៃ</th>
			            		    <th class=""text-center"" width=""15%"" nowrap>ថ្លៃទំនិញ/សេវា</th>
							    </tr>
							    <tr>
			            		    <th class=""text-center"" width=""5%"">No</th>
								    {tdItemCode}
			            		    <th class=""text-center"" width=""45%"">Description</th>
			            		    <th class=""text-center"" width=""5%"">UOM</th>
			            		    <th class=""text-center"" width=""5%"">QTY</th>
			            		    <th class=""text-nowrap text-center"" width=""10%"">Unit Price</th>
			            		    <th class=""text-center"" width=""10%"">Discount</th>
			            		    <th class=""text-center"" width=""15%"">Amount</th>
							    </tr>
						    </thead>
					    ";

                        //Table body
                        int i = 0;
                        foreach (DataRow dr3 in SalesDetail)
                        {
                            i++;
                            string tdItemCodeData = systemId == "25" & Type == "SalesOrder" ? "<td valign=\"top\">" + dr3["ItemCode"].ToString() + " </td>" : "";
                            string tdDiscount = "<td valign=\"top\"><div class=\"d-flex justify-content-between\"><div>" + usd + "</div><div>" + float.Parse(dr3["DisUSD"].ToString()).ToString("#,#0.00") + "</div></div></td>";
                            TblBody += $@"
                                <tr style=""text-align:center"">
                                    <td valign=""top"">{i}</td>
							        {tdItemCodeData}
                                    <td valign=""top"" align=""left"">{"<b>" + dr3["ItemName"] + "</b>" +
                                            (!string.IsNullOrEmpty(dr3["ItemDescription"].ToString())
                                            ? "<br /><div class=\"text-nowrap-pre\" style=\"font-size: 0.9rem;\">" + dr3["ItemDescription"] + "</div>" : "")}</td>
                                    <td valign=""top"">{dr3["UOM"].ToString()}</td>
                                    <td valign=""top"">{dr3["QTY"].ToString()}</td>
                                    <td valign=""top"">
								        <div class=""d-flex justify-content-between"">
									        <div>{usd}</div>
									        <div>{dr3["UnitPrice"].ToString()}<div/>
								        </div>
							        </td>
							        {tdDiscount}
                                    <td valign=""top"">
								        <div class=""d-flex justify-content-between"">
									        <div>{usd}</div>
									        <div>{float.Parse(dr3["Amount"].ToString()).ToString("#,#0.00")}<div/>
								        </div>
							        </td>
                                </tr>
                            ";
                        }
                    }
                }
                else
                {
                    TblHead = $@"
						<thead style=""background:{bgcolor}; color:{txtcolor};"">
							<tr>
			            		<th class=""text-center"" width=""10%"">No</th>
			            		<th class=""text-center"" width=""10%"">Code</th>
			            		<th class=""text-center"" width=""50%"">Description</th>
			            		<th class=""text-center"" width=""10%"">Size</th>
			            		<th class=""text-center"" width=""10%"">QTY</th>
			            		<th class=""text-center"" width=""10%"">UOM</th>
							</tr>
						</thead>
					";

                    //Table body
                    int i = 0;
                    float totalQty = 0;
                    foreach (DataRow dr3 in SalesDetail)
                    {
                        i++;
                        TblBody += $@"
                        <tr style=""text-align:center"">
                            <td valign=""top"">{i}</td>
                            <td valign=""top"">{dr3["ItemCode"].ToString()}</td>
                            <td valign=""top"" align=""left"">{dr3["ItemName"]}</td>
                            <td valign=""top"" nowrap>{dr3["ItemSize"].ToString()}</td>
                            <td valign=""top"" nowrap>{dr3["QTY"].ToString()}</td>
                            <td valign=""top"" nowrap>{dr3["UOM"].ToString()}</td>
                        </tr>
                    ";

                        totalQty += float.Parse(dr3["QTY"].ToString());
                    }
                    if (i < 14)
                    {
                        i++;
                        for (var j = i; j <= 14; j++)
                        {
                            TblBody += $@"
                            <tr>
                                <td align=""center""></td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td align=""right"">&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>";
                        }
                        TblBody += $@"
                        <tr>
                            <td colspan=""4"" align=""right"">Total QTY:</td>
                            <td align=""center"">{totalQty}</td>
                            <td>&nbsp;</td>
                        </tr>";
                    }
                }

                //Table footer
                if (Type == "SalesOrder")
                {
                    if (ClassID == "11" || ClassID == "10")
                    {
                        string colspan = ClassID == "10" ? "3" : "2";
                        // Evaluate condition BEFORE the string
                        string depositRowClass = float.Parse(dr2["Deposit"].ToString()) > 0 ? "" : "d-none";

                        TblFooter += $@"
                            <tfoot>
                                <tr>
                                    <td colspan=""{colspan}"" rowspan=""5"">
                                        <div class=""row"">
                                            <div class=""col"">
                                                <pre class=""text-nowrap-pre p-0"" style=""font-family: 'Times New Roman'!important;"">{dr["TermCondition"].ToString()}</pre>
                                            </div>
                                        </div>
                                    </td>
                                    <th class=""text-right"" colspan=""4"">Total:</th>
                                    <td class=""text-center"">
                                        <div class=""d-flex justify-content-between"">
                                            <div>{usd}</div>
                                            <div>{float.Parse(dr2["SubTotal"].ToString()).ToString("#,#0.00")}</div>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <th class=""text-right"" colspan=""4"">Discount $:</th>
                                    <td class=""text-center"">
                                        <div class=""d-flex justify-content-between"">
                                            <div>{usd}</div>
                                            <div>{float.Parse(dr2["DiscountUSD"].ToString()).ToString("#,#0.00")}</div>
                                        </div>
                                    </td>
                                </tr>
                                <tr class=""{depositRowClass}"">
                                    <th class=""text-right"" colspan=""4"">Deposit:</th>
                                    <td class=""text-center"">
                                        <div class=""d-flex justify-content-between"">
                                            <div>{usd}</div>
                                            <div>{float.Parse(dr2["Deposit"].ToString()).ToString("#,#0.00")}</div>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <th class=""text-right"" colspan=""4"">Grand Total USD:</th>
                                    <td class=""text-center"">
                                        <div class=""d-flex justify-content-between"">
                                            <div>{usd}</div>
                                            <div>{(float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())).ToString("#,#0.00")}</div>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <th class=""text-right"" colspan=""4"">Grand Total KHR:</th>
                                    <td class=""text-center"">
                                        <div class=""d-flex justify-content-between"">
                                            <div>{khr}</div>
                                            <div>{((float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())) * int.Parse(exchangeRate)).ToString("#,#0")}</div>
                                        </div>
                                    </td>
                                </tr>
                            </tfoot>
                        ";
                    }
                    else if (ClassID == "12")
                    {
                        TblFooter += $@"
						<tfoot>
							<tr>
								<td colspan=""2"" rowspan=""5"" style=""border-bottom: 0 !important; border-left: 0 !important;"">
									<div class=""row"">
										<div class=""col"">
											<pre class=""text-nowrap-pre p-0"" style=""font-family: 'Times New Roman'!important;"">{dr["TermCondition"].ToString()}</pre>
										</div>
									</div>
								</td>
								<th class=""text-left"" colspan=""4"">ទឹកប្រាក់សរុប Total Amount:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["SubTotal"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr class=""{(isCM ? "d-none" : "")}"">
								<th class=""text-left"" colspan=""4"">អាករលើតម្លៃបន្ថែម VAT(10%):</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["VAT"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-left"" colspan=""4"">ទឹកប្រាក់កក់ដំបូង 50% First Deposit 50%:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["Deposit"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-left"" colspan=""4"">សរុបរួមទាំងអាករ Total Included VAT{(isCM ? "" : " 10%")}:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{(float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-left"" colspan=""4"">ទឹកប្រាក់សរុបរួមទាំងអាករជាលុយរៀល (KHR):</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>KHR</div>
										<div>{((float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())) * int.Parse(exchangeRate)).ToString("#,#0")}</div>
									</div>
								</td>
							</tr>
						</tfoot>
					";
                    }
                }
                else if (Type == "Invoices" || Type == "ConvertInvoices")
                {
                    if (ClassID == "11" || ClassID == "10")
                    {
                        string borderLeftNone = "border-left: none!important;";
                        string borderBottomNone = "border-bottom: none!important";
                        string newTerm = "<br>3. Can not take to tax declaration.";
                        string termsAndConditions = dr["TermCondition"]?.ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(termsAndConditions))
                        {
                            para.Clear();
                            para.Add("SystemID", systemId);
                            para.Add("WarehouseID", wid);
                            SQL = @"select top 1 TermConditionNonVAT from dbo.TblCompany where SystemID = @SystemID and WarehouseID = @WarehouseID";
                            ds = html.LoadData(SQL, false, para);
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                termsAndConditions = ds.Tables[0].Rows[0]["TermConditionNonVAT"]?.ToString() ?? "";
                            }
                        }
                        string term = termsAndConditions;
                        if (Type == "Invoices" && !string.IsNullOrEmpty(termsAndConditions))
                        {
                            if (termsAndConditions.EndsWith("&#013;&#010;"))
                            {
                                term = termsAndConditions.Substring(0, termsAndConditions.Length - "&#013;&#010;".Length) + newTerm;
                            }
                            else
                            {
                                term = termsAndConditions + newTerm;
                            }
                        }
                        TblFooter += $@"
							<tfoot>
								<tr>
									<td colspan=""2"" rowspan=""5"" style=""{borderLeftNone} {borderBottomNone}"">
										<div class=""row"">
											<div class=""col"">
												<pre class=""text-nowrap-pre p-0"" style=""font-family: 'Times New Roman'!important;"">{term}</pre>
											</div>
										</div>
									</td>
									<th class=""text-right"" colspan=""4"">ទឹកប្រាក់សរុប Total Amount:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["SubTotal"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>
								<tr>
									<th class=""text-right"" colspan=""4"">ទឹកប្រាក់កក់ Deposit Amount:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["Deposit"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>
								<tr>
									<th class=""text-right"" colspan=""4"">បញ្ចុះតម្លៃ Discount Amount:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["DiscountUSD"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>
								<tr>
									<th class=""text-right"" colspan=""4"">ទឹកប្រាក់សរុបរួម Amount Due:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["Balance"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>
								<tr>
									<th class=""text-right"" colspan=""4"">ទឹកប្រាក់សរុបរួមជាលុយរៀល:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>KHR</div>
											<div>{(float.Parse(dr2["Balance"].ToString()) * int.Parse(exchangeRate)).ToString("#,#0")}</div>
			    						</div>
									</td>
								</tr>
							</tfoot>
							";
                    }
                    else if (ClassID == "12")
                    {
                        para.Clear();
                        para.Add("CompanyID", "64");
                        SQL = @"select TermConditionVAT from dbo.TblCompany where CompanyID = @CompanyID";
                        ds = html.LoadData(SQL, false, para);
                        string termBsColtd = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 ? ds.Tables[0].Rows[0]["TermConditionVAT"].ToString() : "";
                        TblFooter += $@"
						<tfoot>
							<tr>
								<td colspan=""2"" rowspan=""5"" style=""border-bottom: 0 !important; border-left: 0 !important;"">
									<div class=""row"">
										<div class=""col d-flex justify-content-between"">
											<pre class=""text-nowrap-pre p-0"" style=""font-family: 'Times New Roman'!important;"">{termBsColtd}</pre>
											<div class=""d-none""><img src=""../Content/images/bs-coltd-qr.jpg"" height=""180"" alt=""Not Found""/></div>
											<div class=""d-none""><img src=""../Content/images/bs-coltd-qr2.jpg"" height=""180"" alt=""Not Found"" style=""filter: brightness(1.1) !important;""/></div>
											<div><img src=""../Content/images/bs-coltd-qr4.jpg"" height=""250"" alt=""Not Found"" style=""filter: brightness(1) !important;""/></div>
										</div>
									</div>
								</td>
								<th class=""text-right"" colspan=""3"">ទឹកប្រាក់សរុប Total Amount:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["SubTotal"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
								<td class=""text-right"">
									<div>{(float.Parse(dr2["SubTotal"].ToString()) * int.Parse(exchangeRate)).ToString("#,#0")}{khr}</div>
								</td>
							</tr>
                            <tr>
								<th class=""text-right"" colspan=""3"">ទឹកប្រាក់កក់ Deposit Amount:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["Deposit"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
                                <td class=""text-right"">
								    <div>{(float.Parse(dr2["Deposit"].ToString()) * int.Parse(exchangeRate)).ToString("#,#0")}{khr}</div>
                                </td>
							</tr>
							<tr class=""{(isCM ? "d-none" : "")}"">
								<th class=""text-right"" colspan=""3"">អាករលើតម្លៃបន្ថែម VAT(10%):</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["VAT"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
                                <td class=""text-right"">
									<div>{(float.Parse(dr2["VAT"].ToString()) * int.Parse(exchangeRate)).ToString("#,#0")}{khr}</div>
								</td>
							</tr>
							<tr>
								<th class=""text-right text-nowrap"" colspan=""3"">សរុបរួមទាំងអាករ Total Included VAT{(isCM ? "" : " 10%")}:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["GrandTotal"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
								<td class=""text-right"">
									<div>{(float.Parse(dr2["GrandTotal"].ToString()) * int.Parse(exchangeRate)).ToString("#,#0")}{khr}</div>
								</td>
							</tr>
                            <tr>
								<th class=""text-right"" colspan=""3"">សមតុល្យដែលត្រូវទូទាត់ (Balance due):</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{(float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())).ToString("#,#0.00")}</div>
									</div>
								</td>
                                <td class=""text-right"">
								    <div>{((float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())) * int.Parse(exchangeRate)).ToString("#,#0")}{khr}</div>
                                </td>
							</tr>
						</tfoot>
					";
                    }
                }
                else if (Type == "Quotation")
                {
                    if (ClassID == "11" || ClassID == "10")
                    {
                        string borderLeftNone = ClassID == "11" ? "border-left: none!important;" : "";
                        string borderBottomNone = ClassID == "11" ? "border-bottom: none!important;" : "";
                        string colspan = ClassID == "10" ? "3" : "2";

                        string termCondition = dr["TermCondition"]?.ToString();
                        if (string.IsNullOrWhiteSpace(termCondition))
                        {
                            string sqlComp = @"select top 1 QuoteTermConditionNonVAT from dbo.TblCompany where SystemID = " + systemId + " and WarehouseID = " + wid;
                            DataSet dsComp = html.LoadData(sqlComp, false, null);
                            if (dsComp != null && dsComp.Tables.Count > 0 && dsComp.Tables[0].Rows.Count > 0)
                            {
                                termCondition = dsComp.Tables[0].Rows[0]["QuoteTermConditionNonVAT"]?.ToString();
                            }
                        }


                        TblFooter += $@"
							<tfoot>
								<tr>
									<td colspan=""{colspan}"" rowspan=""5"" style=""{borderLeftNone} {borderBottomNone}"">  
										<div class=""row"">
											<div class=""col"">
												<pre class=""text-nowrap-pre p-0"" style=""font-family: 'Times New Roman'!important;"">{termCondition}</pre>
											</div>
										</div>
									</td>
									<th class=""text-right"" colspan=""4"">Total:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["SubTotal"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>
								<tr>
									<th class=""text-right"" colspan=""4"">Discount $:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["DiscountUSD"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>

                                <tr>
									<th class=""text-right"" colspan=""4"">Deposit Amount:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{float.Parse(dr2["Deposit"].ToString()).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>

								<tr>
									<th class=""text-right"" colspan=""4"">Grand Total USD:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{usd}</div>
											<div>{(float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())).ToString("#,#0.00")}</div>
										</div>
									</td>
								</tr>

								<tr>
									<th class=""text-right"" colspan=""4"">Grand Total KHR:</th>
									<td class=""text-center"">
										<div class=""d-flex justify-content-between"">
											<div>{khr}</div>
											<div>{((float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())) * int.Parse(exchangeRate)).ToString("#,#0")}</div>
										</div>
									</td>
								</tr>
							</tfoot>
							";
                    }
                    else if (ClassID == "12")
                    {
                        string termBsColtd = dr["TermCondition"]?.ToString();
                        if (string.IsNullOrWhiteSpace(termBsColtd))
                        {
                            string sqlComp = @"select top 1 QuoteTermConditionVAT from dbo.TblCompany where SystemID = " + systemId + " and WarehouseID = " + wid;
                            DataSet dsComp = html.LoadData(sqlComp, false, null);
                            if (dsComp != null && dsComp.Tables.Count > 0 && dsComp.Tables[0].Rows.Count > 0)
                            {
                                termBsColtd = dsComp.Tables[0].Rows[0]["QuoteTermConditionVAT"]?.ToString();
                            }
                        }

                        TblFooter += $@"
						<tfoot>
							<tr>
								<td colspan=""2"" rowspan=""5"">
									<div class=""row"">
										<div class=""col"">
											<pre class=""text-nowrap-pre p-0"" style=""font-family: 'Times New Roman'!important;"">{termBsColtd}</pre>
										</div>
									</div>
								</td>
								<th class=""text-right"" colspan=""4"">ទឹកប្រាក់សរុប Total Amount:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["SubTotal"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-right"" colspan=""4"">អាករលើតម្លៃបន្ថែម VAT(10%):</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["VAT"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-right"" colspan=""4"">ទឹកប្រាក់កក់ Deposit Amount:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{float.Parse(dr2["Deposit"].ToString()).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-right"" colspan=""4"">សរុបរួមទាំងអាករ Total Included VAT 10%:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{usd}</div>
										<div>{(float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())).ToString("#,#0.00")}</div>
									</div>
								</td>
							</tr>
							<tr>
								<th class=""text-right"" colspan=""4"">ទឹកប្រាក់សរុបរួមទាំងអាករជាលុយរៀល:</th>
								<td class=""text-center"">
									<div class=""d-flex justify-content-between"">
										<div>{khr}</div>
										<div>{((float.Parse(dr2["GrandTotal"].ToString()) - float.Parse(dr2["Deposit"].ToString())) * int.Parse(exchangeRate)).ToString("#,#0")}</div>
									</div>
								</td>
							</tr>
						</tfoot>
					";
                    }
                }

                Text = Text.Replace("{TblHead}", TblHead);
                Text = Text.Replace("{TblBody}", TblBody);
                Text = Text.Replace("{TblFoot}", TblFooter);

                //=========Footer Sign=========//
                if (Type != "DeliveryNote")
                {
                    string IssuedBySignature = "";
                    //string wh = ClassID == "10" && (Type == "Invoices" || Type == "ConvertInvoices") ? "60" : "90";
                    //string whdiv = (ClassID == "11" && Type == "ConvertInvoices") || (ClassID == "10" && Type == "Invoices") ? "height:60px" : "height:90px";

                    string wh = "90";
                    if (Type == "Invoices" && ClassID == "12")
                    {
                        wh = "120";
                    }

                    string whdiv = $"height: {wh}px";


                    Text = Text.Replace("{whdiv}", whdiv);
                    SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                    para.Clear();
                    para.Add("UserID", dr["UserID"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        Text = Text.Replace("{IssuedBy}", dr["IssuedBy"].ToString());
                        //Text = Text.Replace("{IssuedByPosition}", dr["IssuedByPosition"].ToString());
                        Text = Text.Replace("{IssuedDate}", dr["IssuedDate"].ToString());

                        if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                        {
                            string[] aa = new string[10];
                            if (Type == "ConvertInvoices")
                            {
                                string invId = dr["INVID"].ToString();
                                SQL = @"select u.Signature, u.FullName from dbo.TblSales s
inner join dbo.TblUserLogin u on u.UserLoginID = s.UserID 
where s.SalesID = " + invId;
                                ds = html.LoadData(SQL, false);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                                }
                            }
                            else
                            {
                                aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            }
                            string bsSign = aa.FirstOrDefault(f => f.ToLower().Contains("bs")) ?? (aa.Length > 1 ? aa[1] : "");
                            IssuedBySignature = "<img height=\"" + wh + "\" src=\"../Uploads/Users/" + (Type == "Invoices" && ClassID == "12" ? (aa.Length > 1 ? bsSign : "") : (aa.Length > 0 ? aa[0] : "")) + "\" class=\"sign\" alt=\"Not Found\">";

                            if (Type == "ConvertInvoices")
                            {
                                //string userId = ClassID == "10" ? "131" : "128"; //131=puthy, 128=laihieng
                                string userId = "128";
                                //wh = userId == "128" ? "60" : "90";
                                para.Clear();
                                para.Add("UserID", userId);
                                SQL = @"select u.Signature, u.FullName from dbo.TblUserLogin u where u.UserLoginID = @UserID";
                                ds = html.LoadData(SQL, false, para);
                                aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');

                                IssuedBySignature = "<img height=\"" + wh + "\" src=\"../Uploads/Users/" + (aa.Length > 0 ? aa[0] : "") + "\" class=\"sign\" alt=\"Not Found\">";

                            }
                        }
                    }
                    else if (dr["CreateBy"].ToString() != "")
                    {
                        SQL = "select [Signature] from dbo.TblUserLogin where TelegramUserName=@UserName";
                        para.Clear();
                        para.Add("UserName", dr["CreateBy"].ToString());
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            IssuedBySignature = "<img height=\"" + wh + "\" src=\"../Uploads/Users/" + aa[0] + "\" class=\"sign\" alt=\"Not Found\">";
                        }
                    }
                    Text = Text.Replace("{IssuedBySignature}", IssuedBySignature);

                    string CheckedBySignature = "";

                    if (ViewBag.StatusID == "92" || ViewBag.StatusID == "90" || ViewBag.StatusID == "94")
                    {
                        Text = Text.Replace("{CheckedBy}", dr["CheckedBy"].ToString());
                        //Text = Text.Replace("{CheckedByPosition}", dr["CheckedByPosition"].ToString());
                        Text = Text.Replace("{CheckedDate}", dr["CheckedDate"].ToString());

                        SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                        para.Clear();
                        para.Add("UserID", dr["CheckByID"].ToString());
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            CheckedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\" class=\"sign\" alt=\"Not Found\">";
                        }
                        else if (dr["CheckedBy"].ToString() != "")
                        {
                            SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                            para.Clear();
                            string app = dr["ApprovedBy"].ToString();
                            if (app != "")
                            {
                                para.Add("UserName", app);
                                ds = html.LoadData(SQL, false, para);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                                    CheckedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\" class=\"sign\" alt=\"Not Found\">";
                                }
                            }
                        }

                        Text = Text.Replace("{CheckedBySignature}", CheckedBySignature);
                    }
                    else
                    {
                        Text = Text.Replace("{CheckedBySignature}", "");
                        Text = Text.Replace("{CheckedBy}", "");
                        //Text = Text.Replace("{CheckedByPosition}", "");
                        Text = Text.Replace("{CheckedDate}", "");
                    }

                    string ApprovedBySignature = "";

                    if (ViewBag.StatusID == "90")
                    {
                        Text = Text.Replace("{ApprovedBy}", dr["ApprovedBy"].ToString());
                        //Text = Text.Replace("{ApprovedByPosition}", dr["ApprovedByPosition"].ToString());
                        Text = Text.Replace("{ApprovedDate}", dr["ApprovedDate"].ToString());

                        SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                        para.Clear();
                        para.Add("UserID", dr["ApproveByID"].ToString());
                        ds = html.LoadData(SQL, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            ApprovedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\" class=\"sign\" alt=\"Not Found\">";
                        }
                        else if (dr["ApprovedBy"].ToString() != "")
                        {
                            SQL = "select [Signature], Job from dbo.TblUserLogin where TelegramUserName=@UserName";
                            para.Clear();
                            string app = dr["ApprovedBy"].ToString();
                            if (app != "")
                            {
                                para.Add("UserName", app);
                                ds = html.LoadData(SQL, false, para);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                                    ApprovedBySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\" class=\"sign\" alt=\"Not Found\">";
                                }
                            }
                        }

                        Text = Text.Replace("{ApprovedBySignature}", ApprovedBySignature);
                    }
                    else
                    {
                        Text = Text.Replace("{ApprovedBySignature}", "");
                        Text = Text.Replace("{ApprovedBy}", "");
                        //Text = Text.Replace("{ApprovedByPosition}", "");
                        Text = Text.Replace("{ApprovedDate}", "");
                    }
                }
                else
                {
                    Text = Text.Replace("{PreparedBy}", dr["PreparedBy"].ToString());
                    Text = Text.Replace("{PreparedPhone}", dr["PreparedPhone"].ToString());
                    Text = Text.Replace("{PreparedDate}", dr["IssuedDate"].ToString());
                    Text = Text.Replace("{DeliveryBy}", dr["DeliveryBy"].ToString());
                    Text = Text.Replace("{DeliveryPhone}", dr["DeliveryPhone"].ToString());
                    Text = Text.Replace("{DeliveryDate}", dr["IssuedDate"].ToString());
                    Text = Text.Replace("{NeedCheckBy}", dr["NeedCheckBy"].ToString());
                    Text = Text.Replace("{NeedCheckPhone}", dr["NeedCheckPhone"].ToString());
                    Text = Text.Replace("{NeedCheckDate}", dr["IssuedDate"].ToString());

                    string PreparedSignature = "";
                    string DeliverySignature = "";
                    string NeedCheckSignature = "";

                    SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                    para.Clear();
                    para.Add("UserID", dr["PreparedByID"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            PreparedSignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }

                    SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                    para.Clear();
                    para.Add("UserID", dr["DeliveryByID"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            DeliverySignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }

                    SQL = "select [Signature] from dbo.TblUserLogin where UserLoginID=@UserID";
                    para.Clear();
                    para.Add("UserID", dr["NeedCheckByID"].ToString());
                    ds = html.LoadData(SQL, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[0].Rows[0]["Signature"].ToString() != "")
                        {
                            string[] aa = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                            NeedCheckSignature = "<img height=\"90\" src=\"../Uploads/Users/" + aa[0] + "\">";
                        }
                    }

                    Text = Text.Replace("{PreparedSignature}", PreparedSignature);
                    Text = Text.Replace("{DeliverySignature}", DeliverySignature);
                    Text = Text.Replace("{NeedCheckSignature}", NeedCheckSignature);
                }
            }

            if (ReturnAllHtmlData)
                return Json(new { Data = Text.ToString() }, JsonRequestBehavior.AllowGet);
            if (RedirectToPOView)
            {
                @ViewBag.Data = Text.ToString();
                return View("PO");
            }
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//body")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();
            return View();
        }
        [Authorize]
        public ActionResult BLPayment(int VendorID, string OfficialReceipt)
        {
            ViewBag.StatusID = "";
            ViewBag.VendorID = VendorID;
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string FilePath = HttpContext.Server.MapPath("~/Templates/PaymentVoucher.html");
            StreamReader str = new StreamReader(FilePath);
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.GetLocalIP());
            Text = Text.Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"));
            Text = Text.Replace("{InvoiceName}", "Payment Voucher (Disburse)");
            Text = Text.Replace("{From}", "Disburse Payment To");

            string sql = @"  select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                        from dbo.TblVendor
                        where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;
                
                    select vp.OfficalReceiptNo, format(vp.PaymentDate, 'dd-MM-yyyy HH:mm:ss tt') PaymentDate, pm.PaymentMethod, format(vp.TotalPaymentAmount, '#,#.00') TotalPaymentAmount,
                    vp.Reference, vp.Payee
                    from dbo.TblVendorPayment vp
                    inner join dbo.TblPaymentMethod pm on pm.PaymentMethodID = vp.PaymentModeID
                    where vp.VendorID = @VendorID and vp.OfficalReceiptNo = @OfficialReceipt and vp.SystemID = @SystemID and vp.WarehouseID = @WarehouseID;

                    select pd.BillNumber, format(pd.BillDate,'dd MMM yyyy') BillDate, format(pd.Amount, '#,#.00') Amount, format(pd.PaymentAmount, '#,#.00') PaymentAmount, format(pd.AmountDue, '#,#.00') AmountDue,
                    case when pd.StatusID = 15 then '<div class=""badge badge-pill badge-warning text-uppercase text-white text-lg"">' + s.[Status] + '</div>'
                    when pd.StatusID = 11 then '<div class=""badge badge-pill badge-danger text-uppercase text-lg"">' + s.[Status] + '</div>'
                    else '<div class=""badge badge-pill badge-primary text-uppercase text-lg"">' + s.[Status] + '</div>'
                    end [Status], pd.StatusID
                    from dbo.TblVendorPaymentDetail pd
                    inner join dbo.TblStatus s on s.StatusID = pd.StatusID
                    where pd.VendorID = @VendorID and pd.OfficalReceiptNo = @OfficialReceipt and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID;
            ";

            para.Add("VendorID", VendorID.ToString());
            para.Add("OfficialReceipt", OfficialReceipt.ToString());
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            ds = html.LoadData(sql, false, para);
            DataRow drvendorInfo = ds.Tables[0].Rows[0];
            DataRow drvendorPaymentInfo = ds.Tables[1].Rows[0];
            Text = Text.Replace("{Status}", ds.Tables[2].Rows[0]["Status"].ToString());
            ViewBag.StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
            Text = Text.Replace("{CompnayName}", drvendorInfo["VendorName"].ToString());
            Text = Text.Replace("{CompanyAddress}", drvendorInfo["Address"].ToString());
            Text = Text.Replace("{CompanyPhone}", drvendorInfo["HendPhone"].ToString());
            Text = Text.Replace("{CompanyEmail}", drvendorInfo["BillingEmail"].ToString());
            Text = Text.Replace("{CompanyContactName}", drvendorInfo["ContactPersonName"].ToString());
            Text = Text.Replace("{CompanyContactPhone}", drvendorInfo["ContactPhoneNumber"].ToString());

            Text = Text.Replace("{OfficialReceipt}", drvendorPaymentInfo["OfficalReceiptNo"].ToString());
            Text = Text.Replace("{PaymentDate}", drvendorPaymentInfo["PaymentDate"].ToString());
            Text = Text.Replace("{PaymentMethod}", drvendorPaymentInfo["PaymentMethod"].ToString());
            Text = Text.Replace("{Reference}", drvendorPaymentInfo["Reference"].ToString());
            Text = Text.Replace("{Payee}", drvendorPaymentInfo["Payee"].ToString());


            string TblHead = @"
                    <tr>
                    <th>#</th>
                    <th>Bill Number</th>
                    <th>Bill Date</th>
                    <th>Bill Amount</th>
                    <th>Payment Amount</th>
                    <th>Due Amount</th>
                    </tr>
                ";
            Text = Text.Replace("{TblHead}", TblHead);
            StringBuilder sb = new StringBuilder();
            int i = 0;
            double PaymentAmount = 0;
            double BillAmount = 0;
            foreach (DataRow dr in ds.Tables[2].Rows)
            {
                i++;
                PaymentAmount += double.Parse(dr["PaymentAmount"].ToString());
                BillAmount += double.Parse(dr["Amount"].ToString());
                sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
                sb.AppendLine("<td>" + dr["BillNumber"] + "</td>");
                sb.AppendLine("<td>" + dr["BillDate"] + "</td>");
                sb.AppendLine("<td align=\"right\">" + dr["Amount"] + "</td>");
                sb.AppendLine("<td align=\"right\">" + dr["PaymentAmount"] + "</td>");
                sb.AppendLine("<td align=\"right\">" + dr["AmountDue"] + "</td></tr>");
            }
            Text = Text.Replace("{TblBody}", sb.ToString());
            sb.Clear();

            TblHead = @"
                    <tr>
                    <th>#</th>
                    <th>Paid Date</th>
                    <th>Bill#</th>
                    <th>Bill Date</th>
                    <th>Bill Amount</th>
                    <th>Amount Paid</th>
                    <th>Amount Due</th>
                    <th>Account</th>
                    <th>Description</th>
                    </tr>
                ";
            Text = Text.Replace("{RelatedTransactions_TblHead}", TblHead);
            i = 0;
            double TotalPaymentAmount = 0;
            foreach (DataRow dr in ds.Tables[2].Rows)
            {
                sql = @"select format(p.PaymentDate, 'dd-MM-yy HH:mm:ss tt') PaymentDate, (c.AccountNo + ' - ' + c.AccountName) + ' ( ' + c.AccountType + ')' AccountName, format(pd.PaymentAmount, '#,#.00') PaymentAmount,
                p.[Description], pd.BillNumber, format(pd.BillDate, 'dd-MM-yy') BillDate, format(pd.PaymentAmount, '#,#.00') PaymentAmount, format(pd.AmountDue, '#,#.00') AmountDue,
                format(pd.Amount, '#,#.00') Amount, pd.AmountPaid
                from dbo.TblVendorPaymentDetail pd
                inner join dbo.TblVendorPayment p on p.VendorPaymentID = pd.VendorPaymentID
                inner join dbo.TblChartOfAccount c on c.ChartOfAccountID = p.ChartOfAccountID
                where format(p.PaymentDate, 'dd-MM-yyyy HH:mm:ss tt') < @PaymentDate
                and p.VendorID = @VendorID and pd.BillNumber = @BillNumber and pd.SystemID = " + html.SystemID + " and pd.WarehouseID = " + html.WarehouseID;
                para.Clear();
                para.Add("VendorID", VendorID.ToString());
                para.Add("PaymentDate", drvendorPaymentInfo["PaymentDate"].ToString());
                para.Add("BillNumber", dr["BillNumber"].ToString());
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                DataSet data = html.LoadData(sql, false, para);
                foreach (DataRow dRow in data.Tables[0].Rows)
                {
                    TotalPaymentAmount += double.Parse(dRow["PaymentAmount"].ToString());
                    i++;
                    sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
                    sb.AppendLine("<td>" + dRow["PaymentDate"] + "</td>");
                    sb.AppendLine("<td>" + dRow["BillNumber"] + "</td>");
                    sb.AppendLine("<td>" + dRow["BillDate"] + "</td>");
                    sb.AppendLine("<td align=\"right\">" + dRow["Amount"] + "</td>");
                    sb.AppendLine("<td align=\"right\">" + dRow["PaymentAmount"] + "</td>");
                    sb.AppendLine("<td align=\"right\">" + dRow["AmountDue"] + "</td>");
                    sb.AppendLine("<td>" + dRow["AccountName"] + "</td>");
                    sb.AppendLine("<td>" + dRow["Description"] + "</td></tr>");
                }
            }
            Text = Text.Replace("{BillAmount}", BillAmount.ToString("N2"));
            Text = Text.Replace("{TotalAmountPaid}", PaymentAmount.ToString("N2"));
            Text = Text.Replace("{TotalPaymentAmount}", (PaymentAmount + TotalPaymentAmount).ToString("N2"));
            Text = Text.Replace("{DueAmount}", (BillAmount - (PaymentAmount + TotalPaymentAmount)).ToString("N2"));

            Text = Text.Replace("{RelatedTransactions_TblHead_TblBody}", sb.ToString());
            sb.Clear();
            var doc = new HtmlDocument
            {
                //OptionOutputAsXml = true,
                //OptionCheckSyntax = true,
                //OptionFixNestedTags = true,
                //OptionAutoCloseOnEnd = true,
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text.ToString());
            HtmlNode node = doc.DocumentNode.SelectNodes("//section")[0];
            sb.Append(node.InnerHtml);
            @ViewBag.Data = sb.ToString();

            return View();
        }
        [Authorize]
        public JsonResult LoadPaymentsMade(int ID)
        {
            HTML html = new HTML
            {
                ReportTitle = "List Vendor Payment",
                GridEventClick = "onGridPaymentsMadeClick(this);",
                BunSearch = "BtnSearchPaymentsMade"
            };
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Paid Date:",
                FromDateID = "txtPaidDate",
                DBField = "p.PaymentDate",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "CboStatus",
                ComboBox = true,
                Label = "Status:",
                DBField = "pd.StatusID",
                Value = "StatusID",
                DisplayText = "Status",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblVendorPaymentDetail b inner join dbo.TblStatus s on s.StatusID = b.StatusID where b.VendorID = " + ID + " and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Receipt Number",
                ISAutoComplete = true,
                SQL = "select distinct top 10 p.OfficalReceiptNo from dbo.TblBills p inner join dbo.TblVendor v on v.VendorID = p.VendorID where v.VendorID = " + ID + " and lower(p.OfficalReceiptNo) like @SearchText and p.SystemID = " + html.SystemID + " and p.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("pd.OfficalReceiptNo") + "," + EncryAndDecry.Encryp("pd.BillNumber"),
                DefaultCheckBok = true,
                Name = "lblPaymentsMade",
                ID = "TxtPaymentsMade"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Bill Number",
                ISAutoComplete = true,
                SQL = "select distinct top 10 p.BillNumber from dbo.TblBills p inner join dbo.TblVendor v on v.VendorID = p.VendorID where v.VendorID = " + ID + " and lower(p.BillNumber) like @SearchText and p.SystemID = " + html.SystemID + " and p.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("pd.OfficalReceiptNo") + "," + EncryAndDecry.Encryp("pd.BillNumber"),
                Name = "lblPaymentsMade",
                ID = "TxtPaymentsMade"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.TblID = "tblPaymentsMade";
            html.FormID = "frmPaymentsMade";
            string sql = @"select ROW_NUMBER() Over(order by pd.BillID desc) as [No], v.VendorID, pd.BillID, v.VendorCode, v.VendorName,
            (v.VendorCode + ' - ' + v.VendorName) VName, pd.BillNumber, pd.PONumber,
            pd.BillDate, p.PaymentDate, format(pd.Amount, '#,#.00') Amount, format(pd.PaymentAmount, '#,#.00') PaymentAmount, format(pd.AmountDue, '#,#.00') AmountDue,
            p.OfficalReceiptNo,
            case when pd.StatusID = 15 then '<div class=""badge badge-pill badge-warning text-uppercase text-white"">' + s.[Status] + '</div>'
            when pd.StatusID = 11 then '<div class=""badge badge-pill badge-danger text-uppercase"">' + s.[Status] + '</div>'
            else '<div class=""badge badge-pill badge-primary text-uppercase"">' + s.[Status] + '</div>'
            end [Status], p.[Description], p.Memo
            from dbo.TblVendorPayment p
            inner join dbo.TblVendorPaymentDetail pd on pd.VendorPaymentID = p.VendorPaymentID
            inner join dbo.TblVendor v on v.VendorID = p.VendorID
            inner join dbo.TblStatus s on s.StatusID = pd.StatusID
            where v.VendorID = " + ID + " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "pd.BillID desc, pd.VendorPaymentDetailID desc";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Receipt#", FieldName = "OfficalReceiptNo", IsGroupByField = true, GroupLabel="Receipt#: ", Colspan=11 },
                new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                new GridField() { Header = "ReceiptNo", FieldName = "OfficalReceiptNo", IsHiddenField= true },
                new GridField() { Header = "Bill Date", FieldName = "BillDate", Format="dd MMM yyyy" },
                new GridField() { Header = "Bill#", FieldName = "BillNumber", },
                new GridField() { Header = "PO#", FieldName = "PONumber", },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right", ClassName="text-primary" },
                new GridField() { Header = "Payment Date", FieldName = "PaymentDate", Format="dd MMM yyyy HH:mm:ss tt" },
                new GridField() { Header = "Paid", FieldName = "PaymentAmount", TextAlign = "right", ClassName="text-primary" },
                new GridField() { Header = "Amount Due", FieldName = "AmountDue", TextAlign = "right", ClassName="text-red" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Description", FieldName = "Description" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadPaymentsMade") }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadPaymentsStatus(int ID)
        {
            string sql = @"select b.VendorID, count(b.BillID) Bill, sum(b.GrandTotal) BillAmount, sum(isnull(b.AmountPaid, 0)) AmountPaid,
            sum(isnull(b.AmountDue, 0)) AmountDue, b.StatusID
            from dbo.TblBills b
            where b.StatusID not in (14) and b.VendorID = @VendorID and b.SystemID = @SystemID and b.WarehouseID = @WarehouseID
            group by b.VendorID, b.StatusID";
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "VendorID", ID.ToString() }
            };
            string Data = html.JsonData(sql, false, para);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadStock(int ID)
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock In"
            };
            if (html.HasPermission("Edit Stock In"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Stock In"))
                html.AllowDelete = true;

            html.BunSearch = "BtnSearchStock";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Stock In Date:",
                FromDateID = "txtStockDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtWarehouseName_Stock_IN",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                SQL_CBO = "select distinct uw.WID, (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end WarehouseName\r\nfrom dbo.TblStockIn b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.WID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.VendorID = " + ID + " and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Vendor_ID",
                UseText = true,
                Hide = true,
                Value = ID.ToString(),
                Label = "VendorID:",
                DBField = "VendorID"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Bill_Number",
                ISAutoComplete = true,
                Label = "Bill Number:",
                DBField = "BillNumber",
                SQLAutoComplete = "select distinct top 10 BillNumber from dbo.TblStockIn where VendorID = " + ID + " and lower(BillNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_PO_Number",
                ISAutoComplete = true,
                Label = "PO Number:",
                DBField = "PONumber",
                SQLAutoComplete = "select distinct top 10 PONumber from dbo.TblStockIn where VendorID = " + ID + " and lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Name_StockIN",
                ISAutoComplete = true,
                Label = "Item Name:",
                DBField = "ItemName",
                SQLAutoComplete = "select distinct top 10 (case when i.Barcode <> '' then i.Barcode + ' - ' + i.ItemName else i.ItemName end) ItemName from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID where lower((case when i.Barcode <> '' then i.Barcode + ' - ' + i.ItemName else i.ItemName end)) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
            });
            //html.HtmlAddControls.Add(new HtmlAddControls()
            //{
            //	ID = "txt_Item_Group_StockIN",
            //	ISAutoComplete = true,
            //	Label = "Item Group:",
            //	DBField = "ItemGroup",
            //	SQLAutoComplete = "select distinct top 10 ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where lower(ig.ItemGroup) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
            //});
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Receive Note#",
                ISAutoComplete = true,
                SQL = "select distinct top 10 ReceiveNoteNumber from dbo.TblStockIn where VendorID = " + ID + " and lower(ReceiveNoteNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("ReceiveNoteNumber"),
                DefaultCheckBok = true,
                Name = "lbl_Receive_Note_Number",
                ID = "TXT_Receive_Note_Number"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.TblID = "tblStock";
            html.FormID = "frmStock";

            html.GH.GridProcedureName = "SP_SHOW_STOCK_IN";
            html.GridEventClick = "onGridStockIn(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField =true },
                new GridField() { Header = "BillID", FieldName = "BillID", IsHiddenField =true },
                new GridField() { Header = "StockInID", FieldName = "StockInID", IsHiddenField =true },
                new GridField() { Header = "ReceiveNoteNumber", FieldName = "ReceiveNoteNumber", IsHiddenField =true },
                //new GridField() { Header = "VendorName", FieldName = "VendorName", IsGroupByField = true, Colspan = 15 },
                new GridField() { Header = "Warehouse Name", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 7, DisplayOneRow= true, },
                //new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 6, DisplayOneRow= true, },
                new GridField() { Header = "Receive Date", FieldName = "ReceiveDateNumber", IsGroupByField= true, Colspan = 7 },
                //new GridField() { Header = "PO#", FieldName = "PONumber", IsGroupByField= true, GroupLabel="PO#: ", DisplayOneRow= true, OnOneRow= true,  Colspan = 14, },
                //new GridField() { Header = "Warehouse Address", FieldName = "WarehouseAddress", IsGroupByField = true, Colspan = 14 },
                
                //new GridField() { Header = "StockInDate", FieldName = "StockInDate", IsGroupByField= true, Format="dd-MM-yyyy", Colspan=8 },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
				//new GridField() { Header = "Item Group", FieldName = "ItemGroup" },
                //new GridField() { Header = "Received#", FieldName = "ReceiveNoteNumber" },
                //new GridField() { Header = "Bill#", FieldName = "BillNumber" },                
                //new GridField() { Header = "Reference", FieldName = "ReferenceNo" },
                //new GridField() { Header = "Item Name", FieldName = "ItemName" },
                //new GridField() { Header = "Barcode", FieldName = "Barcode" },
                new GridField() { Header = "Order QTY", FieldName = "OrderQTY", TextAlign="right", Format="#,#0" },
                new GridField() { Header = "Received QTY", FieldName = "QTYIN", TextAlign="right", Format="#,#0" },
                new GridField() { Header = "UM", FieldName = "UM" },
                //new GridField() { Header = "QTY Unusable", FieldName = "QTYUnusable", TextAlign="right" },
                //new GridField() { Header = "Reason", FieldName = "Reason" },
                //new GridField() { Header = "Maintain Stock", FieldName = "MaintainStock", TextAlign = "center" },
                //new GridField() { Header = "Action", FieldName = "StockInID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblStockInDetail",
                //LinkField="ItemID, StockInDate", DataTableNumber = 3,
                //DisplayFields="Total QTY IN", Colspan = 8, EditFunction = "EditStock(this);"
                //},
                new GridField() { Header = "Action", FieldName = "StockInID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblStockInDetail",
                EditFunction = "EditStock(this);", LinkField="StockInDate, ReceiveNoteNumber", DisplayFields="Total QTY IN", DataTableNumber = 3, Colspan = 7, IsGroupByField = true
                },
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadStock") }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadStockOut(int ID)
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Out",
                BunSearch = "BtnSearchStockOut"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                //DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Stock Out Date:",
                FromDateID = "txtStockOutDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_WarehouseNameStockOut",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                SQL_CBO = "select distinct uw.WID, (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end WarehouseName\r\nfrom dbo.TblStockOut b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.LocationID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Vendor_ID",
                UseText = true,
                Hide = true,
                Value = ID.ToString(),
                Label = "VendorID:",
                DBField = "VendorID"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Invoice_Number",
                ISAutoComplete = true,
                Label = "Invoice#:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 so.InvoiceNumber from dbo.TblStockOut so inner join dbo.TblStockIn si on si.StockInID = so.StockInID inner join dbo.TblVendor v on v.VendorID = si.VendorID where si.VendorID = " + ID + " and lower(so.InvoiceNumber) like @SearchText and so.SystemID = " + html.SystemID + " and so.WarehouseID=" + html.WarehouseID + " order by so.InvoiceNumber",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Customer_Name",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "CustomerName",
                SQLAutoComplete = "select distinct top 10 (c.CustomerCode + ' - ' + c.CustomerName) CustomerName from dbo.TblStockOut so inner join dbo.TblStockIn si on si.StockInID = so.StockInID inner join dbo.TblVendor v on v.VendorID = si.VendorID inner join dbo.TblCustomer c on c.CustomerID = so.CustomerID where si.VendorID = " + ID + " and lower((c.CustomerCode + ' - ' + c.CustomerName)) like @SearchText and so.SystemID = " + html.SystemID + " and so.WarehouseID=" + html.WarehouseID + " order by (c.CustomerCode + ' - ' + c.CustomerName)",
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 (case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName) ItemName from dbo.TblStockOut so inner join dbo.TblStockIn si on si.StockInID = so.StockInID inner join dbo.TblVendor v on v.VendorID = si.VendorID inner join dbo.TblItem i on i.ItemID = so.ItemID where si.VendorID = " + ID + " and lower((case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName)) like @SearchText and so.SystemID = " + html.SystemID + " and so.WarehouseID=" + html.WarehouseID + " order by (case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName)",
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                DefaultCheckBok = true,
                Name = "lbl_Item_Name_StockOut",
                ID = "TXT_Item_Name_StockOut"
            });
            html.TblID = "tblStockOut";
            html.FormID = "frmStockOut";

            html.GH.GridProcedureName = "SP_SHOW_STOCK_OUT";
            //html.GridEventClick = "onGridStockInClick(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 5, DisplayOneRow= true },
                new GridField() { Header = "DateOut", FieldName = "ItemDateOut", IsGroupByField = true, Colspan = 5 },
                new GridField() { Header = "CustomerCode", FieldName = "CustomerCode", IsGroupByField = true, GroupLabel="Customer Name: ", Colspan = 5 },
                //new GridField() { Header = "CustomerName", FieldName = "CustomerName", IsGroupByField = true, GroupLabel ="- ", Colspan = 5 },
                new GridField() { Header = "ItemName", FieldName = "ItemName" },
                new GridField() { Header = "QTY OUT", FieldName = "QTY", TextAlign="right", Format="#,#0" },
                //new GridField() { Header = "InvoiceNumber", FieldName = "InvoiceNumber" },
                new GridField() { Header = "CostPrice", FieldName = "CostPrice", TextAlign="right", Format="#,#0.00" },
                new GridField() { Header = "SalePrice", FieldName = "SalePrice", TextAlign="right", Format="#,#0.00",
                LinkField="InvoiceNo", DataTableNumber=2, DisplayFields="Total QTY OUT, Cost Price, Sale Price"
                },

            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadStockout") }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadStockStatus(int ID)
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Status",
                BunSearch = "BtnSearchStockStatus"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                //DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockStatusDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtWarehouseName_StockStatus",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                SQL_CBO = "select distinct uw.WID, (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end WarehouseName\r\nfrom dbo.TblStockIn b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.WID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Vendor_ID",
                UseText = true,
                Hide = true,
                Value = ID.ToString(),
                Label = "VendorID:",
                DBField = "VendorID"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Group_StockStatus",
                ISAutoComplete = true,
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQLAutoComplete = "select distinct top 10 ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where lower(ig.ItemGroup) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 (case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName) ItemName from dbo.TblStockIn si inner join dbo.TblVendor v on v.VendorID = si.VendorID inner join dbo.TblStockInDetail sd on sd.StockInID = si.StockInID inner join dbo.TblItem i on i.ItemID = sd.ItemID where si.VendorID = " + ID + " and lower((case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName)) like @SearchText and si.SystemID = " + html.SystemID + " and si.WarehouseID=" + html.WarehouseID + " order by (case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName)",
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                DefaultCheckBok = true,
                Name = "lbl_Item_Name_StockStatus",
                ID = "TXT_Item_Name_StockStatus"
            });
            html.TblID = "tblStockStatus";
            html.FormID = "frmStockStatus";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 3;
            html.GH.GridProcedureName = "SP_SHOW_STOCK_STATUS";
            //html.GridEventClick = "onGridStockInClick(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 10, DisplayOneRow= true },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 10 },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                //new GridField() { Header = "Cost", FieldName = "CostPrice", TextAlign = "right", Format="#,#0.00" },
                new GridField() { Header = "IN", FieldName = "QTYIN", TextAlign = "right", Format="#,#0" },
                new GridField() { Header = "OUT", FieldName = "QTYOUT", TextAlign = "right", Format="#,#0", ClassName="text-red" },
                //new GridField() { Header = "Inventory Value", FieldName = "InventoryValue", TextAlign = "right", Format="#,#0.00", ClassName="text-primary" },
                //new GridField() { Header = "Unit Price", FieldName = "UnitPrice", TextAlign = "right", Format="#,#0.00" },
                //new GridField() { Header = "Sales Value", FieldName = "SalesValue", TextAlign = "right", Format="#,#0.00", ClassName="text-primary" },
                new GridField() { Header = "Available", FieldName = "AvailableStock", TextAlign = "right", Format="#,#0", ClassName="text-red" },
                new GridField() { Header = "Status", FieldName = "Status", DataTableNumber = 2, LinkField="WarehouseName, ItemGroup", DisplayFields="Total QTY IN, Total QTY OUT", TextAlign="center" },
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadStockStatus") }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult LoadStockMaintain(int ID = 0)
        {
            ViewBag.VendorID = ID.ToString();
            HTML html = new HTML
            {
                BunSearch = "BtnSearchStockMaintain"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                //DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockStockMaintain",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtWarehouseName",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                //SQL_CBO = "select distinct uw.WID, (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end WarehouseName\r\nfrom dbo.TblStockIn b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.WID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by (br.BranchName + ' - ' + uw.WarehouseName) + ' ' + \r\nuw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + \r\ncase when uw.District <> '' then ', ' + uw.District end + \r\ncase when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end",
                SQL_CBO = "select distinct uw.WID, uw.WarehouseName\r\nfrom dbo.TblStockIn b inner join dbo.TblUserWarehouse uw on uw.WID = b.WID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Group_StockMaintain",
                ISAutoComplete = true,
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQLAutoComplete = "select distinct top 10 ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where lower(ig.ItemGroup) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'')\r\nelse case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName\r\nfrom dbo.TblStockIn si\r\ninner join dbo.TblVendor v on v.VendorID = si.VendorID\r\ninner join dbo.TblStockInDetail sd on sd.StockInID = si.StockInID\r\ninner join dbo.TblItem i on i.ItemID = sd.ItemID where \r\nsi.VendorID = case when " + ID + " <> 0 then " + ID + " else si.VendorID end and \r\nlower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else\r\ncase when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) \r\nlike @SearchText and si.SystemID = 13 and si.WarehouseID=16\r\norder by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'')\r\nelse case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)",
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                DefaultCheckBok = true,
                Name = "lbl_Item_NameStockMaintain",
                ID = "TXT_Item_NameStockMaintain"
            });
            html.TblID = "tblStockMaintain";
            html.FormID = "frmStockMaintain";
            html.OverrideSearch = "ShowStockMaintain();";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "ShowStockMaintain");
            return View();
        }
        [Authorize]
        public ActionResult ShowStockMaintain(string VendorID, string FromDate, string ToDate, string ItemName, string ItemGroup, string WarehouseName)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true,
                ShowPaging = false,
                TblID = "Tbl_Show_Stock_In"
            };
            html.GH.GridProcedureName = "SP_SHOW_STOCK_Maintain";
            html.TblAlign = "center";
            html.TableClassName = "table table-bordered table-head-fixed";
            html.GH.GridPara = new GridPara[]
            {
                new GridPara()
                {
                    ParaName="FromDate",
                    Value=FromDate
                },
                new GridPara()
                {
                    ParaName="ToDate",
                    Value=ToDate
                },
                new GridPara()
                {
                    ParaName="ItemName",
                    Value=ItemName
                },
                new GridPara()
                {
                    ParaName="VendorID",
                    Value=VendorID.ToString()
                },
                new GridPara()
                {
                    ParaName="ItemGroup",
                    Value=ItemGroup
                },
                new GridPara()
                {
                    ParaName="SystemID",
                    Value=html.SystemID
                },
                new GridPara()
                {
                    ParaName="WarehouseID",
                    Value=html.WarehouseID
                },
                new GridPara()
                {
                    ParaName="WarehouseName",
                    Value=WarehouseName
                },
            };
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Item Name", FieldName = "ItemName", IsGroupByField = true, Colspan=3 },
                new GridField() { Header = "Date IN", FieldName = "StockInDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "QTY", FieldName = "QTYIN", TextAlign="right", Format="#,#0"
                , LinkField="ItemID", DataTableNumber = 2, Colspan = 3, DisplayFields="Total QTY IN"
                , ClassName="text-primary"
                }
            };
            ViewBag.STOCKInMaintain = html.DrawHTMLSearchForm(1, "LoadSTOCKInMaintain");

            html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true,
                ShowPaging = false
            };
            html.GH.GridPara = new GridPara[]
            {
                new GridPara()
                {
                    ParaName="FromDate",
                    Value=FromDate
                },
                new GridPara()
                {
                    ParaName="ToDate",
                    Value=ToDate
                },
                new GridPara()
                {
                    ParaName="ItemGroup",
                    Value=ItemGroup
                },
                new GridPara()
                {
                    ParaName="ItemName",
                    Value=ItemName
                },
                new GridPara()
                {
                    ParaName="VendorID",
                    Value=VendorID.ToString()
                },
                new GridPara()
                {
                    ParaName="SystemID",
                    Value=html.SystemID
                },
                new GridPara()
                {
                    ParaName="WarehouseID",
                    Value=html.WarehouseID
                },
                new GridPara()
                {
                    ParaName="WarehouseName",
                    Value=WarehouseName
                },
            };
            html.TblAlign = "center";
            html.TableClassName = "table table-bordered table-head-fixed";
            html.TblID = "Tbl_Show_Stock_Out";
            html.GH.GridProcedureName = "SP_SHOW_STOCK_OUT_Maintain";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Item Name", FieldName = "ItemName", IsGroupByField = true, Colspan=3 },
                new GridField() { Header = "Date OUT", FieldName = "StockInDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "QTY", FieldName = "QTYOUT", TextAlign="right", Format="#,#0", ClassName="text-red", LinkField="ItemID", DataTableNumber = 2, DisplayFields="Total QTY OUT" },
            };
            ViewBag.STOCKOutMaintain = html.DrawHTMLSearchForm(1, "LoadSTOCKOutMaintain");

            html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true,
                ShowPaging = false
            };
            html.GH.GridPara = new GridPara[]
            {
                new GridPara()
                {
                    ParaName="FromDate",
                    Value=FromDate
                },
                new GridPara()
                {
                    ParaName="ToDate",
                    Value=ToDate
                },
                new GridPara()
                {
                    ParaName="ItemGroup",
                    Value=ItemGroup
                },
                new GridPara()
                {
                    ParaName="ItemName",
                    Value=ItemName
                },
                new GridPara()
                {
                    ParaName="VendorID",
                    Value=VendorID.ToString()
                },
                new GridPara()
                {
                    ParaName="SystemID",
                    Value=html.SystemID
                },
                new GridPara()
                {
                    ParaName="WarehouseID",
                    Value=html.WarehouseID
                },
                new GridPara()
                {
                    ParaName="WarehouseName",
                    Value=WarehouseName
                },
            };
            html.TblAlign = "center";
            html.TableClassName = "table table-bordered table-head-fixed";
            html.TblID = "Tbl_Show_Net_Stock";
            html.GH.GridProcedureName = "SP_SHOW_STOCK_NET_STOCK";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Item Name", FieldName = "ItemName", IsGroupByField= true, Colspan=4 },
                new GridField() { Header = "Available Stock", FieldName = "QTY", TextAlign ="right", Format="#,#0", ClassName="text-red" },
                new GridField() { Header = "Reorder Point", FieldName = "ReorderPoint",TextAlign ="right" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
            };
            ViewBag.NetSTOCKMaintain = html.DrawHTMLSearchForm(1, "LoadNetSTOCKMaintain");
            return View();
        }
        [Authorize]
        public JsonResult SaveChangePhotoProfile(string VendorID, string VendorName)
        {
            VendorName = HttpUtility.HtmlDecode(VendorName);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            sqlResult = html.UploadFile("Uploads/VendorProfilePicture/" + VendorName);
            string strfile;
            if (sqlResult.Result == ResultType.Successful)
                strfile = sqlResult.Value;
            else
            {
                return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
            }
            string sql = @"update dbo.TblVendor set VendorPicture = @VendorPicture 
            where VendorID = @VendorID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "VendorPicture", strfile },
                { "VendorID", VendorID.ToString() }
            };
            sqlResult = html.SaveChangeData(sql, false, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReorderStock() // default SystemID=25
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();



            string filePath = html.httpContext.Server.MapPath("~/Templates/Sales/ReorderStock.html");
            string templateHtml = System.IO.File.ReadAllText(filePath, Encoding.UTF8);

            templateHtml = templateHtml.Replace("{Title}", "Stock Expire Report");

            ds = html.LoadData("SP_ReorderPoint_Alert", true);
            System.Diagnostics.Debug.WriteLine("ExpirePdf rows: " + ds.Tables[0].Rows.Count);
            StringBuilder sb = new StringBuilder();
            string tableHeader = @"<thead>
                            <tr>
                                <th>No</th>
                                 <th>Item Code</th>
                                <th>Item Name</th>
                                <th>Reorder Level</th>
                                <th>Stock On Hand</th>                    
                            </tr>
                          </thead>";

            sb.Append("<tbody>");

            if (ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    //string daysUntilExpiry = dr["DaysUntilExpiry"].ToString();
                    string rowClass = "";
                    //if (int.TryParse(daysUntilExpiry, out int days))
                    //{
                    //    if (days < 0) { rowClass = "expired"; expiryStyle = "class='expired-text'"; }
                    //    else if (days < 7) { rowClass = "expiring-soon"; }
                    //}

                    sb.AppendLine($"<tr class='{rowClass}'>");
                    sb.AppendLine($"<td>{dr["No"]}</td>");
                    sb.AppendLine($"<td>{dr["ItemCode"]}</td>");
                    sb.AppendLine($"<td>{dr["ItemName"]}</td>");
                    sb.AppendLine($"<td>{dr["ReorderPoint"]} {dr["UM"]}</td>");
                    sb.AppendLine($"<td>{dr["StockOnHand"]} {dr["UM"]}</td>");

                    sb.AppendLine("</tr>");
                }
            }
            else
            {
                sb.AppendLine("<tr><td colspan='6' align='center'>No Data</td></tr>");
            }
            sb.Append("</tbody>");

            templateHtml = templateHtml.Replace("{TblHead}", tableHeader);
            templateHtml = templateHtml.Replace("{TblBody}", sb.ToString());
            templateHtml = templateHtml.Replace("{DailyDate}", "Date: " + DateTime.Now.ToString("dd-MMM-yyyy"));

            ViewBag.Data = templateHtml;
            return View();
        }

        public ActionResult ExpirePdf() // default SystemID=25
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();



            string filePath = html.httpContext.Server.MapPath("~/Templates/Sales/ExpirePdf.html");
            string templateHtml = System.IO.File.ReadAllText(filePath, Encoding.UTF8);

            templateHtml = templateHtml.Replace("{Title}", "Stock Expire Report");

            // Load stock expire data

            ds = html.LoadData("SP_EXPIRE_STOCK_ALERT", true);
            //System.Diagnostics.Debug.WriteLine("ExpirePdf rows: " + ds.Tables[0].Rows.Count);
            StringBuilder sb = new StringBuilder();
            string tableHeader = @"<thead>
                            <tr>
                                <th>No</th>
                                <th>Item Name</th>
                                <th>Stock On Hand</th>
                                <th>Expiry (Days)</th>
                                <th>Expiration Date</th>
                            </tr>
                          </thead>";

            sb.Append("<tbody>");

            if (ds.Tables[0].Rows.Count > 0)
            {
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string daysUntilExpiry = dr["DaysUntilExpiry"].ToString();
                    string rowClass = "";
                    string expiryStyle = "";
                    if (int.TryParse(daysUntilExpiry, out int days))
                    {
                        if (days < 0) { rowClass = "expired"; expiryStyle = "class='expired-text'"; }
                        else if (days < 7) { rowClass = "expiring-soon"; }
                    }

                    sb.AppendLine($"<tr class='{rowClass}'>");
                    sb.AppendLine($"<td>{i}</td>");
                    sb.AppendLine($"<td>{dr["ItemName"]}</td>");
                    sb.AppendLine($"<td>{dr["StockOnHand"]} {dr["UM"]}</td>");
                    sb.AppendLine($"<td {expiryStyle}>{daysUntilExpiry}</td>");
                    sb.AppendLine($"<td>{dr["ItemExpirationDate"]}</td>");
                    sb.AppendLine("</tr>");
                }
            }
            else
            {
                sb.AppendLine("<tr><td colspan='6' align='center'>No Data</td></tr>");
            }
            sb.Append("</tbody>");

            templateHtml = templateHtml.Replace("{TblHead}", tableHeader);
            templateHtml = templateHtml.Replace("{TblBody}", sb.ToString());
            templateHtml = templateHtml.Replace("{DailyDate}", "Date: " + DateTime.Now.ToString("dd-MMM-yyyy"));

            ViewBag.Data = templateHtml;
            return View();
        }

        // Needs: using System.Globalization;
        //
        // Called by ReportSaleDailyAlertController:
        //   Profile/DailySaleReportForm?sysId=13&period=Weekly&from=2026-11-16&to=2026-11-21
        // Without period/from/to it behaves like before (today, Daily).
        public ActionResult DailySaleReportForm(string sysId, string period = "Daily", DateTime? from = null, DateTime? to = null, int? classId = null)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = "";
            string systemId = sysId;
            CultureInfo ci = CultureInfo.InvariantCulture;

            // ---------------------------------------------------------
            // Report range (defaults to today = old daily behavior)
            // ---------------------------------------------------------
            DateTime today = DateTime.Parse(html.GetCurrentDate()).Date;
            DateTime fromDate = (from ?? today).Date;
            DateTime toDate = (to ?? fromDate).Date;
            if (toDate < fromDate) { var tmp = fromDate; fromDate = toDate; toDate = tmp; }

            if (string.IsNullOrWhiteSpace(period)) period = "Daily";
            bool isDaily = fromDate == toDate && period.Equals("Daily", StringComparison.OrdinalIgnoreCase);

            para.Add("SystemID", systemId);
            para.Add("WarehouseID", html.WarehouseID);
            para.Add("DomainID", "'%" + html.DomainID + "%'");
            // yyyyMMdd is language/DATEFORMAT safe for convert(date, ..., 112)
            para.Add("FromDate", fromDate.ToString("yyyyMMdd", ci));
            para.Add("ToDate", toDate.ToString("yyyyMMdd", ci));
            // 0 = all classes. Only system 13 uses it (report is split per class).
            para.Add("ClassID", (systemId == "13" ? (classId ?? 0) : 0).ToString());

            string filePath = html.httpContext.Server.MapPath("~/Templates/Sales/DailySaleReportTemplate.html");
            StreamReader str = new StreamReader(filePath);
            StringBuilder sb = new StringBuilder();
            string Text = str.ReadToEnd();
            str.Close();
            Text = Text.Replace("{Title}", period + " Sale Report");

            string titleKH = period.Equals("Weekly", StringComparison.OrdinalIgnoreCase) ? "របាយការណ៍ការលក់ប្រចាំសប្តាហ៍"
                           : period.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? "របាយការណ៍ការលក់ប្រចាំខែ"
                           : "របាយការណ៍ការលក់ប្រចាំថ្ងៃ";
            Text = Text.Replace("{TitleKH}", titleKH);

            // Class name for the header (system 13, per-class report)
            string className = "";
            if (systemId == "13" && classId.HasValue && classId.Value > 0)
            {
                DataSet dsClass = html.LoadData(
                    "select top 1 ClassName from dbo.TblClass where SystemID = @SystemID and ClassID = convert(int, @ClassID);",
                    false, para);
                if (dsClass.Tables.Count > 0 && dsClass.Tables[0].Rows.Count > 0)
                    className = dsClass.Tables[0].Rows[0]["ClassName"].ToString();
            }

            if (systemId == "13")
            {
                sql = @"select ROW_NUMBER() over (order by case when isnull(ci.InvoiceStatus,'') = 'Pending' then 0 else 1 end, format(c.RegisterDate,'yyMMdd') desc, ci.InvoiceNo) as [No], c.CustomerID, c.CustomerCode, COALESCE(NULLIF(c.CustomerName, ''), c.CustomerNameLatin) as CustomerName,
c.CustomerMobile, format(c.RegisterDate,'dd/MM/yyyy') RegisterDate, c.RegisterBy,
ci.StartDate, ci.EndDate, case when ci.InvoiceStatusID = 90 then '<span class=""badge bg-primary"">' + ci.InvoiceStatus + '</span>' else '<span class=""badge bg-danger"">' + ci.InvoiceStatus + '</span>' end InvoiceStatus, 
case when (p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>') is null then 
STRING_AGG(ci.ItemDescription, ',')
else 
p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>' end ItemDescription
,(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress, ci.InvoiceNo, sum(ci.Amount - ci.DiscountUSD) as Amount, ci.ExchangeRate
from dbo.TblCustomer c
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
left join dbo.TblPackageDetail pd on pd.PackageDetailID = ci.PackageDetailID
left join dbo.TblPackages p on p.PackageID = ci.PackageID
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = ci.CustomerBillInformationID
where ci.StatusID not in (12,13,14,20) and ci.[Type] = 'Quotation Invoice' and isnull(ci.ItemDescription,'') <> 'Deposit Fee' and ci.SystemID = @SystemID
and c.RegisterDate >= convert(date, @FromDate, 112) and c.RegisterDate < dateadd(day, 1, convert(date, @ToDate, 112))
and (convert(int, @ClassID) = 0 or ci.ClassNameID = convert(int, @ClassID))
group by c.CustomerID, c.CustomerCode,c.CustomerName, c.CustomerMobile, c.RegisterDate, c.RegisterBy,
(p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>'), c.CustomerNameLatin,
ci.InvoiceStatusID,ci.InvoiceStatus,bf.InstallAddress,bf.InstallCommune,bf.InstallDistrict,bf.InstallProvinceCity,ci.InvoiceNo,ci.[Type],
ci.ApproveBy,ci.ApproveDate, ci.StartDate, ci.EndDate, ci.ExchangeRate; 
";
            }
            else if (systemId == "25" || systemId == "27")
            {
                sql = @"
with tempTotalAMT as(
	select s.SalesID, sum((ci.QTY * ci.UnitPrice)) as TotalNoDis, sum(ci.DiscountUSD) + s.DiscountUSD as tempDis
	from dbo.TblSales s
	inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
	where s.Type = 'SalesOrder'
	group by s.SalesID, s.DiscountUSD
)
select ROW_NUMBER() Over(order by s.TranDate, s.ClassID desc, s.SONumber) as [No], format(s.TranDate, 'dd-MMM-yyyy') SODate, s.SalesID,s.SONumber, s.QuoteNumber, 
s.ClassName, s.ClassID, s.StatusID, isnull(s.IsToInvoice, 0) IsToInvoice, case when isnull(s.IsToInvoice, 0) <> 0 then '<span class=""text-primary"">Complete</span>' else '<span class=""text-danger"">Pending</span>' end StatusToInvoice , c.CustomerCode, c.CustomerName, 
case when s.StatusID = 92 then '<span class=""badge badge-pill badge-success text-uppercase"">' + st.[Status] + '</span>' when s.StatusID = 90 then '<span class=""badge badge-pill badge-primary text-white text-uppercase"">' + st.[Status] + '</span>' when s.StatusID = 94 then '<span class=""badge badge-pill badge-success text-uppercase"">' + st.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-danger text-white"">' + st.[Status] + '</span>' end [Status], 
s.GrandTotal, tmp.TotalNoDis Total, tmp.tempDis DiscountUSD, s.SaleNote, case when isnull(s.NeedCheckByID, 0) <> 0 then s.NeedCheckBy else '<span class=""text-info"">No need check</span>' end NeedCheckBy, s.NeedApproveBy, s.SellerName, s.CreateBy, s.ExchangeRate
from tempTotalAMT tmp
inner join dbo.TblSales s on s.SalesID = tmp.SalesID
inner join dbo.TblCustomer c on c.CustomerID = s.CustomerID
inner join dbo.TblStatus st on st.StatusID = s.StatusID
where s.Type = 'SalesOrder' and s.SystemID = @SystemID
and s.TranDate >= convert(date, @FromDate, 112) and s.TranDate < dateadd(day, 1, convert(date, @ToDate, 112));";
            }
            ds = html.LoadData(sql, false, para);

            if (ds.Tables.Count > 0)
            {
                string bgcolor = systemId == "13" ? "#f89c39" : (systemId == "25" ? "#c5d9f1" : (systemId == "27" ? "#ffde16" : "#ffccff"));
                string txtcolor = systemId == "13" ? "#fff" : (systemId == "25" ? "#000" : "#000");
                string tableHeader = "";

                // Header is the same whether or not there is data.
                if (systemId == "13")
                {
                    tableHeader = $@"
							<thead style=""background:{bgcolor}; color:{txtcolor};"">
							<tr>
								<th class=""text-center"" width="""">No</th>
								<th class=""text-center"" width="""">Date</th>
								<th class=""text-center"" width="""">Quotation No.</th>
								<th class=""text-nowrap text-center"" width="""">Customer Name</th>
								<th class=""text-center"" width="""">Package Name</th>
								<th class=""text-center"" width="""">Register By</th>
								<th class=""text-center"" width="""">Amount</th>
							</tr>
							</thead>
						";
                }
                else if (systemId == "25" || systemId == "27")
                {
                    tableHeader = $@"
							<thead style=""background:{bgcolor}; color:{txtcolor};"">
							<tr>
								<th class=""text-center"" width="""">No</th>
								<th class=""text-center"" width="""">Date</th>
								<th class=""text-center"" width="""">SO No.</th>
								<th class=""text-nowrap text-center"" width="""">Customer Name</th>
								<th class=""text-center"" width="""" nowrap>Class Name</th>
								<th class=""text-center"" width="""">Sale Name</th>
								<th class=""text-center"" width="""">Amount</th>
							</tr>
							</thead>
						";
                }

                if (ds.Tables[0].Rows.Count > 0)
                {
                    int i = 0;
                    // decimal avoids float rounding noise when summing a week/month.
                    decimal totalAmount = 0;
                    // KHR is summed per row because the exchange rate can differ between days.
                    decimal totalKhr = 0;

                    if (systemId == "13")
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            i++;

                            decimal amount = decimal.Parse(dr["Amount"].ToString());
                            decimal rate = decimal.Parse(dr["ExchangeRate"].ToString());
                            totalAmount += amount;
                            totalKhr += amount * rate;

                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td align=\"center\">{i}</td>");
                            sb.AppendLine($"<td align=\"center\">{dr["RegisterDate"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre text-center\" nowrap>{dr["InvoiceNo"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre text-center\" nowrap>{dr["CustomerName"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre\">{dr["ItemDescription"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre\">{dr["RegisterBy"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre text-center\">$ {amount.ToString("#,#0.00")}</td>");
                            sb.AppendLine("</tr>");
                        }
                    }
                    else if (systemId == "25" || systemId == "27")
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            i++;

                            decimal amount = decimal.Parse(dr["GrandTotal"].ToString());
                            decimal rate = decimal.Parse(dr["ExchangeRate"].ToString());
                            totalAmount += amount;
                            totalKhr += amount * rate;

                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td align=\"center\">{i}</td>");
                            sb.AppendLine($"<td align=\"center\" nowrap>{dr["SODate"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-center\" nowrap>{dr["SONumber"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre text-center\">{dr["CustomerName"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre text-center\">{dr["ClassName"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-center\" nowrap>{dr["SellerName"].ToString()}</td>");
                            sb.AppendLine($"<td class=\"text-nowrap-pre text-center\">$ {amount.ToString("#,#0.00")}</td>");
                            sb.AppendLine("</tr>");
                        }
                    }

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td align=\"right\" colspan=\"6\" class=\"font-weight-bold\">Total (USD):</td>");
                    sb.AppendLine($"<td align=\"center\" class=\"font-weight-bold\">$ {totalAmount.ToString("#,#0.00")}</td>");
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td align=\"right\" colspan=\"6\" class=\"font-weight-bold\">Total (KHR):</td>");
                    sb.AppendLine($"<td align=\"center\" class=\"font-weight-bold\" nowrap>៛ {totalKhr.ToString("#,#0.00")}</td>");
                    sb.AppendLine("</tr>");
                }
                else
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td align=\"center\" colspan=\"7\">No Data</td>");
                    sb.AppendLine("</tr>");
                }

                Text = Text.Replace("{TblHead}", tableHeader);
                Text = Text.Replace("{TblBody}", sb.ToString());
            }

            // Daily: "Date: 21-Nov-2026"   Weekly/Monthly: "Period: 16-Nov-2026 to 21-Nov-2026"
            string dateLabel = isDaily
                ? "Date: " + fromDate.ToString("dd-MMM-yyyy", ci)
                : "Period: " + fromDate.ToString("dd-MMM-yyyy", ci) + " to " + toDate.ToString("dd-MMM-yyyy", ci);
            if (className != "")
                dateLabel = "Class: " + className + "<br />" + dateLabel;
            Text = Text.Replace("{DailyDate}", dateLabel);

            //Company Name
            string sql2 = @"select top 1 * from dbo.TblCompany where SystemID = @SystemID;";
            ds2 = html.LoadData(sql2, false, para);
            if (ds2.Tables.Count > 0)
            {
                if (ds2.Tables[0].Rows.Count > 0)
                {

                    if (ds2.Tables[0].Rows[0]["CompanyNameKH"].ToString() != "")
                    {
                        string nameSys = ds2.Tables[0].Rows[0]["CompanyNameKH"].ToString();
                        if (systemId == "25")
                        {
                            nameSys = "PS Printing & Advertising Supply";
                        }
                        Text = Text.Replace("{SystemName}", nameSys);
                    }
                    else
                    {
                        Text = Text.Replace("{SystemName}", "");
                    }
                }
            }

            //Logo
            string sql3 = @"select top 1 * from dbo.TblClass where SystemID = @SystemID and (convert(int, @ClassID) = 0 or ClassID = convert(int, @ClassID));";
            DataSet ds3 = html.LoadData(sql3, false, para);
            if (ds3.Tables.Count > 0)
            {
                if (ds3.Tables[0].Rows.Count > 0)
                {
                    string ClassLogo = ds3.Tables[0].Rows[0]["FileLogo"].ToString();
                    string LOGO = "<img src=\"../Uploads/ClassLogo/" + ClassLogo + "\" alt=\"Not found\" width=\"200\" height=\"100\" />";
                    if (systemId == "25")
                    {
                        LOGO = "<img src=\"../Uploads/SystemLogo/ps_logo.png\" alt=\"Not found\" width=\"200\" height=\"100\" />";
                    }
                    Text = Text.Replace("{LOGO}", LOGO);
                }
                else
                {
                    Text = Text.Replace("{LOGO}", "");
                }
            }

            // Load and parse the HTML content
            var doc = new HtmlDocument
            {
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(Text);

            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//html");
            if (bodyNode != null)
            {
                sb.Clear();
                sb.Append(bodyNode.InnerHtml);
                ViewBag.Data = sb.ToString();
            }

            return View();
        }

        public ActionResult InvoiceTMPRequestForm(int VendorID, int RequestFormID, string Type = "RequestForm", bool ReturnHtmlData = false, bool ReturnAllHtmlData = false, bool RedirectToPOView = false)
        {
            HTML html = new HTML();
            string mapPath = Type == "Advance" ? "~/Templates/InvoiceRA.html" : "~/Templates/InvoiceRF.html";
            string templatePath = html.httpContext.Server.MapPath(mapPath);
            string templateText = System.IO.File.ReadAllText(templatePath);

            // Initialize view data
            ViewBag.RequestFormID = RequestFormID;
            ViewBag.Type = Type;

            // Load request form data
            var requestData = LoadRequestFormData(html, RequestFormID);
            if (requestData.Tables[0].Rows.Count == 0)
                return HttpNotFound("Request form not found");

            DataRow requestRow = requestData.Tables[0].Rows[0];
            DataTable itemsTable = requestData.Tables[1];

            // Set viewbag values from request data
            SetViewBagValues(requestRow);

            // Replace basic template placeholders
            templateText = ReplaceBasicPlaceholders(templateText, requestRow, html);

            // Handle logo
            templateText = templateText.Replace("{LOGO}", GetLogoHtml(html, Convert.ToInt32(requestRow["ClassID"])));

            // Handle attached files
            templateText = templateText.Replace("{FileUpload}", GetAttachedFilesHtml(requestRow["ImagePath"].ToString()));

            // Build items table
            templateText = BuildItemsTable(templateText, itemsTable, requestRow);

            // Replace amounts and financial info
            templateText = ReplaceAmountsAndFinancialInfo(templateText, requestRow);

            // Handle signatures
            templateText = ProcessSignatures(templateText, requestRow, html);

            // Return the result based on parameters
            return PrepareFinalResult(templateText, ReturnHtmlData, ReturnAllHtmlData, RedirectToPOView);
        }

        #region Helper Methods For Request Form

        private DataSet LoadRequestFormData(HTML html, int requestFormID)
        {
            string sql = @"SELECT r.RequestFormID, ur.FullName [RequestBy], r.RequestByID, r.Remark, r.SubTotal, r.TotalEstimate, 
                  r.BankInformation, r.DepartmentName, r.ReferenceNumber, FORMAT(r.RequestDate, 'dd-MMM-yyyy') [RequestDate], 
                  FORMAT(r.RequirementDate, 'dd-MMM-yyyy') [RequirementDate], r.PaymentMethod, r.StatusID, r.RequestStatusID, 
                  r.ApprovedByUserID, ur.Job [RequestedByPosition], ua.Job [ApprovedByPosition], r.ApprovedBy [ApprovedBy], 
                  FORMAT(r.ApprovalDate, 'dd-MMM-yyyy HH:mm:ss tt') [ApprovalDate], r.PhoneNumber, r.ReceivedBy, 
                  FORMAT(r.ReceivedDate, 'dd-MMM-yyyy HH:mm:ss tt') [ReceivedDate], r.ReceivedByID, r.ExchangeRate, r.ImagePath, 
                  r.PaymentMean, r.ClassID, r.ClassName, r.DeliverTo, r.SystemID, ISNULL(r.CheckedByUserID, 0) CheckedByUserID, 
                  uc.FullName [CheckedBy], uc.Job [CheckedByPosition], FORMAT(r.CheckedDate, 'dd-MMM-yyyy HH:mm:ss tt') [CheckedDate], 
                  CASE WHEN r.UserID IS NULL THEN r.CreateBy ELSE u.FullName END [CreateBy], u.Job [CreatedByPosition], 
                  FORMAT(r.CreateDate, 'dd-MMM-yyyy HH:mm:ss tt') [CreatedDate], r.UserID, 
                  ISNULL(r.NeedNextApprovalByUserID, 0) NeedNextApprovalByUserID, ISNULL(r.NextApproveStatusID, 0) NextApproveStatusID, 
                  ISNULL(r.NeedToCheckUserID, 0) NeedToCheckUserID, v.VendorName, v.Address, v.Phone, isnull(r.NextApprovedByID, 0) NextApprovedByID,
					isnull(r.NextApprovedBy, '') NextApprovedBy, FORMAT(r.NextApprovedByDate, 'dd-MMM-yyyy HH:mm:ss tt') NextApprovedByDate, un.Job NextApprovedByPosition
                  FROM dbo.TblRequestForm r
                  LEFT JOIN dbo.TblUserLogin u ON u.UserLoginID = r.UserID
                  INNER JOIN dbo.TblUserLogin ur ON ur.UserLoginID = r.RequestByID
                  LEFT JOIN dbo.TblUserLogin ua ON ua.UserLoginID = r.ApprovedByUserID
                  LEFT JOIN dbo.TblUserLogin uc ON uc.UserLoginID = r.CheckedByUserID
                  LEFT JOIN dbo.TblUserLogin un ON un.UserLoginID = r.NextApprovedByID
                  LEFT JOIN dbo.TblVendor v on v.VendorID = r.VendorID
                  WHERE r.RequestFormID = @RequestFormID;

                  SELECT rd.QTY, rd.RequestFormID, rd.ItemName + CASE WHEN ISNULL(i.ItemSize, '') <> '' THEN '<br>'+i.ItemSize ELSE '' END ItemName, 
                  ISNULL(i.SerialNumber, '') SerialNumber, rd.ItemDescription, FORMAT(rd.UnitPrice, '#,#0.000') UnitPrice,  
                  FORMAT(rd.Amount, '#,#0.00') Amount, FORMAT(rd.SalePrice, '#,#0.00') SalePrice, ISNULL(rd.UM, '') UOM, rd.EstimateAmount
                  FROM dbo.TblRequestFormDetail rd
                  LEFT JOIN dbo.TblItem i ON i.ItemID = rd.ItemID
                  WHERE rd.RequestFormID = @RequestFormID
                  ORDER BY rd.RequestFormDetailID;";

            var parameters = new Dictionary<string, string> { { "RequestFormID", requestFormID.ToString() } };
            return html.LoadData(sql, false, parameters);
        }

        private void SetViewBagValues(DataRow requestRow)
        {
            ViewBag.StatusID = requestRow["StatusID"].ToString();
            ViewBag.NeedToCheckUserID = requestRow["NeedToCheckUserID"].ToString();
            ViewBag.NextApprovalByUserID = requestRow["NeedNextApprovalByUserID"].ToString();
            ViewBag.NextApproveStatusID = requestRow["NextApproveStatusID"].ToString();
            ViewBag.RequestStatusID = requestRow["RequestStatusID"].ToString();
        }

        private string ReplaceBasicPlaceholders(string templateText, DataRow requestRow, HTML html)
        {
            return templateText
                .Replace("{Title}", "Request Form")
                .Replace("{InvoiceName}", "Request Form")
                .Replace("{RequestBy}", requestRow["RequestBy"].ToString())
                .Replace("{Department}", requestRow["DepartmentName"].ToString())
                .Replace("{Tel}", requestRow["PhoneNumber"].ToString())
                .Replace("{PaymentMean}", requestRow["PaymentMean"].ToString())
                .Replace("{ReferenceNo}", requestRow["ReferenceNumber"].ToString())
                .Replace("{RequestDate}", requestRow["RequestDate"].ToString())
                .Replace("{RequirementDate}", requestRow["RequirementDate"].ToString())
                .Replace("{PaymentMethod}", requestRow["PaymentMethod"].ToString())
                .Replace("{PrintDate}", DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt"))
                .Replace("{Remark}", requestRow["Remark"].ToString())
                .Replace("{BankInformation}", requestRow["BankInformation"].ToString())
                .Replace("{PaymentMethodTitle}", requestRow["PaymentMethod"].ToString() != "" ? "Payment Method:" : "")
                .Replace("{RemarkTitle}", requestRow["Remark"].ToString() != "" ? "Remark" : "")
                .Replace("{BankInformationTitle}", requestRow["BankInformation"].ToString() != "" ? "Payment Information" : "")
                .Replace("{VendorName}", requestRow["VendorName"].ToString())
                .Replace("{VendorAddress}", requestRow["Address"].ToString())
                .Replace("{VendorAttend}", "")
                .Replace("{VendorContact}", requestRow["Phone"].ToString())
                .Replace("{RequestBy}", requestRow["RequestBy"].ToString())
                .Replace("{DeliveryTo}", requestRow["DeliverTo"].ToString() != "" ? requestRow["DeliverTo"].ToString() : "");
        }

        private string GetLogoHtml(HTML html, int classID)
        {
            string sql = "SELECT * FROM dbo.TblClass WHERE ClassID = @ClassID";
            var parameters = new Dictionary<string, string> { { "ClassID", classID.ToString() } };
            DataSet dsLogo = html.LoadData(sql, false, parameters);

            return dsLogo.Tables[0].Rows.Count > 0
                ? $"<img src=\"../Uploads/ClassLogo/{dsLogo.Tables[0].Rows[0]["FileLogo"]}\" alt=\"Not found\" width=\"200\" />"
                : !string.IsNullOrEmpty(html.SystemLogo)
                ? $"<img src=\"{html.SystemLogo}\" alt=\"Not found\" width=\"200\" />"
                : "<img src=\"../Uploads/WarehouseLogo/2PNG-169344668472033.png\" alt=\"Not found\" width=\"200\" />";
        }

        private string GetAttachedFilesHtml(string imagePaths)
        {
            if (string.IsNullOrEmpty(imagePaths)) return string.Empty;

            var paths = imagePaths.Split('~');
            var sb = new StringBuilder();
            foreach (string path in paths)
            {
                sb.Append($"<img height=\"200\" style=\"margin: 10px 10px 0 0;\" src=\"../Uploads/RequestForm/CashAdvance/{path}\">");
            }
            return sb.ToString();
        }

        private string BuildItemsTable(string templateText, DataTable itemsTable, DataRow requestRow)
        {
            int systemID = Convert.ToInt32(requestRow["SystemID"]);
            int classID = Convert.ToInt32(requestRow["ClassID"]);

            var bgColor = systemID == 13 ? "#f89c39" :
                         (systemID == 25 ? "#c5d9f1" :
                         (systemID == 27 && classID == 11 ? "#ffde16" :
                         (systemID == 27 && classID == 12 ? "#ffccff" : "#2275B9")));

            var textColor = systemID == 13 || systemID == 28 ? "#fff" : "#000";

            var tableHeader = $@"
        <tr style=""background: {bgColor}; color: {textColor};"">
            <th class=""text-center"" width=""10%"">No</th>
            <th class=""text-center"" width=""45%"">Description</th>
            <th class=""text-center"" width=""5%"">Quantity</th>
            <th class=""text-center"" width=""5%"">UOM</th>
            <th class=""text-center"" width=""10%"">Estimate</th>
            <th class=""text-nowrap text-center"" width=""10%"">Unit Price</th>
            <th class=""text-center"" width=""15%"">Amount</th>
        </tr>";

            var sb = new StringBuilder();
            for (int i = 0; i < itemsTable.Rows.Count; i++)
            {
                var row = itemsTable.Rows[i];
                sb.AppendLine("<tr>")
                  .AppendLine($"<td align=\"center\">{i + 1}</td>")
                  .AppendLine($"<td class=\"text-nowrap-pre\">{row["ItemDescription"]}</td>")
                  .AppendLine($"<td align=\"center\" class=\"text-nowrap-pre\">{float.Parse(row["QTY"].ToString()).ToString("#,#0.00")}</td>")
                  .AppendLine($"<td align=\"center\" class=\"text-nowrap-pre\">{row["UOM"]}</td>")
                  .AppendLine($"<td align=\"center\" class=\"text-nowrap-pre\">$ {float.Parse(row["EstimateAmount"].ToString() == "" ? "0" : row["EstimateAmount"].ToString()).ToString("#,#0.00")}</td>")
                  .AppendLine($"<td align=\"center\" class=\"text-nowrap-pre\">$ {float.Parse(row["UnitPrice"].ToString()).ToString("#,#0.00")}</td>")
                  .AppendLine($"<td align=\"center\" class=\"text-nowrap-pre\">$ {float.Parse(row["Amount"].ToString()).ToString("#,#0.00")}</td>")
                  .AppendLine("</tr>");
            }

            return templateText
                .Replace("{TblHead}", tableHeader)
                .Replace("{TblBody}", sb.ToString());
        }

        private string ReplaceAmountsAndFinancialInfo(string templateText, DataRow requestRow)
        {
            float totalAmtKHR = float.Parse(requestRow["SubTotal"].ToString()) * float.Parse(requestRow["ExchangeRate"].ToString());
            float totalEstimateAmtKHR = float.Parse(requestRow["TotalEstimate"].ToString()) * float.Parse(requestRow["ExchangeRate"].ToString());

            return templateText
                .Replace("{TotalAmountUSD}", "$ " + float.Parse(requestRow["SubTotal"].ToString()).ToString("#,#0.00"))
                .Replace("{EstimateAmountUSD}", "$ " + float.Parse(requestRow["TotalEstimate"].ToString()).ToString("#,#0.00"))
                .Replace("{TotalAmountKHR}", "៛ " + totalAmtKHR.ToString("#,#0.00"))
                .Replace("{EstimateAmountKHR}", "៛ " + totalEstimateAmtKHR.ToString("#,#0.00"));
        }

        private string ProcessSignatures(string templateText, DataRow requestRow, HTML html)
        {
            int needCheckById = Convert.ToInt32(requestRow["NeedToCheckUserID"].ToString());
            int needNextApproveById = Convert.ToInt32(requestRow["NeedNextApprovalByUserID"].ToString());

            // Created By Signature
            templateText = ProcessSignature(templateText, requestRow, html,
                "UserID", "CreateBy", "CreatedByPosition", "CreatedDate",
                "CreatedByName", "CreatedByPosition", "CreatedByDate", "CreatedBySignature");

            // Checked By Signature (if needed)
            templateText = ViewBag.StatusID == "92" || ViewBag.StatusID == "90" || ViewBag.StatusID == "94"
                ? ProcessSignature(templateText, requestRow, html,
                    "CheckedByUserID", "CheckedBy", "CheckedByPosition", "CheckedDate",
                    "CheckedBy", "CheckedByPosition", "CheckedDate", "CheckedBySignature")
                : templateText
                    .Replace("{CheckedBySignature}", "")
                    .Replace("{CheckedBy}", "")
                    .Replace("{CheckedByPosition}", "")
                    .Replace("{CheckedDate}", "");

            // Approved By Signature (if needed)
            templateText = ViewBag.RequestStatusID == "90"
                ? ProcessSignature(templateText, requestRow, html,
                    "ApprovedByUserID", "ApprovedBy", "ApprovedByPosition", "ApprovalDate",
                    "ApprovedBy", "ApprovedByPosition", "ApprovedDate", "ApprovedBySignature")
                : templateText
                    .Replace("{ApprovedBySignature}", "")
                    .Replace("{ApprovedBy}", "")
                    .Replace("{ApprovedByPosition}", "")
                    .Replace("{ApprovedDate}", "");

            // Received By Signature (if needed)
            templateText = ViewBag.RequestStatusID == "67"
                ? ProcessSignature(templateText, requestRow, html,
                    "ReceivedByID", "ReceivedBy", "", "ReceivedDate",
                    "ReceievedBy", "ReceievedByPosition", "ReceievedDate", "ReceievedBySignature")
                : templateText
                    .Replace("{ReceievedBySignature}", "")
                    .Replace("{ReceievedBy}", "")
                    .Replace("{ReceievedByPosition}", "")
                    .Replace("{ReceievedDate}", "");

            if (needCheckById != 0)
            {
                templateText = templateText.Replace("{checkByClass}", "");
            }
            else
            {
                if (needNextApproveById != 0)
                    templateText = templateText.Replace("{checkByClass}", "d-none");
            }

            templateText = needNextApproveById != 0
                ? templateText.Replace("{nextApproveClass}", "")
                : templateText.Replace("{nextApproveClass}", "d-none");

            // Approved By Signature (if needed)
            templateText = ViewBag.RequestStatusID == "90"
                ? ProcessSignature(templateText, requestRow, html,
                    "NextApprovedByID", "NextApprovedBy", "NextApprovedByPosition", "NextApprovedByDate",
                    "NextApprovedBy", "NextApprovedByPosition", "NextApprovedDate", "NextApprovedBySignature")
                : templateText
                    .Replace("{NextApprovedBySignature}", "")
                    .Replace("{NextApprovedBy}", "")
                    .Replace("{NextApprovedByPosition}", "")
                    .Replace("{NextApprovedDate}", "");

            return templateText;
        }

        private string ProcessSignature(string templateText, DataRow requestRow, HTML html,
            string userIdField, string nameField, string positionField, string dateField,
            string namePlaceholder, string positionPlaceholder, string datePlaceholder, string signaturePlaceholder)
        {
            // Set name, position and date
            templateText = templateText
                .Replace($"{{{namePlaceholder}}}", requestRow[nameField].ToString())
                .Replace($"{{{datePlaceholder}}}", requestRow[dateField].ToString());

            if (!string.IsNullOrEmpty(positionField))
            {
                templateText = templateText.Replace($"{{{positionPlaceholder}}}", requestRow[positionField].ToString());
            }

            // Get signature image
            string signature = GetSignatureImage(html, requestRow[userIdField].ToString(), requestRow[nameField].ToString());
            return templateText.Replace($"{{{signaturePlaceholder}}}", signature);
        }

        private string GetSignatureImage(HTML html, string userId, string userName)
        {
            if (string.IsNullOrEmpty(userId) || userId == "0") return string.Empty;

            string sql = "SELECT [Signature] FROM dbo.TblUserLogin WHERE UserLoginID = @UserID";
            var parameters = new Dictionary<string, string> { { "UserID", userId } };
            DataSet ds = html.LoadData(sql, false, parameters);

            if (ds.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(ds.Tables[0].Rows[0]["Signature"].ToString()))
            {
                string[] signatureParts = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                return $"<img height=\"90\" src=\"../Uploads/Users/{signatureParts[0]}\">";
            }

            if (!string.IsNullOrEmpty(userName))
            {
                sql = "SELECT [Signature] FROM dbo.TblUserLogin WHERE TelegramUserName = @UserName";
                parameters = new Dictionary<string, string> { { "UserName", userName } };
                ds = html.LoadData(sql, false, parameters);

                if (ds.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(ds.Tables[0].Rows[0]["Signature"].ToString()))
                {
                    string[] signatureParts = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                    return $"<img height=\"90\" src=\"../Uploads/Users/{signatureParts[0]}\">";
                }
            }

            return string.Empty;
        }

        private ActionResult PrepareFinalResult(string htmlContent, bool returnHtmlData, bool returnAllHtmlData, bool redirectToPOView)
        {
            if (returnAllHtmlData)
                return Json(new { Data = htmlContent }, JsonRequestBehavior.AllowGet);

            if (redirectToPOView)
            {
                ViewBag.Data = htmlContent;
                return View("PO");
            }

            var doc = new HtmlDocument { OptionDefaultStreamEncoding = Encoding.UTF8 };
            doc.LoadHtml(htmlContent);
            HtmlNode node = doc.DocumentNode.SelectNodes("//body")[0];

            ViewBag.Data = node.InnerHtml;
            return View();
        }

        #endregion
    }
}