using System;
using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Finance
{
    [Authorize]
    public class EnterBillController : Controller
    {
       
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "Enter Bills"
            };

            if (html.HasPermission("Add Bills") || html.HasPermission("Enter Bill") || html.HasPermission("Purchases") || html.HasPermission("List Bills"))
                html.AllowAdd = true;
            else
                html.AllowAdd = true;

            if (html.HasPermission("Edit Bills") || html.HasPermission("Enter Bill") || html.HasPermission("Purchases") || html.HasPermission("List Bills"))
                html.AllowEdit = true;
            else
                html.AllowEdit = true;

            if (html.HasPermission("Delete Bills") || html.HasPermission("Enter Bill") || html.HasPermission("Purchases") || html.HasPermission("List Bills"))
                html.AllowDelete = true;
            else
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Create";
            html.AddURL = "EnterBill/Create";
            html.TableName = "dbo.TblBills";
            html.PrimaryField = "BillID";
            html.OrderBy = "BillID desc";
            html.TblAlign = "auto";
            html.Cellpadding = 4;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Bill Date:",
                LabelAlign = "left",
                DBField = "po.BillDate",
                DateTimePickerWithRanges = true
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBillNumber",
                ISAutoComplete = true,
                Label = "Bill No:",
                LabelAlign = "left",
                DBField = "po.BillNumber",
                SQLAutoComplete = "SELECT DISTINCT TOP 10 BillNumber FROM dbo.TblBills WHERE LOWER(BillNumber) LIKE @SearchText AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID + " ORDER BY BillNumber DESC"
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorName",
                ISAutoComplete = true,
                Label = "Vendor:",
                LabelAlign = "left",
                DBField = "v.VendorName",
                SQLAutoComplete = "SELECT DISTINCT TOP 10 v.VendorName FROM dbo.TblBills po INNER JOIN dbo.TblVendor v ON v.VendorID = po.VendorID WHERE LOWER(v.VendorName) LIKE @SearchText AND v.SystemID = " + html.SystemID + " AND v.WarehouseID = " + html.WarehouseID + " ORDER BY v.VendorName"
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPONumber",
                ISAutoComplete = true,
                Label = "PO No:",
                LabelAlign = "left",
                DBField = "po.PONumber",
                SQLAutoComplete = "SELECT DISTINCT TOP 10 PONumber FROM dbo.TblBills WHERE LOWER(PONumber) LIKE @SearchText AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID + " ORDER BY PONumber DESC"
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                LabelAlign = "left",
                DBField = "po.StatusID",
                SQL_CBO = "SELECT DISTINCT s.StatusID, s.[Status] FROM dbo.TblBills po INNER JOIN dbo.TblStatus s ON s.StatusID = po.StatusID WHERE po.SystemID = " + html.SystemID + " AND po.WarehouseID = " + html.WarehouseID,
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                ID = "Class",
                OPTLabel = "Class",
                SQL = @"SELECT ClassID, ClassName FROM dbo.TblClass WHERE SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("po.ClassName"),
                DefaultCheckBok = true
            });

            string sql = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY po.BillID DESC) AS [No],
                    po.BillID,
                    '<a class=""text-primary"" style=""cursor: pointer; text-decoration: none; font-weight: 600;"" onclick=""showEnterBillForm(' + CONVERT(VARCHAR, po.BillID) + ', ''' + ISNULL(po.BillNumber, '') + ''')"">' + ISNULL(po.BillNumber, '') + '</a>' AS BillNumberLink,
                    po.BillNumber,
                    ISNULL(po.Warehouse, 'Show Room NSM') AS Location,
                    ISNULL(v.VendorName, '') AS Vendor,
                    CASE 
                        WHEN ISNULL(po.PONumber, '') = '' THEN ''
                        ELSE '<a class=""text-primary"" style=""cursor: pointer; text-decoration: none; font-weight: 600;"" onclick=""previewPO(' 
                             + CONVERT(VARCHAR, ISNULL(po.VendorID, ISNULL(tpo.VendorID, 0))) + ', ' 
                             + CONVERT(VARCHAR, COALESCE(NULLIF(po.PurchaseOrderID, 0), tpo.PurchaseOrderID, 0)) + ', ''' 
                             + REPLACE(ISNULL(po.PONumber, ''), '''', '') + ''')"">' + ISNULL(po.PONumber, '') + '</a>'
                    END AS PONumberLink,
                    ISNULL(po.PONumber, '') AS PONumber,
                    FORMAT(po.BillDate, 'dd/MM/yyyy') AS BillDate,
                    ISNULL(po.VAT, 0) AS Expense,
                    po.GrandTotal AS TotalAmount,
                    ISNULL(po.CustomerNotes, '') AS Memo,
                    ISNULL(po.IssuedBy, '') AS Representative,
                    po.StatusID
                FROM dbo.TblBills po
                LEFT JOIN dbo.TblVendor v ON v.VendorID = po.VendorID
                LEFT JOIN dbo.TblStatus s ON s.StatusID = po.StatusID
                OUTER APPLY (
                    SELECT TOP 1 PurchaseOrderID, VendorID 
                    FROM dbo.TblPurchaseOrder 
                    WHERE LTRIM(RTRIM(PONumber)) = LTRIM(RTRIM(po.PONumber))
                    ORDER BY CASE WHEN SystemID = po.SystemID AND WarehouseID = po.WarehouseID THEN 0 ELSE 1 END, PurchaseOrderID DESC
                ) tpo
                WHERE po.SystemID = " + html.SystemID + @" 
                  AND po.WarehouseID = " + html.WarehouseID;

            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center", Width = "50px" },
                new GridField() { Header = "Bill Nº", FieldName = "BillNumberLink", TextAlign = "left", Width = "140px" },
                new GridField() { Header = "Location", FieldName = "Location", TextAlign = "left", Width = "140px" },
                new GridField() { Header = "Vendor", FieldName = "Vendor", TextAlign = "left", Width = "190px" },
                new GridField() { Header = "PO Nº", FieldName = "PONumberLink", TextAlign = "left", Width = "140px" },
                new GridField() { Header = "Date", FieldName = "BillDate", TextAlign = "center", Width = "100px" },
                new GridField() { Header = "Expense", FieldName = "Expense", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", Width = "100px", IsTotalFooter = true },
                new GridField() { Header = "Total Amount", FieldName = "TotalAmount", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", Width = "120px", IsTotalFooter = true },
                //new GridField() { Header = "Memo", FieldName = "Memo", TextAlign = "left", Width = "160px" },
                new GridField() { Header = "Representative", FieldName = "Representative", TextAlign = "left", Width = "130px" },
                new GridField() { Header = "Action", FieldName = "BillID", TextAlign = "center", Width = "90px", IsModifyField = true, DeleteWithTableName = "TblBillsDetail" }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View("~/Views/EnterBill/Index.cshtml");
        }

      
        public JsonResult GetVendorDeposit(string vendorId, string poNumber = "")
        {
            HTML html = new HTML();
            int vId = 0;
            if (!string.IsNullOrEmpty(vendorId))
            {
                decimal d;
                if (decimal.TryParse(vendorId, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                {
                    vId = (int)d;
                }
            }
            string cleanPO = (poNumber ?? "").Trim();

            string sql = @"
                SELECT TOP 1
                    e.ExpenseID,
                    ISNULL(e.[No], '') AS PVNumber,
                    ISNULL(e.PaymentNo, '') AS PaymentNo,
                    ISNULL(e.Reference, '') AS PONumber,
                    ISNULL(NULLIF(e.PaidAmount, 0), e.TotalAmount) AS DepositAmount,
                    FORMAT(e.PaymentDate, 'dd/MM/yyyy') AS DepositDate,
                    ISNULL(e.Description, '') AS Memo,
                    '122000' AS DepositAccountNo,
                    'Vendor Deposit' AS DepositAccountName
                FROM dbo.Expenses e
                WHERE (
                        (e.ExpenseTransactionType = 'Vendor Deposit' AND (
                            (@PONumber <> '' AND (e.Reference = @PONumber OR e.Reference LIKE '%' + @PONumber + '%'))
                            OR (@PONumber = '' AND @VendorID > 0 AND e.PayeeID = @VendorID)
                        ))
                        OR (@PONumber <> '' AND (e.Reference = @PONumber OR e.Reference LIKE '%' + @PONumber + '%' OR EXISTS (SELECT 1 FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID AND (ed.RefNo = @PONumber OR ed.RefNo LIKE '%' + @PONumber + '%'))))
                      )
                  AND ISNULL(e.PaymentStatus, '') <> 'Applied'
                  AND (@VendorID = 0 OR e.PayeeID = @VendorID OR ISNULL(e.PayeeID, 0) = 0)
                  AND (ISNULL(e.PaidAmount, 0) > 0 OR (e.ExpenseTransactionType = 'Vendor Deposit' AND ISNULL(e.TotalAmount, 0) > 0))
                  AND e.SystemID = " + html.SystemID + @"
                  AND e.WarehouseID = " + html.WarehouseID + @"
                ORDER BY e.ExpenseID DESC";

            Dictionary<string, string> p = new Dictionary<string, string>
            {
                { "VendorID", vId.ToString() },
                { "PONumber", cleanPO }
            };
            DataSet ds = html.LoadData(sql, false, p);
            return Json(new { success = true, data = ds.ToJsonString() }, JsonRequestBehavior.AllowGet);
        }

       
        public ActionResult Create(string PrimaryFieldValue = "0", string Type = "Bill",
                                   string PO_Number = "", bool Clone = false,
                                   string Status = "Bill", int? id = null)
        {
            if (id.HasValue && id.Value > 0)
            {
                PrimaryFieldValue = id.Value.ToString();
            }
            if (string.IsNullOrEmpty(PrimaryFieldValue))
            {
                PrimaryFieldValue = "0";
            }
            if (string.IsNullOrEmpty(Type))
            {
                Type = "Bill";
            }
            if (string.IsNullOrEmpty(Status))
            {
                Status = "Bill";
            }

            string BillType = Status;
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.Status = Status;

            if (Status == "ConvertToBill")
                BillType = "Bill";
            else if (Status == "ConvertToBillFromReceived")
                BillType = "Stock In";
            else if (Status == "ConvertToCreditNote")
            {
                BillType = "Credit Note";
                Type = "Credit Note";
            }

            ViewBag.Type     = Type;
            ViewBag.VendorName = "";
            ViewBag.PONumber   = PO_Number;

            HTML html = new HTML
            {
                isCloned = Clone,
                FormID   = "CreateBills",
                FieldIDFocus = "VendorName",
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);"
            };

            string vendornamevalue    = "";
            string vendorID           = "0";
            string PurchaseOrderID    = "0";
            string PONumber           = "";
            string VendorInvoiceNumber = "";
            string Attention          = "";
            string Reference          = "";
            string BillNumber         = "";
            string sqlv;

            if (PrimaryFieldValue != "0")
            {
                // Check whether PrimaryFieldValue is an existing Bill in TblBills
                string checkBillSql = "select p.BillID from dbo.TblBills p where p.BillID = " + PrimaryFieldValue + " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
                DataSet dsCheck = html.LoadData(checkBillSql);
                bool isExistingBill = (dsCheck != null && dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0);

                if (isExistingBill || Type == "EditBill" || Status == "ConvertToCreditNote")
                {
                    html.PrimaryFieldValue = PrimaryFieldValue;
                    sqlv  = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.PONumber, p.VendorDONumber, p.VendorInvoiceNumber, p.PurchaseOrderID, p.Attention, p.ReceiveNoteNumber, p.ReceiveDate, p.Reference, p.BillNumber from dbo.TblVendor v ";
                    sqlv += "inner join dbo.TblBills p on p.VendorID = v.VendorID ";
                    sqlv += "where p.BillID = " + html.PrimaryFieldValue;
                    sqlv += " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
                    DataSet dsv = html.LoadData(sqlv);
                    if (dsv != null && dsv.Tables.Count > 0 && dsv.Tables[0].Rows.Count > 0)
                    {
                        vendornamevalue     = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                        ViewBag.VendorName  = vendornamevalue;
                        vendorID            = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                        PurchaseOrderID     = dsv.Tables[0].Rows[0]["PurchaseOrderID"].ToString();
                        PONumber            = dsv.Tables[0].Rows[0]["PONumber"].ToString();
                        VendorInvoiceNumber = dsv.Tables[0].Rows[0]["VendorInvoiceNumber"].ToString();
                        Attention           = dsv.Tables[0].Rows[0]["Attention"].ToString();
                        Reference           = dsv.Tables[0].Rows[0]["Reference"].ToString();
                        BillNumber          = dsv.Tables[0].Rows[0]["BillNumber"].ToString();
                    }
                }
                else
                {
                    // PrimaryFieldValue is VendorID (e.g. creating new bill with preselected vendor)
                    sqlv  = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                    sqlv += "where v.VendorID = " + PrimaryFieldValue;
                    sqlv += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
                    html.PrimaryFieldValue = "0";
                    DataSet dsv = html.LoadData(sqlv);
                    if (dsv != null && dsv.Tables.Count > 0 && dsv.Tables[0].Rows.Count > 0)
                    {
                        vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                        vendorID        = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                        ViewBag.VendorName = vendornamevalue;
                    }
                }
            }
            else
            {
                html.PrimaryFieldValue = "0"; // blank new bill
            }

            html.FormData = new HTMLControl[]
            {
                new FormData() {
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
                            OnChangeFunction="onVendorChange()"
                        },
                        new HtmlButton()
                        {
                            ID="btnAddNewVendor", IsCustomForm = true,
                            TextValue="New Vendor", Event="onclick=\"AddNewVendor();\""
                        }
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label=Type=="Credit Note"?"Bill Number": "PO Number",
                            ID=Type=="Credit Note"?"BillNumber": "PONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete=Type=="Credit Note"
                                ?"select distinct top 10 BillNumber from dbo.TblBills where lower(BillNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID
                                :"select distinct top 10 PONumber from dbo.TblPurchaseOrder where POStatus = 90 and lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
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
                new FormData() {
                    TableName="TblBills",
                    PrimaryFieldValue = PrimaryFieldValue,
                    FormTitle=Type=="Credit Note"?"Credit Note Information":"Bills Information",
                    GetIDENTITYByFiled="BillID",
                    PrimaryFieldName="BillID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Vendors/Bills",
                    SQL="update dbo.TblPurchaseOrder set BillStatusID = 64 where PurchaseOrderID = @PurchaseOrderID",
                    UpdateFiledName=Type== "Credit Note"? "": "PurchaseOrderID",
                    dbField=new DBField[]
                    {
                        new DBField() { DBFieldName="IssuedBy",       SetValue=""+html.UserFullName+"" },
                        new DBField() { DBFieldName="IssuedByUserID", SetValue=""+html.UserID+"" },
                        new DBField() { DBFieldName="VendorID",           Value="VendorID",           GetValueFromInputName= true, IsNumber = true },
                        new DBField() { DBFieldName="PurchaseOrderID",    Value="PurchaseOrderID",    GetValueFromInputName= true },
                        new DBField() { DBFieldName=Type=="Credit Note"?"":"PONumber", Value=Type=="Credit Note"?"":"PONumber", GetValueFromInputName= true },
                        new DBField() { DBFieldName="Attention",          Value="Txt_Attention",      GetValueFromInputName= true },
                        new DBField() { DBFieldName="Reference",          Value="TxtReference",       GetValueFromInputName= true },
                        new DBField() { DBFieldName= Type!="Credit Note"?"":"BillNumber", Value=Type!="Credit Note"?"":"BillNumber", GetValueFromInputName= true },
                        new DBField() { DBFieldName="VendorInvoiceNumber",Value="VendorInvoiceNumber",GetValueFromInputName= true },
                        new DBField() { DBFieldName="SubTotal",   Value="SubTotal",   GetValueFromLabelName= true, IsNumber = true },
                        new DBField() { DBFieldName="VAT",        Value="VAT",        GetValueFromLabelName= true, IsNumber = true },
                        new DBField() { DBFieldName="Total",      Value="Total",      GetValueFromLabelName= true, IsNumber = true },
                        new DBField() { DBFieldName="DiscountP",  Value="TxtDisP",    GetValueFromInputName= true, IsNumber = true },
                        new DBField() { DBFieldName="Tax",        Value="Tax",        GetValueFromInputName= true },
                        new DBField() { DBFieldName="Rate",       Value="Tax_Rate",   GetValueFromInputName= true, IsNumber = true },
                        new DBField() { DBFieldName="DiscountUSD",Value="TxtDisUSD",  GetValueFromInputName= true, IsNumber = true },
                        new DBField() { DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true },
                        new DBField() { DBFieldName="AmountPaid", SetValue="0" },
                        new DBField() { DBFieldName="AmountDue",  Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true },
                        new DBField() { DBFieldName="CustomerNotes",  Value="Customer_Notes",  GetValueFromTextAreaName= true },
                        new DBField() { DBFieldName="TermsConditions",Value="TermsConditions", GetValueFromTextAreaName= true },
                        new DBField() { DBFieldName="Type", Value="Type", GetValueFromInputName= true },
                        new DBField() { DBFieldName="BillStatusID", SetValue="64" },
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
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label=Type=="Credit Note"?"Credit Note#": "Bill#",
                            ID=Type=="Credit Note"?"CreditNoteNumber":"BillNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField=Type=="Credit Note"?"CreditNoteNumber": "BillNumber", IsReadOnly = true,
                            isCloned = Clone,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = '"+(Type=="Credit Note"?"Vendor Credit Note":"Bills")+"'; select @Prefix = Prefix from dbo.TblPrefix where Module = '"+(Type=="Credit Note"?"Vendor Credit Note":"Bills")+"' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'" +html.DomainID+"', 1, '', 0, 'BL-{yy}{MM}{dd}-{0000}'",
                            CallBackToControlID=Type=="Credit Note"?"CreditNoteNumber, PrefixCreditNoteNumber": "BillNumber, PrefixBillNumber",
                            UpdatePrefixModule=""+(Type=="Credit Note"?"Vendor Credit Note":"Bills")+"",
                            Prefix=Type=="Credit Note"?"PrefixCreditNoteNumber":"PrefixBillNumber",
                            SetPrefixToDbFieldName=Type=="Credit Note"?"CreditNoteNumber":"BillNumber"
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
                            DivClassCol="col-sm", TypeAheadAddNewFunctionName="AddNewPaymentTerms()",
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
                new FormData() {
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
                new FormData() {
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
                // Category Detail (Expenses & Services)
                new FormData() {
                    FormTitle = "Category Detail",
                    ShowBtnExpans = true,
                    AutoHideCollapse = false,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            UserClassTableResponsive = false,
                            ShowFooter = false,
                            ShowBalance = false,
                            ShowDeposit = false,
                            ShowBankInfo = false,
                            ShowRemark = false,
                            ShowIncluceVAT = false,
                            ShowTotal = false,
                            ShowCustomerNotes = false,
                            ShowTermsConditions = false,

                            GetIDENTITYByFiled = "BillID",
                            PrimaryFieldName = "BillID",
                            TableName = "TblBillsDetail",
                            HtmlTblID = "ExpenseDetails",
                            InputName = "ExpenseDetailsDescription",
                            TRName = "VTR_No",
                            TDName = "VTD_No",
                            ApplyTblTHBackground = false,
                            CustomCondition = "isnull(Type,'') = 'Category'",

                            ScriptCallBackFunction = "Reload_AccountVendorDeposit,TotalAll",
                            dbField = new DBField[]
                            {
                                new DBField() { DBFieldName = "ChartOfAccountID", Value = "AccountVendorDeposit" },
                                new DBField() { DBFieldName = "ItemDescription",  Value = "ExpenseDetailsDescription", GetValueFromTextAreaName = true },
                                new DBField() { DBFieldName = "UnitPrice",        Value = "ExpenseDetailsAmount" },
                                new DBField() { DBFieldName = "QTY",              SetValue = "1" },
                                new DBField() { DBFieldName = "Type",             Value = "Type" }
                            },
                            tblTHead = new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR = "<tr>", Label = "#"
                                },
                                new TblTHead()
                                {
                                     Label = "Account",
                                },
                                new TblTHead()
                                {
                                    Label = "Description", Class = "col-6",
                                },
                                new TblTHead()
                                {
                                     Label = "Amount",
                                     CloseTR = "</tr>"
                                }
                            },
                            tblTBody = new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR = "<tr name=\"VTR_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label = "1", Align = "right", TDName = "VTD_No"
                                        },                                      
                                        new HtmlComboBox()
                                        {
                                            Name = "AccountVendorDeposit",
                                            Class = "input2", IsCustomForm = true,
                                            DivClass = "wrap-input3",
                                            placeholder = "Enter Account",
                                            SQL_CBO = @"SELECT
    coa.ChartOfAccountID ExpensesAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM TblChartOfAccount coa
JOIN TblAccountTypes ct ON ct.AccountTypesID = coa.AccountTypesID
JOIN TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE
    coa.SystemID = " + html.SystemID + @"
    AND coa.WarehouseID = " + html.WarehouseID + @"
ORDER BY AccountNo;",
                                            DBField = "ChartOfAccountID",
                                            Value = "ExpensesAccountID",
                                            DisplayText = "AccountName",
                                            GroupByField = "AccountCategory",
                                            IsKendoMultiColumnComboBox = true,
                                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                                            {
                                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                                            },
                                            KendoMultiColumnComboBoxHeight = 350,
                                        },
                                        new HtmlTextarea()
                                        {
                                            Name = "ExpenseDetailsDescription",
                                            DBField = "ItemDescription",
                                            Class = "input2 CustInfo", IsCustomForm = true,
                                            DivClass = "wrap-input3",
                                            DeleteTDRowFunction = "RemoveTableRow('VTR_No', 'VTD_No', this,'','ExpenseDetailsDescription', 'TotalAll');",                                            
                                            placeholder = "Description",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name = "ExpenseDetailsAmount",
                                            DivClass = "wrap-input3",
                                            Class = "input2 text-center", IsCustomForm = true,
                                            DBField = "Amount", IsAllowInputOnlyNumber = true,
                                            placeholder = "Amount",
                                            HiddenField = "Type", HiddenFieldValue = "Category",
                                            OnChangeFunction = "TotalAll();", OnKeyupFunction = "TotalAll();"
                                        }
                                    },
                                    CloseTR = "</tr>"
                                }
                            }
                        }
                    }
                },
                // Item Details
                new FormData() {
                    FormTitle = "Item Details",
                    ShowBtnExpans = true,
                    AutoHideCollapse = false,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            CustomCondition = "isnull(Type,'') <> 'Category'",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll",
                            FooterColspan = 3,
                            SubTotalColspan = 5,
                            ShowDeposit = false,
                            ShowBalance = false,
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
                                new DBField() { DBFieldName="PurchaseOrderID", SetValue="$(\"#PurchaseOrderID\").val()" },
                                new DBField() { DBFieldName="ItemID",          Value="ItemID" },
                                new DBField() { DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true },
                                new DBField() { DBFieldName="ChartOfAccountID",Value="ChartOfAccountID" },
                                new DBField() { DBFieldName="ItemExpirationDate", Value="ItemExpirationDate", IsDate = true },
                                new DBField() { DBFieldName="Type", Value="Type" },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead() { OpenTR="<tr>", Label="ល.រ" },
                                new TblTHead() { Class="col-5", Label="បរិយាយ" },
                                new TblTHead() { Class="col-3", Label="គណនេយ្យ" },
                                new TblTHead() { Label="បរិមាណ", Class="col-1 text-nowrap" },
                                new TblTHead() { Label="ខ្នាត",   Class="col-1" },
                                new TblTHead() { Label="ថ្លៃឯកតា" },
                                new TblTHead() { Label="តម្លៃលក់" },
                                new TblTHead() { Class="col-2", Label="បញ្ចុះតម្លៃ" },
                                new TblTHead() { Label="សរុប$", Class="col-1", CloseTR="</tr>" },
                                new TblTHead() { OpenTR="<tr>", Label="No." },
                                new TblTHead() { Label="Description" },
                                new TblTHead() { Label="Account" },
                                new TblTHead() { Label="QTY",        Class="text-nowrap" },
                                new TblTHead() { Label="UM",         Class="text-nowrap" },
                                new TblTHead() { Label="Unit Price",  Class="text-nowrap" },
                                new TblTHead() { Label="Sale Price",  Class="text-nowrap" },
                                new TblTHead() { Label="Dis$" },
                                new TblTHead() { Label="Amount$", CloseTR="</tr>" },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel() { Label="1", Align="right", TDName="TDNo" },
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
                                            SQLCallBack=@"select i.UM, i.CostPrice, 
COALESCE(c_inv.ChartOfAccountID, c_exp.ChartOfAccountID, c_pur.ChartOfAccountID) ChartOfAccountID, 
COALESCE(c_inv.AccountNo + ' - ' + c_inv.AccountName, c_exp.AccountNo + ' - ' + c_exp.AccountName, c_pur.AccountNo + ' - ' + c_pur.AccountName) AccountName, 
SellingPrice SalePrice, i.ItemDescription ItemName_Description 
from dbo.TblItem i 
left join dbo.TblChartOfAccount c_inv on c_inv.ChartOfAccountID = i.InventoryChartOfAccountID
left join dbo.TblChartOfAccount c_exp on c_exp.ChartOfAccountID = i.ExpenseAccountID
left join dbo.TblChartOfAccount c_pur on c_pur.ChartOfAccountID = i.PurchaseChartOfAccountID
where i.ItemID = @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID,
                                            DisplayOnFields="UM, UnitPrice, ChartOfAccountID, AccountName, SalePrice, ItemName_Description",
                                            AddCallBackFunction="Reload_AccountName()", InputDescription = true, placeholder="Item Name"
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
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber=true,
                                            OnKeyupFunction="TotalAll();",
                                            HiddenField="PurchaseOrder_ID", HiddenFieldValue="0", Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM", DivClass="wrap-input3",
                                            placeholder="Enter UM", IsReadOnly = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber=true,
                                            OnChangeFunction="TotalAll();", OnKeyupFunction="TotalAll();",
                                            HiddenField="DisP", HiddenFieldValue = "0", Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SalePrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="SalePrice", IsAllowInputOnlyNumber=true,
                                            OnChangeFunction="TotalAll();", OnKeyupFunction="TotalAll();"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber=true,
                                            OnChangeFunction="TotalAll();", OnKeyupFunction="TotalAll();"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber=true,
                                            HiddenField="Type", HiddenFieldValue= BillType,
                                            OnKeyupFunction="AmountKeyup(this);"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };

            html.SaveChangeAndClose = false;
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";



            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View("~/Views/EnterBill/Create.cshtml");
        }

      
        [HttpGet]
        public JsonResult GetChartOfAccounts(string searchText = "")
        {
            HTML html = new HTML();
            string sql = @"
                SELECT 
                    c.ChartOfAccountID, 
                    c.AccountNo, 
                    c.AccountName, 
                    ISNULL(at.AccountTypes, ISNULL(c.AccountType, '')) AS AccountType,
                    (c.AccountNo + ' - ' + c.AccountName) AS DisplayName
                FROM dbo.TblChartOfAccount c
                LEFT JOIN dbo.TblAccountTypes at ON at.AccountTypesID = c.AccountTypesID
                WHERE c.SystemID = " + html.SystemID + @" 
                  AND c.WarehouseID = " + html.WarehouseID + @"
                  AND (
                        @SearchText = '' 
                        OR LOWER(c.AccountNo + ' - ' + c.AccountName) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(ISNULL(at.AccountTypes, ISNULL(c.AccountType, ''))) LIKE '%' + LOWER(@SearchText) + '%'
                      )
                ORDER BY ISNULL(at.AccountTypes, ISNULL(c.AccountType, '')), c.AccountNo";

            Dictionary<string, string> p = new Dictionary<string, string>
            {
                { "SearchText", (searchText ?? "").Trim() }
            };
            DataSet ds = html.LoadData(sql, false, p);
            return Json(new { success = true, data = ds.ToJsonString() }, JsonRequestBehavior.AllowGet);
        }

      
        public JsonResult GetOpenPOs(string vendorId = "")
        {
            HTML html = new HTML();
            int vId = 0;
            if (!string.IsNullOrEmpty(vendorId))
            {
                decimal d;
                if (decimal.TryParse(vendorId, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                {
                    vId = (int)d;
                }
            }

            string sql = @"
                SELECT 
                    p.PurchaseOrderID,
                    p.PONumber,
                    p.VendorID,
                    (v.VendorCode + ' - ' + v.VendorName) AS VendorName,
                    FORMAT(p.PODate, 'dd/MM/yyyy') AS PODate,
                    p.GrandTotal AS TotalAmount,
                    ISNULL(dep.DepositAmount, 0) AS DepositAmount,
                    ISNULL(dep.PVNumber, '') AS PVNumber,
                    (p.GrandTotal - ISNULL(dep.DepositAmount, 0)) AS BalanceDue,
                    ISNULL(p.CustomerNotes, '') AS Memo,
                    STUFF((
                        SELECT TOP 3 ', ' + pd2.ItemName
                        FROM dbo.TblPurchaseOrderDetail pd2
                        WHERE pd2.PurchaseOrderID = p.PurchaseOrderID
                        FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, '') AS ItemSummary
                FROM dbo.TblPurchaseOrder p
                INNER JOIN dbo.TblVendor v ON v.VendorID = p.VendorID
                OUTER APPLY (
                    SELECT TOP 1 
                        ISNULL(NULLIF(e.PaidAmount, 0), e.TotalAmount) AS DepositAmount,
                        ISNULL(e.[No], e.PaymentNo) AS PVNumber
                    FROM dbo.Expenses e
                    WHERE (
                            (e.ExpenseTransactionType = 'Vendor Deposit' AND (e.Reference = p.PONumber OR e.Reference LIKE '%' + p.PONumber + '%'))
                            OR (p.PONumber <> '' AND (e.Reference = p.PONumber OR e.Reference LIKE '%' + p.PONumber + '%' OR EXISTS (SELECT 1 FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID AND (ed.RefNo = p.PONumber OR ed.RefNo LIKE '%' + p.PONumber + '%'))))
                          )
                      AND ISNULL(e.PaymentStatus, '') <> 'Applied'
                      AND (e.PayeeID = p.VendorID OR ISNULL(p.VendorID, 0) = 0 OR ISNULL(e.PayeeID, 0) = 0)
                      AND (ISNULL(e.PaidAmount, 0) > 0 OR (e.ExpenseTransactionType = 'Vendor Deposit' AND ISNULL(e.TotalAmount, 0) > 0))
                      AND e.SystemID = p.SystemID AND e.WarehouseID = p.WarehouseID
                    ORDER BY e.ExpenseID DESC
                ) dep
                WHERE p.POStatus = 90
                  AND p.SystemID = " + html.SystemID + @"
                  AND p.WarehouseID = " + html.WarehouseID + @"
                  AND (@VendorID = '0' OR @VendorID = '' OR p.VendorID = CAST(@VendorID AS INT))
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.TblBills b 
                      WHERE b.PurchaseOrderID = p.PurchaseOrderID
                        AND b.SystemID = p.SystemID 
                        AND b.WarehouseID = p.WarehouseID
                  )
                ORDER BY p.PurchaseOrderID DESC";

            Dictionary<string, string> p = new Dictionary<string, string>
            {
                { "VendorID", vId.ToString() }
            };

            DataSet ds = html.LoadData(sql, false, p);
            return Json(new { success = true, data = ds.ToJsonString() }, JsonRequestBehavior.AllowGet);
        }

      
        public JsonResult GetOpenPOsByVendor(string vendorId)
        {
            HTML html = new HTML();
            int vId = 0;
            if (!string.IsNullOrEmpty(vendorId))
            {
                decimal d;
                if (decimal.TryParse(vendorId, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                {
                    vId = (int)d;
                }
            }

            string SQL = @"
                SELECT
                    p.PurchaseOrderID,
                    p.PONumber,
                    FORMAT(p.PODate, 'dd-MMM-yyyy')              AS PODate,
                    p.GrandTotal                                  AS Total,
                    ISNULL(p.Balance, p.GrandTotal)               AS Balance,
                    STUFF((
                        SELECT TOP 3 ', ' + pd2.ItemName
                        FROM dbo.TblPurchaseOrderDetail pd2
                        WHERE pd2.PurchaseOrderID = p.PurchaseOrderID
                        FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, '') AS ItemSummary
                FROM dbo.TblPurchaseOrder p
                WHERE p.VendorID    = @VendorID
                  AND p.POStatus    = 90
                  AND p.SystemID    = @SystemID
                  AND p.WarehouseID = @WarehouseID
                  AND NOT EXISTS (
                        SELECT 1 FROM dbo.TblBills b
                        WHERE b.PurchaseOrderID = p.PurchaseOrderID
                    )
                ORDER BY p.PurchaseOrderID DESC";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "VendorID",    vId.ToString() },
                { "SystemID",    html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

       
        public JsonResult LoadPO(string PONumber, string Type = "")
        {
            HTML html = new HTML();
            string SQL = @"select p.VendorID, p.PurchaseOrderID, p.DeliverTo, p.Tax, p.Rate, p.CurrencyID, p.CurrencyName,
pd.ItemID, pd.ItemName, pd.ItemDescription, 
COALESCE(c_inv.ChartOfAccountID, c_exp.ChartOfAccountID, c_pur.ChartOfAccountID) ChartOfAccountID, 
COALESCE(c_inv.AccountNo + ' - ' + c_inv.AccountName, c_exp.AccountNo + ' - ' + c_exp.AccountName, c_pur.AccountNo + ' - ' + c_pur.AccountName) AccountName, 
p.ContactName, p.ContactPhone,
pd.QTY, pd.UnitPrice, pd.DisP, pd.DisUSD, p.WID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.ClassID,
p.ClassName, p.PODate, p.PaymentTerms, format(p.PaymentDueDate, 'dd/MM/yyyy')PaymentDueDate, p.NumberofDays,
pd.UM, 0 AS Deposit, p.GrandTotal AS Balance, pd.SalePrice, 'Bill' AS [Type]
from dbo.TblPurchaseOrder p
inner join dbo.TblVendor v on v.VendorID = p.VendorID
inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = p.PurchaseOrderID
left join dbo.TblItem i on i.ItemID = pd.ItemID
left join dbo.TblChartOfAccount c_inv on c_inv.ChartOfAccountID = i.InventoryChartOfAccountID
left join dbo.TblChartOfAccount c_exp on c_exp.ChartOfAccountID = i.ExpenseAccountID
left join dbo.TblChartOfAccount c_pur on c_pur.ChartOfAccountID = i.PurchaseChartOfAccountID
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
                order by pd.ItemID";
            }

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "PONumber",    PONumber },
                { "SystemID",    html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  AUTOMATIC GENERAL LEDGER POSTING (Dr Expense / Cr 210000 A/P)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [HttpGet]
        public JsonResult PostBillToGL(int billId = 0, string billNumber = "")
        {
            HTML html = new HTML();
            if (billId <= 0 && !string.IsNullOrEmpty(billNumber))
            {
                string findSql = "SELECT TOP 1 BillID FROM dbo.TblBills WHERE BillNumber = '" + billNumber.Replace("'", "''") + "' AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID + " ORDER BY BillID DESC";
                DataSet dsFind = html.LoadData(findSql);
                if (dsFind != null && dsFind.Tables.Count > 0 && dsFind.Tables[0].Rows.Count > 0)
                {
                    billId = Convert.ToInt32(dsFind.Tables[0].Rows[0]["BillID"]);
                }
            }

            if (billId <= 0)
            {
                return Json(new { success = false, message = "Invalid BillID or BillNumber." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // 1. Fetch Bill Header
                string sqlBill = @"
                    SELECT 
                        b.BillID,
                        b.BillNumber,
                        b.BillDate,
                        b.VendorID,
                        ISNULL(v.VendorName, '') AS VendorName,
                        ISNULL(b.GrandTotal, 0) AS GrandTotal,
                        ISNULL(b.SubTotal, 0) AS SubTotal,
                        ISNULL(b.VAT, 0) AS VAT,
                        ISNULL(b.DiscountUSD, 0) AS DiscountUSD,
                        ISNULL(b.PONumber, '') AS PONumber,
                        b.SystemID,
                        b.WarehouseID,
                        ISNULL(b.DomainID, '') AS DomainID,
                        ISNULL(b.ClassID, 0) AS ClassID,
                        ISNULL(b.ClassName, '') AS ClassName,
                        ISNULL(b.UserID, 0) AS UserID
                    FROM dbo.TblBills b
                    LEFT JOIN dbo.TblVendor v ON v.VendorID = b.VendorID
                    WHERE b.BillID = " + billId;

                DataSet dsBill = html.LoadData(sqlBill);
                if (dsBill == null || dsBill.Tables.Count == 0 || dsBill.Tables[0].Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Bill not found in TblBills." }, JsonRequestBehavior.AllowGet);
                }

                DataRow bRow = dsBill.Tables[0].Rows[0];
                billNumber = (bRow["BillNumber"] != DBNull.Value) ? bRow["BillNumber"].ToString() : (billNumber ?? "");
                DateTime billDate = (bRow["BillDate"] != DBNull.Value) ? Convert.ToDateTime(bRow["BillDate"]) : DateTime.Now;
                int vendorId = (bRow["VendorID"] != DBNull.Value) ? Convert.ToInt32(bRow["VendorID"]) : 0;
                string vendorName = (bRow["VendorName"] != DBNull.Value) ? bRow["VendorName"].ToString() : "";
                decimal grandTotal = (bRow["GrandTotal"] != DBNull.Value) ? Convert.ToDecimal(bRow["GrandTotal"]) : 0m;
                decimal subTotal = (bRow["SubTotal"] != DBNull.Value) ? Convert.ToDecimal(bRow["SubTotal"]) : 0m;
                decimal vat = (bRow["VAT"] != DBNull.Value) ? Convert.ToDecimal(bRow["VAT"]) : 0m;
                decimal discountUSD = (bRow["DiscountUSD"] != DBNull.Value) ? Convert.ToDecimal(bRow["DiscountUSD"]) : 0m;
                string poNumber = (bRow["PONumber"] != DBNull.Value) ? bRow["PONumber"].ToString() : "";
                int sysId = (bRow["SystemID"] != DBNull.Value) ? Convert.ToInt32(bRow["SystemID"]) : Convert.ToInt32(html.SystemID);
                int whId = (bRow["WarehouseID"] != DBNull.Value) ? Convert.ToInt32(bRow["WarehouseID"]) : Convert.ToInt32(html.WarehouseID);
                string domainId = (bRow["DomainID"] != DBNull.Value && bRow["DomainID"].ToString() != "") ? bRow["DomainID"].ToString() : html.DomainID;
                int classId = (bRow["ClassID"] != DBNull.Value) ? Convert.ToInt32(bRow["ClassID"]) : 0;
                int userId = (bRow["UserID"] != DBNull.Value) ? Convert.ToInt32(bRow["UserID"]) : 0;
                if (userId == 0 && !string.IsNullOrEmpty(html.UserID)) int.TryParse(html.UserID, out userId);

                if (grandTotal <= 0)
                {
                    return Json(new { success = false, message = "Bill GrandTotal is 0 or negative. Skipping GL posting." }, JsonRequestBehavior.AllowGet);
                }

                // 2. Fetch Bill Details
                string sqlDetails = @"
                    SELECT 
                        bd.BillDetailID,
                        bd.ItemID,
                        ISNULL(bd.ItemName, '') AS ItemName,
                        ISNULL(bd.ChartOfAccountID, 0) AS ChartOfAccountID,
                        ISNULL(bd.AccountName, '') AS AccountName,
                        ISNULL(bd.Amount, 0) AS Amount,
                        ISNULL(bd.DisUSD, 0) AS DisUSD
                    FROM dbo.TblBillsDetail bd
                    WHERE bd.BillID = " + billId;

                DataSet dsDetails = html.LoadData(sqlDetails);
                DataTable dtDetails = (dsDetails != null && dsDetails.Tables.Count > 0) ? dsDetails.Tables[0] : null;

                // 3. Build Transaction & Journal Entry SQL Script
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("BEGIN TRANSACTION;");

                // Step 3a: Remove any previous GL transaction for this Bill (for updates/re-saves)
                sb.AppendFormat(@"
                    DECLARE @OldTxID INT;
                    SELECT TOP 1 @OldTxID = TransactionID 
                    FROM dbo.TblAccount_Transaction 
                    WHERE (InvoiceNo = N'{0}' OR ExpenseNo = N'{0}')
                      AND [Type] = 'Bill'
                      AND SystemID = {1} AND WarehouseID = {2};

                    IF (@OldTxID IS NOT NULL AND @OldTxID > 0)
                    BEGIN
                        DELETE FROM dbo.TblAccount_JournalEntries WHERE TransactionID = @OldTxID;
                        DELETE FROM dbo.TblAccount_Transaction WHERE TransactionID = @OldTxID;
                    END
                ", billNumber.Replace("'", "''"), sysId, whId);

                // Step 3b: Insert into TblAccount_Transaction (Header)
                sb.AppendFormat(@"
                    DECLARE @NewTxID INT;
                    DECLARE @JournalNo NVARCHAR(50);
                    EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] 'JV{{yy}}{{MM}}_{{00000}}', 'JournalNo', {0}, {1}, @JournalNo OUTPUT, 1, 0, '{2}', '{3:yyyy-MM-dd HH:mm:ss}';
                    IF (@JournalNo IS NULL OR @JournalNo = '')
                        SET @JournalNo = 'JV' + FORMAT(CAST('{3:yyyy-MM-dd}' AS DATETIME), 'yyMM') + '_00001';

                    INSERT INTO dbo.TblAccount_Transaction (
                        InvoiceNo, ExpenseNo, TransactionNo,
                        TransactionDate, InvoiceDate, Amount,
                        Description, [Type], TransactionType,
                        VendorID, PayeeID, StatusID,
                        SystemID, WarehouseID, DomainID, ClassNameID,
                        CreateByUserID, CreateDate, JournalNo, IsPosted
                    ) VALUES (
                        N'{4}', N'{4}', N'{4}',
                        '{3:yyyy-MM-dd HH:mm:ss}', '{3:yyyy-MM-dd HH:mm:ss}', {5},
                        N'Vendor Bill {4}' + CASE WHEN N'{6}' <> '' THEN ' (' + N'{6}' + ')' ELSE '' END + CASE WHEN N'{7}' <> '' THEN ' - PO: ' + N'{7}' ELSE '' END,
                        'Bill', 'Vendor',
                        {8}, {8}, 10,
                        {0}, {1}, '{2}', {9},
                        {10}, GETDATE(), @JournalNo, 1
                    );
                    SET @NewTxID = SCOPE_IDENTITY();
                ",
                    sysId,
                    whId,
                    domainId,
                    billDate,
                    billNumber.Replace("'", "''"),
                    grandTotal,
                    vendorName.Replace("'", "''"),
                    poNumber.Replace("'", "''"),
                    vendorId,
                    classId,
                    userId
                );

                // Step 3c: Insert Debit (Dr) Journal Lines for each item / expense category
                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    foreach (DataRow dRow in dtDetails.Rows)
                    {
                        decimal lineAmt = Convert.ToDecimal(dRow["Amount"]);
                        if (lineAmt <= 0) continue;

                        int lineAccountId = Convert.ToInt32(dRow["ChartOfAccountID"]);
                        int itemId = (dRow["ItemID"] != DBNull.Value) ? Convert.ToInt32(dRow["ItemID"]) : 0;
                        string itemName = dRow["ItemName"].ToString().Replace("'", "''");

                        sb.AppendFormat(@"
                            DECLARE @LineAccID_{0} INT = {1};
                            IF (@LineAccID_{0} IS NULL OR @LineAccID_{0} = 0)
                            BEGIN
                                -- Resolve from Item if missing on line
                                SELECT TOP 1 @LineAccID_{0} = COALESCE(ExpenseAccountID, InventoryChartOfAccountID, PurchaseChartOfAccountID)
                                FROM dbo.TblItem WHERE ItemID = {2} AND SystemID = {3} AND WarehouseID = {4};
                            END
                            IF (@LineAccID_{0} IS NULL OR @LineAccID_{0} = 0)
                            BEGIN
                                -- Fallback to default Cost of Service (510000) or first expense account
                                SELECT TOP 1 @LineAccID_{0} = ChartOfAccountID 
                                FROM dbo.TblChartOfAccount 
                                WHERE AccountNo = '510000' AND SystemID = {3} AND WarehouseID = {4};
                            END
                            IF (@LineAccID_{0} IS NULL OR @LineAccID_{0} = 0)
                            BEGIN
                                SELECT TOP 1 @LineAccID_{0} = ChartOfAccountID 
                                FROM dbo.TblChartOfAccount 
                                WHERE AccountTypesID IN (5, 10) AND SystemID = {3} AND WarehouseID = {4};
                            END

                            INSERT INTO dbo.TblAccount_JournalEntries (
                                TransactionID, AccountID, Debit, Credit, Description,
                                TransactionDate, EntryDate, [Type], InvoiceNo, ExpenseNo, JournalNo,
                                SystemID, WarehouseID, DomainID, ClassNameID, CreateByUserID, CreateDate, IsPosted, ItemID
                            ) VALUES (
                                @NewTxID, @LineAccID_{0}, {5}, 0, N'Bill {6} - ' + N'{7}',
                                '{8:yyyy-MM-dd HH:mm:ss}', '{8:yyyy-MM-dd HH:mm:ss}', 'Bill', N'{6}', N'{6}', @JournalNo,
                                {3}, {4}, '{9}', {10}, {11}, GETDATE(), 1, {2}
                            );
                        ",
                            dRow["BillDetailID"],
                            lineAccountId,
                            itemId,
                            sysId,
                            whId,
                            lineAmt,
                            billNumber.Replace("'", "''"),
                            itemName,
                            billDate,
                            domainId,
                            classId,
                            userId
                        );
                    }
                }
                else
                {
                    // If bill has no detail rows, post single line for net expense
                    decimal netExpense = grandTotal - vat + discountUSD;
                    sb.AppendFormat(@"
                        DECLARE @DefAccID INT;
                        SELECT TOP 1 @DefAccID = ChartOfAccountID 
                        FROM dbo.TblChartOfAccount 
                        WHERE AccountNo = '510000' AND SystemID = {0} AND WarehouseID = {1};
                        IF (@DefAccID IS NULL OR @DefAccID = 0)
                            SELECT TOP 1 @DefAccID = ChartOfAccountID FROM dbo.TblChartOfAccount WHERE AccountTypesID IN (5, 10) AND SystemID = {0} AND WarehouseID = {1};

                        INSERT INTO dbo.TblAccount_JournalEntries (
                            TransactionID, AccountID, Debit, Credit, Description,
                            TransactionDate, EntryDate, [Type], InvoiceNo, ExpenseNo, JournalNo,
                            SystemID, WarehouseID, DomainID, ClassNameID, CreateByUserID, CreateDate, IsPosted
                        ) VALUES (
                            @NewTxID, @DefAccID, {2}, 0, N'Bill {3} - Expense',
                            '{4:yyyy-MM-dd HH:mm:ss}', '{4:yyyy-MM-dd HH:mm:ss}', 'Bill', N'{3}', N'{3}', @JournalNo,
                            {0}, {1}, '{5}', {6}, {7}, GETDATE(), 1
                        );
                    ",
                        sysId,
                        whId,
                        netExpense,
                        billNumber.Replace("'", "''"),
                        billDate,
                        domainId,
                        classId,
                        userId
                    );
                }

                // Step 3d: Insert Debit (Dr) for VAT Input if applicable
                if (vat > 0)
                {
                    sb.AppendFormat(@"
                        DECLARE @VATAccID INT;
                        SELECT TOP 1 @VATAccID = ChartOfAccountID 
                        FROM dbo.TblChartOfAccount 
                        WHERE (AccountNo = '140001' OR AccountName LIKE '%VAT Input%')
                          AND SystemID = {0} AND WarehouseID = {1};

                        IF (@VATAccID IS NOT NULL AND @VATAccID > 0)
                        BEGIN
                            INSERT INTO dbo.TblAccount_JournalEntries (
                                TransactionID, AccountID, Debit, Credit, Description,
                                TransactionDate, EntryDate, [Type], InvoiceNo, ExpenseNo, JournalNo,
                                SystemID, WarehouseID, DomainID, ClassNameID, CreateByUserID, CreateDate, IsPosted
                            ) VALUES (
                                @NewTxID, @VATAccID, {2}, 0, N'Bill {3} - VAT Input',
                                '{4:yyyy-MM-dd HH:mm:ss}', '{4:yyyy-MM-dd HH:mm:ss}', 'Bill', N'{3}', N'{3}', @JournalNo,
                                {0}, {1}, '{5}', {6}, {7}, GETDATE(), 1
                            );
                        END
                    ",
                        sysId,
                        whId,
                        vat,
                        billNumber.Replace("'", "''"),
                        billDate,
                        domainId,
                        classId,
                        userId
                    );
                }

                // Step 3e: Insert Credit (Cr) for Purchase Discount if header discount exists
                if (discountUSD > 0)
                {
                    sb.AppendFormat(@"
                        DECLARE @DiscAccID INT;
                        SELECT TOP 1 @DiscAccID = ChartOfAccountID 
                        FROM dbo.TblChartOfAccount 
                        WHERE (AccountNo = '512000' OR AccountName LIKE '%Purchase Discount%')
                          AND SystemID = {0} AND WarehouseID = {1};

                        IF (@DiscAccID IS NOT NULL AND @DiscAccID > 0)
                        BEGIN
                            INSERT INTO dbo.TblAccount_JournalEntries (
                                TransactionID, AccountID, Debit, Credit, Description,
                                TransactionDate, EntryDate, [Type], InvoiceNo, ExpenseNo, JournalNo,
                                SystemID, WarehouseID, DomainID, ClassNameID, CreateByUserID, CreateDate, IsPosted
                            ) VALUES (
                                @NewTxID, @DiscAccID, 0, {2}, N'Bill {3} - Purchase Discount',
                                '{4:yyyy-MM-dd HH:mm:ss}', '{4:yyyy-MM-dd HH:mm:ss}', 'Bill', N'{3}', N'{3}', @JournalNo,
                                {0}, {1}, '{5}', {6}, {7}, GETDATE(), 1
                            );
                        END
                    ",
                        sysId,
                        whId,
                        discountUSD,
                        billNumber.Replace("'", "''"),
                        billDate,
                        domainId,
                        classId,
                        userId
                    );
                }

                // Step 3f: Insert Credit (Cr) Accounts Payable (210000) for GrandTotal
                sb.AppendFormat(@"
                    DECLARE @APAccountID INT;
                    SELECT TOP 1 @APAccountID = ChartOfAccountID 
                    FROM dbo.TblChartOfAccount 
                    WHERE AccountNo = '210000' AND SystemID = {0} AND WarehouseID = {1};

                    IF (@APAccountID IS NULL OR @APAccountID = 0)
                    BEGIN
                        SELECT TOP 1 @APAccountID = ChartOfAccountID 
                        FROM dbo.TblChartOfAccount 
                        WHERE (AccountName LIKE '%Account Payable%' OR AccountName LIKE '%Accounts Payable%')
                          AND SystemID = {0} AND WarehouseID = {1};
                    END
                    IF (@APAccountID IS NULL OR @APAccountID = 0) SET @APAccountID = 464;

                    INSERT INTO dbo.TblAccount_JournalEntries (
                        TransactionID, AccountID, Debit, Credit, Description,
                        TransactionDate, EntryDate, [Type], InvoiceNo, ExpenseNo, JournalNo,
                        SystemID, WarehouseID, DomainID, ClassNameID, CreateByUserID, CreateDate, IsPosted
                    ) VALUES (
                        @NewTxID, @APAccountID, 0, {2}, N'Bill {3} from ' + N'{4}',
                        '{5:yyyy-MM-dd HH:mm:ss}', '{5:yyyy-MM-dd HH:mm:ss}', 'Bill', N'{3}', N'{3}', @JournalNo,
                        {0}, {1}, '{6}', {7}, {8}, GETDATE(), 1
                    );
                ",
                    sysId,
                    whId,
                    grandTotal,
                    billNumber.Replace("'", "''"),
                    vendorName.Replace("'", "''"),
                    billDate,
                    domainId,
                    classId,
                    userId
                );

                sb.AppendLine("COMMIT TRANSACTION;");
                sb.AppendLine("SELECT @NewTxID AS TransactionID, @JournalNo AS JournalNo;");

                DataSet dsPost = html.LoadData(sb.ToString());
                string txId = (dsPost != null && dsPost.Tables.Count > 0 && dsPost.Tables[0].Rows.Count > 0)
                    ? dsPost.Tables[0].Rows[0]["TransactionID"].ToString()
                    : "0";
                string jNo = (dsPost != null && dsPost.Tables.Count > 0 && dsPost.Tables[0].Rows.Count > 0)
                    ? dsPost.Tables[0].Rows[0]["JournalNo"].ToString()
                    : "";

                return Json(new { 
                    success = true, 
                    message = "Bill posted to General Ledger successfully.", 
                    billId = billId, 
                    billNumber = billNumber, 
                    transactionId = txId, 
                    journalNo = jNo,
                    grandTotal = grandTotal 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error posting Bill to GL: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
