using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class BillsController : Controller
    {
        // GET: Bills
        [Authorize(Roles = "Add Bills, Edit Bills, Delete Bills")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            //html.ReportTitle = "List Purchase Order";
            if (html.HasPermission("Add Bills"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Bills"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Bills"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "ExpenseTransactions/AddExpenses?IsFromBills=true";


            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "e.PaymentDate",
                DefaultValueDate = false

            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Draft No",
                SQL = "select top 10 [No] from dbo.Expenses where lower([No]) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by [No]",
                RadioValue = EncryAndDecry.Encryp("ae.DraftNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });

            html.TableName = "Expenses";
            html.PrimaryField = "ExpenseID";
            //html.OrderBy = "ExpensesID desc";
            //html.ShowSearchButton = false;
            html.ShowPaging = false;
            html.GH.SQL = @"SELECT
        row_number() over(order by e.PaymentDate desc, ISNULL(exp.ExpenseNo, e.ExpenseNo) desc, e.ExpenseID desc) as [RowNo],
        e.ExpenseID,
        '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, e.ExpenseID) +','''+e.[No]+''','''+e.[Status]+''','''+e.[PaymentNo]+''');"">' + e.[No] +'</a>' [No],
        e.PaymentTeam,
        e.PaidOnDate,
        '<a href=""#!/vendorcenter?id='+convert(varchar, e.PayeeID)+'"" target=""_blank"">' + py.VendorName + '</a>' PayeeName,
        CASE
            WHEN COALESCE(NULLIF(e.Description, ''), NULLIF(exp.Description, '')) IS NOT NULL 
                 AND NULLIF(items.ItemNames, '') IS NOT NULL
                 AND COALESCE(NULLIF(e.Description, ''), exp.Description) != items.ItemNames
            THEN COALESCE(NULLIF(e.Description, ''), exp.Description) + ' - ' + items.ItemNames
            WHEN COALESCE(NULLIF(e.Description, ''), NULLIF(exp.Description, '')) IS NOT NULL
            THEN COALESCE(NULLIF(e.Description, ''), exp.Description)
            ELSE items.ItemNames
        END AS Description,
        e.[Status],
        e.TotalAmount,
        e.PaidAmount,
        e.BalanceAmount,
        e.PaymentStatus,
        e.PaymentNo,
        '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, e.ExpenseID) +','''+e.[No]+''','''+e.[Status]+''','''+e.[PaymentNo]+''');"">' + ISNULL(exp.ExpenseNo, e.ExpenseNo) +'</a>' ExpenseNo,
        e.PaymentDate
    FROM Expense_PaymentTransactions e
    LEFT JOIN dbo.Expenses exp ON exp.ExpenseID = e.ExpenseID
    INNER JOIN dbo.TblVendor py ON py.VendorID = e.PayeeID
    OUTER APPLY (
        SELECT STUFF((
            SELECT ', ' + CASE 
                WHEN NULLIF(ed.ItemName, '') IS NOT NULL AND NULLIF(ed.ItemName_Description, '') IS NOT NULL 
                     AND ed.ItemName != ed.ItemName_Description
                    THEN ed.ItemName + ' - ' + ed.ItemName_Description
                ELSE COALESCE(NULLIF(ed.ItemName_Description, ''), ed.ItemName, '')
            END
            FROM dbo.ExpenseDetails ed WITH (NOLOCK)
            WHERE ed.ExpenseID = ISNULL(e.ExpenseID, exp.ExpenseID) 
              AND COALESCE(NULLIF(ed.ItemName_Description, ''), ed.ItemName, '') <> ''
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ItemNames
    ) items
    WHERE e.Type='Bill' and e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID;

            html.OrderBy = "e.PaymentDate DESC, ISNULL(exp.ExpenseNo, e.ExpenseNo) DESC, e.ExpenseID DESC";
            //html.IsHaveWhereCondition = true;
            //html.IsUseHavingCondition = true;
            html.GH.GridTable = new GridField[]
{
    new GridField()
    {
        Header = "#",
        FieldName = "RowNo",
        TextAlign = "right"
    },

    new GridField()
    {
        Header = "Expense#",
        FieldName = "ExpenseNo",
        TextAlign = "center"
    },
    new GridField()
    {
        Header = "PV.Date",
        FieldName = "PaymentDate",
        TextAlign = "center",
        FormatDateTime = "dd-MM-yyyy"
    },
    //new GridField()
    //{
    //    Header = "Paid On Date",
    //    FieldName = "PaidOnDate",
    //    TextAlign = "center",
    //    FormatDateTime = "dd-MM-yyyy"
    //},

    new GridField()
    {
        Header = "Payee Name",
        FieldName = "PayeeName"
    },

    new GridField()
    {
        Header = "Description",
        FieldName = "Description",
        Width = "500px",
        ClassName = "text-truncate"
    },

    new GridField()
    {
        Header = "Status",
        FieldName = "Status",
        TextAlign = "center"
    },

    new GridField()
    {
        Header = "Total",
        FieldName = "TotalAmount",
        TextAlign = "right",
        Format = "#,#0.00"
    },
    new GridField()
    {
        Header = "Paid",
        FieldName = "PaidAmount",
        TextAlign = "right",
        Format = "#,#0.00"
    },
    new GridField()
    {
        Header = "Balance",
        FieldName = "BalanceAmount",
        TextAlign = "right",
        Format = "#,#0.00"
    },
    new GridField()
    {
        Header = "Payment Status",
        FieldName = "PaymentStatus",
        TextAlign = "center",
    },
    new GridField()
    {
        Header = "Action",
        FieldName = "ExpenseID",
        TextAlign = "Center",
        IsModifyField = true,
        DeleteWithTableName = "Expenses, ExpenseDetails, Expense_PaymentTransactions, TblAccount_Transaction, TblAccount_JournalEntries",
        HideEditOnActionNameNotMatch = true,
        ShowActionNameOnConditionField = "Status",
        ShowActionNameOnConditionValue = "Draft,Rejected,Confirm Rejected"
    }
};

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreateBill(string PrimaryFieldValue, string Type = "", string PO_Number = "", bool Clone = false, string Status = "Bill")
        {
            string BillType = Status;
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.Status = Status;
            if (Status == "ConvertToBill")
            {
                BillType = "Bill";
            }
            else if (Status == "ConvertToBillFromReceived")
            {
                BillType = "Stock In";
            }
            else if (Status == "ConvertToCreditNote")
            {
                BillType = "Credit Note";
                Type = "Credit Note";
            }
            ViewBag.Type = Type;
            ViewBag.VendorName = "";
            ViewBag.PONumber = PO_Number;
            HTML html = new HTML
            {
                isCloned = Clone,
                FormID = "CreateBills",
                FieldIDFocus = "VendorName",
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);"
            };
            string vendornamevalue = "";
            string vendorID = "0";
            string PurchaseOrderID = "0";
            string PONumber = "";
            string VendorInvoiceNumber = "";
            string Attention = "";
            string Reference = "";
            string BillNumber = "";
            string sqlv;
            if ((Type == "Bill" || Type == "Credit Note") && Status != "ConvertToCreditNote")
            {
                // PrimaryFieldValue holds VendorID here (used when opening from Vendor Profile)
                // When opening a blank new bill (PrimaryFieldValue = "0"), skip the lookup
                if (PrimaryFieldValue != "0")
                {
                    sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                    sqlv += "where v.VendorID = " + PrimaryFieldValue;
                    sqlv += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
                    html.PrimaryFieldValue = "0";
                    DataSet dsv = html.LoadData(sqlv);
                    if (dsv != null && dsv.Tables.Count > 0 && dsv.Tables[0].Rows.Count > 0)
                    {
                        vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                        vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                        ViewBag.VendorName = vendornamevalue;
                    }
                }
                else
                {
                    // Blank new bill — vendor will be typed/selected in the form
                    html.PrimaryFieldValue = "0";
                }
            }

            else if (PrimaryFieldValue != "0")
            {
                html.PrimaryFieldValue = PrimaryFieldValue;
                sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.PONumber, p.VendorDONumber, p.VendorInvoiceNumber, p.PurchaseOrderID, p.Attention, p.ReceiveNoteNumber, p.ReceiveDate, p.Reference, p.BillNumber from dbo.TblVendor v ";
                sqlv += "inner join dbo.TblBills p on p.VendorID = v.VendorID ";
                sqlv += "where p.BillID = " + html.PrimaryFieldValue.ToString();
                sqlv += " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
                DataSet dsv = html.LoadData(sqlv);
                vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                ViewBag.VendorName = vendornamevalue;
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                PurchaseOrderID = dsv.Tables[0].Rows[0]["PurchaseOrderID"].ToString();
                PONumber = dsv.Tables[0].Rows[0]["PONumber"].ToString();
                string VendorDONumber = dsv.Tables[0].Rows[0]["VendorDONumber"].ToString();
                VendorInvoiceNumber = dsv.Tables[0].Rows[0]["VendorInvoiceNumber"].ToString();
                Attention = dsv.Tables[0].Rows[0]["Attention"].ToString();
                string ReceiveNoteNumber = dsv.Tables[0].Rows[0]["ReceiveNoteNumber"].ToString();
                string ReceiveDate = dsv.Tables[0].Rows[0]["ReceiveDate"].ToString();
                Reference = dsv.Tables[0].Rows[0]["Reference"].ToString();
                BillNumber = dsv.Tables[0].Rows[0]["BillNumber"].ToString();
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="Vendor_Name", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete = true, HiddenField="VendorID",
                            Value=vendornamevalue, HiddenFieldValue=vendorID,
                            ISCallBack = true,
                            SQL_AutoComplete="select top 10 VendorID, (VendorCode + ' - ' + VendorName) VendorName from dbo.TblVendor where lower((VendorCode + ' - ' + VendorName)) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            SQLCallBack="select Attention from dbo.TblVendorBillingInformation b inner join dbo.TblVendor v on v.VendorID = b.VendorID where lower(v.VendorCode + ' - ' + v.VendorName) = @SearchText and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
                            DisplayOnFields="Txt_Attention",
                            //IsReadOnly = (PO_Number==""? false:true),
                            OnChangeFunction="onVendorChange()"
                        },
                        new HtmlButton()
                        {
                            ID="btnAddNewVendor", IsCustomForm = true,
                            TextValue="New Vendor", Event="onclick=\"AddNewVendor();\""
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label=Type=="Credit Note"?"Bill Number": "PO Number", ID=Type=="Credit Note"?"BillNumber": "PONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete=Type=="Credit Note"?"select distinct top 10 BillNumber from dbo.TblBills where lower(BillNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID: "select distinct top 10 PONumber from dbo.TblPurchaseOrder where POStatus = 90 and lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            CallBackFunction="OnPONumberChange",
                            Value = Type=="Credit Note"?BillNumber: PONumber
                        },
                        new HtmlTextBox()
                        {
                            Label="PurchaseOrderID", ID="PurchaseOrderID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", Hide = true, Value=PurchaseOrderID
                        },
                        new HtmlTextBox()
                        {
                            Label="Attention (ATT)", ID="Txt_Attention",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= Attention, Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Type", ID="Type",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= BillType, Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Invoice Number", ID="VendorInvoiceNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= VendorInvoiceNumber
                        },
                        new HtmlTextBox()
                        {
                            Label="Reference", ID="TxtReference",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= Reference
                        }
                    }
                },
                new  FormData() {
                    TableName="TblBills",
                    PrimaryFieldValue = PrimaryFieldValue,
                    FormTitle=Type=="Credit Note"?"Credit Note Information":"Bills Information",
                    GetIDENTITYByFiled="BillID",
                    PrimaryFieldName="BillID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Vendors/Bills",
					//SQL=(Type=="Credit Note"?"": "update dbo.TblPurchaseOrder set StatusID = "+(BillType=="Bill"?66:67)+" where PurchaseOrderID = @PurchaseOrderID"),
					SQL="update dbo.TblPurchaseOrder set BillStatusID = 64 where PurchaseOrderID = @PurchaseOrderID",
                    UpdateFiledName=Type== "Credit Note"? "": "PurchaseOrderID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="IssuedBy", SetValue=""+html.UserFullName+""
                        },
                        new DBField()
                        {
                            DBFieldName="IssuedByUserID", SetValue=""+html.UserID+""
                        },
                        new DBField()
                        {
                            DBFieldName="VendorID", Value="VendorID", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="PurchaseOrderID", Value="PurchaseOrderID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName=Type=="Credit Note"?"":"PONumber", Value=Type=="Credit Note"?"":"PONumber", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Attention", Value="Txt_Attention", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Reference", Value="TxtReference", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName= Type!="Credit Note"?"":"BillNumber", Value=Type!="Credit Note"?"":"BillNumber", GetValueFromInputName= true
                        },
                        //new DBField()
                        //{
                        //    DBFieldName="VendorDONumber", Value="VendorDONumber", GetValueFromInputName= true
                        //},
                        new DBField()
                        {
                            DBFieldName="VendorInvoiceNumber", Value="VendorInvoiceNumber", GetValueFromInputName= true
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
                            DBFieldName="Tax", Value="Tax", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Rate", Value="Tax_Rate", GetValueFromInputName= true, IsNumber = true
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
                            DBFieldName="AmountPaid", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
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
                            DBFieldName="Type", Value="Type", GetValueFromInputName= true
                        },
                        //new DBField()
                        //{
                        //    DBFieldName="ReceiveNoteNumber", Value="ReceiveNoteNumber", GetValueFromInputName= true
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="ReceiveDate", Value="ReceiveDate", GetValueFromInputName= true
                        //},
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PODate", ID="PODate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PODate", Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label=Type=="Credit Note"?"Credit Note#": "Bill#", ID=Type=="Credit Note"?"CreditNoteNumber":"BillNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField=Type=="Credit Note"?"CreditNoteNumber": "BillNumber", IsReadOnly = true,
                            isCloned = Clone,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = '"+(Type=="Credit Note"?"Vendor Credit Note":"Bills")+"'; select @Prefix = Prefix from dbo.TblPrefix where Module = '"+(Type=="Credit Note"?"Vendor Credit Note":"Bills")+"' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'BL-{yy}{MM}{dd}-{0000}'",
                            CallBackToControlID=Type=="Credit Note"?"CreditNoteNumber, PrefixCreditNoteNumber": "BillNumber, PrefixBillNumber", UpdatePrefixModule=""+(Type=="Credit Note"?"Vendor Credit Note":"Bills")+"", Prefix=Type=="Credit Note"?"PrefixCreditNoteNumber":"PrefixBillNumber", SetPrefixToDbFieldName=Type=="Credit Note"?"CreditNoteNumber":"BillNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Date", ID="BillDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BillDate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
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
                            DBField="PaymentDueDate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Warehouse", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            SQL="select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID", TypeaheadGroupByField="WarehouseName",
                            DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone"})
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
                            DBField="ContactName", DivClassCol="col-sm-2"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone", ID="TxtContactPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactPhone", DivClassCol="col-sm-2",
                            HiddenField="IssuedBy,IssuedByUserID", HiddenFieldValue=html.UserFullName+","+html.UserID
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
                            Label="Tax", ID="Tax", DivClassCol="col-sm-2",
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead = true, FieldName="Tax",
                            SQL="select Rate, TaxName + ' [' + convert(varchar, format(Rate, '#')) + '%]' Tax from dbo.TblTax where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by TaxName",
                            HiddenField="Rate", HiddenFieldValue="0"
                        },
                    }
                },
                new  FormData() {
                    TableName="TblBillsDetail",
                    GetIDENTITYByFiled="BillID",
                    PrimaryFieldName="BillID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll",//Reload_AccountName,ApplySummernote('summernote')
                            FooterColspan = 3,
                            SubTotalColspan = 5,
                            GetIDENTITYByFiled="BillID",
                            PrimaryFieldName="BillID",
                            TableName="TblBillsDetail",
                            ReturnFieldNameValue="BillID, VendorID, BillNumber",
                            ReturnFieldNameValueFromTblName = "TblBills",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="PurchaseOrderID", SetValue="$(\"#PurchaseOrderID\").val()",
                                },
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
                                    DBFieldName="ChartOfAccountID", Value="ChartOfAccountID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemExpirationDate", Value="ItemExpirationDate", IsDate = true,
                                },
                                new DBField()
                                {
                                    DBFieldName="Type", Value="Type"
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
                                new TblTHead()
                                {
                                     Class="col-3", Label="គណនេយ្យ"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                //new TblTHead()
                                //{
                                //     Label="បរិមាណ ទទួល", Class="col-1 text-nowrap", Hide = (Status == "ConvertToBill"?true:false)
                                //},
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
                                new TblTHead()
                                {
                                     Label="Account"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Order QTY"
                                //},
                                new TblTHead()
                                {
                                     Label="QTY", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="UM", Class="text-nowrap"
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
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select top 10 ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName
from dbo.TblItem i where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                                            SQLCallBack="select i.UM, i.CostPrice, i.PurchaseChartOfAccountID ChartOfAccountID, (c.AccountNo + ' - ' + c.AccountName) AccountName, SellingPrice SalePrice, i.ItemDescription ItemName_Description from dbo.TblItem i inner join dbo.TblChartOfAccount c on c.ChartOfAccountID = i.PurchaseChartOfAccountID where i.ItemID = @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID,
                                            DisplayOnFields="UM, UnitPrice, ChartOfAccountID, AccountName, SalePrice, ItemName_Description",
                                            AddCallBackFunction="Reload_AccountName()", InputDescription = true,placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="AccountName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="AccountName", DivClass="wrap-input3",
                                            isTypeahead = true, HiddenField="ChartOfAccountID",
                                            SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                                            TypeaheadGroupByField = "AccountType"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="OrderQTY", DivClass="wrap-input3 text-center",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="OrderQTY", Value = "0", IsallowInputOnlyNumber =true,
                                        //    IsReadOnly = true
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="TotalAll();",
                                            HiddenField="PurchaseOrder_ID", HiddenFieldValue="0", Digit="0.000"
                                            //Hide = (Status == "ConvertToBill" ? true:false)
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", IsReadOnly = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();",
                                            HiddenField="DisP", HiddenFieldValue = "0", Digit="0.000"
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
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            HiddenField="Type", HiddenFieldValue= BillType,
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
        public JsonResult LoadPONumber()
        {
            HTML html = new HTML();
            string SQL = "declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'PO' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + ";";
            SQL += "exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ", '" + html.DomainID + "'";
            return Json(new { data = html.JsonData(SQL, false) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadPO(string PONumber, string Type = "")
        {
            HTML html = new HTML();
            string SQL = @"select p.VendorID, p.PurchaseOrderID, p.DeliverTo, p.Tax, p.Rate, p.CurrencyID, p.CurrencyName,
pd.ItemID, pd.ItemName, pd.ItemDescription, i.PurchaseChartOfAccountID ChartOfAccountID, (c.AccountNo + ' - ' + c.AccountName) AccountName, p.ContactName, p.ContactPhone,
pd.QTY, pd.UnitPrice, pd.DisP, pd.DisUSD, p.WID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.ClassID,
p.ClassName, p.PODate, p.PaymentTerms, format(p.PaymentDueDate, 'dd/MM/yyyy')PaymentDueDate, p.NumberofDays,
pd.UM, p.Deposit, p.Balance, pd.SalePrice
from dbo.TblPurchaseOrder p
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = p.PurchaseOrderID
left join dbo.TblItem i on i.ItemID = pd.ItemID
left join dbo.TblChartOfAccount c on c.ChartOfAccountID = i.PurchaseChartOfAccountID
            where p.PONumber = @PONumber and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID
            order by pd.ItemID";

            if (Type == "Credit Note")
            {
                SQL = @"select p.VendorID, p.PurchaseOrderID, p.Warehouse DeliverTo, p.Tax, p.Rate, p.CurrencyID, p.CurrencyName,
                pd.ItemID, pd.ItemName, pd.ItemDescription, pd.ChartOfAccountID, pd.AccountName, p.ContactName, p.ContactPhone,
                pd.QTY, pd.UnitPrice, pd.DisP, pd.DisUSD, p.WID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.ClassID,
                p.ClassName, p.PODate
                from dbo.TblBills p
                inner join dbo.TblVendor v on v.VendorID = p.VendorID
                inner join dbo.TblBillsDetail pd on pd.BillID = p.BillID
                where p.BillNumber = @PONumber and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID
                order by pd.ItemID
                ";
            }

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "PONumber", PONumber },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

    }
}
