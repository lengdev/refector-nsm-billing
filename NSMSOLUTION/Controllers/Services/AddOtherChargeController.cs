using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddOtherChargeController : Controller
    {
        // GET: AddOtherCharge
        [Authorize(Roles = "Add Other Charge, Edit Other Charge, Delete Other Charge")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Other Charge"
            };
            if (html.HasPermission("Add Other Charge"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Other Charge"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Other Charge"))
                html.AllowDelete = true;

            html.AddButton = "Add Other Charge";
            html.AddURL = "AddOtherCharge/CreateOtherCharge";
            html.TableName = "TblOtherCharge";
            html.PrimaryField = "OtherChargeID";
            html.OrderBy = "OtherChargeID desc";
            html.DialogBoxHeight = 500;
            html.DialogBoxWidth = "1000";
            html.IsHaveWhereCondition = true;

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtServiceName",
                Label = "Service Name:",
                DBField = "oc.ServiceID",
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ServiceName",
                DisplayText = "ServiceName",
                Value = "ServiceID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Other Charge Item",
                SQL = "select distinct top 10 OtherChargeItem from dbo.TblOtherCharge where lower(OtherChargeItem) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by OtherChargeItem",
                //SelectValue = "OtherChargeID",
                //DisplayField = "OtherChargeItem",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("oc.OtherChargeItem"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select ROW_NUMBER() over (order by s.ServiceName) [No],
oc.OtherChargeID, s.ServiceName, oc.OtherChargeItem, oc.OtherChargeDescription [Description], coa1.AccountNo + ' - ' + coa1.AccountName ExpenseAccount, isnull(oc.CostPrice,0) CostPrice,
coa.AccountNo + ' - ' + coa.AccountName IncomeAccount, isnull(oc.Fee,0) SalesPrice, oc.UM, oc.[Status], oc.CreateBy, oc.CreateDate,
isnull(STUFF((
    select ',' + 
    case 
        when FileName like '%/%' then right(FileName, charindex('/', reverse(FileName)) - 1) 
        else FileName 
    end 
    from dbo.TblOtherChargeImage 
    where ReferenceID = oc.OtherChargeID AND ReferenceType = 'OtherCharge'
    order by OtherChargeImageID
    for xml path('')
), 1, 1, ''), '') AttachmentFileName
from dbo.TblOtherCharge oc
inner join dbo.TblServices s on s.ServiceID = oc.ServiceID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = oc.IncomeAccountID
left join dbo.TblChartOfAccount coa1 on coa1.ChartOfAccountID = oc.ExpenseAccountID
where oc.SystemID = " + html.SystemID + " and oc.WarehouseID = " + html.WarehouseID + " and oc.DomainID like '%" + html.DomainID + "%'";
            html.OrderBy = " s.ServiceName";
            html.GH.SQL = sql;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right", DeletedCallBackFunction = "CleanupImagesJS(DataID, responetext);" },
                new GridField() { Header = "Service Name", FieldName = "ServiceName", IsGroupByField = true, Colspan = 13 },
                new GridField() { Header = "Other Charge", FieldName = "OtherChargeItem" },
                new GridField() { Header = "Description", FieldName = "Description" ,IsHiddenField=html.SystemID == "13",Width="300px", ClassName="truncate-cell"}, // aliased as [Description] in SQL
                new GridField() { Header = "UM", FieldName = "UM", TextAlign = "center" },
                //new GridField() { Header = "Expense Account", FieldName = "ExpenseAccount" },
                new GridField() { Header = "Cost Price", FieldName = "CostPrice", TextAlign = "right", Format="#,#0.00" },                
                //new GridField() { Header = "Income Account", FieldName = "IncomeAccount" },
                new GridField() { Header = "Sales Price", FieldName = "SalesPrice", TextAlign = "right", Format="#,#0.00" },
                new GridField() { Header = "Attachment", FieldName = "AttachmentFileName", ImagePath="Uploads/Services/", TextAlign="center" ,IsHiddenField=html.SystemID == "13" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Created By", FieldName = "CreateBy" },
                new GridField() { Header = "Created Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yy" },
                new GridField() { Header = "Action", FieldName = "OtherChargeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Other Charge, Edit Other Charge")]
        public ActionResult CreateOtherCharge(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                UseTable = true,
                TableColumn = 2,
                FormID = "CreateOtherCharge",
                FieldIDFocus = "TxtOtherChargeItem",
                TableName = "TblOtherCharge",
                PrimaryField = "OtherChargeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Service Name:",
                ID = "TxtServiceName",
                DBField = "ServiceID",
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ServiceName",
                Value = "ServiceID",
                DisplayText = "ServiceName",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Other Charge Item:",
                ID = "TxtOtherChargeItem",
                DBField = "OtherChargeItem",
                Required = true,
                //ISUniqueField = true,
                //CheckExistingWithField = "WarehouseID",
                //CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Description:",
                ID = "TxtDescription",
                DBField = "OtherChargeDescription",
                Hide = html.SystemID == "13"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Cost Price:",
                ID = "TxtCostPrice",
                DBField = "CostPrice",
                Value = "0",
                Type = "number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Sales Price:",
                ID = "TxtSalesPrice",
                DBField = "Fee",
                Required = true,
                Type = "number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "UM:",
                ID = "TxtUM",
                DBField = "UM",
                isTypeahead = true,
                //Required = true,
                SQL = "select distinct [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]"
            });
            string InventoryAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' AND (
      coa.AccountName LIKE '%Inventory%'
  ) and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Inventory Account:",
                ID = "Inventory",
                Class = "input2 CustInfo",
                IsCustomForm = true,
                DBField = "InventoryAccountID",
                SQL_CBO = InventoryAccount,
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350,
            });
            string ExpenseAccount = @"select distinct coa.ChartOfAccountID ExpenseAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' AND NOT (
      coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Expense Account:",
                ID = "TxtExpenseAccountID",
                SQL_CBO = ExpenseAccount,
                DBField = "ExpenseAccountID",
                Value = "ExpenseAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350,
                Required = true,
            });

            string InputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and (coa.AccountName LIKE '%VAT%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Input VAT Account:",
                ID = "TxtInputVATAccount",
                SQL_CBO = InputVATAccount,
                DBField = "InputVATAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string ARAccounts = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "AR Account:",
                ID = "Txt_ARAccount",
                SQL_CBO = ARAccounts,
                DBField = "ARAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string IncomeAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where 
--ct.AccountTypes = 'Revenue'  and 
not (coa.AccountName LIKE '%Discount%')  and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Income Account:",
                ID = "TxtIncomeAccount",
                SQL_CBO = IncomeAccount,
                DBField = "IncomeAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "150" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name" , width = "300"}
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string DeferredRevenueAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' and (coa.AccountName LIKE '%Deferred%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Deferred Revenue Account:",
                ID = "TxtDeferredRevenueAccount",
                SQL_CBO = DeferredRevenueAccount,
                DBField = "DeferredRevenueAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "150" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                //Required = true,
            });

            string OutputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' AND (coa.AccountName LIKE '%VAT%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Output VAT Account:",
                ID = "TxtOutputVATAccount",
                SQL_CBO = OutputVATAccount,
                DBField = "OutputVATAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name",  width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string SalesDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue' and (coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Sales Discount Account:",
                ID = "Txt_SalesDiscountAccount",
                SQL_CBO = SalesDiscountAccount,
                DBField = "SalesDiscountAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name",  width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string PurchaseDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' and (coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Purchase Discount Account:",
                ID = "Txt_PurchaseDiscountAccount",
                SQL_CBO = PurchaseDiscountAccount,
                DBField = "PurchaseDiscountAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName" , title = "Account Name", width = "300"}
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "Status",
                SQL_CBO = "select 'Enable' [Status]  union select 'Disable' [Status]",
                DisplayText = "Status",
                Value = "Status"
            });

            html.CustomValidateFunction = "CleanKendoNulls();";
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        /// <summary>
        /// Called after the framework deletes a TblOtherCharge row to clean up
        /// the associated images (DB records + physical files on disk).
        /// </summary>
        [HttpPost]
        public JsonResult CleanupImages(int id)
        {
            try
            {
                string logFile = Server.MapPath("~/Uploads/cleanup_log.txt");
                System.IO.File.AppendAllText(logFile, "\n[" + DateTime.Now + "] CleanupImages called with ID: " + id);

                if (id <= 0) return Json(new { Result = 0, Error = "Invalid ID" });

                string uploadPath = Server.MapPath("~/Uploads/Services/");
                HTML html = new HTML();

                string selSql = "SELECT FileName FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = 'OtherCharge'";
                var selPara = new Dictionary<string, string> { { "ReferenceID", id.ToString() } };
                var ds = html.LoadData(selSql, false, selPara);

                System.IO.File.AppendAllText(logFile, " | Found images: " + (ds?.Tables[0]?.Rows.Count ?? 0));

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string fileName = row["FileName"].ToString();
                        if (fileName.Contains("/"))
                            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
                        string physPath = Path.Combine(uploadPath, fileName);
                        if (System.IO.File.Exists(physPath))
                        {
                            System.IO.File.Delete(physPath);
                            System.IO.File.AppendAllText(logFile, " | Deleted file: " + fileName);
                        }
                    }
                }

                var delPara = new Dictionary<string, string>
                {
                    { "ReferenceID", id.ToString() },
                    { "ReferenceType", "OtherCharge" }
                };
                var sqlRes = html.SaveChangeData(
                    "DELETE FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = @ReferenceType",
                    false, delPara);

                System.IO.File.AppendAllText(logFile, " | Delete query result: " + sqlRes.Result);

                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText(Server.MapPath("~/Uploads/cleanup_log.txt"), " | ERROR: " + ex.Message); } catch { }
                return Json(new { Result = 0, Error = ex.Message });
            }
        }

        [HttpPost]

        public JsonResult UploadImage(int id, string remainingFiles = "")
        {
            if (id <= 0)
                return Json(new { Result = 0, Message = "Invalid OtherChargeID: " + id });

            try
            {
                string uploadPath = Server.MapPath("~/Uploads/Services/");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                HTML html = new HTML();

                // Save any newly uploaded files to disk
                var newFileNames = new List<string>();
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            file.SaveAs(Path.Combine(uploadPath, fileName));
                            newFileNames.Add(fileName);
                        }
                    }
                }

                // Build list of files the user wants to keep
                var keepFiles = new List<string>();
                if (!string.IsNullOrWhiteSpace(remainingFiles))
                {
                    foreach (var f in remainingFiles.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(f))
                        {
                            string cleanF = f.Trim();
                            if (cleanF.Contains("/"))
                                cleanF = cleanF.Substring(cleanF.LastIndexOf('/') + 1);
                            keepFiles.Add(cleanF);
                        }
                    }
                }
                keepFiles.AddRange(newFileNames);

                // Load existing DB records
                string selSql = "SELECT OtherChargeImageID, FileName FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = 'OtherCharge'";
                var selPara = new Dictionary<string, string> { { "ReferenceID", id.ToString() } };
                var ds = html.LoadData(selSql, false, selPara);

                var dbFiles = new List<Tuple<int, string>>();
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                        dbFiles.Add(Tuple.Create(Convert.ToInt32(row["OtherChargeImageID"]), row["FileName"].ToString()));
                }

                // Delete images the user removed
                foreach (var dbRow in dbFiles)
                {
                    string dbFileName = dbRow.Item2;
                    if (dbFileName.Contains("/"))
                        dbFileName = dbFileName.Substring(dbFileName.LastIndexOf('/') + 1);

                    if (!keepFiles.Contains(dbFileName))
                    {
                        var delPara = new Dictionary<string, string> { { "OtherChargeImageID", dbRow.Item1.ToString() } };
                        html.SaveChangeData("DELETE FROM dbo.TblOtherChargeImage WHERE OtherChargeImageID = @OtherChargeImageID", false, delPara);

                        string physPath = Path.Combine(uploadPath, dbFileName);
                        if (System.IO.File.Exists(physPath))
                            System.IO.File.Delete(physPath);
                    }
                }

                // Insert newly uploaded images
                var existingFileNames = dbFiles.Select(t =>
                {
                    string fn = t.Item2;
                    if (fn.Contains("/")) fn = fn.Substring(fn.LastIndexOf('/') + 1);
                    return fn;
                }).ToList();

                foreach (var fn in newFileNames)
                {
                    if (!existingFileNames.Contains(fn))
                    {
                        string insSql = @"INSERT INTO dbo.TblOtherChargeImage
    (ReferenceID, ReferenceType, FileName, CreateBy, CreateDate, SystemID, WarehouseID, DomainID)
VALUES
    (@ReferenceID, @ReferenceType, @FileName, @CreateBy, @CreateDate, @SystemID, @WarehouseID, @DomainID)";
                        var insPara = new Dictionary<string, string>
                        {
                            { "ReferenceID",   id.ToString() },
                            { "ReferenceType", "OtherCharge" },
                            { "FileName",      "/Uploads/Services/" + fn },
                            { "CreateBy",      html.UserFullName ?? "System" },
                            { "CreateDate",    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "SystemID",      html.SystemID },
                            { "WarehouseID",   html.WarehouseID },
                            { "DomainID",      html.DomainID ?? "" }
                        };
                        html.SaveChangeData(insSql, false, insPara);
                    }
                }

                return Json(new { Result = 1, Message = "Upload successful. Files: " + newFileNames.Count });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = "Upload error: " + ex.Message });
            }
        }


        [HttpGet]
        public JsonResult GetImages(int id)
        {
            var files = new List<string>();
            try
            {
                HTML html = new HTML();
                string sql = "SELECT FileName FROM dbo.TblOtherChargeImage WHERE ReferenceID = @id AND ReferenceType = 'OtherCharge' ORDER BY OtherChargeImageID";
                var para = new Dictionary<string, string> { { "id", id.ToString() } };
                var ds = html.LoadData(sql, false, para);
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                        files.Add(row["FileName"].ToString());
                }
            }
            catch { }
            return Json(new { files = files }, JsonRequestBehavior.AllowGet);
        }
    }
}