using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class AddItemController : Controller
    {
        // GET: AddItem
        [Authorize(Roles = "Add Item, Edit Item, Delete Item")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Item"
            };
            if (html.HasPermission("Add Item"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item"))
                html.AllowDelete = true;

            html.AddButton = "Add Item";
            html.AddURL = "AddItem/CreateItem";
            //html.AddURL = "Items/CreateItemGroup";
            //html.TableName = "TblItem";
            html.PrimaryField = "ItemID";
            html.OrderBy = "ItemID desc";
            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 600;
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
    -- Anchor member definition (Level 1 records)
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

    -- Recursive member definition (Child records)
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
    -- Select items based on the selected category in TblItem
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
    -- Get all parent categories for the selected items
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
                //ComboBox = true,
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
                SQL = "select distinct top 10 i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end) ItemName from dbo.TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where lower(i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end end)) like replace(@SearchText, '[', '') and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end)",
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
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Is Have Sub Item", FieldName = "IsHaveSubItem", IsHiddenField = true  },
                new GridField() { Header = "Image", FieldName = "ItemImage", ImagePath="Uploads/Items/", ClassName="text-center", DataTitleOverlay = "#=ItemCode#, #=ItemName#" },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center",Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true},
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true},// Style="width: 350px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" },
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
        [Authorize(Roles = "Add Item, Edit Item")]
        public ActionResult CreateItem(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            string sqlStockName = @"
WITH RecursiveHierarchy AS (
    -- Anchor member definition
    SELECT 
        StockNameID AS ID,
        StockName AS Name,
        CAST(StockName AS NVARCHAR(MAX)) AS Path,
        Parent_Category_ID,
        1 AS Level
    FROM 
        TblStockName
    WHERE 
        isnull(Parent_Category_ID, 0) = 0
		and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @"
    UNION ALL
    
    -- Recursive member definition
    SELECT 
        c.StockNameID AS ID,
        c.StockName AS Name,
        CAST(rh.Path + ' -> ' + c.StockName AS NVARCHAR(MAX)) AS Path,
        c.Parent_Category_ID AS Parent_Category_ID,
        rh.Level + 1 AS Level
    FROM 
        TblStockName c
    INNER JOIN 
        RecursiveHierarchy rh ON c.Parent_Category_ID = rh.ID
	where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
)

SELECT 
    ID StockNameID,
    [Name],
    Path StockName,
    Level
FROM 
    RecursiveHierarchy
	order by [Name];";
            string sql = @"select TypeID,InventoryAccountID,ExpenseAccountID,IncomeAccountID, i.TypeName ,
ac.AccountNo + ' - ' + ac.AccountName InventoryAccount, ad.AccountNo + ' - ' + ad.AccountName IncomeAccount, ae.AccountNo + ' - ' + ae.AccountName ExpenseAccount
from TblSetDefaultAddItemForm s
inner join dbo.TblItemType i on i.ItemTypeID = s.TypeID
inner join dbo.TblChartOfAccount ac on ac.ChartOfAccountID = s.InventoryAccountID
inner join dbo.TblChartOfAccount ad on ad.ChartOfAccountID = s.IncomeAccountID
inner join dbo.TblChartOfAccount ae on ae.ChartOfAccountID = s.ExpenseAccountID where s.SystemID = " + html.SystemID + " and s.WarehouseID = " + html.WarehouseID;
            DataSet ds = html.LoadData(sql);
            string TypeName = "", TypeID = "";
            string InventoryAccount;
            string IncomeAccount;
            string ExpenseAccount;
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    TypeID = dr["TypeID"].ToString();
                    TypeName = dr["TypeName"].ToString();
                    string InventoryAccountID = dr["InventoryAccountID"].ToString();
                    string ExpenseAccountID = dr["ExpenseAccountID"].ToString();
                    string IncomeAccountID = dr["IncomeAccountID"].ToString();
                    InventoryAccount = dr["InventoryAccount"].ToString();
                    IncomeAccount = dr["IncomeAccount"].ToString();
                    ExpenseAccount = dr["ExpenseAccount"].ToString();
                }
            }

            if (Type == "NewItem")
            {
                html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewItem();";
            }
            html.FormID = "CreateItems";
            html.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            bool IsEdit = false;
            if (PrimaryFieldValue != "0")
            {
                IsEdit = true;
            }
            InventoryAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' AND (
      coa.AccountName LIKE '%Inventory%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            ExpenseAccount = @"select distinct coa.ChartOfAccountID ExpenseAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' AND NOT (
      coa.AccountName LIKE '%Discount%' 
      OR coa.AccountNo LIKE '4%'
  ) and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            string InputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and (coa.AccountName LIKE '%VAT%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            string ARAccounts = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and (coa.AccountName LIKE '%Receivable%' OR coa.AccountNo LIKE '11%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            IncomeAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue'  and not (coa.AccountName LIKE '%Discount%')  and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            string DeferredRevenueAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' and (coa.AccountName LIKE '%Deferred%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            string OutputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' AND (coa.AccountName LIKE '%VAT%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            string SalesDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue' and (coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            string PurchaseDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' and (coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblItem",
                    FormTitle="Item Information",
                    GetIDENTITYByFiled="ItemID",
                    PrimaryFieldName="ItemID",
					//ISAddAttachmentFile = true,
					//DestinationFile = "Uploads/Item",
     //               tableKeepDataSaving= new TableKeepDataSaving[]
					//{
					//	new TableKeepDataSaving
					//	{
					//		TableName="TblItemPriceList",
					//		FieldName="ItemID",
					//		PrimaryFieldName="ItemID",
					//		PrimaryFieldValue =PrimaryFieldValue.ToString(),
					//	},
					//	new TableKeepDataSaving
					//	{
					//		TableName="TblItemPriceList",
					//		FieldName="CostPrice",
					//		Value="$(\"#PurchasePrice\").val()"
					//	},
					//	new TableKeepDataSaving
					//	{
					//		TableName="TblItemPriceList",
					//		FieldName="SellingPrice",
					//		Value="$(\"#SellingPrice\").val()"
     //                   }
					//},
					dbField=new DBField[]
                    {
                        //new DBField()
                        //{
                        //    DBFieldName="ItemID", Value="ItemID", TblName= "TblItemPriceList", GetIDENTITYByFiled="ItemID", PrimaryFieldValue=PrimaryFieldValue.ToString(),
                        //    UpdateTblName="TblItem", UpdateDBFieldName="ItemPriceID", UpdateValue = "0",
                        //    FieldNeedToUpdate="CostPrice, SalePrice, Date"
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="CostPrice", Value="PurchasePrice", TblName= "TblItemPriceList", IsNumber=true, GetValueFromInputName = true
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="SalePrice", Value="SellingPrice", TblName= "TblItemPriceList", IsNumber=true, GetValueFromInputName = true
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="Date", TblName= "TblItemPriceList", SetValue=html.GetCurrentDate()
                        //},
                        //new DBField()
                        //{
                        //    DBFieldName="Active", TblName= "TblItemPriceList", SetValue="true"
                        //},
                        new DBField()
                        {
                            DBFieldName="StockNameID", SetValue = "$(\"#StockName_StockNameID\").val()"
                        },
                        new DBField()
                        {
                            DBFieldName="ItemCodeSeq", Value="ItemCodeSeq", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="IsUpdate", SetValue=IsEdit.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="ExpenseAccountID", Value = "PurchaseChartOfAccountID", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="IncomeAccountID", Value = "SaleChartOfAccountID", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="DeferredRevenueAccountID", Value = "TxtDeferredRevenueAccount", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="OutputVATAccountID", Value = "TxtOutputVATAccount", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="InputVATAccountID", Value = "TxtInputVATAccount", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="ARAccountID", Value = "TxtARAccount", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="SalesDiscountAccountID", Value = "TxtSalesDiscountAccount", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="PurchaseDiscountAccountID", Value = "TxtPurchaseDiscountAccount", GetValueFromInputName = true
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
       //                 new HtmlComboBox()
       //                 {
       //                     Label="Categories:", ID="StockName", Required=true,
       //                     Class="input2 CustInfo", IsCustomForm = true,
       //                     DBField="StockNameID", 
							////SQL="select StockNameID, StockName from dbo.TblStockName where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by StockName",
							//SQL_CBO=sqlStockName,
       //                     //HiddenFeild="StockNameID",
							////isTypeahead=true, 
							////SQLCallBack="select ItemGroupID, ItemGroup from dbo.TblItemGroup where StockNameID = @StockNameID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemGroup",
							////CallBackToControlID="ItemGroup", isApplyTypeaheadOnCallBack=true,
							////TypeAheadAddNewFuntionName="AddNewStockName()",
							//AddNewFuntionName ="AddNewStockName()",
       //                     DivClass="pt-1",
       //                     DisplayText="StockName",
       //                     Value="StockNameID",
       //                     Multiple = true
       //                 },
                        new HtmlTextBox()
                        {
                            Label="Categories:", ID="StockName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StockName", 
							//SQL="select StockNameID, StockName from dbo.TblStockName where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by StockName",
							SQL=sqlStockName,
                            HiddenFieldValue="0",
                            HiddenField="StockNameID",
                            isTypeahead=true, 
							//SQLCallBack="select ItemGroupID, ItemGroup from dbo.TblItemGroup where StockNameID = @StockNameID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemGroup",
							//CallBackToControlID="ItemGroup", isApplyTypeaheadOnCallBack=true,
							//TypeAheadAddNewFuntionName="AddNewStockName()",
                            AddButtonFunction="AddNewStockName()",
                            TypeAheadAddNewFunctionName ="AddNewStockName()",
                            DivClassCol = "col-sm-auto",
                            //DisplayText="StockName",
                            //Multiple = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Type:", ID="TypeName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TypeName",
                            SQL="select ServiceID ItemTypeID, ServiceName TypeName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ServiceName",
                            isTypeahead=true, HiddenField="ItemTypeID",
                            HiddenFieldValue=TypeID, Value=TypeName,
                            AddButtonFunction="AddNewService()",
                            TypeAheadAddNewFunctionName="AddNewService()",
                            DivClassCol = "col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Name:", ID="Txt_ItemName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemName", //ISUniqueFiled=true,
							SQL_AutoComplete = "select distinct top 10 i.ItemName from dbo.TblItem i where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                            ISAutoComplete = true,
                        },

                        new HtmlTextBox()
                        {
                            Label="Item Code:", ID="ItemCode",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemCode", DivClassCol = "col-sm",
                            TableName="TblItem", Required = true,
                        },
                        new HtmlTextBox()
                        {
                            ID="ItemCodeSeq",Hide=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID",
                            Multiple = true, Hide = true,
                        },

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Base On:", ID="BaseOn",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BaseOn",
                            TableName="TblItem",
                            SQL_CBO="select 'Barcode' BaseOn union all select 'Serial Number' BaseOn",
                            DisplayText="BaseOn",
                            Value="BaseOn", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Group:", ID="ItemGroup", //Required=true,
							Class="input2 CustInfo", IsCustomForm = true,
                            SQL="select distinct ig.ItemGroupID, ig.ItemGroup from dbo.TblItemGroup ig where ig.SystemID = " + html.SystemID + " and ig.WarehouseID=" + html.WarehouseID + " order by ig.ItemGroup",
                            DBField="ItemGroup",HiddenField="ItemGroupID",isTypeahead=true,
                            AddButtonFunction="AddNewItemGroup()",
                            TypeAheadAddNewFunctionName="AddNewItemGroup()",
                            AddValueToTblNameIfNotExistingData="TblItemGroup",
                            AddValueToTblNameIfNotExistingDataFieldName="ItemGroup",
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Size:", ID="ItemSize",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemSize", //DivClassCol = "col-sm-auto",
							HiddenField="ItemSizeID",
                            isTypeahead=true,
                            AddButtonFunction="AddNewItemSize()",
                            SQL="select ItemSizeID, ItemSize from dbo.TblItemSize where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemSize",
                            TypeAheadAddNewFunctionName="AddNewItemSize()",
                            AddValueToTblNameIfNotExistingData="TblItemSize",
                            AddValueToTblNameIfNotExistingDataFieldName="ItemSize",
                        },
                        new HtmlTextBox()
                        {
                            Label="Other Field:", ID="OtherField",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="OtherField", //DivClassCol = "col-sm-auto",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
						//new HtmlTextBox()
						//{
						//	Label="Manufacturer:", ID="Txt_Manufacturer",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="Manufacturer", DivClassCol = "col-sm-auto",
						//	SQL = "select distinct top 10 i.Manufacturer from dbo.TblManufacturer i where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.Manufacturer",
						//	isTypeahead = true,
						//	AddButtonFunction="AddNewManufacturer()",
						//	TypeAheadAddNewFuntionName="AddNewManufacturer()",
						//	AddValueToTblNameIfNotExistingData="TblManufacturer",
						//	AddValueToTblNameIfNotExistingDataFieldName="Manufacturer"
						//},
						//new HtmlTextBox()
						//{
						//	Label="Unit:", ID="Txt_UMName", Required=true,
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="UM", DivClassCol="col-sm-auto",
						//	SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
						//	isTypeahead=true, HiddenFeild="UomID",
						//	TypeAheadAddNewFuntionName="AddNewUM();",
						//	AddButtonFunction="AddNewUM()"
						//},
                        
                        
                        new HtmlTextBox()
                        {
                            Label="Item Barcode:", ID="Barcode",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Barcode", ISUniqueField=true,
                            IsDisplayAutoGenerateBarcodeNumber = true,
                            TableName="TblItem", DivClassCol = "col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Brand:", ID="ItemBrand",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemBrand",HiddenField="ItemBrandID",isTypeahead=true,
                            AddButtonFunction="AddNewItemBrand()",
                            SQL="select ItemBrandID, ItemBrand from dbo.TblItemBrand where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemBrand",
                            TypeAheadAddNewFunctionName="AddNewItemBrand()",
                            AddValueToTblNameIfNotExistingData="TblItemBrand",
                            AddValueToTblNameIfNotExistingDataFieldName="ItemBrand",
                            DivClassCol = "col-sm",
                        },
                        new HtmlTextBox()
                        {
                            Label="Serial Number:", ID="SerialNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SerialNumber", ISUniqueField=true,
                            TableName="TblItem",
                            CheckExistingWithControlName="Model",
                            CheckExistingWithField="Model", Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="SKU:", ID="SKU",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SKU", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Model:", ID="Model",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Model",//ISUniqueFiled=true,
                            //CheckExsitingWithControllName="SerialNumber",
                            //CheckExsitingWithField="SerialNumber",
                            TableName="TblItem",
                            //CheckExsitingWithDefaultValue=""
                        },
                        new HtmlTextBox()
                        {
                            Label="Alert Reorder:", ID="ReorderPoint",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReorderPoint", ISNumeric = true, ISInteger = true,
                            DivClassCol = "col-sm",
                        },
                        new HtmlTextBox()
                        {
                            Label="CreateBy:", ID="CreateBy", Hide=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CreateBy",
                            DivClassCol = "col-sm",
                        },
                        new HtmlTextBox()
                        {
                            Label="CreateDate:", ID="CreateDate", Hide=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CreateDate",
                            DivClassCol = "col-sm",
                        },
						//new HtmlTextBox()
						//{
						//	Label="Location:", ID="Location",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="Location",
						//	TableName="TblItem"
						//},
					}
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {                        
						//new HtmlTextBox()
						//{
						//	Label="Cost Price USD:", ID="PurchasePrice",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="CostPrice", ISNumeric=true,
						//	Required= true, Value="0",
						//},
						//new HtmlTextBox()
						//{
						//	Label="Selling Price USD:", ID="SellingPrice", Required = true,
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="SellingPrice", ISNumeric=true,
						//	Value = "0",DivClassCol="col-sm-auto",
						//},
						new HtmlTextBox()
                        {
                            Label="Returnable Item:", ID="ReturnableItem", //Value = "true",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", DivClass="wrap-input2 mt-2 pt-1",
                            DBField="ReturnableItem", ISCheckBox=true,
                            Type="checkbox",
                            TableName="TblItem", Hide=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="VendorName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VendorName",
                            HiddenField="VendorID,Reference_VendorName",
                            HiddenFieldValue="0,0",
                            SQL="select VendorID, (VendorCode + ' - ' + VendorName) VendorName, VendorName Reference_VendorName from dbo.TblVendor where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by (VendorCode + ' - ' + VendorName)",
                            isTypeahead = true,
                            AddButtonFunction="AddNewVendor()",
                            TypeAheadAddNewFunctionName="AddNewVendor()",
                            DivClassCol="col-sm-3", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Maximum Point:", ID="MaximumPoint", DivClassCol="col-sm-auto",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="MaximumPoint", ISNumeric = true, ISInteger = true,
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Not for selling:", ID="Notforselling", DivClass="noborder", DivClassCol="col-sm-auto",
                            IsCustomForm = true,
                            DBField="Notforselling", Type="checkbox", Value="False",
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Maintain Stock:", ID="MaintainStock", DivClass="noborder", DivClassCol="col-sm-auto",
                            IsCustomForm = true,
                            DBField="MaintainStock", Type="checkbox", DefaultCheckBoxChecked = true, Value="True",
                            Hide=true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Inventory Asset Account:", ID="Inventory",
                            //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="InventoryChartOfAccountID",
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
                            //isTypeahead=true,
                            //TypeAheadAddNewFuntionName ="AddNewChartOfAccount('Inventory')",
                            //SQL="select ChartOfAccountID InventoryChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) InventoryAccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            //TypeaheadGroupByField= "AccountType",
                            //HiddenFeild="InventoryChartOfAccountID",
                            //HidenFieldValue = InventoryAccountID,
                            //CboValue = InventoryAccountID
                        },

                        new HtmlComboBox()
                        {
                            Label="Expense Account:", ID="PurchaseChartOfAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PurchaseChartOfAccountID",
                            SQL_CBO = ExpenseAccount,
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
                            //CboValue=ExpenseAccountID,                           
                        },
                        new HtmlComboBox()
                        {
                            Label="Income Account:", ID="SaleChartOfAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SaleChartOfAccountID",
                            SQL_CBO=IncomeAccount,
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
                            //CboValue=IncomeAccountID,                          
                        },
                    },
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {

                        new HtmlComboBox()
                        {
                            Label="Deferred Revenue Account:", ID="TxtDeferredRevenueAccount",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeferredRevenueAccountID",
                            SQL_CBO=DeferredRevenueAccount,
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
                        },
                        new HtmlComboBox()
                        {
                            Label="Input VAT Account:", ID="TxtInputVATAccount",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="InputVATAccountID",
                            SQL_CBO=InputVATAccount,
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
                        },
                        new HtmlComboBox()
                        {
                            Label="Output VAT Account:", ID="TxtOutputVATAccount",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="OutputVATAccountID",
                            SQL_CBO=OutputVATAccount,
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
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {

                        new HtmlComboBox()
                        {
                            Label="AR Account:", ID="TxtARAccount",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ARAccountID",
                            SQL_CBO=ARAccounts,
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
                        },
                        new HtmlComboBox()
                        {
                            Label="Purchase Discount Account:", ID="TxtPurchaseDiscountAccount",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PurchaseDiscountAccountID",
                            SQL_CBO=PurchaseDiscountAccount,
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
                        },
                        new HtmlComboBox()
                        {
                            Label="Sales Discount Account:", ID="TxtSalesDiscountAccount",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SalesDiscountAccountID",
                            SQL_CBO=SalesDiscountAccount,
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
                        }
                    }
                },
				//new  FormData() {
				//	htmlControl = new HTMLControl[]
				//	{
				//		new HtmlTextBox()
				//		{
				//			Label="Opening Stock:", ID="OpeningStock", Value = "0",
				//			Class="input2 CustInfo", IsCustomForm = true,
				//			ISNumeric =true, ISInteger = true,
				//			DBField="OpeningStock",
				//			TableName="TblItem"
				//		},
				//		new HtmlTextBox()
				//		{
				//			Label="Opening Stock Rate Per Unit:", ID="OpeningStockRatePerUnit",
				//			Class="input2 CustInfo", IsCustomForm = true,
				//			ISNumeric =true,
				//			DBField="OpeningStockRatePerUnit",
				//			TableName="TblItem"
				//		},
    //                    new HtmlTextBox()
				//		{
				//			Label="Expiration Date:", ID="ItemExpirationDate",
				//			Class="input2 CustInfo", IsCustomForm = true,
				//			SingleDatePicker=true,
				//			DefaultShowDateValue = false,
    //                        DBField="ItemExpirationDate",
    //                        TableName="TblItem"
    //                    },
    //                }
				//},
				new  FormData() {					
                    //FormTitle="Unit Of Measure",
                    //AddRightLabel="Add Variants Item",
                    //OnClickRightLabelFunction="AddVariantsItem()",
                    //TableName="TblUMItem",
                    //GetIDENTITYByFiled="ItemID",
                    //PrimaryFieldName="ItemID",					
					//SQL="update dbo.TblItem set IsHaveSubItem = 1 where ItemID = @ItemID",
					//UpdateFiledName="ItemID",
					htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            //SQL="select *\r\nfrom dbo.TblItemVariant where UMItemID = "+UMID,
                            ShowFooter=false,
                            GetIDENTITYByFiled="ItemID",
                            PrimaryFieldName="ItemID",
                            TableName="TblItemVariant",
                            PrimaryFieldValue=PrimaryFieldValue,
							//UpdateValueFiledName = "1",
							//UpdateValueFiledNameToTblName = "TblItem",
							//UpdateValueFiledNameConditionFiledName = "ItemID",
							//UpdateFiledName = "IsHaveSubItem",
                            ReturnFieldNameValue="IsUpdate",
                            ReturnFieldNameValueFromTblName = "TblItem",
                            HtmlTblID="Table_UMItem",
                            InputName="iUM",
                            TRName="TR_No",
                            TDName="TD_No",
                            ScriptCallBackFunction="Reload_iUM,ItemExpirationDate_ApplyDatePicker",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="StockNameID", SetValue="$(\"#StockName\").val().toString()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemTypeID", SetValue="$(\"#TypeName_ItemTypeID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="TypeName", SetValue="$(\"#TypeName\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemGroupID", SetValue="$(\"#ItemGroup_ItemGroupID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemGroup", SetValue="$(\"#ItemGroup\").val()",
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="ItemCode", SetValue="$(\"#ItemCode\").val()",
                                //},
                                new DBField()
                                {
                                    DBFieldName="ItemName", SetValue="$(\"#Txt_ItemName\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemSize", SetValue="$(\"#ItemSize\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemSizeID", SetValue="$(\"#ItemSize_ItemSizeID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemBrandID", SetValue="$(\"#ItemBrand_ItemBrandID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemBrand", SetValue="$(\"#ItemBrand\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="UM", Value="iUM", GetValueFromInputName=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID", GetValueFromInputName=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="SerialNumber", SetValue="$(\"#SerialNumber\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="Model", SetValue="$(\"#Model\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ReorderPoint", SetValue="$(\"#ReorderPoint\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="InventoryChartOfAccountID", SetValue="$(\"#Inventory\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="InventoryAccountName", SetValue="$(\"input[name=Inventory_input]\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="PurchaseChartOfAccountID", SetValue="$(\"#PurchaseChartOfAccountID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="PurchaseChartOfAccount", SetValue="$(\"input[name=PurchaseChartOfAccountID_input]\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="SaleChartOfAccountID", SetValue="$(\"#SaleChartOfAccountID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="SaleChartOfAccount", SetValue="$(\"input[name=SaleChartOfAccountID_input]\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", SetValue="$(\"#Txt_ItemDescription\").val()", GetValueFromTextAreaName =true
                                },
                                new DBField()
                                {
                                    DBFieldName="Barcode", Value="iBarcode", GetValueFromInputName = true
                                },
                                new DBField()
                                {
                                    DBFieldName="Multiplier", Value="iMultiplier", GetValueFromInputName = true, IsNumber = true,
                                },
                                new DBField()
                                {
                                    DBFieldName="CostPrice", Value="iCostPrice", GetValueFromInputName = true, //IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="SellingPrice", Value="iSellingPrice", GetValueFromInputName = true, //IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="Warehouse", SetValue = "$(\"#Warehouse\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="WID", SetValue = "$(\"#Warehouse_WID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="ExpenseAccountID", SetValue = "$(\"#PurchaseChartOfAccountID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="IncomeAccountID", SetValue = "$(\"#SaleChartOfAccountID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="DeferredRevenueAccountID", SetValue = "$(\"#TxtDeferredRevenueAccount\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="OutputVATAccountID", SetValue = "$(\"#TxtOutputVATAccount\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="InputVATAccountID", SetValue = "$(\"#TxtInputVATAccount\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="ARAccountID", SetValue = "$(\"#TxtARAccount\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="SalesDiscountAccountID", SetValue = "$(\"#TxtSalesDiscountAccount\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="PurchaseDiscountAccountID", SetValue = "$(\"#TxtPurchaseDiscountAccount\").val()"
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Multiplier"
                                },
                                new TblTHead()
                                {
                                     Label="Cost Price"
                                },
                                new TblTHead()
                                {
                                     Label="Selling Price"
                                },
                                new TblTHead()
                                {
                                     Label="Barcode", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                    Label="Variant Code"
                                },
                                new TblTHead()
                                {
                                     Label="Opening Stock", Hide = true
                                },
                                new TblTHead()
                                {
                                     Label="Rate/Unit", Hide = true
                                },
                                new TblTHead()
                                {
                                     Label="Expire Date", Hide = true
                                },
                                new TblTHead()
                                {
                                     Label="Item Image",
                                     CloseTR="</tr>"
                                }
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_No"
                                        },

                                        new HtmlTextBox()
                                        {
                                            Name="iUM",
                                            Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM",
                                            SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                                            isTypeahead=true,
                                            HiddenField="UomID",
                                            TypeAheadAddNewFunctionName="AddNewUM();",
											//RequiredInputIfOtherFieldIsNotNull =true,
                                            DeleteTDRowFunction="RemoveTableRow('TR_No', 'TD_No', this,'','iUM');",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iMultiplier", DivClass="wrap-input3",
                                            Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Multiplier", ISNumeric=true, IsAllowInputOnlyNumber = true,
                                            placeholder="Enter Multiplier", Digit="0"
											//RequiredInputIfOtherFieldIsNotNull =true,
										},
                                        new HtmlTextBox()
                                        {
                                            Name="iCostPrice", DivClass="wrap-input3 text-center",
                                            Required=true,
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="CostPrice", //ISNumeric=true, IsallowInputOnlyNumber = true,
                                            placeholder="Enter Cost Price", 
											//RequiredInputIfOtherFieldIsNotNull =true,
										},
                                        new HtmlTextBox()
                                        {
                                            Name="iSellingPrice", DivClass="wrap-input3 text-center",
                                            Required=true,
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="SellingPrice", //ISNumeric=true, IsallowInputOnlyNumber = true,
                                            placeholder="Enter Selling Price"
											//RequiredInputIfOtherFieldIsNotNull =true,
										},
                                         new HtmlTextBox()
                                        {
                                            Name="iBarcode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Barcode",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Barcode",
                                            AutoGenerateBarcodeNumberIfNotEntry = true,
                                            IsDisplayAutoGenerateBarcodeNumber = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemCode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Code",
                                            DBField="ItemCode",
							                //ISUniqueFiled =true,
							                //TableName="TblItem", //Required = true
						                },
                                        new HtmlTextBox()
                                        {
                                            Name="OpeningStock", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            Value = "0", ISNumeric =true, ISInteger = true,
                                            DBField="OpeningStock",
                                            placeholder="Enter Opening Stock", Hide = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="OpeningStockRatePerUnit", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            ISNumeric =true,
                                            DBField="OpeningStockRatePerUnit",
                                            placeholder="Enter Rate/Unit", Hide = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemExpirationDate", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            SingleDatePicker=true,
                                            DefaultShowDateValue = false,
                                            DBField="ItemExpirationDate",
                                            placeholder="Enter Expire Date", Hide = true
                                        },
                                        new HtmlFile()
                                        {
                                            Name="iItemImage",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemImage", IsDragDropFile = true,
                                            AcceptImageOnly=true, DestinationFile="Uploads/Items/",
                                            ShowDropZoneEnabled = false, Multiple = true,
                                            TableName= "TblUMItem", PrimaryFieldName = "ItemID",
                                            PrimaryFieldValue = PrimaryFieldValue, Align = "right"
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
                new  FormData() {
                    //FormTitle="Item Description",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextarea()
                        {
                            ID="Txt_ItemDescription",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemDescription", IsTextEditorSummernote=true,
                        },
                    }
                },
				//new  FormData() {
    //                //FormTitle="Item image",
    //                htmlControl = new HTMLControl[]
				//	{
				//		new HtmlFile()
				//		{
				//			ID = "ItemImage",
				//			Class = "input2 CustInfo", IsCustomForm = true,
				//			DBField = "ItemImage",
				//			AcceptImageOnly = true,
				//			DestinationFile = "Uploads/Items/",
				//			Multiple = true,
				//			UploadFilesV2 = true,
				//			TableName = "TblItem",
				//			PrimaryFieldName = "ItemID",
				//			PrimaryFieldValue = PrimaryFieldValue
				//		},
				//	}
				//},
			};

            html.FieldIDFocus = "StockName";
            html.FocusFieldIDAfterReset = "ItemCode";
            html.RestFields = "ItemCode, SerialNumber, Model, SKU, MaximumPoint";
            html.SaveChangeCallbackOnCloseDialog = true;
            html.SaveChangeCallbackOnCloseDialogValue = "Txt_ItemName";
            html.CustomValidateFunction = "checkopeningstock()";
            html.SaveChangeCallbackFunction = "addCategoryToItem(response)";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult CreateVariantsItem(string ItemName)
        {
            HTML html = new HTML
            {
                FormID = "CreateVariantsItem",
                FormData = new HTMLControl[]
                {
                    new  FormData() {
                        FormTitle="Variants Item Information for Item Name: " + ItemName,
                        htmlControl = new HTMLControl[]
                        {
                            new HtmlMyTable()
                            {
                                ShowFooter=false,
                                TableResponsive=false,
                            //ScriptCallBackGetDataTblEditFunction="get_Date_Edit",
                            //GetIDENTITYByFiled="StockInID",
                            //PrimaryFieldName="StockInDetailID",
                            //TableName="TblStockInDetail",
                            //CustomCondition="ItemType='Adjustment Stock'",
                            //ReturnFieldNameValue="StockInID, VendorID, PONumber, BillNumber, ReceiveNoteNumber",
                            //ReturnFieldNameValueFromTblName = "TblStockIn",
                            //UpdateValueFiledName="ItemDescription",
                            //UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            //UpdateValueFiledNameConditionFiledName="ItemID",
                                HtmlTblID="TableItemVariation",
                                InputName="OptionName",
                                TRName="TRNoIV",
                                TDName="TDNoIV",
                                tblTHead= new List<TblTHead>()
                                {
                                    new TblTHead()
                                    {
                                        OpenTR="<tr>",
                                        Label="No."
                                    },
                                    new TblTHead()
                                    {
                                         Label="Option Name", Class="text-nowrap"
                                    },
                                    new TblTHead()
                                    {
                                         Label="Option Values",
                                         CloseTR="</tr>"
                                    }
                                },
                                tblTBody= new List<TblTBody>()
                                {
                                    new TblTBody()
                                    {
                                        OpenTR="<tr name=\"TRNoIV\">",
                                        html_Control = new HTMLControl[]
                                        {
                                            new HtmlLabel()
                                            {
                                                Label="1", Align="right", TDName="TDNoIV"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="OptionName",
                                                Class="input2 CustInfo", IsCustomForm = true,
                                            //DBField="ItemName",
                                                DivClass="wrap-input3",
                                                placeholder="Enter Option Name",
                                                DeleteTDRowFunction="RemoveTableRow('TRNoIV', 'TDNoIV', this,'','OptionName');"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="OptionValues", DivClass="wrap-input3",
                                                Class="input2 CustInfo", IsCustomForm = true,
                                                DBField="Multiplier",placeholder="Enter Option Values",
                                                isTagsInput = true, onTagsInputAddedFunction="onAddedVariants"
                                            }
                                        },
                                        CloseTR="</tr>"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult SaveChangeItem(dynamic[] ItemCategory, dynamic[] ItemPriceList, dynamic[] OpeningStock, dynamic[] DateTimeCreate, string Status, dynamic[] ItemAttribute = null, dynamic[] ItemAttrsDetail = null)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            dynamic ItemCategories = new JavaScriptSerializer().DeserializeObject(ItemCategory[0]);
            //dynamic ItemPriceLists = new JavaScriptSerializer().DeserializeObject(ItemPriceList[0]);
            dynamic OpeningStocks = new JavaScriptSerializer().DeserializeObject(OpeningStock[0]);
            dynamic DateTimeCreates = new JavaScriptSerializer().DeserializeObject(DateTimeCreate[0]);
            int itemId = int.Parse(OpeningStocks[0]["ItemID"]);
            dynamic ItemAttrsDetails = null;
            if (ItemAttrsDetail != null)
                ItemAttrsDetails = new JavaScriptSerializer().DeserializeObject(ItemAttrsDetail[0]);
            dynamic ItemAttributes = null;
            if (ItemAttribute != null)
                ItemAttributes = new JavaScriptSerializer().DeserializeObject(ItemAttribute[0]);
            string keepCatID = "";
            string sql;
            foreach (dynamic date in DateTimeCreates)
            {
                bool IsUpdate = bool.Parse(date["IsUpdate"].ToString());

                if (IsUpdate)
                {
                    DateTime createDate = Convert.ToDateTime(date["CreateDate"]);
                    string CreateBy = date["CreateBy"].ToString();
                    string formattedCreateDate = createDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string formattedUpdateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    sql = $"UPDATE dbo.TblItem SET CreateDate = '{formattedCreateDate}', CreateBy = '{CreateBy}'";
                    sql += $", UpdateDate = '{formattedUpdateDate}', UpdateBy = '{html.UserFullName}'";
                    sql += $" WHERE ItemID = {itemId}";
                    html.SaveChangeData(sql, false, false);
                }
            }


            foreach (dynamic item in ItemCategories)
            {
                string ItemID = item["ItemID"].ToString();
                sql = "delete from dbo.TblItemCategory where ItemID = " + ItemID;
                html.SaveChangeData(sql, false, false);
            }
            foreach (dynamic item in ItemCategories)
            {
                string ItemID = item["ItemID"].ToString();
                string CategoryID = item["CategoryID"].ToString();
                if (keepCatID == "")
                    keepCatID = CategoryID;
                else
                    keepCatID += "," + CategoryID;
                //				string sql = @"IF OBJECT_ID('tempdb..#tmp') IS NOT NULL
                //	DROP TABLE #tmp
                //	CREATE TABLE #tmp(
                //		[No]			INT IDENTITY(1,1),
                //		ItemID			int,
                //		CategoryID		int,
                //		PRIMARY KEY([No])
                //	);
                //insert into #tmp (ItemID, CategoryID)
                //select "+ ItemID + " ItemID, "+ CategoryID + @" CategoryID;

                //MERGE TblItemCategory AS TARGET
                //USING #tmp AS SOURCE
                //ON (TARGET.ItemID = SOURCE.ItemID and TARGET.CategoryID = SOURCE.CategoryID)
                //WHEN NOT MATCHED BY TARGET
                //	INSERT (ItemID, CategoryID) VALUES (TARGET.ItemID, TARGET.CategoryID)
                //WHEN NOT MATCHED BY SOURCE 
                //THEN DELETE;";
                sql = @"if (not exists(select ItemID from dbo.TblItemCategory where ItemID = " + ItemID + @" and CategoryID = " + CategoryID + @"))
begin
insert into TblItemCategory (ItemID, CategoryID, CreateBy, CreateDate, SystemID, WarehouseID)
values (" + ItemID + ", " + CategoryID + ", '" + html.UserFullName + "', '" + html.GetCurrentDate() + "', " + html.SystemID + ", " + html.WarehouseID + @")
end
";
                result = html.SaveChangeData(sql, false, false);
            }

            //			foreach (dynamic item in ItemPriceLists)
            //			{
            //				string ItemID = item["ItemID"].ToString();
            //				string CostPrice = item["CostPrice"].ToString();
            //				string SellingPrice = item["SellingPrice"].ToString();
            //				sql = @"declare @ItemPriceID int
            //select @ItemPriceID = ItemPriceID from TblItem where ItemID = " + ItemID + @";
            //delete from TblItemPriceList where ItemPriceID = @ItemPriceID;
            //insert into dbo.TblItemPriceList (Date, ItemID, CostPrice, SalePrice, CreateDate, CreateBy, Active, SystemID, WarehouseID)
            //values ('" + html.GetCurrentDate() + "', " + ItemID + ", " + CostPrice + ", " + SellingPrice + ", '" + html.GetCurrentDate() + @"',
            //'" + html.UserFullName + "', 1," + html.SystemID + ", " + html.WarehouseID + @");
            //update dbo.TblItem set ItemPriceID = @@IDENTITY where ItemID = " + ItemID;
            //				result = html.SaveChangeData(sql, false, false);
            //			}



            foreach (dynamic item in OpeningStocks)
            {
                int ItemID = int.Parse(item["ItemID"].ToString());
                sql = @"SELECT
							iv.UMItemID,
							i.ItemID,
							iv.UomID,
							iv.UM,
							iv.OpeningStock,
							iv.ItemExpirationDate,
							iv.OpeningStockRatePerUnit 
						FROM
							dbo.TblItem i
							INNER JOIN dbo.TblItemVariant iv ON iv.ItemID = i.ItemID 
						WHERE i.ItemID = " + ItemID;

                DataSet ds = html.LoadData(sql);
                if (ds.Tables.Count > 0)
                {
                    if (Status == "AddItemSerial")
                    {
                        sql = @"UPDATE dbo.TblItemVariant 
                        SET Multiplier = 1 
                        WHERE ItemID = " + ItemID;

                        html.SaveChangeData(sql, false, false);
                    }

                    //                  sql = @"DELETE sd
                    //				FROM TblStockInDetail sd
                    //				INNER JOIN TblStockIn s ON sd.StockInID = s.StockInID
                    //				WHERE s.ItemOpeningStockID IN (
                    //					SELECT op.ItemOpeningStockID
                    //					FROM TblItemOpeningStock op
                    //					WHERE op.ItemID = " + ItemID + @"
                    //				);

                    //				DELETE s
                    //				FROM TblStockIn s
                    //				WHERE s.ItemOpeningStockID IN (
                    //					SELECT op.ItemOpeningStockID
                    //					FROM TblItemOpeningStock op
                    //					WHERE op.ItemID = " + ItemID + @"
                    //				);

                    //				DELETE op
                    //				FROM TblItemOpeningStock op
                    //				WHERE op.ItemID = " + ItemID;
                    //                  ;
                    //                  html.SaveChangeData(sql, false, false);

                    //                  foreach (DataRow dr in ds.Tables[0].Rows)
                    //                  {

                    //                      int OStock = int.Parse(dr["OpeningStock"].ToString() == "" ? "0" : dr["OpeningStock"].ToString());
                    //                      double CostPrice = double.Parse(dr["OpeningStockRatePerUnit"].ToString() == "" ? "0" : dr["OpeningStockRatePerUnit"].ToString());
                    //                      string VendorName = html.HtmlDecode(item["VendorName"].ToString());
                    //                      string VendorID = item["VendorID"].ToString();
                    //                      string Warehouse = html.HtmlDecode(item["Warehouse"].ToString());
                    //                      string WID = item["WID"].ToString();
                    //                      string ItemName = html.HtmlDecode(item["ItemName"].ToString());
                    //                      //string UM = item["UM"].ToString();
                    //                      string UM = html.HtmlDecode(dr["UM"].ToString());
                    //                      int UomID = int.Parse(dr["UomID"].ToString());
                    //                      string ItemExpirationDate = dr["ItemExpirationDate"].ToString();
                    //                      var UnitCal = html.GetUnitCal(ItemID, UomID, OStock, CostPrice);
                    //                      if (OStock > 0)
                    //                      {
                    //                          //StatusID = 83 for openning stock 
                    //                          sql = @"declare @ItemOpeningStockID int, @KeepItemOpeningStockID int;
                    //select @KeepItemOpeningStockID = ItemOpeningStockID from TblItemOpeningStock where ItemID = " + ItemID + @";

                    //      insert into TblItemOpeningStock (ItemID, QTY, OpeningStockRatePerUnit, [Date], ItemExpirationDate)
                    //      values(" + ItemID + ", " + OStock + "," + CostPrice + ",'" + html.GetCurrentDate() + @"','" + ItemExpirationDate + @"');
                    //      select @ItemOpeningStockID = @@IDENTITY;

                    //      insert into TblStockIn (VendorName, VendorID, ReceiveDate, WID, Warehouse, StatusID, ItemType,
                    //      CreateBy, CreateDate, UserID, SystemID, WarehouseID, ItemOpeningStockID) values (N'" + VendorName + "', " + VendorID + ",'" + html.GetCurrentDate() + @"',
                    //      " + WID + ",'" + Warehouse + "',83,'Opening Stock', N'" + html.UserFullName + "','" + html.GetCurrentDate() + "'," + html.UserID + "," + html.SystemID + ", " + html.WarehouseID + @",@ItemOpeningStockID);
                    //      declare @StockInID int; select @StockInID = @@IDENTITY;

                    //      insert into dbo.TblStockInDetail (StockInID, ItemID, ItemName, QTY, QtyCal, OpeningStock, CostPrice, CostPriceCal, Type, StockInDate, StatusID, LocationID, 
                    //      ItemType,UomID, UM, SystemID, WarehouseID, ItemOpeningStockID, ItemExpirationDate, OrderQTY, Reason)
                    //      values (@StockInID, " + ItemID + ", N'" + ItemName + "'," + OStock + "," + UnitCal["QtyCal"] + "," + OStock + "," + CostPrice + "," + UnitCal["CostPriceCal"] + ", 'Opening Stock','" + html.GetCurrentDate() + @"',
                    //      83," + WID + ",'Opening Stock'," + UomID + ", N'" + UM + "'," + html.SystemID + "," + html.WarehouseID + ", @ItemOpeningStockID,'" + ItemExpirationDate + "', 0, 'Opening Stock')";
                    //                          result = html.SaveChangeData(sql, false, false);
                    //                      }
                    //                  }
                }
            }



            if (ItemAttributes != null)
            {
                foreach (dynamic item in ItemAttributes)
                {
                    string Attribute = item["Attribute"].ToString();
                    string Options = item["Options"].ToString();
                    //string ItemID = item["ItemID"].ToString();
                    //string ItemGroupName = item["ItemGroupName"].ToString();
                    sql = @"declare @ItemAttributeID int;
if(not exists(select Attribute from dbo.TblItemAttribute where lower(Attribute)=N'" + Attribute.ToLower() + @"'))
begin
	insert into dbo.TblItemAttribute (Attribute,Options,CreateBy,CreateDate,SystemID,WarehouseID)
	values (N'" + Attribute + "',N'" + Options + "','" + html.UserFullName + "','" + html.GetCurrentDate() + "'," + html.SystemID + "," + html.WarehouseID + @");
select @ItemAttributeID = @@IDENTITY;
end
else
begin
select @ItemAttributeID = ItemAttributeID from dbo.TblItemAttribute where lower(Attribute)=N'" + Attribute.ToLower() + @"'
end;
";
                    string[] Opts = Options.Split(',');
                    foreach (string O in Opts)
                    {
                        sql += @"if(not exists(select Options from dbo.TblItemAttributeOptions where lower(Options)=N'" + O.ToLower() + "' and lower(Attribute)=N'" + Attribute.ToLower() + @"'))
begin
	insert into dbo.TblItemAttributeOptions (ItemAttributeID,Attribute, Options,CreateBy,CreateDate,SystemID,WarehouseID)
	values (@ItemAttributeID,N'" + Attribute + "',N'" + O + "','" + html.UserFullName + "','" + html.GetCurrentDate() + "'," + html.SystemID + "," + html.WarehouseID + @");
end;";
                        //					string[] ItemIDs = ItemID.Split('~');
                        //					foreach (string items in ItemIDs)
                        //					{
                        //						sql += @"if(not exists(select ItemID from dbo.TblItemAttributeValue where ItemID = "+ items + " and lower(ItemAttributeValue)=N'" + O.ToLower() + "' and lower(ItemAttribute)=N'" + Attribute.ToLower() + @"'))
                        //begin
                        //	insert into dbo.TblItemAttributeValue (ItemID, ItemGroupName, ItemAttributeID,ItemAttribute, ItemAttributeValue,CreateBy,CreateDate,SystemID,WarehouseID)
                        //	values (" + items+",N'"+ ItemGroupName + "',@ItemAttributeID,N'" + Attribute + "',N'" + O + "','" + html.UserFullName + "','" + html.GetCurrentDate() + "'," + html.SystemID + "," + html.WarehouseID + @");
                        //end;";
                        //					}
                    }
                    result = html.SaveChangeData(sql, false, false);
                }
            }
            if (ItemAttrsDetails != null)
            {
                foreach (dynamic item in ItemAttrsDetails)
                {
                    string ItemID = item["ItemID"].ToString();
                    string ItemGroupName = item["ItemGroupName"].ToString();
                    string AttrsDetail = item["AttrsDetail"].ToString();
                    string[] Attributes = AttrsDetail.Split('~');
                    foreach (string Attr in Attributes)
                    {
                        string[] strs = Attr.Split('|');
                        string Attribute = strs[0];
                        string ItemAttributeValue = strs[1];
                        sql = @"insert into dbo.TblItemAttributeValue (ItemID, ItemGroupName,ItemAttribute, ItemAttributeValue,CreateBy,CreateDate,SystemID,WarehouseID)
	values (" + ItemID + ",N'" + ItemGroupName + "',N'" + Attribute + "',N'" + ItemAttributeValue + "','" + html.UserFullName + "','" + html.GetCurrentDate() + "'," + html.SystemID + "," + html.WarehouseID + @");";
                        result = html.SaveChangeData(sql, false, false);
                    }
                }
            }
            //            sql = @"
            //if(not exists(select ItemID from dbo.TblItemAttributeValue where ItemID = " + ItemID + " and lower(ItemAttributeValue)=N'" + ItemAttributeValue.ToLower() + "' and lower(ItemAttribute)=N'" + Attribute.ToLower() + @"'))
            //begin
            //	insert into dbo.TblItemAttributeValue (ItemID, ItemGroupName, ItemAttributeID,ItemAttribute, ItemAttributeValue,CreateBy,CreateDate,SystemID,WarehouseID)
            //	values (" + ItemID + ",N'" + ItemGroupName + "',@ItemAttributeID,N'" + Attribute + "',N'" + ItemAttributeValue + "','" + html.UserFullName + "','" + html.GetCurrentDate() + "'," + html.SystemID + "," + html.WarehouseID + @");
            //end;";
            return Json(new { Resutl = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLastSeq()
        {
            HTML html = new HTML();
            string sql = @"select top 1 isnull(ItemCodeSeq, 0) seq from dbo.TblItem where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemCodeSeq desc";
            return Json(new { data = html.JsonData(sql, false) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, ValidateInput(false)]
        public async Task<JsonResult> SendNotificationToTelegram(int ItemID)
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            try
            {
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    {"ItemID", ItemID.ToString() },
                };
                string sql = @"select s.SystemName, i.* from 
dbo.TblItem i
inner join dbo.TblSystem s on i.SystemID = s.SystemID
where ItemID = @ItemID;";
                DataSet ds = html.LoadData(sql, false, para);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string ItemCode = ds.Tables[0].Rows[0]["ItemCode"].ToString();
                        string ItemName = ds.Tables[0].Rows[0]["ItemName"].ToString();
                        string CreatedBy = ds.Tables[0].Rows[0]["CreateBy"].ToString();
                        string SystemName = ds.Tables[0].Rows[0]["SystemName"].ToString();
                        string dbSystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                        string CreatedDate = DateTime.Parse(ds.Tables[0].Rows[0]["CreateDate"].ToString()).ToString("dd-MMM-yyyy hh:mm:ss tt");
                        string ItemProfileUrl = $"{html.ServerRoot}?SystemID={dbSystemID}&WarehouseID={html.WarehouseID}#!/ItemProfile?ID={ItemID}";

                        string msg = "";
                        msg += "<b>🆕 NEW ITEM CREATED.</b><br><br>";
                        msg += "Dear Team, <br><br>Now we have new Item need to set up chart of account information of:<br><br>";
                        msg += $"<b>🆔 Item Code: <a href=\"{ItemProfileUrl}\">{ItemCode}</a> </b><br>";
                        msg += $"<b>📦 Item Name: <a href=\"{ItemProfileUrl}\">{ItemName}</a> </b><br><br>";
                        msg += $"👤 Created by: {CreatedBy} <br>";
                        msg += $"📅 Created date: {CreatedDate} <br>";
                        msg += $"🖥 System: <b>[{(html.SystemID == "13" ? "🟠" : "🔴")} {SystemName}]</b><br><br>";
                        msg += $"Thanks you!<br>";


                        string botToken = html.GetBotTokenBySystemID(int.Parse(dbSystemID));
                        long groupChatId = -1004287125726;
                        int topicId = dbSystemID == "13" ? 2 : 4;
                        sqlresult = await notificationTelegram.NotificationTelegramBotToTopic(
                            MsgText: msg,
                            chatId: groupChatId,
                            topicID: topicId,
                            inlineKeyboard: null,
                            MessageID: 0,
                            TelegramMessage: "",
                            Files: "",
                            Caption: "",
                            EditMessage: false,
                            DeleteTelegramMessageID: "",
                            IsReplyMsg: false,
                           newBotClient: botToken
                        );
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}