using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddStockNameController : Controller
    {
        // GET: AddStockName
        [Authorize(Roles = "Add Category, Edit Category, Delete Category")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Category"
            };
            if (html.HasPermission("Add Category"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Category"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Category"))
                html.AllowDelete = true;

            html.AddButton = "Add Category";
            html.AddURL = "AddStockName/CreateStockName";
            html.TableName = "TblStockName";
            html.PrimaryField = "StockNameID";
            //html.OrderBy = "StockNameID desc";
            string sql = @"WITH RecursiveHierarchy AS (
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
    Name,
    Path,
    Level
FROM 
    RecursiveHierarchy
	order by ID, Name;";
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Category",
                SQL = sql,//"select StockNameID, StockName from dbo.TblStockName where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by StockName",
                SelectValue = "StockNameID",
                DisplayField = "Path",
                RadioValue = EncryAndDecry.Encryp("StockNameID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            //html.OrderBy = "ID, Name";
            html.GH.GridProcedureName = "SP_SHOW_STOCK_NAME";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "Stock Code", FieldName = "StockCode" },
                new GridField() { Header = "Category", FieldName = "Path",Width="100%" },
				//new GridField() { Header = "Memo", FieldName = "Memo" },
				new GridField() { Header = "Level", FieldName = "Level", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "ParentCategoryID", TextAlign = "Center",
                    DeleteUseProcedureName="SP_DELETE_STOCK_NAME", IsModifyField = true }
            };
            html.DialogBoxWidth = "500";
            html.DialogBoxHeight = 500;
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Category, Edit Category")]
        public ActionResult CreateStockName(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.FormID = "CreateStockName";
            html.FieldIDFocus = "TxtStockName";
            html.TableName = "TblStockName";
            html.PrimaryField = "StockNameID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Stock Code:",
            //    ID = "TxtStockCode",
            //    DBField = "StockCode",
            //    Required = true,
            //    ISUniqueFiled = true,
            //    CheckExsitingWithField = "WarehouseID",
            //    CheckExsitingWithDefaultValue = html.WarehouseID
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Category:",
                ID = "TxtStockName",
                DBField = "StockName",
                Required = true,
                ISAutoComplete = true,
                SQL_AutoComplete = "select distinct top 10 StockName from dbo.TblStockName where lower(StockName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by StockName",
                //ISUniqueFiled = true,
            });
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //	Label = "Memo:",
            //	ID = "TxtMemo",
            //	DBField = "Memo"
            //});
            string sql = @"WITH RecursiveHierarchy AS (
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

select * from 
(SELECT 
    0 Parent_Category_ID,
    'No Category' [Name],
    'No Category' [Path],
    0 [Level]
	union all
SELECT 
    ID Parent_Category_ID,
    [Name],
    Path,
    Level
FROM 
    RecursiveHierarchy) t
	order by case when t.[Name] = 'No Category' then '0' else t.[Name] end;";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Parent category:",
                ID = "Parent",
                DBField = "Parent_Category_ID",
                SQL_CBO = sql,
                DisplayText = "Path",
                Value = "Parent_Category_ID"
                //isTypeahead = true,
                //DisplayOnFields= "Parent_Category_ID"
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Label = "Category_ID:",
            //	ID = "Parent_Category_ID",
            //	DBField = "Parent_Category_ID",
            //	Hide = true
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Display Order:",
                ID = "TxtOrder",
                DBField = "DisplayOrder",
                Required = true,
                Type = "number",
                Value = "1",
            });

            //if (PrimaryFieldValue == 0)
            //{
            //	html.insTable.Add(
            //		new InsertTable()
            //		{
            //			PrimaryFieldValue = PrimaryFieldValue.ToString(),
            //			GetIndetityFiled = "ParentCategoryID",
            //			TableName = "TblStockName",
            //			Fields = new
            //			{
            //				StockName = "$(\"#TxtCategory\").val().toString()",
            //				DisplayOrder = "$(\"#TxtOrder\").val().toString()",
            //				CreateBy = "'" + html.UserFullName + "'",
            //				CreateDate = "'" + html.GetCurrentDate() + "'",
            //				SystemID = "'" + html.SystemID + "'",
            //				WarehouseID = "'" + html.WarehouseID + "'",
            //			},
            //		}
            //   );
            //}
            //ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            //html.SaveChangeFunction = "SaveChangeStockName();";
            html.SaveChangeCallbackFunction = "Reload_Parent();";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public ActionResult SaveChangeStockName(dynamic Data)
        {
            SqlResult result = new SqlResult();
            HTML html = new HTML();
            Dictionary<string, string> DataInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(Data[0]);
            string StockName = DataInfo["TxtStockName"];
            string DisplayOrder = DataInfo["TxtOrder"];
            string Parent_Category_ID = DataInfo["TxtCategory_ID"] == "" ? "NULL" : DataInfo["TxtCategory_ID"];
            string CreateBy = html.UserFullName;
            string CreateDate = html.GetCurrentDate();
            string SystemID = html.SystemID;
            string WarehouseID = html.WarehouseID;
            string Category = DataInfo["TxtCategory"];
            string PrimaryFieldValue = DataInfo["PrimaryFieldValue"];
            if (PrimaryFieldValue == "0")
            {
                string sql = @"insert into dbo.TblStockName (StockName, Parent_Category_ID, DisplayOrder, CreateBy, CreateDate, SystemID, WarehouseID)
values(N'" + StockName + "'," + Parent_Category_ID + "," + DisplayOrder + ",N'" + CreateBy + "',N'" + CreateDate + "'," + SystemID + "," + WarehouseID + ");" +
"select @@IDENTITY;";
                result = html.SaveChangeData(sql, false, true);
                //				if (Category != "")
                //				{
                //					sql = @"insert into dbo.TblStockName (StockName, Parent_Category_ID, DisplayOrder, CreateBy, CreateDate, SystemID, WarehouseID)
                //values(N'" + Category + "'," + result.Value + "," + DisplayOrder + ",N'" + CreateBy + "',N'" + CreateDate + "'," + SystemID + "," + WarehouseID + ");" +
                //	"select @@IDENTITY;";
                //					result = html.SaveChangeData(sql, false, true);
                //				}
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}