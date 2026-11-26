using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddItemProfileController : Controller
    {
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

            html.TableName = "TblItem";
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
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ServiceName",
                Hide = true,
            });
            string sqlstockname = @"WITH RecursiveHierarchy AS (
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
	where c.SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @"
)
select distinct c.ID StockNameID, c.[Path] StockName from(
SELECT 
    ID,
    [Name],
    [Path],
    [Level]
FROM 
    RecursiveHierarchy
	--order by [Name]
) as c
inner join dbo.TblItem i on c.ID
in (select [Data] from dbo.Split(i.StockNameID, ','))
where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID;
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
                Hide = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                DefaultCheckBok = true,
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end) ItemName from dbo.TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where lower(i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end end)) like replace(@SearchText, '[', '') and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName + (case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end + case when isnull(iv.ItemCode,'') <> '' then ' - ' + isnull(iv.ItemCode,'') else '' end)",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {

                OPTLabel = "Barcode",
                SQL = "select distinct top 10 iv.Barcode from TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where isnull(iv.Barcode, '') <> '' and lower(iv.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by iv.Barcode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Code",
                SQL = "select distinct top 10 i.ItemCode from TblItem i where isnull(i.ItemCode, '') <> '' and lower(i.ItemCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
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
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridProcedureName = "SP_ITEM_SHOW_ADDED_ITEM";
            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true },
                new GridField() { Header = "Item Code", FieldName = "ItemCode" },
                new GridField() { Header = "Barcode", FieldName = "Barcode" },
                new GridField() { Header = "Created By", FieldName = "CreateBy" },
                new GridField() { Header = "Created Date", FieldName = "CreateDate", ClassName="text-center" },
            };
            //html.DefaultSearch = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

    }
}