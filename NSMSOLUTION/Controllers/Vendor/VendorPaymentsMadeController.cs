using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class VendorPaymentsMadeController : Controller
    {
        // GET: VendorPaymentsMade
        public ActionResult Index(string PrimaryFieldValue, bool onItsForm = false, string BillID = "")
        {
            int VendorID = int.Parse(PrimaryFieldValue);
            ViewBag.onItsForm = onItsForm;
            ViewBag.BillID = BillID;
            HTML html = new HTML();
            string sql = @"select v.VendorID, v.VendorName ReceivedFromName, (v.VendorCode + ' - ' + v.VendorName) VendorName
                    from  dbo.TblVendor v where v.VendorID = " + VendorID + " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
            DataSet ds = html.LoadData(sql);
            string VendorName = "", ReceivedFromName = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                VendorName = ds.Tables[0].Rows[0]["VendorName"].ToString();
                VendorID = int.Parse(ds.Tables[0].Rows[0]["VendorID"].ToString());
                ReceivedFromName = ds.Tables[0].Rows[0]["ReceivedFromName"].ToString();
            }
            html.FormID = "FrmVendorPaymentMade";
            html.SaveChangeFunction = "SaveChangeVendorPayment()";
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Record Payment",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Vendors/Payment",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "VendorPaymentID:",
                            ID = "VendorPaymentID",
                            Class="input2 CustInfo",
                            IsCustomForm = true, Hide = true,
                           Value = "0"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Pay To:",
                            ID = "Payee",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10  b.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName, v.VendorName ReceivedFromName from dbo.TblBills b inner join dbo.TblVendor v on v.VendorID = b.VendorID where lower(v.VendorCode + ' - ' + v.VendorName) like @SearchText and b.SystemID =" + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
                            HiddenField="VendorID",
                            Value =VendorName,
                            HiddenFieldValue = VendorID.ToString(),
                            ISCallBack = true,
                            SQLCallBack="select v.VendorName ReceivedFromName from dbo.TblBills b inner join dbo.TblVendor v on v.VendorID = b.VendorID where lower(v.VendorCode + ' - ' + v.VendorName) = @SearchText and b.SystemID =" + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
                            DisplayOnFields="ReceivedFromName"
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
                            GroupByField="AccountType", DivClass="border-0 mt-1"
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
                            Value = "PaymentMethodID", AddNewFuntionName="AddPaymentMethod('PaymentModeID')", DivClass="border-0 mt-1"
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
                            AddNewFuntionName="AddPaymentCategory('PaymentCategoryID')", DivClass="border-0 mt-1"
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
                            placeholder="Notes (Internal use. Not visible to vendor)",
                            DivClass="noborder", Style="style=\"height: 150px;\""
                        },
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult TransactionDetails(int VendorID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() Over(order by (v.VendorCode + ' - ' + v.VendorName), b.BillID desc) as [No], 
            b.BillID, b.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName,
            b.BillDate, b.PaymentDueDate, b.BillNumber, b.PONumber, format(b.GrandTotal, '#,#.00') BillAmount, 
            case when isnull(b.AmountDue, 0) = 0 then b.GrandTotal else b.AmountDue end AmountDue,
            s.[Status], isnull(b.AmountPaid, 0) AmountPaid
            from dbo.TblBills b
            inner join dbo.TblVendor v on v.VendorID = b.VendorID
            inner join dbo.TblStatus s on s.StatusID = b.StatusID
            where b.StatusID in (11, 15) and v.VendorID = " + VendorID + " and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.OrderBy = "(v.VendorCode + ' - ' + v.VendorName), b.BillID desc";
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.DefaultSearch = false;
            html.TblID = "tblVendorUnpaid";
            html.ShowPrintOnTopTable = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "BillID", FieldName = "BillID", IsHiddenField= true },
                new GridField() { Header = "Bill Date", FieldName = "BillDate", Format = "dd-MM-yyyy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format = "dd-MM-yyyy" },
                new GridField() { Header = "Bill#", FieldName = "BillNumber", },
                new GridField() { Header = "PO#", FieldName = "PONumber", },
                new GridField() { Header = "Amount", FieldName = "BillAmount", TextAlign = "right", Format="#,#.00" },
                new GridField() { Header = "Amount Paid", FieldName = "AmountPaid", TextAlign = "right", Format="#,#.00", ClassName="text-primary" },
                new GridField() { Header = "Amount Due", FieldName = "AmountDue", TextAlign = "right", Format="#,#.00", ClassName="text-danger" },
                new GridField() { Header = "Payment Amount", FieldName = "AmountDue", DisplayTextBox = true },
                new GridField() { Header = "Action", FieldName = "BillID", DisplayCheckBox = true },
            };
            string DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadVendorUnpaid");
            return Json(new { Data = DrawHTMLSearchForm }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveChangeVendorPaymentsMade(string ObjForm, string[] ObjTbl)
        {
            HTML html = new HTML();
            Dictionary<string, string> FormData = JsonConvert.DeserializeObject<Dictionary<string, string>>(ObjForm);
            dynamic TblData = new JavaScriptSerializer().DeserializeObject(ObjTbl[0].ToString());

            StringBuilder sbForm = new StringBuilder();
            sbForm.AppendLine("<FormData>");
            foreach (KeyValuePair<string, string> str in FormData)
            {
                if (str.Key == "PaymentDate")
                    sbForm.AppendFormat("<{0}>{1}</{0}>", str.Key, str.Value + " " + DateTime.Parse(html.GetCurrentDate()).ToString("HH:mm:ss tt"));
                else
                {
                    if (str.Value.ToString() != "")
                        sbForm.AppendFormat("<{0}>{1}</{0}>", str.Key, HttpUtility.UrlDecode(str.Value));
                }
            }
            sbForm.AppendFormat("<{0}>{1}</{0}>", "SystemID", html.SystemID);
            sbForm.AppendFormat("<{0}>{1}</{0}>", "WarehouseID", html.WarehouseID);

            sbForm.AppendLine("</FormData>");
            sbForm.AppendLine("<TblData>");
            foreach (dynamic item in TblData)
            {
                sbForm.AppendLine("<Data>");
                foreach (dynamic str in item)
                {
                    sbForm.AppendFormat("<{0}>{1}</{0}>", str.Key, str.Value);
                }
                sbForm.AppendLine("</Data>");
            }
            sbForm.AppendLine("</TblData>");

            SqlResult sqlResult = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "Data", sbForm.ToString() }
            };
            sqlResult = html.SaveChangeData("SP_INSERT_VENDOR_PAYMENT", true, para, true, true);
            string strVendorID = html.JsonConvertDataTable(sqlResult.Data.Tables[1]);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = strVendorID }, JsonRequestBehavior.AllowGet);
        }
    }
}