using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class CustomerSaleOrderProfileController : Controller
    {
        // GET: CustomerSaleOrderProfile
        public ActionResult Index(int ID, int ISSO = 0)
        {
            ViewBag.CustomerID = ID;
            ViewBag.ISSO = ISSO;
            HTML html = new HTML();
            bool roleManager = HttpContext.GetOwinContext().Request.User.IsInRole("Show Customer Invoice Profile");
            ViewBag.ShowCustomerInvoiceProfile = roleManager;
            bool roleCustomerProfileAttachment = HttpContext.GetOwinContext().Request.User.IsInRole("Customer Profile Attachment File");
            ViewBag.roleCustomerProfileAttachment = roleCustomerProfileAttachment;
            bool NewServiceProduct = HttpContext.GetOwinContext().Request.User.IsInRole("New Service & Product");
            ViewBag.NewServiceProduct = NewServiceProduct;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            string SQL = "select c.CustomerCode + ' [' + c.CustomerName + ']' + ' (' + c.CustomerGender + ')' CustomerName, ";
            SQL += "c.CustomerAddress + case when c.CustomerCommune <> '' then + ', ' + c.CustomerCommune else '' end + case when c.CustomerDistrict <> '' then + ', ' + c.CustomerDistrict else '' end + case when c.CustomerProvinceCity <> '' then + ', ' + c.CustomerProvinceCity else '' end [Address], ";
            SQL += "c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerIdentification, convert(varchar, c.RegisterDate, 103) RegisterDate, ";
            SQL += "c.CustomerNationality, c.CustomerIdentification, (e.Salutation + e.FullName) EmployeeName, ct.CustomerType, ";
            SQL += "i.IndustryType, o.OrderType, c.CustomerID ";
            SQL += "from dbo.TblCustomer_SaleOrder c ";
            SQL += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
            SQL += "inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID ";
            SQL += "inner join dbo.TblIndustryType i on i.IndustryTypeID = c.IndustryTypeID ";
            SQL += "left join dbo.TblOrderType o on o.OrderTypeID = c.CustomerTypeID ";
            SQL += "where c.CustomerID = " + ID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " ";
            ViewBag.CustInfo = html.JsonData(SQL);
            SQL = "select s.ServiceName, (p.PackageName  + ' - ' + CONVERT(varchar, cd.Bandwidth) + ' Mbps - ' + cd.UM) PackageName, ";
            SQL += "cd.Duration, cd.FreeDuration, cd.Bandwidth, cd.DowSpeed, cd.UpSpeed, cd.DepositFee, cd.BillCycle ";
            SQL += ", convert(varchar, CompletedDate, 103) CompletedDate, convert(varchar, StartChargingDate, 103) StartChargingDate, StartChargingDate InvoiceDate ";
            SQL += "from dbo.TblCustomerDetail_SaleOrder cd ";
            SQL += "inner join dbo.TblServices s on cd.ServiceID = s.ServiceID ";
            SQL += "inner join dbo.TblPackages p on p.PackageID = cd.PackageID ";
            SQL += "where cd.CustomerID = " + ID;
            ViewBag.CustDetail = html.JsonData(SQL);
            SQL = "select sum(cb.DepositFee) Deposit, sum(cb.GrandTotal) Total, sum(cb.Unpaid) Unpaid, convert(varchar, NextInvoiceDate, 103) NextInvoiceDate from dbo.TblCustomerBalance_SaleOrder cb ";
            SQL += "where cb.CustomerID = " + ID + " and cb.StatusID = 11 group by convert(varchar, NextInvoiceDate, 103)";
            ViewBag.CustDeposit = html.JsonData(SQL);
            SQL = "select top 1 format(e.ExchangeRate, '#,#') ExchangeRate, format(e.DateEntry, 'dd-MM-yyyy') DateEntry from dbo.TblExchangeRate e inner join dbo.TblCurrency c on c.CurrencyID = e.CurrencyID where c.CurrencyName = 'Riel' and e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID + " order by format(e.DateEntry, 'yyyyMMdd') desc";
            ViewBag.ExchangeRate = html.JsonConvertDataTable(html.LoadData(SQL).Tables[0]);
            return View();
        }
        public JsonResult LoadCustInfo(int ID)
        {
            HTML html = new HTML();
            string SQL = "select isnull(c.CustomerCode, '') + ' [' + c.CustomerName + ']' + ' (' + c.CustomerGender + ')' FullCustomerName, ";
            SQL += "c.CustomerAddress + case when c.CustomerCommune <> '' then + ', ' + c.CustomerCommune else '' end + case when c.CustomerDistrict <> '' then + ', ' + c.CustomerDistrict else '' end + case when c.CustomerProvinceCity <> '' then + ', ' + c.CustomerProvinceCity else '' end [Address], ";
            SQL += "c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerIdentification, convert(varchar, c.RegisterDate, 103) RegisterDate, ";
            SQL += "c.CustomerNationality, c.CustomerIdentification, (e.Salutation + e.FullName) EmployeeName, isnull(ct.CustomerType, '') CustomerType, c.CustomerTypeID, c.CustomerName, c.ATTName,";
            SQL += "i.IndustryType, c.CustomerID, c.CustomerAddress, c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, c.CustomerMobile, c.CustomerEmail, c.CustomerIdentification, c.CustomerCode, c.CustomerGender, c.CustomerGender CustomerGenderID, c.CustomerName Cust_Name, c.CustomerGroup, c.CustomerGroupID ";
            SQL += "from dbo.TblCustomer_SaleOrder c ";
            SQL += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
            SQL += "left join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID ";
            SQL += "left join dbo.TblIndustryType i on i.IndustryTypeID = c.IndustryTypeID ";
            SQL += "where c.CustomerID = @CustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCompanyInfo(int ID)
        {
            HTML html = new HTML();
            string SQL = "select CompanyName, CompanyNameKH, CompanyDeskPhone, CompanyMobile, ";
            SQL += "VATNumber, t.IndustryType, c.IndustryTypeID BusinessTypeID, c.CompanyAddress, c.CompanyCommune, ";
            SQL += "c.CompanyDistrict, c.CompanyProvinceCity, c.CompanyContactName, ";
            SQL += "c.CompanyContactPosition, c.CompanyContactMobileNumber ";
            SQL += "from dbo.TblCustomerCompany_SaleOrder c ";
            SQL += "inner join dbo.TblIndustryType t on t.IndustryTypeID = c.IndustryTypeID ";
            SQL += "where c.CustomerID = @CustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustInv(int ID)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a name=\"Payment\" href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(Unpaid,'#,#.00')+''',10, '+ convert(varchar, CustomerID) +');\">Pay Now</a>' Payment ";
            SQL += ", format(PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 11"; //11 = unpaid invoice
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustInvoice(int ID)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=UnpaidInvoice\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, InvoiceDate, ";
            SQL += "case when DATEDIFF(day, InvoiceDate, GETDATE()) > 0 then DATEDIFF(day, InvoiceDate, GETDATE()) else 0 end [Day], PaymentDueDate, SubTotal, VAT, Total, DiscountP, [Type],";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a name=\"Payment\" href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(Unpaid,'#,#.00')+''',10, '+ convert(varchar, CustomerID) +','''+[Type]+''');\">Pay Now</a>' Payment ";
            SQL += ", format(PaidDate, 'dd-MM-yy HH:mm:ss tt') PaidDate, ConnectionID from dbo.TblCustomerBalance_SaleOrder where CustomerID = " + ID + " and StatusID = 11"; //11 = unpaid invoice
            html.GH.SQL = SQL;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustInvoice";
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.ShowPrintOnTopTable = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                //new GridField() { Header = "Connection ID", FieldName = "ConnectionID", ClassName="text-nowrap" },
                new GridField() { Header = "Invoice#", FieldName = "InvNo", ClassName="text-nowrap" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", TextAlign = "center", Format="dd-MM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", TextAlign = "center", Format="dd-MM-yy" },
                new GridField() { Header = "Paid Date", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                //new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                //new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                                  IsTotalFooter=true, Colspan = 7, TotalLabel="Total:", Format="#,#.00"
                },
                new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                    ActionName="Proforma To Official Invoice",
                    ActionIcon="fas fa-file-invoice",
                    ActionClickFunction="OfficialInvoice(this)",
                    ShowActionNameOnConditionValue="ProformaInvoice",
                    ShowActionNameOnConditionField="Type",
                    //HideEditOnActionNameNotMatch=true,
                    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this);",
                    DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
                    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE",
                }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadCustomerInvoices") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustRollBackInvoice(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +');\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(GrandTotal,'#,#.00')+''',11);\">Rollback</a>' Payment ";
            SQL += ", format(PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate, PaidBy from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 10"; //10 = paid invoice
            html.OrderBy = "format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc";
            html.GH.SQL = SQL;
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustRollback_Invoice";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Inv.No", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "PaidDate", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                new GridField() { Header = "PaidBy", FieldName = "PaidBy" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center",
                    //IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"
                //},
                new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadRollback_Inv") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustPaidInvoice(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ConnectionID, ('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=PaidInvoice\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false, this,''PaidInvoice'');\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(GrandTotal,'#,#.00')+''',11);\">Roll Back</a>' Payment ";
            SQL += ", format(PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate, PaidBy, '<a name=\"APreviewInv\" href=\"Payment/OfficialReceipt/?PaymentID=' + convert(varchar, PaymentID) + '&CompanyID=' + convert(varchar, InvoiceTmpID) + '&OfficialReceiptNo=' + ReceiptNo + '&InvoiceNo=' + InvoiceNo + '&SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "\" onclick=\"showOfficialReceipt('+ convert(varchar, PaymentID) + ',' + convert(varchar, InvoiceTmpID) + ', ''' + ReceiptNo +''', '''+ InvoiceNo + ''');\">'+ ReceiptNo + '</a>' ReceiptNo from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 10";//10 = paid invoice
            html.GH.SQL = SQL;
            html.OrderBy = "format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc";
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustPaidInvoice";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                //new GridField() { Header = "Connection ID", FieldName = "ConnectionID", ClassName="text-nowrap" },
                new GridField() { Header = "Invoice#", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Receipt#", FieldName = "ReceiptNo", ClassName="text-nowrap" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", TextAlign = "center" },
                new GridField() { Header = "Paid Date", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                //new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                //new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "Paid By", FieldName = "PaidBy" },
                new GridField() { Header = "Total", FieldName = "GrandTotal",
                    TextAlign = "center", Format = "#,#.00",
                    IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"},
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center",
                //    IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                //                  IsTotalFooter=true, Colspan = 7, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                //new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                //    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this);",
                //    DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
                //    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE"
                //}
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadPaidInv") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustCancelInvoice(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(CancelDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ('<a target=\"_blank\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=PaidInvoice\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "SubTotal, CancelComment, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo ";
            SQL += ", format(CancelDate, 'dd/MM/yyyy HH:mm:ss') CancelDate, CancelBy from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 13";//13 = cancel invoice
            html.GH.SQL = SQL;
            html.OrderBy = "format(CancelDate, 'yyyy/MM/dd HH:mm:ss') desc";
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustCancelInvoice";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Inv.No", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Cancel Date", FieldName = "CancelDate", TextAlign = "center" },
                new GridField() { Header = "Cancel By", FieldName = "CancelBy", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Comment", FieldName = "CancelComment", ClassName="text-nowrap" },
                new GridField() { Header = "Total", FieldName = "GrandTotal",
                    TextAlign = "right", Format = "#,#.00",
                    IsTotalFooter=true, Colspan = 4, TotalLabel="Total:"},
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center",
                //    IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                //                  IsTotalFooter=true, Colspan = 7, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                //new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                //    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this);",
                //    DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
                //    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE"
                //}
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadPaidInv") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustPaidInvoiceHistory(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            //string SQL = "select ROW_NUMBER() OVER(ORDER BY format(p.PaymentDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ";
            //SQL += "p.Payer, format(p.PaymentDate, 'dd/MM/yyyy HH:mm:ss') PaymentDate, m.PaymentMethod, p.FromBankName, p.FromAccountNumber, p.FromAccountName, ";
            //SQL += "p.FromPhoneNumber, p.ToPhoneNumber, p.ChequeNumber, (ac.AccountTitle + case when ac.AccountNumber <> '' then ' - ' + ";
            //SQL += "ac.AccountNumber else '' end + case when ac.AccountName <> '' then ' - ' + ac.AccountName else '' end) AccountName, ";
            //SQL += "c.Category, p.PaymentDescription, p.ReceivedFromName, p.InvoiceNo, p.PaymentAmount, p.PaidBy ";
            //SQL += "from dbo.TblPayment p ";
            //SQL += "inner join dbo.TblPaymentMethod m on m.PaymentMethodID = p.PaymentMenthod ";
            //SQL += "inner join dbo.TblAccount ac on ac.AccountID = p.ToAccountNumber ";
            //SQL += "inner join dbo.TblPaymentCategory c on c.PaymentCategoryID = p.Category ";
            //SQL += "where p.CustomerID = " + ID + " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
            //html.OrderBy = "format(p.PaymentDate, 'yyyy/MM/dd HH:mm:ss') desc";

            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(p.PayDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+ convert(varchar, p.CustomerID) +', '''+ p.InvoiceNo +''','+ convert(varchar, i.IsIncludedVAT) +','+ convert(varchar, i.StatusID) +');\">' + p.InvoiceNo + '</a>') InvoiceNo, ";
            SQL += "format(p.PayDate, 'dd/MM/yyyy HH:mm:ss') PayDate, PayBy, PaymentDescription, PayTo, PayAmount, Category, PayDescription, ReceivedFromName ";
            SQL += "from dbo.TblPaymentHistory p ";
            SQL += "inner join dbo.TblCustomerInvoice i on i.InvoiceNo = p.InvoiceNo ";
            SQL += "where p.CustomerID = " + ID + " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
            SQL += "group by ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+ convert(varchar, p.CustomerID) +', '''+ p.InvoiceNo +''','+ convert(varchar, i.IsIncludedVAT) +','+ convert(varchar, i.StatusID) +');\">' + p.InvoiceNo + '</a>') ";
            SQL += ", format(p.PayDate, 'dd/MM/yyyy HH:mm:ss'), PayBy, PaymentDescription, PayTo, PayAmount, Category, PayDescription, ReceivedFromName ";
            SQL += ", format(p.PayDate, 'yyyy/MM/dd HH:mm:ss')";

            html.OrderBy = "format(p.PayDate, 'yyyy/MM/dd HH:mm:ss') desc";
            html.GH.SQL = SQL;
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustPaidInvoiceHistory";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo", ClassName="text-nowrap" },
                new GridField() { Header = "PayDate", FieldName = "PayDate", ClassName="text-nowrap" },
                new GridField() { Header = "PayBy", FieldName = "PayBy", ClassName="text-nowrap" },
                new GridField() { Header = "Description", FieldName = "PaymentDescription", ClassName="text-nowrap" },
                new GridField() { Header = "PayTo", FieldName = "PayTo", ClassName="text-nowrap" },
                new GridField() { Header = "PayAmount", FieldName = "PayAmount", TextAlign="right", Format="#,#.00" },
                new GridField() { Header = "Category", FieldName = "Category", ClassName="text-nowrap" },
                new GridField() { Header = "PayDescription", FieldName = "PayDescription", ClassName="text-nowrap" },
                new GridField() { Header = "ReceivedFromName", FieldName = "ReceivedFromName", ClassName="text-nowrap" },
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadInvHistory") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustQuotation(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, CustomerID ";
            SQL += "from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 20  order by CustomerBalanceID desc";
            html.GH.SQL = SQL;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustQuotation";
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Quotation"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Quotation"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Quotation No.", FieldName = "InvNo", TextAlign = "center" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", TextAlign = "center" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "GrandTotal", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center" },
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this, 1);",
                    DeleteWithTableName="TblCustomerInvoice, TblCustomerBillInformation, TblCustomerDetail, TblCustomerBalance, TblCustomerBillingInvoice",
                    DeletedCallBackFunction="LoadCustQuotation(" + ID + ")", DeletePrimaryTblName="TblCustomer",
                    DeletePrimaryFiledName="CustomerID", DeletePrimaryValue=ID.ToString(), DeletePrimaryTblNameDependOnTblName="TblCustomerBalance"
                }
            };
            //string data = html.DrawHTMLSearchForm(1, "LoadCustomerQT");
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadCustomerQT") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveChangeExchageRate(string Value)
        {
            HTML html = new HTML();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("if (not EXISTS(select ExchangeRateID from dbo.TblExchangeRate where convert(varchar, ExchangeRate) <> ''))");
            sb.AppendLine("begin");
            sb.AppendLine("insert into dbo.TblExchangeRate(ExchangeRate) values(" + Value.ToString() + ");");
            sb.AppendLine("end");
            sb.AppendLine("else");
            sb.AppendLine("begin");
            sb.AppendLine("update dbo.TblExchangeRate set ExchangeRate = " + Value.ToString() + "");
            sb.AppendLine("end");
            ;
            return Json(new { data = html.SaveChangeData(sb.ToString(), false, false) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustomerServices(int CustomerID)
        {
            HTML html = new HTML();
            string SQL = @"select isnull(tb.[SID], 'Pending') [SID], tb.OpenTicketID, tb.TicketNumber, ac.RouterID, s.ServiceName, p.PackageName, pd.Bandwidth, tb.UM, tb.StatusID, isnull(tb.IsChangePlan, 0) IsChangePlan, isnull(tb.IsApplyChangePlan, 0) IsApplyChangePlan, tb.ConnectionID,
tb.StartDate, tb.Duration, case when DATEDIFF(M, convert(varchar, tb.StartDate, 111),
convert(varchar, tb.EndDate, 111)) >= tb.Duration then
tb.FreeDuration else 0 end FreeDuration, isnull(ac.ConnectivityStatus, 43) ConnectivityStatus, ss.Status, isnull(ac.ConnectionType, '') ConnectionType, ac.IPAddress, isnull(ac.VLanName, '') VLanName,
tb.NextInvoiceDate, isnull(tb.IsRenew, 0) IsRenew,
case when DATEDIFF(day, tb.StartDate, GETDATE()) > 0 then DATEDIFF(day, tb.StartDate, GETDATE()) else 0 end [Day], tb.EndDate, tb.ServiceID, p.PackageTypeID, tb.PackageDetailID, tb.BillingInvoiceID
, tb.InvoiceNo, (case when DATEDIFF(d, getdate(), tb.EndDate) >=0 then case when DATEDIFF(d, getdate(), tb.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), tb.EndDate)) + ' days'
when DATEDIFF(d, getdate(), tb.StartDate) > 0 then convert(varchar, DATEDIFF(d, tb.StartDate, tb.EndDate)) + ' days'
end else 'Expired' end) RemainDay
, tb.PaymentDueDate, ac.PPPoEUserName, tb.ActivateCustomerID,
isnull((select top 1 case when bi.IsApplyChangePlan =1 then case when isnull(bi.invTmp, 1) = 1 then bi.PackageName else bi.ServiceName end end
from dbo.TblCustomerBillingInvoice_SaleOrder bi where bi.InvoiceNo = tb.OldInvoiceNo),'') OldPlanName
from dbo.TblCustomerBillingInvoice_SaleOrder tb
left join dbo.TblActivateCustomer_SaleOrder ac on ac.ActivateCustomerID = tb.ActivateCustomerID
inner join dbo.TblServices s on s.ServiceID = tb.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = tb.PackageDetailID
inner join dbo.TblPackages p on p.PackageID = tb.PackageID
inner join dbo.TblStatus ss on ss.StatusID = tb.ConnectivityStatus
inner join (select cbi.CustomerID, count(*) as [Count] from TblCustomerBillingInvoice cbi where cbi.CustomerID = " + CustomerID + @" group by cbi.CustomerID) gc on gc.CustomerID = tb.CustomerID
where isnull(tb.IsApplyChangePlan, 0) = 0 and tb.CustomerID = " + CustomerID + @" and (tb.StatusID not in (12,13, 14, 20))
and format(GETDATE(), 'yyyy-MM') <= case when gc.[Count] > 1 then format(tb.NextInvoiceDate, 'yyyy-MM') else format(GETDATE(), 'yyyy-MM') end
order by tb.ConnectionID, tb.BillingInvoiceID";
            string data = html.JsonData(SQL);
            if (data == "[]")
            {
                SQL = @"select top 1 isnull(tb.[SID], 'Pending') [SID], tb.OpenTicketID, tb.TicketNumber, ac.RouterID, s.ServiceName, p.PackageName, 
pd.Bandwidth, tb.UM, tb.StatusID, isnull(tb.IsChangePlan, 0) IsChangePlan, isnull(tb.IsApplyChangePlan, 0) IsApplyChangePlan, tb.ConnectionID,
tb.StartDate, tb.Duration, case when DATEDIFF(M, convert(varchar, tb.StartDate, 111),
convert(varchar, tb.EndDate, 111)) >= tb.Duration then
tb.FreeDuration else 0 end FreeDuration, isnull(ac.ConnectivityStatus, 43) ConnectivityStatus, ss.Status, isnull(ac.ConnectionType, '') ConnectionType, ac.IPAddress, isnull(ac.VLanName, '') VLanName,
tb.NextInvoiceDate, isnull(tb.IsRenew, 0) IsRenew,
case when DATEDIFF(day, tb.StartDate, GETDATE()) > 0 then DATEDIFF(day, tb.StartDate, GETDATE()) else 0 end [Day], tb.EndDate, tb.ServiceID, p.PackageTypeID, tb.PackageDetailID, tb.BillingInvoiceID
, tb.InvoiceNo, (case when DATEDIFF(d, getdate(), tb.EndDate) >=0 then case when DATEDIFF(d, getdate(), tb.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), tb.EndDate)) + ' days'
when DATEDIFF(d, getdate(), tb.StartDate) > 0 then convert(varchar, DATEDIFF(d, tb.StartDate, tb.EndDate)) + ' days'
end else 'Expired' end) RemainDay
, tb.PaymentDueDate, ac.PPPoEUserName, tb.ActivateCustomerID,
isnull((select top 1 case when bi.IsApplyChangePlan =1 then case when isnull(bi.invTmp, 1) = 1 then bi.PackageName else bi.ServiceName end end
from dbo.TblCustomerBillingInvoice_SaleOrder bi where bi.InvoiceNo = tb.OldInvoiceNo),'') OldPlanName
from dbo.TblCustomerBillingInvoice_SaleOrder tb
left join dbo.TblActivateCustomer_SaleOrder ac on ac.ActivateCustomerID = tb.ActivateCustomerID
inner join dbo.TblServices s on s.ServiceID = tb.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = tb.PackageDetailID
inner join dbo.TblPackages p on p.PackageID = tb.PackageID
inner join dbo.TblStatus ss on ss.StatusID = tb.ConnectivityStatus
inner join (select cbi.CustomerID, count(*) as [Count] from TblCustomerBillingInvoice cbi where cbi.CustomerID = " + CustomerID + @" group by cbi.CustomerID) gc on gc.CustomerID = tb.CustomerID
where isnull(tb.IsApplyChangePlan, 0) = 0 and tb.CustomerID = " + CustomerID + @" and (tb.StatusID not in (12,13, 14, 20))
and format(tb.NextInvoiceDate, 'yyyy-MM') <= format(GETDATE(), 'yyyy-MM') 
order by format(tb.NextInvoiceDate, 'yyyy-MM') desc, tb.ConnectionID, tb.BillingInvoiceID";
                data = html.JsonData(SQL);
            }
            //SQL = @" select PPPoEUserName, IPAddress, s.[Status], ac.ConnectionType, ac.VLanName 
            //, ac.ConnectivityStatus
            //from dbo.TblActivateCustomer ac
            //inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
            //where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID
            //+ " and ac.CustomerID = " + CustomerID;
            //string ConnectivityData = html.JsonData(SQL);
            SQL = @"select bi.BillingInvoiceID, case when bi.invTmp = 1 then '<div class=""row""><div class=""col text-danger"">This package: ' + bi.PackageName + ' not yet upgrade due to not yet payment. </div></div>' else '<div class=""row""><div class=""col text-danger"">This package: ' + bi.ServiceName + ' not yet upgrade due to not yet payment. </div></div>' end NewPackageName
            from dbo.TblCustomerBillingInvoice_SaleOrder bi where CustomerID = " + CustomerID + " and StatusID = 11 and isnull(bi.IsChangePlan,0) = 1 and isnull(bi.IsAlreadyApplyProfile,0) = 0";
            string UpgradeP = html.JsonData(SQL);

            return Json(new { data = data, UpgradeP = UpgradeP }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustomerBalance(int ID)
        {
            HTML html = new HTML();
            string SQL = "select sum(DepositFee) DepositFee, sum(GrandTotal) Total, sum(Paid) Paid, sum(Unpaid) Unpaid ";
            SQL += "from dbo.TblCustomerBalance where CustomerID = @CustomerID and (StatusID not in (12,13,20)) ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustServices(int ID)
        {
            HTML html = new HTML();
            bool roleEditCustomer = HttpContext.GetOwinContext().Request.User.IsInRole("Edit Customer");
            ViewBag.roleEditCustomer = roleEditCustomer;
            string SQL = @"select (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) ServiceName,
            bf.CompanyName,bf.CompanyNameKH, bf.CompanyDeskPhone, bf.CompanyMobile, bf.VATNumber, bf.IndustryTypeID, it.IndustryType,
            bi.ConnectionID, '<span class=""text-red"">' + isnull(bi.[SID], 'Pending') + '</span>' [SID], s.[Status], bf.CompanyEmail, bf.CompanyContactMobileNumber, bf.BillToAddress, bf.IndustryTypeID,
            bf.BillToCommune, bf.BillToDistrict, bf.BillToProvinceCity, bf.InstallAddress, bf.InstallCommune, bf.CompanyAddress, bf.CompanyCommune, bf.CompanyDistrict, bf.ATTName, 
            bf.InstallDistrict, bf.InstallProvinceCity, bf.GoogleMap_FullAddress, bf.GoogleMap_Latitude, bf.GoogleMap_Longitude, bf.CompanyProvinceCity, bf.CompanyContactName, bf.CompanyContactPosition, bf.InstallDistrict, bf.InstallProvinceCity, bf.CustomerBillInformationID,
            case when sum(DATEDIFF(D, getdate(), bi.EndDate)) >=0 then convert(varchar, sum(DATEDIFF(D, getdate(), bi.EndDate))) + ' days' else 'Expired' end RemainDay
            from dbo.TblCustomerBillInformation bf
            left join dbo.TblCustomerBillingInvoice bi on bi.CustomerBillInformationID = bf.CustomerBillInformationID
            left join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            inner join dbo.TblIndustryType it on it.IndustryTypeID = bf.IndustryTypeID
            where bf.CustomerID = @CustomerID and isnull(bi.StatusID,0) <> 20 and isnull(bf.StatusID,0) <> 12 and bf.SystemID = @SystemID and bf.WarehouseID = @WarehouseID
            group by 
            (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end),
            bf.CompanyName,bf.CompanyNameKH, bf.CompanyDeskPhone, bf.CompanyMobile, bf.VATNumber, bf.IndustryTypeID, it.IndustryType,
            bi.ConnectionID, '<span class=""text-red"">' + isnull(bi.[SID], 'Pending') + '</span>', s.[Status], bf.CompanyEmail, bf.CompanyContactMobileNumber, bf.BillToAddress, bf.IndustryTypeID,
            bf.BillToCommune, bf.BillToDistrict, bf.BillToProvinceCity, bf.InstallAddress, bf.InstallCommune, bf.CompanyAddress, bf.CompanyCommune, bf.CompanyDistrict, bf.ATTName, 
            bf.InstallDistrict, bf.InstallProvinceCity, bf.GoogleMap_FullAddress, bf.GoogleMap_Latitude, bf.GoogleMap_Longitude, bf.CompanyProvinceCity, bf.CompanyContactName, bf.CompanyContactPosition,
            bf.InstallDistrict, bf.InstallProvinceCity, bf.CustomerBillInformationID
            order by bi.ConnectionID desc";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadAttachmentFile(int ID)
        {
            HTML html = new HTML();
            string sql = "select PaymentID, AttachmentFile, (PaymentDescription + '<br> Pay Date: ' + format(PaymentDate, 'dd/MM/yyyy HH:mm:ss')) Title, DestinationFolder from dbo.TblPayment ";
            sql += "where AttachmentFile <> '' and CustomerID = " + ID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            sql += "order by format(PaymentDate, 'yyyy/MM/dd HH:mm:ss') desc";
            DataSet ds = html.LoadData(sql);
            string data = "";
            dynamic FileName = new List<dynamic>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dynamic row = new ExpandoObject();
                        row.FileName = dr["AttachmentFile"].ToString();
                        row.Title = dr["Title"].ToString();
                        row.ID = dr["PaymentID"].ToString();
                        FileName.Add(row);
                        //if (FileName == "")
                        //{
                        //    FileName = dr["AttachmentFile"].ToString();
                        //    Title = dr["Title"].ToString();
                        //}
                        //else
                        //{
                        //    FileName += "~" + dr["AttachmentFile"].ToString();
                        //    Title += "~" + dr["Title"].ToString();
                        //}
                    }
                    //object FName = Json(new
                    //{
                    //    FileName = fileName,
                    //    Title = Title
                    //}, JsonRequestBehavior.AllowGet);

                    //var obj = new { FileName = FileName, Title = Title };
                    data = html.ShowAttachmentFile(DestinationFolder, FileName);
                }
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustAttachmentFile(int ID)
        {
            HTML html = new HTML();
            string sql = "select AttachmentFileID, ('InvoiceNo.: ' + InvoiceNo) Title, DestinationFolder, AttachmentFileName AttachmentFile, format(AddDate, 'dd/MM/yyyy') AddDate ";
            sql += "from TblCustomerAttachmentFile where CustomerID = " + ID;
            sql += " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            sql += " order by InvoiceNo, format(AddDate, 'dd/MM/yyyy') desc";
            DataSet ds = html.LoadData(sql);
            string data = "";
            dynamic FileName = new List<dynamic>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //string localPath = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                    string desc = "/" + ds.Tables[0].Rows[0]["DestinationFolder"].ToString() + "/";
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dynamic row = new ExpandoObject();
                        if (dr["AttachmentFile"].ToString() != "")
                        {
                            row.FileName = dr["AttachmentFile"].ToString();
                            row.Title = dr["Title"].ToString() + "<br />" + dr["AddDate"].ToString();
                            row.ID = dr["AttachmentFileID"].ToString();
                            FileName.Add(row);
                        }
                    }
                    data = html.ShowAttachmentFile(desc, FileName, true, true, "CustomerProfile/DeletCustomerAttFile");
                }
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeletCustomerAttFile(string DestinationFile, string DeleteFileName, dynamic KeepFileName, int ID)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string SQL = "update dbo.TblCustomerAttachmentFile set AttachmentFileName = @AttachmentFileName where AttachmentFileID = @AttachmentFileID";
            var FileName = "";
            foreach (string fname in KeepFileName)
            {
                if (FileName == "")
                    FileName = fname;
                else
                    FileName += "~" + fname;
            }
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "AttachmentFileName", FileName },
                { "AttachmentFileID", ID.ToString() }
            };
            sqlResult = html.DeleteAttachmentFile(DestinationFile, DeleteFileName, KeepFileName, SQL, para);

            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult SaveChangeCustomerUploadFile(object[] Data)
        {
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            HTML html = new HTML();
            string CustomerName = HttpUtility.UrlDecode(CustomerDataInfo["CustomerName"]);
            string CID = HttpUtility.UrlDecode(CustomerDataInfo["CustomerCode"]);
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName;
            SqlResult result = new SqlResult();
            result = html.UploadFile(DestinationFolder, "", false, true);
            string AttachmentFile;
            if (result.Result == ResultType.Successful)
                AttachmentFile = result.Value;
            else
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result,
                    Data = result.Value
                });
            }
            string SystemID = html.SystemID;
            string WarehouseID = html.WarehouseID;
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);

            string sql = "insert into TblCustomerAttachmentFile (CustomerID, AttachmentFileName, DestinationFolder, SystemID, WarehouseID, AddDate)";
            sql += " values (@CustomerID, @AttachmentFileName, @DestinationFolder, @SystemID, @WarehouseID, @AddDate);";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerDataInfo["CustomerID"].ToString() },
                { "AttachmentFileName", AttachmentFile },
                { "DestinationFolder", DestinationFolder },
                { "SystemID", SystemID.ToString() },
                { "WarehouseID", WarehouseID.ToString() },
                { "AddDate", easternTimeNow.ToString("yyyy/MM/dd HH:mm:ss tt") }
            };
            SqlResult sqlresult = new SqlResult();
            sqlresult = html.SaveChangeData(sql, false, para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            });
        }
        public ActionResult SaveChangeCustomerProfilePicture(object[] Data)
        {
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            string CustomerName = HttpUtility.UrlDecode(DataInfo["CustomerName"]);
            string CID = HttpUtility.UrlDecode(DataInfo["CustomerCode"]);
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/Profile Picture/";
            sqlresult = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (sqlresult.Result == ResultType.Successful)
                AttachmentFile = sqlresult.Value;
            else
            {
                return Json(new
                {
                    Message = sqlresult.Message,
                    Result = sqlresult.Result,
                    Data = sqlresult.Value
                }, JsonRequestBehavior.AllowGet);
            }
            string SQL = "update dbo.TblCustomer set DestinationFile = @DestinationFile, CustomerProfilePicture= @CustomerProfilePicture where CustomerID = @CustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", DataInfo["CustomerID"].ToString() },
                { "DestinationFile", DestinationFolder },
                { "CustomerProfilePicture", AttachmentFile }
            };
            sqlresult = html.SaveChangeData(SQL, false, para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustContractFile(int ID)
        {
            HTML html = new HTML();
            string sql = "select c.CustomerBillInformationID, c.DestinationFolder, c.imgFileName, pdfFileName, format(c.InvoiceDate, 'dd/MM/yyyy') InvoiceDate ";
            sql += "from dbo.TblCustomerBillInformation c where c.CustomerID = " + ID;
            sql += " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            sql += " order by format(c.InvoiceDate, 'dd/MM/yyyy')";
            DataSet ds = html.LoadData(sql);
            string data = "";
            dynamic FileName = new List<dynamic>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //string localPath = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                    string desc = "/" + ds.Tables[0].Rows[0]["DestinationFolder"].ToString() + "/";
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dynamic row = new ExpandoObject();
                        if (dr["imgFileName"].ToString() != "")
                        {
                            row.FileName = dr["imgFileName"].ToString();
                            row.Title = "Date: " + dr["InvoiceDate"].ToString();
                            row.ID = dr["CustomerBillInformationID"].ToString();
                            row.PDFFileName = dr["pdfFileName"].ToString();
                            FileName.Add(row);
                        }
                    }
                    data = html.ShowAttachmentFile(desc, FileName, true, false, "", true, false, true);
                }
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustMap(int ID)
        {
            HTML html = new HTML();
            string SQL = "select distinct cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude ";
            SQL += "from dbo.TblCustomerBillInformation cb ";
            //SQL += "inner join dbo.TblCustomerInvoice c on c.InvoiceNo = cb.InvoiceNo ";
            SQL += "where cb.CustomerID=@CustomerID ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCustProfilePicture(int ID)
        {
            HTML html = new HTML();
            string SQL = "select DestinationFile, CustomerProfilePicture from dbo.TblCustomer ";
            SQL += "where CustomerID=@CustomerID ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckConnectivityStatus(int CustomerID)
        {
            HTML html = new HTML();
            StringBuilder sb = new StringBuilder();
            string sql = @" select PPPoEUserName, IPAddress, s.[Status], ac.ConnectionType, ac.VLanName 
                            , ac.ConnectivityStatus
                            from dbo.TblActivateCustomer ac
                            inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
                            where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID
                            + " and ac.CustomerID = " + CustomerID;
            string Data = html.JsonData(sql);
            sql = @"select case when bi.invTmp = 1 then '<div class=""row""><div class=""col text-danger"">This package not yet upgrade due to not yet payment. </div></div><div class=""row""><div class=""col text-danger"">' + bi.PackageName + '</div></div>' else '<div class=""row""><div class=""col text-danger"">This package not yet upgrade due to not yet payment. </div></div><div class=""row""><div class=""col text-danger"">' + bi.ServiceName + '</div></div>' end NewPackageName
                    from dbo.TblCustomerBillingInvoice bi where CustomerID = " + CustomerID + " and ConnectivityStatus = 43 and StatusID = 11 and bi.IsChangePlan = 1";
            string UpgradeP = html.JsonData(sql);
            return Json(new { Data = Data, UpgradeP = UpgradeP }, JsonRequestBehavior.AllowGet);

            //        if (ds.Tables.Count > 0)
            //        {
            //            if (ds.Tables[0].Rows.Count > 0)
            //            {
            //                sb.AppendLine("<div class=\"row\">");
            //                sb.AppendLine("<div class=\"col\">");
            //                sb.AppendLine("");
            //                int i = 0;
            //                foreach(DataRow dr in ds.Tables[0].Rows)
            //                {
            //                    if (dr["ConnectionType"].ToString() == "PPPoE")
            //                    {

            //                        i++;
            //                    }
            //                }
            //                sb.AppendLine("</div></div>");
            //            }
            //        }
            //        string str = @"<div class=""container-xl"">
            //    <div class=""row"">
            //        <div class=""col-lg-8 mx-auto"">
            //            <div id=""myCarousel"" class=""carousel slide"" data-ride=""carousel"">
            //                <ol class=""carousel-indicators"">
            //                    <li data-target=""#myCarousel"" data-slide-to=""0"" class=""active""></li>
            //                    <li data-target=""#myCarousel"" data-slide-to=""1""></li>
            //                    <li data-target=""#myCarousel"" data-slide-to=""2""></li>
            //                </ol>
            //                <div class=""carousel-inner"">
            //                    <div class=""carousel-item active"">
            //                        <div class=""img-box""><img src=""""></div>
            //                        <p class=""testimonial"">Lorem ipsum dolor sit amet, consectetur adipiscing elit.Nam eu sem tempor, varius quam at, luctus dui.Mauris magna metus, dapibus nec turpis vel, semper malesuada ante.Idac bibendum scelerisque non non purus.Suspendisse varius nibh non aliquet.</p>
            //                        <p class=""overview""><b>Paula Wilson</b>, Media Analyst</p>
            //                    </div>
            //                    <div class=""carousel-item"">
            //                        <div class=""img-box""><img src = """"></div>
            //                        <p class=""testimonial"">Vestibulum quis quam ut magna consequat faucibus.Pellentesque eget nisi a mi suscipit tincidunt.Utmtc tempus dictum risus.Pellentesque viverra sagittis quam at mattis.Suspendisse potenti. Aliquam sit amet gravida nibh, facilisis gravida odio.</p>
            //                        <p class=""overview""><b>Antonio Moreno</b>, Web Developer</p>
            //                    </div>
            //                    <div class=""carousel-item"">
            //                        <div class=""img-box""><img src = """"></div>
            //                        <p class=""testimonial"">Phasellus vitae suscipit justo.Mauris pharetra feugiat ante id lacinia.Etiam faucibus mauris id tempor egestas. Duis luctus turpis at accumsan tincidunt. Phasellus risus risus, volutpat vel tellus ac, tincidunt fringilla massa.Etiam hendrerit dolor eget rutrum.</p>
            //                        <p class=""overview""><b>Michael Holz</b>, Seo Analyst</p>
            //                    </div>
            //                </div>
            //            </div>
            //        </div>
            //    </div>
            //</div>";
            //        sb.AppendLine(str);
            //        return Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet);

            //HTML html = new HTML();
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("<div class=\"btn-group\">");
            //StringBuilder sbvlan = new StringBuilder();
            //string sql = "select top 1 bi.ConnectivityStatus, s.[Status], bi.BillingInvoiceID ";
            //sql += "from dbo.TblCustomerBillingInvoice bi ";
            //sql += "inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus ";
            //sql += "where ConnectivityStatus = 43 and CustomerID = @CustomerID"; // and format(EndDate, 'yyyyMMdd') >= format(GetDate(), 'yyyyMMdd')
            //Dictionary<string, string> para = new Dictionary<string, string>();
            //para.Add("CustomerID", CustomerID.ToString());
            //DataSet ds = html.LoadData(sql, false, para);
            //if (ds.Tables.Count > 0)
            //{
            //    if (ds.Tables[0].Rows.Count > 0)
            //    {
            //        sb.AppendLine("<label class=\"btn bg-danger active\">" + ds.Tables[0].Rows[0]["Status"] + "</label>");
            //        sb.AppendLine("<label onclick=\"ActivateCustomerService(" + ds.Tables[0].Rows[0]["BillingInvoiceID"] + ");\" class=\"btn bg-cyan\">Activate Now</label>");
            //    }
            //    else
            //    {
            //        sql = "select top 1 bi.ConnectivityStatus, s.[Status], bi.BillingInvoiceID, bi.PPPoEUserName, bi.ConnectionType, bi.IPAddress, bi.VLanName ";
            //        sql += "from dbo.TblActivateCustomer bi ";
            //        sql += "inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus ";
            //        sql += "where ConnectivityStatus = 40 and ";
            //        sql += "CustomerID = @CustomerID"; // and format(EndDate, 'yyyyMMdd') >= format(GetDate(), 'yyyyMMdd')
            //        ds = html.LoadData(sql, false, para);
            //        if (ds.Tables[0].Rows.Count > 0)
            //        {
            //            sb.AppendLine("<label class=\"btn bg-primary active text-uppercase\">Status: " + ds.Tables[0].Rows[0]["Status"] + "</label>");

            //            if (ds.Tables[0].Rows[0]["ConnectionType"].ToString() == "PPPoE")
            //                sb.AppendLine("<label class=\"btn bg-cyan\">PPPoE User: " + ds.Tables[0].Rows[0]["PPPoEUserName"] + "</label>");
            //            else
            //            {
            //                sb.AppendLine("<label class=\"btn bg-cyan\">User IP: " + ds.Tables[0].Rows[0]["IPAddress"] + "</label>");
            //                sbvlan.AppendLine("<div class=\"row\"><div class=\"col\">");
            //                sbvlan.AppendLine("<label class=\"col-form-label\">Vlan Name: " + ds.Tables[0].Rows[0]["VLanName"] + "</label>");
            //                sbvlan.AppendLine("</div></div>");
            //            }
            //                //sb.AppendLine("<label onclick=\"ActivateCustomerService(" + ds.Tables[0].Rows[0]["BillingInvoiceID"] + ");\" class=\"btn bg-cyan\">Lock Now</label>");
            //        }
            //        else
            //        {
            //            sql = "select top 1 bi.ConnectivityStatus, s.[Status], bi.BillingInvoiceID, bi.PPPoEUserName, bi.ConnectionType, bi.IPAddress, bi.VLanName ";
            //            sql += "from dbo.TblActivateCustomer bi ";
            //            sql += "inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus ";
            //            sql += "where ConnectivityStatus = 41 and ";
            //            sql += "CustomerID = @CustomerID"; //and format(EndDate, 'yyyyMMdd') >= format(GetDate(), 'yyyyMMdd')
            //            ds = html.LoadData(sql, false, para);
            //            if (ds.Tables[0].Rows.Count > 0)
            //            {
            //                sb.AppendLine("<label class=\"btn bg-danger active text-uppercase\">Status: " + ds.Tables[0].Rows[0]["Status"] + "</label>");
            //                if (ds.Tables[0].Rows[0]["ConnectionType"].ToString() == "PPPoE")
            //                    sb.AppendLine("<label class=\"btn bg-cyan\">PPPoE User: " + ds.Tables[0].Rows[0]["PPPoEUserName"] + "</label>");
            //                else
            //                {
            //                    sb.AppendLine("<label class=\"btn bg-cyan\">User IP: " + ds.Tables[0].Rows[0]["IPAddress"] + "</label>");
            //                    sbvlan.AppendLine("<div class=\"row\"><div class=\"col\">");
            //                    sbvlan.AppendLine("<label class=\"col-form-label\">Vlan Name: " + ds.Tables[0].Rows[0]["VLanName"] + "</label>");
            //                    sbvlan.AppendLine("</div></div>");
            //                }
            //                //sb.AppendLine("<label onclick=\"ActivateCustomerService(" + ds.Tables[0].Rows[0]["BillingInvoiceID"] + ");\" class=\"btn bg-cyan\">Re-Activate Now</label>");
            //            }
            //            else
            //            {
            //                sql = "select top 1 bi.ConnectivityStatus, s.[Status], bi.BillingInvoiceID ";
            //                sql += "from dbo.TblActivateCustomer bi ";
            //                sql += "inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus ";
            //                sql += "where ConnectivityStatus = 50 and ";
            //                sql += "CustomerID = @CustomerID";// and format(EndDate, 'yyyyMMdd') >= format(GetDate(), 'yyyyMMdd')
            //                ds = html.LoadData(sql, false, para);
            //                if (ds.Tables[0].Rows.Count > 0)
            //                {
            //                    sb.AppendLine("<label class=\"btn bg-danger active text-uppercase\">Status: " + ds.Tables[0].Rows[0]["Status"] + "</label>");
            //                    //sb.AppendLine("<label onclick=\"ActivateCustomerService(" + ds.Tables[0].Rows[0]["BillingInvoiceID"] + ");\" class=\"btn bg-cyan\">Re-Activate Now</label>");
            //                }
            //            }
            //        }
            //    }
            //}
            //sb.Append("</div>");
            //sb.Append(sbvlan.ToString());
            //return Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomerChangePlan(string PrimaryFieldValue)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            Dictionary<string, string> Datainfo = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(PrimaryFieldValue);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @" select ac.ActivateCustomerID, c.CustomerName + ' [' + c.CustomerCode + ']' CustomerName,
                            s.ServiceName, pt.PackageTypeName, p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName,
                            pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate,
                            case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, ac.ServiceID, bi.PackageDetailID, bi.FreeDuration, bi.Duration
                            from dbo.TblActivateCustomer ac
                            inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
                            inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
                            inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                            inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
                            inner join dbo.TblCustomerBillingInvoice bi on bi.ActivateCustomerID = ac.ActivateCustomerID
                            where ac.ActivateCustomerID = @ActivateCustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                { "ActivateCustomerID", Datainfo["ActivateCustomerID"] }
            };
            DataSet ds = html.LoadData(sql, false, Para);
            DataRow dr = ds.Tables[0].Rows[0];
            html.TableColumn = 2;
            html.UseTable = true;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Name:",
                ID = "CustomerName",
                DBField = "CustomerName",
                IsReadOnly = true,
                Value = dr["CustomerName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Service Name:",
                ID = "ServiceName",
                DBField = "ServiceName",
                IsReadOnly = true,
                Value = dr["ServiceName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Connection Type:",
                ID = "PackageTypeName",
                DBField = "PackageTypeName",
                IsReadOnly = true,
                Value = dr["PackageTypeName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Name:",
                ID = "PackageName",
                DBField = "PackageName",
                IsReadOnly = true,
                Value = dr["PackageName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bandwidth:",
                ID = "Bandwidth",
                DBField = "Bandwidth",
                IsReadOnly = true,
                Value = dr["Bandwidth"].ToString()
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "UM:",
            //    ID = "UM",
            //    DBField = "UM",
            //    IsReadOnly = true,
            //    Value = dr["UM"].ToString()
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Download:",
                ID = "DowSpeed",
                DBField = "DowSpeed",
                IsReadOnly = true,
                Value = dr["DowSpeed"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Upload:",
                ID = "UpSpeed",
                DBField = "UpSpeed",
                IsReadOnly = true,
                Value = dr["UpSpeed"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount:",
                ID = "Price",
                DBField = "Price",
                IsReadOnly = true,
                Value = dr["Price"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Duration:",
                ID = "Txt_Duration",
                DBField = "Duration",
                IsReadOnly = true,
                Value = dr["UM"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Free Duration:",
                ID = "Txt_FreeDuration",
                DBField = "FreeDuration",
                IsReadOnly = true,
                Value = html.Pluralize("Month", int.Parse(dr["FreeDuration"].ToString()))
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Invoice Date:",
                ID = "InvoiceDate",
                DBField = "InvoiceDate",
                IsReadOnly = true,
                Value = dr["InvoiceDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Start Date:",
                ID = "StartDate",
                DBField = "StartDate",
                IsReadOnly = true,
                Value = dr["StartDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "End Date:",
                ID = "EndDate",
                DBField = "EndDate",
                IsReadOnly = true,
                Value = dr["EndDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Remain:",
                ID = "Remain",
                DBField = "Remain",
                IsReadOnly = true,
                Value = html.Pluralize("Day", int.Parse(dr["Remain"].ToString()))
            });
            html.FormID = "FrmCustChange_Plan";
            html.SaveChangeFunction = "SaveChangeCustomerChangePlan()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            Para.Clear();
            html = new HTML();
            sql = @"select distinct pd.PackageDetailID, s.ServiceID,
                    s.ServiceName + ' - ' + p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' +  + pd.UM PackageName 
                    from dbo.TblServices s
                    inner join dbo.TblPackages p on p.ServiceID = s.ServiceID
                    inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID
                    where s.ServiceID = " + dr["ServiceID"].ToString()
                    + " order by s.ServiceName + ' - ' + p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' +  + pd.UM";
            string sql_callback = @"select distinct pd.Price, pd.Duration, pd.FreeDuration
                    from dbo.TblServices s
                    inner join dbo.TblPackages p on p.ServiceID = s.ServiceID
                    inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID
                    where pd.PackageDetailID = @SearchText";
            Para.Add("ServiceID", dr["ServiceID"].ToString());
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Package Name:",
                ID = "Cbo_PackageName",
                DBField = "PackageName",
                SQL_CBO = sql,
                DisplayText = "PackageName",
                Value = "PackageDetailID",
                Infinity = false,
                SQLCallBack = sql_callback,
                CallBackWithControlValueID = "Cbo_PackageName",
                DisplayOnFields = "TxtPackagePrice, TxtPackageDuration, TxtPackageFreeDuration",
                IsCallBack = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Duration:",
                ID = "TxtPackageDuration",
                ISNumeric = true,
                ISInteger = true,
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Free Duration:",
                ID = "TxtPackageFreeDuration",
                ISNumeric = true,
                ISInteger = true,
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Price:",
                ID = "TxtPackagePrice",
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "Change Date:",
                ID = "ChangeDate",
                DBField = "ChangeDate"
            });
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "End Date:",
                ID = "TxtEndDate",
                DBField = "EndDate",
                //Value = easternTimeNow.AddDays(int.Parse(dr["Remain"].ToString())).ToString("dd/MM/yyyy"),
                //IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Duration:",
                ID = "TxtRemainDuration",
                DBField = "RemainDuration",
                //Value= html.Pluralize("Day", int.Parse(dr["Remain"].ToString())),
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount to Pay:",
                ID = "TxtAmount",
                DBField = "Amount",
                ISNumeric = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Reason For Change:",
                ID = "ReasonForChagne",
                DBField = "ReasonForChagne"
            });
            ViewBag.Remain = dr["Remain"].ToString();
            html.FormID = "FrmCustChangePlan";
            html.SaveChangeFunction = "SaveChangeCustomerChangePlan()";
            ViewBag.NewPlaneForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult ProformatInvoiceToOfficialInvoice(object[] Data)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            Dictionary<string, string> paraData = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(Data[0].ToString());
            Dictionary<string, string> para = new Dictionary<string, string>();
            string InvoiceDate = html.GetCurrentDate();
            if (Data[0].ToString().Contains("InvoiceDate"))
            {
                InvoiceDate = paraData["InvoiceDate"];
            }
            para.Add("InvoiceDate", InvoiceDate);
            para.Add("InvoiceNo", paraData["InvoiceNo"]);
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            sqlResult = html.SaveChangeData("SP_CONVERT_PI_TO_IN", true, para, true, true);
            DataSet ds = sqlResult.Data;
            DataRow dr = ds.Tables[0].Rows[0];
            string DataCallBack = html.JsonConvertDataTable(ds.Tables[0]);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, DataCallBack = DataCallBack }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadCustPieChart(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select 
            isnull(format(sum(isnull(InvoiceAmount, 0)), '#'), 0) InvoiceAmount,
            isnull(format(sum(isnull(DepositFee,0)), '#'), 0) DepositFee,
            isnull(format(sum(isnull(PaidAmount,0)), '#'), 0) PaidAmount, 
            isnull(format(sum(isnull(InvoiceAmount, 0)) - sum(isnull(PaidAmount,0)), '#'), 0) UnpaidAmount,
            isnull(format(sum(isnull(PartiallyPaid,0)), '#'), 0) PartiallyPaid,
            isnull(format(sum(isnull(AdvancePaymentAmount,0)), '#'), 0) AdvancePaymentAmount,
            isnull(format(sum(isnull(InputAmount,0)), '#'), 0) InputAmount,
            isnull(format(count(distinct InvoiceNo), '#'), 0) CountOfficailInvoice
            from dbo.TblCustomerAccount where 
            CustomerID = @CustomerID and isnull(StatusID, 0) not in (12, 13, 14, 20)";//[Type] = 'Official Invoice' and

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID.ToString() }
            };
            SqlResult sqlResult = new SqlResult();
            DataSet ds = html.LoadData(sql, false, para);
            DataRow dr = ds.Tables[0].Rows[0];
            //StringBuilder sb = new StringBuilder();
            //sb.Append("[" + dr["InvoiceAmount"] + "," + 
            //    dr["DepositFee"]  + "," +
            //    dr["PaidAmount"] + "," + 
            //    dr["UnpaidAmount"] + "," +
            //    dr["PartiallyPaid"] 
            //    //+ "," + 
            //    //dr["AdvancePaymentAmount"] + "," +
            //    //dr["InputAmount"] + "," + 
            //    //dr["CountOfficailInvoice"]
            //    + "]");
            //ViewBag.CustBalanceData = sb.ToString();
            ViewBag.CustBalanceData = html.JsonConvertDataTable(ds.Tables[0]).ToString();
            return View();
        }
        public JsonResult LoadCustConnection(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() OVER(ORDER BY c.CustomerID desc, bi.BillingInvoiceID desc, bi.ConnectionID desc) AS [No], c.CustomerCode, 
            c.CustomerName, case when bi.IsRenew = 1 then bi.[SID] + ' (Renew)' else isnull(bi.[SID], '<span class=""text-red"">Pending</span>') end [SID], bi.InvoiceNo,
            (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress,
            '<a class=""text-dark"" href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, bi.CustomerID) + ', '''+ bi.InvoiceNo +''','+convert(varchar, bi.IsIncludedVAT)+','+convert(varchar, bi.StatusID)+','''',false,this);"">' + bi.ConnectionID + '</a>' ConnectionID,
            '<a href=""javascript:void(0);"" onclick=""ShowDialogBox(''Activation Customer'', ''Auto'', ''500'', ''ActivateCustomer/Activate'', { CID: ''' + c.CustomerCode + ''', CustomerID:'+convert(varchar, bi.CustomerID)+', CustomerName:'''+(c.CustomerName + ' [' + c.CustomerCode + ']')+''', ServiceID: '+convert(varchar, bi.ServiceID) +', ConnectionTypeID:'+convert(varchar, p.PackageTypeID)+',PackageDetailID:' + convert(varchar, bi.PackageDetailID) + ', BillingInvoiceID:'+convert(varchar, bi.BillingInvoiceID)+', IsOnCustProfile:true, ActivateCustomerID: '+convert(varchar, isnull(bi.ActivateCustomerID,0))+', RouterID:'+convert(varchar, isnull(bi.RouterID,0)) + '});"">' + s.[Status] + '</a>' [Status], (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) PackageName,
            format(c.RegisterDate, 'dd-MM-yyyy') RegisterDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, 
            format(bi.EndDate, 'dd-MM-yyyy') EndDate, (c.CustomerName + ' [' + c.CustomerCode + ']') CustName, c.RegisterBy,
            '<span style=""color:red"">' + convert(varchar, (select count(distinct _bi.ConnectionID) from dbo.TblCustomerBillingInvoice _bi where _bi.CustomerID = bi.CustomerID and _bi.StatusID not in (12,13,20))) + '</span>' CountConnection,
            convert(varchar, DATEDIFF(D, getdate(), bi.EndDate))+ ' days' RemainDay
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
            inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            where bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)";

            sql += ";select (select count(distinct bi.ConnectionID) ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "where bi.ConnectivityStatus = 40 and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)) Active, ";
            sql += "(select count(distinct bi.ConnectionID) ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "where bi.ConnectivityStatus = 41 and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)) Lock, ";
            sql += "(select count(distinct bi.ConnectionID) ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "where bi.ConnectivityStatus = 43 and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)) Inactive;";
            html.GH.SQL = sql;
            html.TableName = "tblcustconnection";
            html.TblID = "tblcustconnection";
            html.TableClassName = "table table-bordered table-head-fixed text-nowrap table-responsive";
            html.isShowLoadingGrid = false;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 1;
            html.GridEventClick = "ShowCustomer_ConnectionInv(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", IsGroupByField = true, GroupLabel="CID: ", Colspan=10 },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel="Name: ", Colspan=10 },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", IsGroupByField = true, GroupLabel="Reg.Date: ", Colspan=10 },
                new GridField() { Header = "Register By", FieldName = "RegisterBy", IsGroupByField = true, GroupLabel="Reg.By: ", Colspan=10 },
                new GridField() { Header = "Count Connection", FieldName = "CountConnection", IsGroupByField = true, GroupLabel="Connection: ", Colspan=10 },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-red", TextAlign="center" },
                new GridField() { Header = "Package Name", FieldName = "PackageName", ClassName="text-info" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate", ClassName="text-red" },
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", ClassName="text-red" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" }
            };
            //html.DefaultSearch = false;
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;
            //html.GridEventClick = "ShowCustomerInfo(this)";
            //ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadCustConnection") }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadTransaction(int CustomerID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string sql = "select ROW_NUMBER() OVER(ORDER BY p.ReceiptNo desc, p.InvoiceNo) as [No], ";
            sql += "p.ReceiptDate, p.ReceiptBy, ";
            sql += "'<a href=\"Payment/OfficialReceipt/?PaymentID='+convert(varchar, p.PaymentID) +'&CompanyID='+convert(varchar, p.CompanyID) +'&OfficialReceiptNo='+p.ReceiptNo+'&InvoiceNo='+p.InvoiceNo+'&SystemID='+convert(varchar, p.SystemID)+'&WarehouseID='+convert(varchar, p.WarehouseID)+'\" target=\"_blank\">' + p.ReceiptNo + '</a>' ReceiptNo, ";
            sql += "('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID='+convert(varchar, p.CustomerID)+'&InvNo='+p.InvoiceNo+'&ISVAT='+ convert(varchar, p.IsIncludedVAT) +'&StatusID='+ convert(varchar, p.StatusID) +'&AutoPrint=false&index=0&InvoiceType=UnpaidInvoice\" onclick=\"PrintInvoice(' + convert(varchar, p.CustomerID) +', '''+ p.InvoiceNo +''','+ convert(varchar, p.IsIncludedVAT) +','+ convert(varchar, p.StatusID) +','''',false,this);\" target=\"_blank\">' + p.InvoiceNo + '</a>') InvoiceNo, ";
            sql += "(ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')')  AccountName, ";
            sql += "'Income' [Type], p.[Description], p.GrandTotal, p.TotalPaid, p.UnpaidAmount ";
            sql += "from dbo.TblCustomerAccount p ";
            sql += "inner join dbo.TblChartOfAccount ca on ca.ChartOfAccountID = p.ChartOfAccountID ";
            sql += "where isnull(p.ReceiptNo, '') <> '' and ";
            sql += "p.CustomerID = " + CustomerID + " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID + " and isnull(p.StatusID,0) not in(12,13,20)";

            html.GH.SQL = sql;
            html.OrderBy = "p.ReceiptNo desc";
            html.TblID = "tbltransaction";
            html.TableClassName = "table table-bordered table-head-fixed text-nowrap table-responsive";
            html.isShowLoadingGrid = false;
            html.ShowPaging = true;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            //html.JqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "ReceiptDate", Format="dd-MM-yy HH:mm:ss tt" },
                 new GridField() { Header = "Inv.No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Receipt", FieldName = "ReceiptNo" },
                new GridField() { Header = "Paid By", FieldName = "ReceiptBy" },
                new GridField() { Header = "Account", FieldName = "AccountName" },
                new GridField() { Header = "Type", FieldName = "Type" },
                new GridField() { Header = "Description", FieldName = "Description" },
                new GridField() { Header = "Total", FieldName = "GrandTotal",TextAlign = "right" },
                new GridField() { Header = "Total Paid", FieldName = "TotalPaid",TextAlign = "right" },
                new GridField() { Header = "Unpaid", FieldName = "UnpaidAmount",TextAlign = "right" }
            };
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadCustTransaction") }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadInvoiceForCreditNotes(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                AddButton = "Add Credit Notes",
                AddFUNCTION = "AddCreditNotes(" + ID.ToString() + ")"
            };

            string sqla = "select distinct top 10  InvoiceNo from dbo.TblCustomerBalance where StatusID = 14 and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            if (ID != 0)
                sqla = " and CustomerID = " + ID;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "",
                RadioValue = EncryAndDecry.Encryp("InvoiceNo"),
                DefaultCheckBok = true,
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string SQL = "select ROW_NUMBER() OVER(ORDER BY cb.InvoiceNo desc) as [No], ";
            SQL += "c.CustomerCode CID, c.CustomerName, ";
            SQL += "('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, cb.IsIncludedVAT) +'&StatusID='+ convert(varchar, cb.StatusID) +'&AutoPrint=false&index=0&InvoiceType=PaidInvoice\" onclick=\"PrintInvoice(" + ID + ", '''+ cb.InvoiceNo +''','+ convert(varchar, cb.IsIncludedVAT) +','+ convert(varchar, cb.StatusID) +','''',false, this,''PaidInvoice'');\">' + cb.InvoiceNo + '</a>')InvoiceNo, ";
            SQL += "cb.InvoiceDate, cb.PaymentDueDate DueDate, ReferenceNo, ";
            SQL += "s.[Status], cb.GrandTotal, ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')' AccountName ";
            SQL += "from dbo.TblCustomerBalance cb ";
            SQL += "inner join dbo.TblStatus s on s.StatusID = cb.StatusID ";
            SQL += "inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID ";
            SQL += "inner join dbo.TblChartOfAccount ca on ca.ChartOfAccountID = cb.ChartOfAccountID ";
            SQL += "where cb.StatusID = 14 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID;
            if (ID != 0)
                SQL += " and c.CustomerID = " + ID;
            html.GH.SQL = SQL;
            html.OrderBy = "cb.InvoiceNo desc";
            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "CID", FieldName = "CID" },
                //new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Account", FieldName = "AccountName" },
                new GridField() { Header = "Ref.No", FieldName = "ReferenceNo" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", Format="dd-MM-yy", TextAlign="center" },
                new GridField() { Header = "Due Date", FieldName = "DueDate", Format="dd-MM-yy", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "GrandTotal", TextAlign="right", Format = "#,#.00",
                    IsTotalFooter=true, Colspan = 5, TotalLabel="Total:" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadCreditNotes");
            return View();
        }

        public ActionResult ShowCustActivateLog(int CustomerID, bool ShowDetail, string PPPoEUserName, string ConnectionID)
        {
            HTML html = new HTML();
            ViewBag.ShowDetail = ShowDetail;
            ViewBag.PPPoEUserName = PPPoEUserName;
            html.ReportTitle = "list Customers Activity Connection Log";
            //html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            //{
            //    DefaultValueDate = false,
            //    DateRangePicker = true,
            //    LabelFromDate = "Start Date:",
            //    DBField = "bi.StartDate"
            //});
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Connection ID",
                SQL = "select ConnectionID from dbo.TblActivateCustomer where CustomerID =" + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayField = "ConnectionID",
                SelectValue = "ConnectionID",
                RadioValue = EncryAndDecry.Encryp("ac.ConnectionID"),
                DefaultCheckBok = true,
                Value = ConnectionID
            });
            string sql = @"select ROW_NUMBER() over(order by ac.ConnectionID) as[No], ha.[Date], ac.ConnectionID, 
            ac.[SID], ac.PPPoEProfileName ProfileName, ha.ActivateBy, ac.ActivateDate, ha.Comment, s.[Status]
            from dbo.TblHistoryActivateCustomer ha
            inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ha.ActivateCustomerID
            inner join dbo.TblStatus s on s.StatusID = ha.ConnectivityStatus
            where ha.CustomerID = " + CustomerID + " and ha.SystemID = " + html.SystemID + " and ha.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "[No], ac.ConnectionID desc";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date", Format="dd-MM-yy hh:mm:ss tt" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Profile Name", FieldName = "ProfileName" },
                new GridField() { Header = "Action By", FieldName = "ActivateBy" },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", Format="dd-MM-yy hh:mm:ss tt" },
                new GridField() { Header = "Comment", FieldName = "Comment" },
                new GridField() { Header = "Status", FieldName = "Status" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public JsonResult GetPPPeEUserOnline(bool ShowDetail, string PPPoEUserName)
        {
            Mikrotik mikrotik = new Mikrotik();
            return Json(new { Data = mikrotik.GetPPPeEUserOnline(ShowDetail, true, PPPoEUserName) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Change Device")]
        public ActionResult ChangeDevice(int CustomerID, string PrimaryFieldValue = "0")
        {
            HTML html = new HTML
            {
                FormID = "CreateChangeDevice",
                //html.FieldIDFocus = "TxtDepartmentName";
                //html.TableName = "TblDepartment";
                //html.PrimaryField = "DepartmentID";
                SaveChangeCallbackFunction = "CallBackSaveChangeDevice();"
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Current Device Information",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Item Name:", ID="Cbo_CurrentItemName",
                            Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL_CBO="select distinct ce.ItemName IName, ce.EquipmentID, ce.ActivateCustomerID, ce.ItemID, case when ce.Barcode <> '' then ce.Barcode + ' - ' else '' end + ce.ItemName ItemName,\r\nce.MACAddress, ce.SerialNo, ce.ConnectionID, ce.[SID],\r\n'' PackageName\r\nfrom dbo.TblCustomerEquipment ce\r\ninner join dbo.TblCustomerBillingInvoice bi on bi.ConnectionID = ce.ConnectionID where ce.[Status]='Active' and ce.CustomerID="+CustomerID+" and ce.SystemID = " + html.SystemID + " and ce.WarehouseID = " + html.WarehouseID + " order by case when ce.Barcode <> '' then ce.Barcode + ' - ' else '' end + ce.ItemName",
                            DisplayText="ItemName",
                            Value="ActivateCustomerID", DisplayOnFields=JsonConvert.SerializeObject(new {IName="IName",EquipmentID="EquipmentID",ActivateCustomerID="ActivateCustomerID",CurrentMACAddress="MACAddress",CurrentSerialNo="SerialNo",CurrentConnectionID="ConnectionID", CurrentSID="SID",CurrentPackageName="PackageName"})
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="IName:", ID="IName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="EquipmentID:", ID="EquipmentID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="ActivateCustomerID:", ID="ActivateCustomerID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="MAC Address:", ID="CurrentMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Serial No:", ID="CurrentSerialNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="CustomerID:", ID="Txt_Customer_ID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true, Value=CustomerID.ToString(),
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ConnectionID:", ID="CurrentConnectionID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="SID:", ID="CurrentSID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Package Name:", ID="CurrentPackageName",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true
                        },
                    }
                },
                new  FormData() {
                    FormTitle="Choose New Device",
                    TableName="TblCustomerEquipment",
                    GetIDENTITYByFiled="EquipmentID",
                    PrimaryFieldName="EquipmentID",
                    SQL="update dbo.TblCustomerEquipment set [Status]='Changed', Reason = @Txt_Reason where EquipmentID = @EquipmentID",
                    UpdateFiledName="EquipmentID, Txt_Reason",
                    dbField = new DBField[]
                    {
                        new DBField{DBFieldName="ActivateCustomerID", Value="ActivateCustomerID", GetValueFromInputName = true},
                        new DBField{DBFieldName="CustomerID", Value="Txt_Customer_ID", GetValueFromInputName = true},
                        new DBField{DBFieldName ="ConnectionID", Value="CurrentConnectionID", GetValueFromInputName = true},
                        new DBField{DBFieldName ="SID", Value="CurrentSID", GetValueFromInputName = true}
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label = "Class Name:",
                            ID = "cbo_ClassName",
                            SQL_CBO = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            DisplayText = "ClassName",
                            Value = "ClassID", Class="input2 CustInfo", IsCustomForm = true,
                            Required = true, DivClassCol="col-sm-2"
                        },
                        new HtmlComboBox()
                        {
                            Label = "Warehouse Name:",
                            ID = "cbo_Warehouse_Name",
                            SQL_CBO = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName from dbo.TblUserWarehouse uw\r\ninner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by (b.BranchName + ' - ' + uw.WarehouseName)",
                            DisplayText = "WarehouseName",
                            Value = "WID",
                            SQLCallBack= "select i.ItemID, case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName ItemName, i.Barcode, i.SerialNumber\r\nfrom dbo.TblStockInDetail sd\r\ninner join dbo.TblItem i on i.ItemID = sd.ItemID\r\n where sd.SystemID = "+html.SystemID+" and sd.WarehouseID="+html.WarehouseID+ " and sd.LocationID=@SearchText group by i.ItemID, i.Barcode, i.SerialNumber, i.ItemName\r\nhaving sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0)) > 0",
                            ISCallBackToCbo= true,
                            CallBackToControlID= "cboItemName",
                            CallBackDisplayText= "ItemName",
                            CallBackDisplayValue = "ItemID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Required = true
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label = "Item Name:",
                            ID = "cboItemName",
                            Infinity= false,
                            IsCallBack = true,
                            SQLCallBack= "select i.ItemName, i.Barcode, i.SerialNumber, sd.StockInID, i.UM, sd.CostPrice, sd.UnitPrice, i.MACAddress\r\nfrom dbo.TblStockInDetail sd\r\ninner join dbo.TblItem i on i.ItemID = sd.ItemID where sd.SystemID = " + html.SystemID +" and sd.WarehouseID = " + html.WarehouseID + " and sd.ItemID = @SearchText",
                            DisplayOnFields= "TxtItemName, TxtBarcode, TxtSerialNo, Txt_Stock_In_ID, Txt_UM, Txt_Cost_Price, Txt_Unit_Price, TxtMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemID",
                            Required = true
                        },
                        new HtmlTextBox()
                        {
                            Hide= true,
                            Label = "ItemName:",
                            ID = "TxtItemName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemName"
                        },
                        new HtmlTextBox()
                        {
                            Hide= true,
                            Label = "StockInID:",
                            ID = "Txt_Stock_In_ID",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Hide= true,
                            Label = "UM:",
                            ID = "Txt_UM",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Hide= true,
                            Label = "CostPrice:",
                            ID = "Txt_Cost_Price",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },new HtmlTextBox()
                        {
                            Label = "UnitPrice:",
                            ID = "Txt_Unit_Price",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "Barcode:",
                            ID = "TxtBarcode",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Barcode"
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "MAC Address:",
                            ID = "TxtMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="MACAddress"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Serial No:",
                            ID = "TxtSerialNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SerialNo"
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        //new HtmlComboBox()
                        //{
                        //    Label = "Status:",
                        //    ID = "cboStatus",
                        //    SQL_CBO="select StatusID, [Status] from dbo.TblRouterDeviceStatus where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Status]",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DisplayText="Status",
                        //    Value="Status",
                        //    DBField="Status",
                        //    DivClassCol="col-sm-2", AddNewFuntionName="AddNewStatus();",
                        //    Required = true
                        //},
                        new HtmlTextBox()
                        {
                            Label = "Status:",
                            ID = "TxtStatus",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Status",
                            Required = true, Value="Active", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "Reason:",
                            ID = "Txt_Reason",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL_AutoComplete="select Reason from dbo.TblStockReason\r\nwhere lower(Reason) like @SearchText and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID +" order by Reason",
                            //DBField="Reason",
                            Required = true, ISAutoComplete = true
                        },
                        //new HtmlComboBox()
                        //{
                        //    Label = "Reason:",
                        //    ID = "Txt_Reason",
                        //    SQL_CBO = "select ReasonID, Reason from dbo.TblStockReason where SystemID = "+html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Reason",
                        //    DisplayText = "Reason",
                        //    Value = "Reason",
                        //    DBField="Reason",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    AddNewFuntionName="AddNewStockReason()",
                        //    Required = true
                        //},
                        new HtmlTextBox()
                        {
                            Label = "Remark:",
                            ID = "Txt_Remark",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Remark",
                            Required = true,
                            Hide= true
                        },
                        new HtmlTextBox()
                        {
                            Label="NSM:", ID="NSM", DivClass="noborder", DivClassCol="col-sm-auto",
                            IsCustomForm = true,
                            DBField="NSM", Type="checkbox", DefaultCheckBoxChecked = true, Value="1"
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer:", ID="Customer", DivClass="noborder", DivClassCol="col-sm-auto",
                            IsCustomForm = true,
                            DBField="Customer", Type="checkbox", Value="0", DefaultCheckBoxChecked = false
                        },
                    }
                }
            };

            html.insTable.Add(
                new InsertTable()
                {
                    TableName = "TblStockInDetail",
                    Fields = new
                    {
                        SystemID = html.SystemID,
                        WarehouseID = html.WarehouseID,
                        ItemID = "$(\"#cboItemName\").val().toString()",
                        ItemName = "$(\"#TxtItemName\").val()",
                        StockInID = "$(\"#Txt_Stock_In_ID\").val()",
                        UM = "$(\"#Txt_UM\").val()",
                        CostPrice = "$(\"#Txt_Cost_Price\").val()",
                        QTYOUT = "1",
                        Type = "'Stock Out'",
                        Reason = "$(\"#Txt_Reason\").val().toString()",
                        StockInDate = "'" + html.GetCurrentDate() + "'",
                        StockOutDate = "'" + html.GetCurrentDate() + "'",
                        CustomerID = "$(\"#Txt_Customer_ID\").val()",
                        StatusID = "80",
                        LocationID = "$(\"#cbo_Warehouse_Name\").val().toString()",
                        UnitPrice = "0",//"$(\"#Txt_Unit_Price\").val()",
                        ClassNameID = "$(\"#cbo_ClassName\").val().toString()",
                    },
                    GetIdentityField = "StockInDetailID",
                    PrimaryField = "StockInDetailID",
                    PrimaryFieldValue = PrimaryFieldValue.ToString(),
                });
            html.insTable.Add(
                new InsertTable()
                {
                    TableName = "TblStockOut",
                    Fields = new
                    {
                        SystemID = html.SystemID,
                        WarehouseID = html.WarehouseID,
                        CustomerID = "$(\"#Txt_Customer_ID\").val()",
                        StockInID = "$(\"#Txt_Stock_In_ID\").val()",
                        ItemID = "$(\"#cboItemName\").val().toString()",
                        UM = "$(\"#Txt_UM\").val()",
                        QTY = "1",
                        CostPrice = "$(\"#Txt_Cost_Price\").val()",
                        UnitPrice = "0",//"$(\"#Txt_Unit_Price\").val()",
                        DiscountP = "0",
                        DiscountUSD = "0",
                        Amount = "0",//"$(\"#Txt_Unit_Price\").val()",
                        StatusID = "80",
                        IsUsed = "1",
                        InvoiceNumber = "''",
                        ConnectionID = "$(\"#CurrentConnectionID\").val()",
                        LocationID = "$(\"#cbo_Warehouse_Name\").val().toString()",
                        Type = "'Stock Out'",
                        ClassNameID = "$(\"#cbo_ClassName\").val().toString()",
                        IsIncludedVAT = "0",
                        DateOut = "'" + html.GetCurrentDate() + "'",
                    },
                    //GetIndetityFiled = "StockInDetailID",
                    //PrimaryField = "StockOutID",
                    //PrimaryFieldValue = PrimaryFieldValue.ToString(),
                });
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        [Authorize(Roles = "Receive Return Device")]
        public ActionResult ReceivedDevice(int CustomerID, string PrimaryFieldValue = "0")
        {
            HTML html = new HTML
            {
                FormID = "CreateReceivedDevice",
                SaveChangeCallbackFunction = "CallBackSaveChangeReceivedDevice();"
            };
            //html.SaveChangeAndClose = true;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Current Device Information",
                    TableName="TblStockIn",
                    PrimaryFieldName="StockInID",
                    GetIDENTITYByFiled="StockInID",
                    PrimaryFieldValue = PrimaryFieldValue,
                    SQL="update dbo.TblCustomerEquipment set Barcode = @Barcode, Status='Return', Reason = 'Received Date: ' + @ReceiveDate + '; ' + @Reason where EquipmentID = @EquipmentID; update dbo.TblStockOut set IsReturn = 1 where ConnectionID = @CurrentConnectionID",
                    UpdateFiledName="Barcode, EquipmentID, ReceiveDate, Reason, CurrentConnectionID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Receive Date:", ID="ReceiveDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceiveDate", SingleDatePicker = true
                        },
                        new HtmlComboBox()
                        {
                            Label="Item Name:", ID="Cbo_CurrentItemName",
                            Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,//case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end
                            SQL_CBO="select distinct i.ItemName iName, ce.EquipmentID, ce.ItemID,\r\nce.MACAddress, ce.SerialNo, i.Barcode, i.UM, so.CostPrice, ce.ConnectionID, ce.[SID],\r\n'' PackageName, \r\ncase when i.Barcode <> '' then i.Barcode + ' - ' else '' end + ce.ItemName ItemName\r\nfrom dbo.TblCustomerEquipment ce\r\ninner join dbo.TblItem i on i.ItemID = ce.ItemID inner join dbo.TblStockOut so on so.ItemID = ce.ItemID inner join dbo.TblCustomerBillingInvoice bi on bi.ConnectionID = ce.ConnectionID where ce.[Status]='Active' and ce.CustomerID = " + CustomerID + " and ce.SystemID = " + html.SystemID + " and ce.WarehouseID = " + html.WarehouseID + " order by case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + ce.ItemName",
                            DisplayText="ItemName",
                            Value="ItemID", DisplayOnFields=JsonConvert.SerializeObject(new {iName="iName",EquipmentID="EquipmentID",ItemID="ItemID",CurrentMACAddress="MACAddress",CurrentSerialNo="SerialNo",Barcode="Barcode",Txt_UM="UM",Txt_Cost_Price="CostPrice",CurrentConnectionID="ConnectionID", CurrentSID="SID",CurrentPackageName="PackageName"})
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="StatusID:", ID="TxtStatusID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StatusID", Value="83",
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="iName:", ID="iName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="EquipmentID:", ID="EquipmentID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="ItemID:", ID="ItemID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Barcode:", ID="Barcode",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="UM:", ID="Txt_UM",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="CostPrice:", ID="Txt_Cost_Price",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="MAC Address:", ID="CurrentMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Serial No:", ID="CurrentSerialNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="CustomerID:", ID="Txt_Customer_ID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true, Value=CustomerID.ToString(),
                        },new HtmlTextBox()
                        {
                            Label="Return:", ID="Return",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true, Value="Return",
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ConnectionID:", ID="CurrentConnectionID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="SID:", ID="CurrentSID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Package Name:", ID="CurrentPackageName",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label = "Class Name:",
                            ID = "cbo_ClassName",
                            SQL_CBO = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            DisplayText = "ClassName",
                            Value = "ClassID", Class="input2 CustInfo", IsCustomForm = true,
                            Required = true, DivClassCol="col-sm-2",
                            DBField="ClassID"
                        },
                        new HtmlComboBox()
                        {
                            Label = "Warehouse Name:",
                            ID = "cbo_Warehouse_Name",
                            SQL_CBO = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName from dbo.TblUserWarehouse uw\r\ninner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by (b.BranchName + ' - ' + uw.WarehouseName)",
                            DisplayText = "WarehouseName",
                            Value = "WID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Required = true, DBField="WID"
                        },
                        new HtmlComboBox()
                        {
                            Label = "Stock Reason:",
                            ID = "Reason",
                            SQL_CBO = "select ReasonID, Reason from dbo.TblStockReason where SystemID = "+html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Reason",
                            DisplayText = "Reason",
                            Value = "Reason",
                            Class="input2 CustInfo", IsCustomForm = true,
                            AddNewFuntionName="AddNewStockReason()",
                            Required = true
                        }
                    }
                }
            };

            html.insTable.Add(
                new InsertTable()
                {
                    GetIdentityField = "StockInID",
                    PrimaryField = "StockInID",
                    PrimaryFieldValue = PrimaryFieldValue.ToString(),
                    TableName = "TblStockInDetail",
                    Fields = new
                    {
                        SystemID = html.SystemID,
                        WarehouseID = html.WarehouseID,
                        ItemID = "$(\"#ItemID\").val()",
                        ItemName = "$(\"#iName\").val()",
                        //StockInID = "0",
                        UM = "$(\"#Txt_UM\").val()",
                        CostPrice = "$(\"#Txt_Cost_Price\").val()",
                        QTY = "1",
                        Type = "'Stock In'",
                        Reason = "$(\"#Reason\").val().toString()",
                        StockInDate = "'" + html.GetCurrentDate() + "'",
                        //StockOutDate = "'" + html.GetCurrentDate() + "'",
                        CustomerID = "$(\"#Txt_Customer_ID\").val()",
                        StatusID = "83", //Return Stock Status
                        LocationID = "$(\"#cbo_Warehouse_Name\").val().toString()",
                        UnitPrice = "0",//"$(\"#Txt_Unit_Price\").val()",
                        ClassNameID = "$(\"#cbo_ClassName\").val().toString()",
                    },
                });
            //html.insTable.Add(
            //    new InsertTable()
            //    {
            //        TableName = "TblStockOut",
            //        Fields = new
            //        {
            //            SystemID = html.SystemID,
            //            WarehouseID = html.WarehouseID,
            //            CustomerID = "$(\"#Txt_Customer_ID\").val()",
            //            StockInID = "$(\"#Txt_Stock_In_ID\").val()",
            //            ItemID = "$(\"#cboItemName\").val().toString()",
            //            UM = "$(\"#Txt_UM\").val()",
            //            QTY = "1",
            //            CostPrice = "$(\"#Txt_Cost_Price\").val()",
            //            UnitPrice = "0",//"$(\"#Txt_Unit_Price\").val()",
            //            DiscountP = "0",
            //            DiscountUSD = "0",
            //            Amount = "0",//"$(\"#Txt_Unit_Price\").val()",
            //            StatusID = "80",
            //            InvoiceNumber = "''",
            //            ConnectionID = "$(\"#CurrentConnectionID\").val()",
            //            LocationID = "$(\"#cbo_Warehouse_Name\").val().toString()",
            //            Type = "'Stock Out'",
            //            ClassNameID = "$(\"#cbo_ClassName\").val().toString()",
            //            IsIncludedVAT = "0",
            //            DateOut = "'" + html.GetCurrentDate() + "'",
            //        },
            //        GetIndetityFiled = "StockInDetailID",
            //        PrimaryField = "StockOutID",
            //        PrimaryFieldValue = PrimaryFieldValue.ToString(),
            //    });
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public ActionResult Products(int CustomerID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true
            };
            html.GH.GridPara = new GridPara[]
            {
                new GridPara() { ParaName="FromDate", Value="" },
                new GridPara() { ParaName="ToDate", Value="" },
                new GridPara() { ParaName="CustomerName", Value="" },
                new GridPara() { ParaName="SystemID", Value=html.SystemID },
                new GridPara() { ParaName="WarehouseID", Value=html.WarehouseID },
                new GridPara() { ParaName="WarehouseName", Value="" },
                new GridPara() { ParaName="ItemName", Value="" },
                new GridPara() { ParaName="CustomerID", Value=CustomerID.ToString() },
            };
            html.GH.GridProcedureName = "SP_SHOW_STOCK_OUT";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 7, DisplayOneRow= true },
                new GridField() { Header = "Date", FieldName = "DateOut", Format="dd-MM-yyyy"  },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNumber" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "QTY", FieldName = "QTY", TextAlign="right", Format="#,#0" },
                new GridField() { Header = "Amount USD", FieldName = "SalePrice", TextAlign="right", Format="#,#0.00" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult DeviceInfo(int CustomerID)
        {
            HTML html = new HTML();
            bool ChangeDevice = HttpContext.GetOwinContext().Request.User.IsInRole("Change Device");
            ViewBag.ChangeDevice = ChangeDevice;
            bool ReceiveReturnDevice = HttpContext.GetOwinContext().Request.User.IsInRole("Receive Return Device");
            ViewBag.ReceiveReturnDevice = ReceiveReturnDevice;
            string sql = @"select ac.ConnectionID, ac.[SID], ac.ConnectionType, ce.CreateDate, ce.ItemName,
            ce.MACAddress, ce.SerialNo, ce.[Status], ce.Reason, ce.Remark,
            r.RouterName, ac.ConnectionType, ac.VLanName, ac.VLanIPAddress,
            ac.IPAddress, ac.GetwayIP, ac.SubnetIP, ac.DNS1, ac.DNS2, ac.PPPoEProfileName,
            ac.PPPoEUserName, ac.PPPoEPassword, ac.DeviceName, ac.WifiIP, ac.WifiName,
            ac.WifiKey, ac.WifiLoginName, ac.WifiLoginPassword, ce.NSM, ce.Customer
            from dbo.TblCustomerEquipment ce
            inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ce.ActivateCustomerID
            inner join dbo.TblRouter r on r.RouterID = ac.RouterID
            where ac.ConnectionID in (select distinct bi.ConnectionID from dbo.TblCustomerBillingInvoice bi where isnull(bi.StatusID,0) <> 12 
            and bi.CustomerID=@CustomerID)
            and ac.CustomerID = @CustomerID order by ac.ConnectionID desc;";
            Dictionary<string, string> para = new Dictionary<string, string> {
                {"CustomerID",CustomerID.ToString() }
            };
            ViewBag.Data = JsonConvert.DeserializeObject(html.JsonData(sql, false, para, true)).ToString().Replace("`", string.Empty);
            return View();
        }

        public ActionResult CustomerService(int CustomerID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true
            };
            html.GH.GridProcedureName = "SP_SEARCH_CUSTOMER_CONNECTION_FOR_SALE_ORDER";
            html.GH.GridPara = new GridPara[]
            {
                new GridPara() { ParaName="FromDate", Value="" },
                new GridPara() { ParaName="ToDate", Value="" },
                new GridPara() { ParaName="ConnectivityStatus", Value="" },
                new GridPara() { ParaName="PackageName", Value="" },
                new GridPara() { ParaName="CustomerCode", Value="" },
                new GridPara() { ParaName="CustomerName", Value="" },
                new GridPara() { ParaName="CompanyContactMobileNumber", Value="" },
                new GridPara() { ParaName="SID", Value="" },
                new GridPara() { ParaName="InstallAddress", Value="" },
                new GridPara() { ParaName="ConnectionID", Value="" },
                new GridPara() { ParaName="SystemID", Value=html.SystemID },
                new GridPara() { ParaName="WarehouseID", Value=html.WarehouseID },
                new GridPara() { ParaName="CustomerID", Value=CustomerID.ToString() },
            };
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "CID", FieldName = "CustomerCode", IsGroupByField = true, GroupLabel="CID: ", Colspan=9 },
                //new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel="Name: ", Colspan=9 },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", IsGroupByField = true, GroupLabel="Register Date: ", Colspan=10 },
                new GridField() { Header = "Register By", FieldName = "RegisterBy", IsGroupByField = true, GroupLabel="Register By: ", Colspan=9 },
                new GridField() { Header = "Count Connection", FieldName = "CountConnection", IsGroupByField = true, GroupLabel="Connection: ", Colspan=10 },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-red", TextAlign="center" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate", ClassName="text-red" },
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", ClassName="text-red" },
                new GridField() { Header = "InstallAddress", FieldName = "InstallAddress" }
            };
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            //html.GridEventClick = "ShowCustomerInfo(this)";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public JsonResult AlertCustConInfo(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select BillingInvoiceID,
            ConnectionID, [SID], PaymentDueDate,
            'Connection ID: ' + ConnectionID + ' will be down speed to 3M on date: '+ format(dateadd(d,-3, PaymentDueDate), 'dd-MM-yyyy') + ' due to not yet payment.' Comment
            from dbo.TblCustomerBillingInvoice
            where isnull([SID], '') <> '' and (DATEDIFF(D, getdate(), PaymentDueDate)-3) = 1 and CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            return Json(new { Data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet); ;
        }

        [Authorize(Roles = "Add Customer, Edit Customer, Delete Customer")]

        public ActionResult CustomerInfo()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers",
                isShowLoadingGrid = false
            };
            if (html.HasPermission("Add Customer"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Customer"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 600;
            html.AddButton = "Register New Customer";
            html.AddURL = "AddCustomer/CreateCustomer";
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                //DateTimePickerWithRanges = true,
                DateRangePicker = true,
                LabelFromDate = "Register Date:",
                DBField = "c.RegisterDate",
                DefaultValueDate = false

            });
            //sql = "select distinct s.StatusID, s.[Status] from dbo.TblCustomer c ";
            //sql += "inner join dbo.TblStatus s on s.StatusID = c.StatusID ";
            //sql += "where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by s.StatusID";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerType",
                Label = "Customer Type:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = @"select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct
