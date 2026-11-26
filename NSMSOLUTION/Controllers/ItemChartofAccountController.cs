using MVC;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ItemChartofAccountController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "Item Chart of Account",
                DialogBoxWidth = "1200",
                TableName = "TblItem",
                PrimaryField = "ItemID",
                OrderBy = "ItemID desc",
                AllowAdd = true,
                AddButton = "UpdateItem",
                AddURL = "ItemChartofAccount/CreateItemChartofAccount"
            };

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemType",
                ComboBox = true,
                Label = "Type:",
                DBField = "Type",
                Value = "ServiceID",
                DisplayText = "ServiceName",
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ServiceName"
            });
            string sqlstockname = @"WITH RecursiveHierarchy AS (
    SELECT 
        StockNameID AS ID,
        StockName AS Name,
        CAST(StockName AS NVARCHAR(MAX)) AS Path,
        Parent_Category_ID,
        1 AS Level
    FROM 
        TblStockName
    WHERE 
        ISNULL(Parent_Category_ID, 0) = 0
        AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID + @"
    UNION ALL
    SELECT 
        c.StockNameID AS ID,
        c.StockName AS Name,
        CAST(rh.Path + ' -> ' + c.StockName AS NVARCHAR(MAX)) AS Path,
        c.Parent_Category_ID,
        rh.Level + 1 AS Level
    FROM 
        TblStockName c
    INNER JOIN 
        RecursiveHierarchy rh ON c.Parent_Category_ID = rh.ID
    WHERE 
        c.SystemID = " + html.SystemID + " AND c.WarehouseID = " + html.WarehouseID + @"
),
SelectedItems AS (
    SELECT DISTINCT 
        c.ID 
    FROM 
        RecursiveHierarchy c
    INNER JOIN 
        dbo.TblItem i ON c.ID IN (SELECT [Data] FROM dbo.Split(i.StockNameID, ','))
    WHERE 
        i.SystemID = " + html.SystemID + " AND i.WarehouseID = " + html.WarehouseID + @"
),
ParentHierarchy AS (
    SELECT 
        rh.ID, 
        rh.Name, 
        rh.Path, 
        rh.Level
    FROM 
        RecursiveHierarchy rh
    INNER JOIN 
        SelectedItems si ON rh.ID = si.ID
    UNION
    SELECT 
        rh.ID, 
        rh.Name, 
        rh.Path, 
        rh.Level
    FROM 
        RecursiveHierarchy rh
    WHERE 
        rh.ID IN (
            SELECT DISTINCT Parent_Category_ID 
            FROM RecursiveHierarchy 
            WHERE ID IN (SELECT ID FROM SelectedItems)
        )
)
SELECT DISTINCT 
    rh.ID AS StockNameID, 
    rh.Path AS StockName, 
    rh.Name, 
    rh.Level
FROM 
    ParentHierarchy rh
ORDER BY 
    rh.Level, rh.Path;";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStockName",
                Label = "Categories:",
                DBField = "Categories",
                Value = "StockNameID",
                DisplayText = "StockName",
                SQL_CBO = sqlstockname,
                ComboBox = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemSize",
                ComboBox = true,
                Label = "Item Size:",
                DBField = "ItemSize",
                Value = "ItemSize",
                DisplayText = "ItemSize",
                SQL_CBO = "select distinct ItemSize from dbo.TblItem where isnull(ItemSize, '') <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemSize"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBrand",
                ComboBox = true,
                Label = "Item Brand:",
                DBField = "ItemBrand",
                Value = "ItemBrandID",
                DisplayText = "ItemBrand",
                SQL_CBO = "select distinct ItemBrandID, ItemBrand from dbo.TblItemBrand where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemBrand"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtModel",
                ComboBox = true,
                Label = "Model:",
                DBField = "Model",
                Value = "Model",
                DisplayText = "Model",
                SQL_CBO = "select distinct Model from dbo.TblItem where isnull(Model, '') <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Model"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemCode",
                Label = "Item Code:",
                DBField = "ItemCode",
                SQL_CBO = "select distinct i.ItemCode from TblItem i where isnull(i.ItemCode, '') <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayText = "ItemCode",
                Value = "ItemCode",
                ComboBox = true,
                Multiple = true,
                placeholder = "Select Item Code"
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                DefaultCheckBok = true,
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end) ItemName from dbo.TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where lower(i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end)) like replace(@SearchText, '[', '') and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end)",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
                placeholder = "Enter Item Name"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Barcode",
                SQL = "select distinct top 10 iv.Barcode from TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where isnull(iv.Barcode, '') <> '' and lower(iv.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by iv.Barcode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
                placeholder = "Enter Barcode"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Serial Number",
                SQL = @"SELECT distinct top 10 SerialNumber
