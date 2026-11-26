using MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddClassController : Controller
    {
        // GET: AddClass
        [Authorize(Roles = "Add Class, Edit Class, Delete Class")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Class"
            };
            if (html.HasPermission("Add Class"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Class"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Class"))
                html.AllowDelete = true;

            html.AddButton = "Add Class";
            html.DialogBoxWidth = "auto";
            html.AddURL = "AddClass/CreateClass";
            html.TableName = "TblClass";
            html.PrimaryField = "ClassID";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ClassName",
                SelectValue = "ClassID",
                DisplayField = "ClassName",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true
            };
            html.GH.SQL = "select ROW_NUMBER() over(order by c.ClassID desc) [No], c.ClassID, c.ClassName, c.FullName, c.Memo, s.[Status], c.PrefixInvoice, c.StartNumberInvoice, c.PrefixSO, c.StartNumberSO, c.PrefixQuote, c.FileLogo from dbo.TblClass c inner join dbo.TblStatus s on s.StatusID = c.StatusID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
            html.OrderBy = "ClassID desc";
            html.IsHaveWhereCondition = true;
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                 new GridField() { Header = "Logo", FieldName = "FileLogo", ImagePath="Uploads/ClassLogo/" },
                new GridField() { Header = "Class Name", FieldName = "ClassName" },
                new GridField() { Header = "Full Name", FieldName = "FullName" },
                new GridField() { Header = "Prefix Invoice No", FieldName = "PrefixInvoice" },
                new GridField() { Header = "Start Invoice Number", FieldName = "StartNumberInvoice", TextAlign="right" },
                new GridField() { Header = "Prefix SO", FieldName = "PrefixSO" },
                new GridField() { Header = "Start SO Number", FieldName = "StartNumberSO", TextAlign="right" },
                new GridField() { Header = "Prefix Quote", FieldName = "PrefixQuote" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Status", FieldName = "Status" },

                new GridField() { Header = "Action", FieldName = "ClassID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Class, Edit Class")]
        public ActionResult CreateClass(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateClass",
                TableColumn = 2,
                UseTable = true,
                FieldIDFocus = "TxtClassName",
                TableName = "TblClass",
                PrimaryField = "ClassID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Class Name:",
                ID = "TxtClassName",
                DBField = "ClassName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company EN Name:",
                ID = "TxtFullName",
                DBField = "FullName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company KH Name:",
                ID = "TxtKHName",
                DBField = "KHName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in (40,43)",
                DisplayText = "Status",
                Value = "StatusID",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Invoice Number :",
                ID = "TxtStartNumberInvoice",
                DBField = "StartNumberInvoice",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "CISP" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yy") + "-{000000}",
                Label = "Prefix InvoiceNo:",
                ID = "TxtPrefixInvoice",
                DBField = "PrefixInvoice",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start SO Number:",
                ID = "TxtStartNumberSO",
                DBField = "StartNumberSO",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "SO_CISP" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yy") + "-{000000}",
                Label = "Prefix SO:",
                ID = "TxtPrefixSO",
                DBField = "PrefixSO",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Quote Number:",
                ID = "TxtStartNumberQuote",
                DBField = "StartNumberQuote",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "Q/" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yy") + "-{000000}",
                Label = "Prefix Quote:",
                ID = "TxtPrefixQuote",
                DBField = "PrefixQuote",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start PO Number:",
                ID = "TxtStartNumberPO",
                DBField = "StartNumberPO",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "PO" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yy") + "-{000000}",
                Label = "Prefix PO:",
                ID = "TxtPrefixPO",
                DBField = "PrefixPO",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start CM Number:",
                ID = "TxtStartNumberCM",
                DBField = "StartNumberCM",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "CM" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yy") + "-{000000}",
                Label = "Prefix Commercial Invoice:",
                ID = "TxtPrefixCommercial",
                DBField = "PrefixCommercial",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Credit Number:",
                ID = "TxtStartNumberCredit",
                DBField = "StartNumberCreditNote",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "CN/" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yy") + "-{000000}",
                Label = "Prefix Credit Note:",
                ID = "TxtPrefixCreditNote",
                DBField = "PrefixCreditNote",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Ex Number:",
                ID = "TxtStartPrefixEx",
                DBField = "StartPrefixEx",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "EX" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yyMM") + "-{000000}",
                Label = "Prefix Ex No:",
                ID = "TxtPrefixExNo",
                DBField = "PrefixExpense",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start PV Number:",
                ID = "TxtStartPrefixPV",
                DBField = "StartPrefixPV",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "PV" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yyMM") + "-{000000}",
                Label = "Prefix PV No:",
                ID = "TxtPrefixPVNo",
                DBField = "PrefixPV",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Journal Entry:",
                ID = "TxtStartJournalEntry",
                DBField = "StartJournalEntry",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "ISP" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yyMM") + "-{000000}",
                Label = "Journal Entry No:",
                ID = "TxtJournalEntryNo",
                DBField = "JournalEntryNo",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Advance Expense:",
                ID = "TxtStartAdvanceExpenseNo",
                DBField = "StartAdvanceExpenseNo",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "NSMVAT" + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yyMM") + "-{000000}",
                Label = "Advance Expense No:",
                ID = "TxtAdvanceExpenseNo",
                DBField = "AdvanceExpenseNo",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "RF{yy}{MM}-{00000}",
                Label = "Prefix Request Form:",
                ID = "TxtPrefixRF",
                DBField = "PrefixRequestForm",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start RF:",
                ID = "TxtStartRF",
                DBField = "StartRF",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "Q/{yy}-{000000}",
                Label = "Prefix Quotation:",
                ID = "TxtPrefixQuote",
                DBField = "PrefixQuotation",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Quote Number:",
                ID = "TxtStartQuote",
                DBField = "StartQuoteNumber",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "RV{yy}/{000000}",
                Label = "Prefix Collection:",
                ID = "TxtPrefixCollection",
                DBField = "PrefixCollection",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "0",
                Label = "Start Collection Number:",
                ID = "TxtStartCollection",
                DBField = "StartCollectionNumber",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo",
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Logo:",
                ID = "FileLogo",
                DBField = "FileLogo",
                DestinationFile = "Uploads/ClassLogo",
                AcceptImageOnly = true,
                ISFile = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}