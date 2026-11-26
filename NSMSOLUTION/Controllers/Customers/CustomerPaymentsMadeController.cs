using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class CustomerPaymentsMadeController : Controller
    {
        // GET: CustomerPaymentsMade
        public ActionResult Index(string PrimaryFieldValue)
        {
            int CustomerID = int.Parse(PrimaryFieldValue);
            string CustomerName = "", CustomerCode = "";
            HTML html = new HTML();
            string sql = @"select CustomerID, CustomerCode, CustomerName from dbo.TblCustomer where CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            string sqlpayto = "select AccountID, (AccountTitle + case when AccountNumber <> '' then ' - ' + AccountNumber else '' end";
            sqlpayto += "+ case when AccountName <> '' then ' - ' + AccountName else '' end) AccountName from dbo.TblAccount ";
            sqlpayto += "where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by case when AccountTitle = 'Cash' then 0 else 1 end, AccountName";
            DataSet ds = html.LoadData(sql);

            sql = "select PaymentAccountID, PaymentMethodID, Category, PayTo from TblSetDefaultCustomerPayment ";
            sql += "where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            DataSet dsp = html.LoadData(sql);
            DataRow dr = null;
            if (dsp.Tables[0].Rows.Count > 0)
                dr = dsp.Tables[0].Rows[0];

            string ReceivedFromName = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                CustomerCode = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
                CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
                CustomerID = int.Parse(ds.Tables[0].Rows[0]["CustomerID"].ToString());
                ReceivedFromName = CustomerName;
            }
            ViewBag.CustomerCode = CustomerCode;
            ViewBag.CustomerName = CustomerName;

            html.FormID = "PaymentInvoice";
            html.SaveChangeFunction = "SaveChangeCustomerPayment()";
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Record Payment",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Customers/Payment",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "StatusID:",
                            ID = "StatusID",
                            Class="input2 CustInfo",
                            IsCustomForm = true, Hide = true,
                           Value = "10"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Payee:",
                            ID = "Payee",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete="select top 10 CustomerID, CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            HiddenField="CustomerID",
                            Value =CustomerName,
                            HiddenFieldValue = CustomerID.ToString()
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
                            Label = "Payment Date:",
                            ID = "PaymentDate",
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
                            CboValue = dr["PaymentAccountID"].ToString(),
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
                            Value = "PaymentMethodID", AddNewFuntionName="AddPaymentMethod('PaymentModeID')",
                            CboValue = dr["PaymentMethodID"].ToString(),
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
                            AddNewFuntionName="AddPaymentCategory('PaymentCategoryID')",
                            CboValue = dr["Category"].ToString(),
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
                            Value = ReceivedFromName,
                            Hide = true
                        },
                        new HtmlComboBox()
                        {
                            Label = "Pay To:",
                            ID = "ToAccountNumber",
                            DBField = "ToAccountNumber",
                            Required = true,
                            SQL_CBO = sqlpayto,
                            Value = "AccountID",
                            DisplayText = "AccountName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                           CboValue = dr["PayTo"].ToString(),
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
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextarea()
                        {
                            ID = "Memo",
                            DivClassCol="col-sm-6",
                            IsCustomForm = true,
                            placeholder="Notes (Internal use. Not visible to customer)",
                            DivClass="noborder", Style="style=\"height: 150px;\"",
                            Hide = true
                        },
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult TransactionDetails(int CustomerID)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a name=\"APreviewInv\" href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + CustomerID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, InvoiceNo, InvoiceDate, ";
            SQL += "case when DATEDIFF(day, InvoiceDate, GETDATE()) > 0 then DATEDIFF(day, InvoiceDate, GETDATE()) else 0 end [Day], PaymentDueDate, SubTotal, VAT, Total, DiscountP, [Type],";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a name=\"Payment\" href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(Unpaid,'#,#.00')+''',10, '+ convert(varchar, CustomerID) +','''+[Type]+''');\">Pay Now</a>' Payment ";
            SQL += ", format(PaidDate, 'dd-MM-yy HH:mm:ss tt') PaidDate, ConnectionID, isnull(BillingInvoiceID, 0) BillingInvoiceID, IsIncludedVAT from dbo.TblCustomerBalance where CustomerID = " + CustomerID + " and StatusID = 11"; //11 = unpaid invoice
            html.GH.SQL = SQL;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustInvoice";
            html.isShowLoadingGrid = false;
            html.ShowPrintOnTopTable = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Invoice#", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", TextAlign = "center", Format="dd-MM-yyyy" },
                new GridField() { Header = "IsIncludedVAT", FieldName = "IsIncludedVAT", IsHiddenField = true },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", TextAlign = "center", Format="dd-MM-yyyy" },
                new GridField() { Header = "Paid Date", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                //new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Amount Due", FieldName = "Unpaid", TextAlign = "right" },
                new GridField() { Header = "Payment Amount", FieldName = "Unpaid",
                    IsTotalFooter=true, Colspan = 7, TotalLabel="Total Balance:", Format="#,#.00",
                    TextAlign = "right", DisplayTextBox = true,
                },
                new GridField() { Header = "Action", FieldName = "BillingInvoiceID", DisplayCheckBox = true }
            };
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadCustomerPaymentMade") }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveChangePayment(object[] PaymentData)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PaymentData[0].ToString());
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            StringBuilder sb = new StringBuilder();
            string CustomerID = DataInfo[0]["CustomerID"];
            string sql = "select CustomerCode, CustomerName from dbo.TblCustomer where CustomerID = @CustomerID and SystemID = @SystemID and WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            DataSet ds = new DataSet();
            ds = html.LoadData(sql, false, para);
            DataRow drdatarow = ds.Tables[0].Rows[0];
            string CustomerName = drdatarow["CustomerName"].ToString(); //HttpUtility.UrlDecode(DataInfo[0]["CustomerName"]);
            string CID = drdatarow["CustomerCode"].ToString(); //HttpUtility.UrlDecode(DataInfo[0]["CustomerCode"]);
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/Payment AttFile/";
            SqlResult result = new SqlResult();
            result = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (result.Result == ResultType.Successful)
                AttachmentFile = result.Value;
            else
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result
                }, JsonRequestBehavior.AllowGet);
            }
            sb.AppendLine("<Data>");
            foreach (dynamic lists in DataInfo)
            {
                sb.AppendLine("<PaymentInfo>");
                foreach (dynamic item in lists)
                {
                    string key = item.Key;
                    string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                    if (key == "PaymentDate")
                        value = html.StringToDate(value);
                    sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
                }
                sb.AppendLine("<PaidBy>" + html.UserName + "</PaidBy>");
                sb.AppendLine("<SystemID>" + html.SystemID + "</SystemID>");
                sb.AppendLine("<WarehouseID>" + html.WarehouseID + "</WarehouseID>");
                sb.AppendLine("<DestinationFolder>" + DestinationFolder + "</DestinationFolder>");
                sb.AppendLine("<AttachmentFile>" + AttachmentFile + "</AttachmentFile>");
                sb.AppendLine("</PaymentInfo>");
            }
            sb.AppendLine("</Data>");
            para = new Dictionary<string, string>()
            {
                { "Data", sb.ToString() }
            };
            SqlResult sqlresult = html.SaveChangeData("SP_INSERT_PAYMENT_ALL", true, para, true, true);
            string OfficialReceiptData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
            foreach (DataRow drdata in sqlresult.Data.Tables[0].Rows)
            {
                string InvoiceNo = drdata["InvoiceNo"].ToString();
                if (sqlresult.Result == ResultType.Successful)
                {
                    sql = "select b.CustomerID, b.BillingInvoiceID, b.InvoiceType, b.PPPoEUserName, b.ConnectivityStatus, b.StartDate, b.EndDate, b.Duration, isnull(b.IsAlreadyApplyProfile, 0) IsAlreadyApplyProfile, b.ActivateCustomerID, b.IsChangePlan, case when b.invTmp = 1 then b.PackageName else REPLACE(b.ServiceName, s.ServiceName+' - ', '') end PackageName  ";
                    sql += "from dbo.TblCustomerBillingInvoice b ";
                    sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                    sql += "where b.InvoiceNo = N'" + InvoiceNo + "' and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID;
                    ds = html.LoadData(sql);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            CustomerID = dr["CustomerID"].ToString();
                            string InvoiceType = dr["InvoiceType"].ToString();
                            string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                            string StartDate = dr["StartDate"].ToString();
                            string EndDate = dr["EndDate"].ToString();
                            string Duration = dr["Duration"].ToString();
                            string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                            string IsAlreadyApplyProfile = dr["IsAlreadyApplyProfile"].ToString();
                            string IsChangePlan = dr["IsChangePlan"].ToString();
                            string PackageName = dr["PackageName"].ToString();
                            if (InvoiceType == "Renew Invoice" && IsAlreadyApplyProfile == "False")
                            {
                                sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate, [SID], ActivateDate from dbo.TblActivateCustomer where ";
                                sql += "ActivateCustomerID =" + ActivateCustomerID;
                                ds = html.LoadData(sql);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    string ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                                    string ActivateDate = ds.Tables[0].Rows[0]["ActivateDate"].ToString();
                                    string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                                    string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                                    string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                                    string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                                    string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                                    string PPPoEProfileName = ds.Tables[0].Rows[0]["PPPoEProfileName"].ToString();
                                    string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                                    string Comment = "Customer Renew Services With Profile Name: " + PPPoEProfileName;
                                    string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                                    string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();
                                    //if (RenewDate != "")
                                    //{
                                    //    if (DateTime.Parse(RenewDate).ToString("yyyyMM") != easternTimeNow.ToString("yyyyMM"))
                                    //    {
                                    if (ConnectivityStatus != "42")
                                    {
                                        Mikrotik MK = new Mikrotik();
                                        MK.SetConnection(int.Parse(RouterID));
                                        if (ConnectionType == "PPPoE")
                                        {
                                            if (ConnectivityStatus == "50")
                                            {
                                                MK.ClearProfileName(PPPoEUserName);
                                                Comment = "Customer Services was reverse to Profile Name: " + PPPoEProfileName + " Date:" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserName;
                                            }
                                            if (ConnectivityStatus == "41")
                                            {
                                                Comment = "Customer Services was re-activated on " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserName;
                                                MK.ReActivateUser(PPPoEUserName, Comment);
                                            }
                                            if (IsChangePlan == "False" || IsChangePlan == "")
                                                sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                            else
                                            {
                                                DateTime SDate = DateTime.Parse(StartDate);
                                                if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                {
                                                    Comment = "Customer Change Plan From: <b>" + PPPoEProfileName + "</b> TO <b>" + PackageName + "</b>; Apply Changed by: <b>" + html.UserName + "</b>; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                                                    PPPoEProfileName = PackageName;
                                                    MK.ClearProfileName(PPPoEUserName);
                                                    sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Comment = "Customer Renew Services With Vlan Name: " + VLanName;
                                            sqlresult = MK.EnableVlanID(VLanID, Comment);
                                        }
                                    }
                                    if (sqlresult.Result == ResultType.Successful)
                                    {
                                        string ConnectionID;
                                        if (ConnectivityStatus != "42")
                                        {
                                            sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ConnectionType = '" + ConnectionType + "', RenewBy = '" + html.UserName + "', RenewDate = '" + html.GetCurrentDate() + "', [ExpireDate] = '" + EndDate + "',";
                                            sql += "RouterID=" + RouterID + ", [SID] = '" + ServiceID + "', PPPoEPassword='" + PPPoEPassword + "', ActivateDate ='" + ActivateDate + "',ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                            sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active', RenewBy = '" + html.UserName + "', RenewDate = '" + html.GetCurrentDate() + "' ";
                                            sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                            if (IsChangePlan == "False" || IsChangePlan == "")
                                            {
                                                sql += "update dbo.TblActivateCustomer set BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                sql += ";";
                                                sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, ConnectionType, ";
                                                sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                sql += "select ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                                sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, ac.ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                //sql += "inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID ";
                                                sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;

                                                //send notification to telegram when customer services is renew
                                                sql = "select IsEnable from TblTelegramNotification where IsEnable = 1 and StatusID = 44 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                                                ds = html.LoadData(sql);
                                                if (ds.Tables[0].Rows.Count > 0)
                                                {
                                                    sql = "select c.CustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber, ";
                                                    sql += "(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM + ";
                                                    sql += "case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber, ";
                                                    sql += "format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate ";
                                                    sql += ", ta.ConnectionType, ta.VLanID from dbo.TblActivateCustomer ta ";
                                                    sql += "inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID ";
                                                    sql += "inner join dbo.TblServices s on s.ServiceID = ta.ServiceID ";
                                                    sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID ";
                                                    sql += "inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID ";
                                                    sql += "where ta.ActivateCustomerID=@ActivateCustomerID and ta.ConnectivityStatus = 40 and b.StatusID not in(12,13) and format(b.RenewDate, 'yyyyMMdd') = @DateNow";
                                                    para = new Dictionary<string, string>
                                                    {
                                                        { "DateNow", easternTimeNow.ToString("yyyyMMdd") },
                                                        { "ActivateCustomerID", ActivateCustomerID }
                                                    };
                                                    ds = html.LoadData(sql, false, para);

                                                    if (ds.Tables[0].Rows.Count > 0)
                                                    {
                                                        foreach (DataRow drow in ds.Tables[0].Rows)
                                                        {
                                                            CustomerID = drow["CustomerID"].ToString();
                                                            ConnectionID = drow["ConnectionID"].ToString();
                                                            string SID = drow["SID"].ToString();
                                                            CID = drow["CID"].ToString();
                                                            string CName = drow["CName"].ToString();
                                                            string CompanyContactMobileNumber = drow["CompanyContactMobileNumber"].ToString();
                                                            string TicketNumber = drow["TicketNumber"].ToString();
                                                            CustomerName = drow["CustomerName"].ToString();
                                                            PackageName = drow["PackageName"].ToString();
                                                            StartDate = drow["StartDate"].ToString();
                                                            EndDate = drow["EndDate"].ToString();
                                                            InvoiceNo = drow["InvoiceNo"].ToString();
                                                            RouterID = drow["RouterID"].ToString();
                                                            string PaymentDueDate = drow["PaymentDueDate"].ToString();

                                                            string msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកបានបន្តការប្រើប្រាស់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»\n\n";
                                                            msg += "ឈ្មោះអតិថិជន: " + CName + "\n";
                                                            msg += "លេខសំគាល់អតិថិជន: " + CID + "\n";
                                                            msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName + "</b>\n\n";
                                                            msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                                                            msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n";
                                                            msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                                                            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                                                            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                                                            msg += "\nសូមអរគុណ!";
                                                            CompanyContactMobileNumber = CompanyContactMobileNumber.Trim().Replace(" ", string.Empty);
                                                            var rg_ex = new Regex(Regex.Escape("0"));
                                                            CompanyContactMobileNumber = rg_ex.Replace(CompanyContactMobileNumber, "+855", 1);
                                                            notificationTelegram.NotificationToCustomer(CompanyContactMobileNumber, CustomerName, CID, int.Parse(CustomerID), msg);
                                                        }
                                                    }
                                                }
                                                //end send notification to telegram when customer services is renew
                                            }
                                            else
                                            {
                                                DateTime SDate = DateTime.Parse(StartDate);
                                                if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                {
                                                    sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                    sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                    sql += ";";
                                                    sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                                    sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                    sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                    sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                                    sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, ac.ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                    sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                    sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                    sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                    sqlresult = html.SaveChangeData(sql, false, false);
                                                    sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                                                    ds = html.LoadData(sql);
                                                    DataRow dr_data = ds.Tables[0].Rows[0];
                                                    string Ticket_Number = dr_data["TicketNumber"].ToString();
                                                    string Comment_s = @"Dear Team, <br><br>" + Comment + ".";

                                                    Comment_s += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                                                    sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                                    Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                                    ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                                    select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                                    OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                                    N'" + Comment_s + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                                    '' DescriptionChangeStatus
                                                    from dbo.TblSupportOpenTicket
                                                    where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                                                    para = new Dictionary<string, string>
                                                    {
                                                        { "TicketNumber", Ticket_Number },
                                                        { "SystemID", html.SystemID },
                                                        { "WarehouseID", html.WarehouseID }
                                                    };
                                                    html.SaveChangeData(sql, false, para);
                                                    notificationTelegram.NotificationTelegramBot((Comment_s + " " + Ticket_Number).Replace("<br>", "\n"));
                                                    notificationTelegram.NotificationTelegramBot((Comment_s + " " + Ticket_Number).Replace("<br>", "\n"), -635916546);//635916546 = test send finance group                                                    
                                                }
                                            }
                                        }
                                        sql = "select top 1 cb.GrandTotal Amount, s.ServiceName, c.CustomerCode FirstName, c.CustomerName LastName, bi.CompanyEmail Email, bi.CompanyContactMobileNumber Phone, bi.InstallProvinceCity [Location], 'Activation by: " + html.UserName + "; Activate Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "' Comment ";
                                        sql += ", b.PDFFileName, b.DestinationFolder, case when b.invTmp = 1 then b.ServiceName + ' - ' + b.PackageName else b.ServiceName end PackageName, ";
                                        sql += "format(b.StartDate, 'dd-MMM-yyyy') StartDate, format(b.EndDate, 'dd-MMM-yyyy') EndDate, b.ConnectionID, b.[SID] ";
                                        sql += "from dbo.TblCustomer c ";
                                        sql += "inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID ";
                                        sql += "inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = bi.CustomerID and b.CustomerBillInformationID = bi.CustomerBillInformationID ";
                                        sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                                        sql += "inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = b.InvoiceNo ";
                                        sql += "where b.InvoiceNo = '" + InvoiceNo + "' and c.CustomerID = " + CustomerID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
                                        ds = html.LoadData(sql);
                                        ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                                        ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
                                        string ServiceName = ds.Tables[0].Rows[0]["ServiceName"].ToString();
                                        string FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                        string LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                                        string Email = ds.Tables[0].Rows[0]["Email"].ToString();
                                        string Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                                        string Location = ds.Tables[0].Rows[0]["Location"].ToString();
                                        string PDFFileName = ds.Tables[0].Rows[0]["PDFFileName"].ToString();
                                        DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                                        PackageName = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                        StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                        EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                        string Amount = ds.Tables[0].Rows[0]["Amount"].ToString();
                                        //Comment = ds.Tables[0].Rows[0]["Comment"].ToString();
                                        StringBuilder MsgBody = new StringBuilder();
                                        string Subject = "YOUR " + ServiceName.ToUpper() + " IS RENEW ON DATE: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt");
                                        MsgBody.AppendLine(Subject);
                                        MsgBody.AppendLine("\nDear " + LastName + " [" + FirstName + "],\n");
                                        MsgBody.AppendLine("Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n");
                                        MsgBody.AppendLine("We would like to confirm your service is renew already on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                        MsgBody.AppendLine("\nYour current service: " + ServiceName);
                                        MsgBody.AppendLine("Package Name: " + PackageName);
                                        MsgBody.AppendLine("Service ID: " + ServiceID);
                                        MsgBody.AppendLine("Connection ID: " + ConnectionID);
                                        MsgBody.AppendLine("Start Date: " + StartDate);
                                        MsgBody.AppendLine("Expired Date: " + EndDate);
                                        MsgBody.AppendLine("Amount USD: " + Amount);
                                        MsgBody.AppendLine("Phone Number: " + Phone);
                                        if (Email != "")
                                            MsgBody.AppendLine("Email: " + Email);
                                        MsgBody.AppendLine("Renew By: " + html.UserName);
                                        MsgBody.AppendLine("\nThank you and best regards,\n");
                                        MsgBody.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                                        MsgBody.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                                        MsgBody.AppendLine("Customer Service: 086 588 777, 078 588 777");
                                        MsgBody.AppendLine("Website: nsmisp.com.kh");
                                        Phone = Phone.Trim().Replace(" ", string.Empty);
                                        var regex = new Regex(Regex.Escape("0"));
                                        Phone = regex.Replace(Phone, "+855", 1);

                                        //notificationTelegram.NotificationToCustomer(Phone, FirstName, LastName, int.Parse(CustomerID), MsgBody.ToString(), DestinationFolder, PDFFileName);
                                        MsgBody.Clear();

                                        MsgBody.AppendLine("Customer Name: " + LastName.ToUpper() + " [" + FirstName.ToUpper() + "] IS RENEW SUCCESSFUL.");
                                        MsgBody.AppendLine("\nDear TEAM,\n");
                                        MsgBody.AppendLine("We would like to confirm Customer Name: " + LastName.ToUpper() + "[" + FirstName.ToUpper() + "] is renew on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                        MsgBody.AppendLine("\nService ID: " + ServiceID);
                                        MsgBody.AppendLine("Connection ID: " + ConnectionID);
                                        MsgBody.AppendLine("\nServices: " + ServiceName);
                                        MsgBody.AppendLine("Package Name: " + PackageName);
                                        MsgBody.AppendLine("Start Date: " + StartDate);
                                        MsgBody.AppendLine("Expired Date: " + EndDate);
                                        MsgBody.AppendLine("Amount USD: " + Amount);
                                        MsgBody.AppendLine("Phone Number: " + Phone);
                                        if (Email != "")
                                            MsgBody.AppendLine("Email: " + Email);
                                        MsgBody.AppendLine("Renew By: " + html.UserName);
                                        //notificationTelegram.NotificationTelegramBot(MsgBody.ToString());                                        
                                    }

                                    //sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                                    //ds = html.LoadData(sql);
                                    //DataRow drdata = ds.Tables[0].Rows[0];
                                    ////string ConType = drdata["ConnectionType"].ToString();
                                    ////string PPPoEUserName = dr["PPPoEUserName"].ToString();
                                    ////string PPPoEPassword = dr["PPPoEPassword"].ToString();
                                    ////string VLanName = dr["VLanName"].ToString();
                                    ////string VLanIPAddress = dr["VLanIPAddress"].ToString();
                                    ////string CustomerIPAddress = dr["IPAddress"].ToString();
                                    ////string GetwayIP = dr["GetwayIP"].ToString();
                                    //string TicketNumber = drdata["TicketNumber"].ToString();
                                    //string Comments = @"Dear Team, <br><br>" + Comment + ".";

                                    //Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                                    //sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                    //Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                    //ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                    //select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                    //OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                    //N'" + Comments + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                    //'' DescriptionChangeStatus
                                    //from dbo.TblSupportOpenTicket
                                    //where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                                    //Dictionary<string, string> para = new Dictionary<string, string>();
                                    //para.Add("TicketNumber", TicketNumber);
                                    //para.Add("SystemID", html.SystemID);
                                    //para.Add("WarehouseID", html.WarehouseID);
                                    //html.SaveChangeData(sql, false, para);
                                    //notificationTelegram.NotificationTelegramBot((Comments + " " + TicketNumber).Replace("<br>", "\n"));
                                }
                            }
                            else if (InvoiceType == "Created Invoice" && IsAlreadyApplyProfile == "False")
                            {
                                sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate from dbo.TblActivateCustomer where ";
                                sql += "ActivateCustomerID =" + ActivateCustomerID;
                                ds = html.LoadData(sql);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                                    string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                                    string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                                    string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                                    string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                                    string PPPoEProfileName = ds.Tables[0].Rows[0]["PPPoEProfileName"].ToString();
                                    string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                                    string Comment = "New Customer With Profile Name: " + PPPoEProfileName;
                                    string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                                    string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();
                                    //if (RenewDate != "")
                                    //{
                                    //    if (DateTime.Parse(RenewDate).ToString("yyyyMM") != easternTimeNow.ToString("yyyyMM"))
                                    //    {
                                    Mikrotik MK = new Mikrotik();
                                    MK.SetConnection(int.Parse(RouterID));
                                    if (ConnectionType == "PPPoE")
                                    {
                                        if (ConnectivityStatus == "50")
                                        {
                                            sqlresult = MK.ClearProfileName(PPPoEUserName);
                                            Comment = "New Customer With Profile Name: " + PPPoEProfileName + " Date:" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserName;
                                        }
                                        else if (ConnectivityStatus == "41")
                                        {
                                            Comment = "Your Services was re-activated on " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserName;
                                            sqlresult = MK.ReActivateUser(PPPoEUserName, Comment);

                                        }
                                        //if (IsChangePlan == "False" || IsChangePlan == "")
                                        //    sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                        //else
                                        //{
                                        //    DateTime SDate = DateTime.Parse(StartDate);
                                        //    if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                        //    {
                                        //        Comment = "Change from plan: " + PPPoEProfileName + " to " + PackageName + "; Change plan to Customer by: " + html.UserName + "; Change Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                                        //        PPPoEProfileName = PackageName;
                                        //        MK.ClearProfileName(PPPoEUserName);
                                        //        sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        Comment = "New Customer With Vlan Name: " + VLanName;
                                        sqlresult = MK.EnableVlanID(VLanID, Comment);
                                    }

                                    if (sqlresult.Result == ResultType.Successful)
                                    {
                                        sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ";
                                        sql += "ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                        sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active' ";
                                        sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                        if (IsChangePlan == "False" || IsChangePlan == "")
                                        {
                                            sql += "update dbo.TblActivateCustomer set BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                            sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                            sql += ";";
                                            sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, ConnectionType, ";
                                            sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                            sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                            sql += "select ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                            sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, ac.ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                            sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                            sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                            //sql += "inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID ";
                                            sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                        }
                                        else
                                        {
                                            DateTime SDate = DateTime.Parse(StartDate);
                                            if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                            {
                                                sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                sql += ";";
                                                sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                                sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                                sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, ac.ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                            }
                                        }
                                        sqlresult = html.SaveChangeData(sql, false, false);

                                        //for notificationTelegram

                                        //sql = "select top 1 cb.GrandTotal Amount, s.ServiceName, c.CustomerCode FirstName, c.CustomerName LastName, bi.CompanyEmail Email, bi.CompanyContactMobileNumber Phone, bi.InstallProvinceCity [Location], 'Activation by: " + html.UserName + "; Activate Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "' Comment ";
                                        //sql += ", b.PDFFileName, b.DestinationFolder, case when b.invTmp = 1 then b.ServiceName + ' - ' + b.PackageName else b.ServiceName end PackageName, ";
                                        //sql += "format(b.StartDate, 'dd-MMM-yyyy') StartDate, format(b.EndDate, 'dd-MMM-yyyy') EndDate ";
                                        //sql += "from dbo.TblCustomer c ";
                                        //sql += "inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID ";
                                        //sql += "inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = bi.CustomerID and b.CustomerBillInformationID = bi.CustomerBillInformationID ";
                                        //sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                                        //sql += "inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = b.InvoiceNo ";
                                        //sql += "where b.InvoiceNo = '" + InvoiceNo + "' and c.CustomerID = " + CustomerID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
                                        //ds = html.LoadData(sql);
                                        //string ServiceName = ds.Tables[0].Rows[0]["ServiceName"].ToString();
                                        //string FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                        //string LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                                        //string Email = ds.Tables[0].Rows[0]["Email"].ToString();
                                        //string Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                                        //string Location = ds.Tables[0].Rows[0]["Location"].ToString();
                                        //string PDFFileName = ds.Tables[0].Rows[0]["PDFFileName"].ToString();
                                        //DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                                        //PackageName = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                        //StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                        //EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                        //string Amount = ds.Tables[0].Rows[0]["Amount"].ToString();
                                        //Comment = ds.Tables[0].Rows[0]["Comment"].ToString();
                                        //StringBuilder MsgBody = new StringBuilder();
                                        //string Subject = "YOUR " + ServiceName.ToUpper() + " IS ACTIVATED ON DATE: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt");
                                        //MsgBody.AppendLine(Subject);
                                        //MsgBody.AppendLine("\nDear " + LastName + " [" + FirstName + "],\n");
                                        //MsgBody.AppendLine("Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n");
                                        //MsgBody.AppendLine("We would like to confirm your service is activated on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                        //MsgBody.AppendLine("\nYour current service: " + ServiceName);
                                        //MsgBody.AppendLine("Package Name: " + PackageName);
                                        //MsgBody.AppendLine("Start Date: " + StartDate);
                                        //MsgBody.AppendLine("Expired Date: " + EndDate);
                                        //MsgBody.AppendLine("Amount USD: " + Amount);
                                        //MsgBody.AppendLine("Phone Number: " + Phone);
                                        //if (Email != "")
                                        //    MsgBody.AppendLine("Email: " + Email);
                                        //MsgBody.AppendLine("Created By: " + html.UserName);
                                        //MsgBody.AppendLine("\nThank you and best regards,\n");
                                        //MsgBody.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                                        //MsgBody.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                                        //MsgBody.AppendLine("Customer Service: 086 588 777, 078 588 777");
                                        //MsgBody.AppendLine("Website: nsmisp.com.kh");
                                        //Phone = Phone.Trim().Replace(" ", string.Empty);
                                        //var regex = new Regex(Regex.Escape("0"));
                                        //Phone = regex.Replace(Phone, "+855", 1);
                                        //NotificationTelegram notificationTelegram = new NotificationTelegram();
                                        //notificationTelegram.NotificationToCustomer(Phone, FirstName, LastName, int.Parse(CustomerID), MsgBody.ToString(), DestinationFolder, PDFFileName);
                                        //MsgBody.Clear();

                                        //MsgBody.AppendLine("Customer Name: " + LastName.ToUpper() + " [" + FirstName.ToUpper() + "] IS ACTIVATED SUCCESSFUL.");
                                        //MsgBody.AppendLine("\nDear TEAM,\n");
                                        //MsgBody.AppendLine("We would like to confirm Customer Name: " + LastName.ToUpper() + "[" + FirstName.ToUpper() + "] is renew on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                        //MsgBody.AppendLine("\nServices: " + ServiceName);
                                        //MsgBody.AppendLine("Package Name: " + PackageName);
                                        //MsgBody.AppendLine("Start Date: " + StartDate);
                                        //MsgBody.AppendLine("Expired Date: " + EndDate);
                                        //MsgBody.AppendLine("Amount USD: " + Amount);
                                        //MsgBody.AppendLine("Phone Number: " + Phone);
                                        //if (Email != "")
                                        //    MsgBody.AppendLine("Email: " + Email);
                                        //MsgBody.AppendLine("Created By: " + html.UserName);
                                        //notificationTelegram.NotificationTelegramBot(MsgBody.ToString());

                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value,
                OfficialReceiptData = OfficialReceiptData
            }, JsonRequestBehavior.AllowGet);
        }
    }
}