FROM (
    SELECT SerialNumber 
    FROM TblItem
    UNION
    SELECT SerialNumber 
    FROM TblItemVariant
    UNION
    SELECT SerialNumber 
    FROM TblItemSerialDetail
) AS CombinedResults
WHERE ISNULL(SerialNumber, '') <> '' 
AND LOWER(SerialNumber) LIKE  @SearchText
ORDER BY SerialNumber;",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
                placeholder = "Enter Serial No."
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            html.AllowEdit = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Is Have Sub Item", FieldName = "IsHaveSubItem", IsHiddenField = true  },
                new GridField() { Header = "Image", FieldName = "ItemImage", ImagePath="Uploads/Items/", ClassName="text-center"},
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center",Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true},
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true},
                new GridField() { Header = "Model", FieldName = "Model", ClassName="text-center" },
                new GridField() { Header = "Barcode", FieldName = "Barcode", ClassName="text-center"},
                new GridField() { Header = "Item Size", FieldName = "ItemSize", ClassName="text-center"},
                new GridField() { Header = "Item Brand", FieldName = "ItemBrand", },
                new GridField() { Header = "QOH", FieldName = "QOH", ClassName="text-danger fnt-poppins"},
                new GridField() { Header = "UoM", FieldName = "ProductVaraint", ClassName="text-center"},
                new GridField() { Header = "Created By", FieldName = "CreateBy" },
                new GridField() { Header = "Created Date", FieldName = "CreateDate", TextAlign="center"},
                new GridField() { Header = "Update By", FieldName = "UpdateBy" },
                new GridField() { Header = "Update Date", FieldName = "UpdateDate", TextAlign="center"},
                new GridField() { Header = "Action", FieldName = "ItemID", TextAlign = "Center",  IsModifyField = true,  DeleteUseProcedureName="SP_DELETE_ITEM" }
            };

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ServerPaging = true,
                ProcedureName = "SP_ITEM_SHOW_ADDED_ITEM",
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        public ActionResult CreateItemChartofAccount(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            html.TableColumn = 2;
            html.UseTable = true;
            html.FormID = "CreateItemChartofAccount";
            html.FieldIDFocus = PrimaryFieldValue == "0" ? "ItemID" : "InventoryChartOfAccountID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.TableName = "TblItem";
            html.PrimaryField = "ItemID";
            html.SQLForEdit = @"select ItemID, 
                nullif(InventoryChartOfAccountID, 0) InventoryChartOfAccountID, 
                nullif(ExpenseAccountID, 0) PurchaseChartOfAccountID, 
                nullif(IncomeAccountID, 0) SaleChartOfAccountID, 
                nullif(DeferredRevenueAccountID, 0) DeferredRevenueAccountID, 
                nullif(OutputVATAccountID, 0) OutputVATAccountID, 
                nullif(InputVATAccountID, 0) InputVATAccountID, 
                nullif(ARAccountID, 0) ARAccountID, 
                nullif(PurchaseDiscountAccountID, 0) PurchaseDiscountAccountID, 
                nullif(SalesDiscountAccountID, 0) SalesDiscountAccountID 
                from dbo.TblItem where ItemID = " + PrimaryFieldValue;

            string InventoryAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' AND (coa.AccountName LIKE '%Inventory%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string ExpenseAccount = @"select distinct coa.ChartOfAccountID ExpenseAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' AND NOT (coa.AccountName LIKE '%Discount%' OR coa.AccountNo LIKE '4%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string IncomeAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue'  and not (coa.AccountName LIKE '%Discount%')  and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string DeferredRevenueAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' and (coa.AccountName LIKE '%Deferred%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string InputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and (coa.AccountName LIKE '%VAT%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string OutputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' AND (coa.AccountName LIKE '%VAT%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string ARAccounts = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and (coa.AccountName LIKE '%Receivable%' OR coa.AccountNo LIKE '11%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string PurchaseDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' and (coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            string SalesDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue' and (coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            if (PrimaryFieldValue == "0")
            {
                string itemSql = "select ItemID, (isnull(ItemCode, '') + ' - ' + isnull(ItemName, '')) ItemName from dbo.TblItem where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemName";
                html.HTMLControls.Add(new HtmlComboBox()
                {
                    Label = "Select Item to Update:",
                    ID = "ItemID",
                    DBField = "ItemID",
                    SQL_CBO = itemSql,
                    DisplayText = "ItemName",
                    Value = "ItemID",
                    Required = true
                });
            }

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Inventory Asset Account:",
                ID = "InventoryChartOfAccountID",
                DBField = "InventoryChartOfAccountID",
                SQL_CBO = InventoryAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Expense Account:",
                ID = "PurchaseChartOfAccountID",
                DBField = "PurchaseChartOfAccountID",
                SQL_CBO = ExpenseAccount,
                DisplayText = "AccountName",
                Value = "ExpenseAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Income Account:",
                ID = "SaleChartOfAccountID",
                DBField = "SaleChartOfAccountID",
                SQL_CBO = IncomeAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Deferred Revenue Account:",
                ID = "DeferredRevenueAccountID",
                DBField = "DeferredRevenueAccountID",
                SQL_CBO = DeferredRevenueAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Input VAT Account:",
                ID = "InputVATAccountID",
                DBField = "InputVATAccountID",
                SQL_CBO = InputVATAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Output VAT Account:",
                ID = "OutputVATAccountID",
                DBField = "OutputVATAccountID",
                SQL_CBO = OutputVATAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "AR Account:",
                ID = "ARAccountID",
                DBField = "ARAccountID",
                SQL_CBO = ARAccounts,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Purchase Discount Account:",
                ID = "PurchaseDiscountAccountID",
                DBField = "PurchaseDiscountAccountID",
                SQL_CBO = PurchaseDiscountAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Sales Discount Account:",
                ID = "SalesDiscountAccountID",
                DBField = "SalesDiscountAccountID",
                SQL_CBO = SalesDiscountAccount,
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350
            });

            html.SaveChangeFunction = "saveChange()";
            string formHtml = html.DrawHTMLForm();
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "draw_html_form_debug.txt", formHtml);
            ViewBag.DrawHTMLForm = formHtml;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetItemChartOfAccount(int itemId)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    string sql = @"select 
                        nullif(InventoryChartOfAccountID, 0) InventoryChartOfAccountID, 
                        nullif(ExpenseAccountID, 0) PurchaseChartOfAccountID, 
                        nullif(IncomeAccountID, 0) SaleChartOfAccountID, 
                        nullif(DeferredRevenueAccountID, 0) DeferredRevenueAccountID, 
                        nullif(OutputVATAccountID, 0) OutputVATAccountID, 
                        nullif(InputVATAccountID, 0) InputVATAccountID, 
                        nullif(ARAccountID, 0) ARAccountID, 
                        nullif(PurchaseDiscountAccountID, 0) PurchaseDiscountAccountID, 
                        nullif(SalesDiscountAccountID, 0) SalesDiscountAccountID 
                        from dbo.TblItem where ItemID = @p0";
                    var dt = await ctx.Database.SqlQuery<ItemChartofAccount>(sql, itemId).FirstOrDefaultAsync();
                    if (dt == null)
                        return Json(new { Result = 0, Data = (object)null }, JsonRequestBehavior.AllowGet);
                    return Json(new { Result = 2, Data = dt }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveChange(ItemChartofAccount data)
        {
            try
            {
                var html = new HTML();
                using (var ctx = new DBContext())
                {
                    string sqlUpdateItem = @"UPDATE dbo.TblItem 
                        SET InventoryChartOfAccountID = @p0, 
                            PurchaseChartOfAccountID = @p1, 
                            ExpenseAccountID = @p2,
                            SaleChartOfAccountID = @p3, 
                            IncomeAccountID = @p4,
                            DeferredRevenueAccountID = @p5, 
                            OutputVATAccountID = @p6, 
                            InputVATAccountID = @p7,
                            ARAccountID = @p8,
                            PurchaseDiscountAccountID = @p9,
                            SalesDiscountAccountID = @p10,
                            UpdateBy = @p11,
                            UpdateDate = @p12
                        WHERE ItemID = @p13";
                    await ctx.Database.ExecuteSqlCommandAsync(sqlUpdateItem,
                        data.InventoryChartOfAccountID,
                        data.PurchaseChartOfAccountID,
                        data.PurchaseChartOfAccountID, // ExpenseAccountID
                        data.SaleChartOfAccountID,
                        data.SaleChartOfAccountID, // IncomeAccountID
                        data.DeferredRevenueAccountID,
                        data.OutputVATAccountID,
                        data.InputVATAccountID,
                        data.ARAccountID,
                        data.PurchaseDiscountAccountID,
                        data.SalesDiscountAccountID,
                        html.UserFullName,
                        DateTime.Now,
                        data.ItemID);

                    string sqlUpdateVariant = @"UPDATE dbo.TblItemVariant 
                        SET InventoryChartOfAccountID = @p0, 
                            PurchaseChartOfAccountID = @p1, 
                            ExpenseAccountID = @p2,
                            SaleChartOfAccountID = @p3,
                            IncomeAccountID = @p4,
                            DeferredRevenueAccountID = @p5, 
                            OutputVATAccountID = @p6, 
                            InputVATAccountID = @p7,
                            ARAccountID = @p8,
                            PurchaseDiscountAccountID = @p9,
                            SalesDiscountAccountID = @p10
                        WHERE ItemID = @p11";
                    await ctx.Database.ExecuteSqlCommandAsync(sqlUpdateVariant,
                        data.InventoryChartOfAccountID,
                        data.PurchaseChartOfAccountID,
                        data.PurchaseChartOfAccountID, // ExpenseAccountID
                        data.SaleChartOfAccountID,
                        data.SaleChartOfAccountID, // IncomeAccountID
                        data.DeferredRevenueAccountID,
                        data.OutputVATAccountID,
                        data.InputVATAccountID,
                        data.ARAccountID,
                        data.PurchaseDiscountAccountID,
                        data.SalesDiscountAccountID,
                        data.ItemID);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
            return Json(new { Result = 2, Message = "Changes saved successfully." });
        }

        //[HttpPost]
        //public async Task<JsonResult> BatchUpdateEmpty()
        //{
        //    try
        //    {
        //        var html = new HTML();
        //        using(var ctx = new DBContext())
        //        {
        //            string sql = @"
        //                UPDATE i
        //                SET i.InventoryChartOfAccountID = s.InventoryAccountID,
        //                    i.PurchaseChartOfAccountID = s.ExpenseAccountID,
        //                    i.SaleChartOfAccountID = s.IncomeAccountID
        //                FROM dbo.TblItem i
        //                INNER JOIN dbo.TblItemType it ON it.ItemTypeID = i.ItemTypeID
        //                INNER JOIN dbo.TblSetDefaultAddItemForm s ON s.TypeID = it.ItemTypeID
        //                WHERE i.SystemID = @p0 AND i.WarehouseID = @p1 
        //                  AND (i.InventoryChartOfAccountID IS NULL OR i.InventoryChartOfAccountID = 0
        //                       OR i.PurchaseChartOfAccountID IS NULL OR i.PurchaseChartOfAccountID = 0
        //                       OR i.SaleChartOfAccountID IS NULL OR i.SaleChartOfAccountID = 0);

        //                UPDATE v
        //                SET v.InventoryChartOfAccountID = s.InventoryAccountID,
        //                    v.PurchaseChartOfAccountID = s.ExpenseAccountID,
        //                    v.SaleChartOfAccountID = s.IncomeAccountID
        //                FROM dbo.TblItemVariant v
        //                INNER JOIN dbo.TblItem i ON i.ItemID = v.ItemID
        //                INNER JOIN dbo.TblItemType it ON it.ItemTypeID = i.ItemTypeID
        //                INNER JOIN dbo.TblSetDefaultAddItemForm s ON s.TypeID = it.ItemTypeID
        //                WHERE i.SystemID = @p0 AND i.WarehouseID = @p1 
        //                  AND (v.InventoryChartOfAccountID IS NULL OR v.InventoryChartOfAccountID = 0
        //                       OR v.PurchaseChartOfAccountID IS NULL OR v.PurchaseChartOfAccountID = 0
        //                       OR v.SaleChartOfAccountID IS NULL OR v.SaleChartOfAccountID = 0);
        //            ";

        //            await ctx.Database.ExecuteSqlCommandAsync(sql, html.SystemID, html.WarehouseID);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //    return Json(new { success = true, message = "Batch update completed successfully." });
        //}
    }
}