inner join dbo.TblCustomer c on c.CustomerTypeID = ct.CustomerTypeID
where ct.SystemID = " + html.SystemID + " and ct.WarehouseID = " + html.WarehouseID + @"
order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "c.CustomerGroupID",
                SQL_CBO = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                DBField = "c.CustomerName",
                SQLAutoComplete = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerName",
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            //radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "Customer Name",
            //    SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerName",
            //    RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
            //    ISAutoComplete = true
            //};
            //htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Phone Number",
                SQL = "select distinct top 10 CompanyContactMobileNumber from dbo.TblCustomerCompany where lower(CompanyContactMobileNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CompanyContactMobileNumber",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Address",
                SQL = "select distinct top 10 CustomerAddress from dbo.TblCustomer where lower(CustomerAddress) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerAddress",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Commune",
                SQL = "select distinct top 10 CustomerCommune from dbo.TblCustomer where lower(CustomerCommune) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCommune",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "District",
                SQL = "select distinct top 10 CustomerDistrict from dbo.TblCustomer where lower(CustomerDistrict) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerDistrict",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Province/City",
                SQL = "select distinct top 10 CustomerProvinceCity from dbo.TblCustomer where lower(CustomerProvinceCity) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerProvinceCity",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.GridEventClick = "onGridClick_CustInfo(this);";
            //html.GridHeight = "400px";
            //html.DefaultSearch = false;
            html.ReportTitle = "List Customers Information";
            html.TableName = "TblCustomer";

            //string sqlLocation = "select distinct CustomerProvinceCity from dbo.TblCustomer where lower(CustomerProvinceCity) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by CustomerProvinceCity";
            //HtmlAddControls htmlAddControl = new HtmlAddControls()
            //{
            //    ID = "txtLocation",
            //    Label = "Location:",
            //    DBField = "Location",
            //    SQLAutoComplete = sqlLocation,
            //    ISAutoComplete = true
            //};
            //htmlAddControls.Add(htmlAddControl);
            string sql = @"select ROW_NUMBER() over(order by cg.CustomerGroup) as [No], c.CustomerID, 
            c.CustomerCode, c.CustomerName, c.CustomerNationality, c.CustomerContactMobileNumber PhoneNumber,
            c.RegisterDate, c.RegisterBy, c.CustomerAddress, c.CustomerCommune, c.CustomerDistrict, 
            c.CustomerProvinceCity, cg.CustomerGroup + ' (<span class=""text-danger"">' +
convert(varchar, (select distinct count(cc.CustomerGroupID) from dbo.TblCustomer cc where cc.CustomerGroupID = cg.CustomerGroupID))+'</span>)' CustomerGroup,
ct.CustomerType + ' (<span class=""text-danger"">' +
convert(varchar, (select distinct count(cc.CustomerTypeID) from dbo.TblCustomer cc where cc.CustomerTypeID = ct.CustomerTypeID))+'</span>)' CustomerType
            from dbo.TblCustomer c
            inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerType ct on c.CustomerTypeID = ct.CustomerTypeID
            where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.OrderBy = "cg.CustomerGroup, ct.CustomerType, c.CustomerCode";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", GroupLabel="Group Name: ", IsGroupByField= true, Colspan=11, DisplayOneRow = true },
                new GridField() { Header = "Customer Type", FieldName = "CustomerType", GroupLabel="Customer Type: ", IsGroupByField= true, Colspan=11 },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone Number", FieldName = "PhoneNumber" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-danger", Format="dd/MM/yyyy" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "Address", FieldName = "CustomerAddress" },
                new GridField() { Header = "Commune", FieldName = "CustomerCommune", },
                new GridField() { Header = "District", FieldName = "CustomerDistrict" },
                new GridField() { Header = "Province/City", FieldName = "CustomerProvinceCity" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public ActionResult DepositList(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ConnectionID) as [No], DepositFeeID,
ConnectionID, '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+ convert(varchar, CustomerID) +', '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +',10,'''',false,this);"">'+ InvoiceNo+'</a>' InvoiceNo, DepositFee, CreateDate [Date], Memo, '<a href=""javascript:void(0);"" onclick=""ReturnDeposit('+convert(varchar, CustomerID)+');"">Return Deposit</a>' Action
from dbo.TblCustomerDeposit
where CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.ReportTitle = "Deposit Fee List";
            html.ShowGridOnly = true;
            html.OrderBy = "ConnectionID";
            html.TblID = "TableDepositFee";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date", Format="dd-MM-yyyy"  },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Deposit Fee", FieldName = "DepositFee",
                    TextAlign="right", Format="#,#0.00",
                IsTotalFooter=true, Colspan = 4, TotalLabel="Total:"},
                new GridField() { Header = "Action", FieldName = "Action", HideOnPrint = true },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadDepositFee");
            return View();
        }

        public ActionResult ReturnDeposit(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select c.CustomerID, c.CustomerCode + ' - ' + c.CustomerName CustomerName
from dbo.TblCustomer c where c.CustomerID = @CustomerID and c.SystemID = @SystemID and c.WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>()
            {
                { "CustomerID",CustomerID.ToString() },
                { "SystemID",html.SystemID },
                { "WarehouseID",html.WarehouseID },
            };
            DataSet ds = html.LoadData(sql, false, para);
            string CustomerName = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
            }
            html.FormID = "FrmCustomerReturnDeposit";
            html.SaveChangeFunction = "SaveChangeVendorPayment()";
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Return Deposit",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "Uploads/Vendors/Payment",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Pay To:",
                            ID = "Payee",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 c.CustomerID, c.CustomerCode + ' - ' + c.CustomerName CustomerName from dbo.TblCustomerInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where isnull(bi.DepositFee,0) <> 0 and bi.SystemID = "+ html.SystemID + " and bi.WarehouseID = " + html.WarehouseID,
                            HiddenField="CustomerID",
                            Value =CustomerName,
                            HiddenFieldValue = CustomerID.ToString(),
                            //ISCallBack = true,
                            //SQLCallBack="select v.VendorName ReceivedFromName from dbo.TblBills b inner join dbo.TblVendor v on v.VendorID = b.VendorID where lower(v.VendorCode + ' - ' + v.VendorName) = @SearchText and b.SystemID =" + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
                            //DisplayOnFields="ReceivedFromName"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Reference#:",
                            ID = "Reference",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Return Date:",
                            ID = "ReturnDate",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            SingleDatePicker = true,
                            Required = true
                        },
                    }
                },
                new  FormData() {

                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Payment Account:", ID="ChartOfAccountID",
                            Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            AddNewFuntionName="AddNewChartOfAccount('ChartOfAccountID')",
                            SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            DisplayText="AccountName",
                            Value="ChartOfAccountID",
                            GroupByField="AccountType",
                        },
                        new HtmlComboBox()
                        {
                            Label = "Payment Method:",
                            ID = "PaymentModeID",
                            Required = true,
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by case when PaymentMethod = 'Cash' then 0 else 1 end, PaymentMethod",
                            DisplayText = "PaymentMethod",
                            Value = "PaymentMethodID", AddNewFuntionName="AddPaymentMethod('PaymentModeID')"
                        },
                        new HtmlComboBox()
                        {
                            Label = "Category:",
                            ID = "PaymentCategoryID",
                            Required = true,
                            IsCustomForm = true,
                            SQL_CBO = "select PaymentCategoryID, Category from dbo.TblPaymentCategory " +
                            "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Category",
                            DisplayText = "Category", Value = "PaymentCategoryID",
                            AddNewFuntionName="AddPaymentCategory('PaymentCategoryID')"
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "From Bank Name:",
                            ID = "FromBankName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Account Number:",
                            ID = "FromAccountNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Account Name:",
                            ID = "FromAccountName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        }

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Cheque Number:",
                            ID = "ChequeNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "From Phone Number:",
                            ID = "FromPhoneNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "To Phone Number:",
                            ID = "ToPhoneNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Description:",
                            ID = "Description",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Received From Name:",
                            ID = "ReceivedFromName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            //Value = ReceivedFromName,
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "Paid By:",
                            ID = "PaidBy",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Value = html.UserName,
                            Required = true
                        }
                    }
                },
                new  FormData() {
                    FormTitle="Transaction Details",
                    htmlControl = new HTMLControl[]
                    {
                       new HtmlLabel()
                       {
                           Label="<div id=\"divTblTransactionDetails\" class=\"col-12 table-responsive\"></div>"
                       }
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult LoadDepositList(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ConnectionID) as [No], DepositFeeID,
ConnectionID, '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+ convert(varchar, CustomerID) +', '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +',10,'''',false,this);"">'+ InvoiceNo+'</a>' InvoiceNo, DepositFee, CreateDate [Date], Memo, '<a href=""javascript:void(0);"" onclick=""ReturnDeposit('+convert(varchar, CustomerID)+');"">Return Deposit</a>' Action
from dbo.TblCustomerDeposit
where CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.ShowPrintOnTopTable = false;
            //html.ReportTitle = "Deposit Fee List";
            html.ShowGridOnly = true;
            html.OrderBy = "ConnectionID";
            html.TblID = "TableLoadDepositFee";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date", Format="dd-MM-yyyy"  },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Deposit Fee", FieldName = "DepositFee",
                    TextAlign="right", Format="#,#0.00",
                IsTotalFooter=true, Colspan = 4, TotalLabel="Total:"},
                new GridField() { Header = "Action", FieldName = "ConnectionID", DisplayCheckBox = true },
            };
            string DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "Load_Deposit_Fee");
            return Json(new { Data = DrawHTMLSearchForm }, JsonRequestBehavior.AllowGet);
        }
    }
}