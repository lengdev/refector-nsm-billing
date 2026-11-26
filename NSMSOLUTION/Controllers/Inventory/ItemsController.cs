using HtmlAgilityPack;
using MVC;
using Newtonsoft.Json;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class ItemsController : Controller
    {
        // GET: Items
        public ActionResult Barcode()
        {
            HTML html = new HTML();
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                ISAutoComplete = true,
                OPTLabel = "PO#",
                SQL = "select distinct PONumber from dbo.TblPurchaseOrder where lower(PONumber) like @SearchText and StatusID = 66 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by PONumber",
                RadioValue = EncryAndDecry.Encryp("PONumber"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.OverrideSearch = "Search();";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Print Item Barcode")]
        public ActionResult PrintItemBarcode()
        {
            HTML html = new HTML
            {
                TableName = "dbo.TblItem",
                PrimaryField = "ItemID"
            };
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                DefaultCheckBok = true,
                OPTLabel = "Item Name",
                //SQL = "select i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName from dbo.TblItem i where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i where lower(i.ItemName) like replace(@SearchText, '[', '') and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                ISAutoComplete = true,
                //SelectValue = "ItemID",
                //DisplayField = "ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {

                OPTLabel = "Barcode",
                SQL = "select distinct top 10 i.Barcode from TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where isnull(i.Barcode, '') <> '' and lower(i.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.Barcode",
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

            html.GH.GridProcedureName = "SP_ITEM_PRINT_BARCODE";
            //html.IsHaveWhereCondition = true;
            //html.GH.SQL = sql;
            //html.OrderBy = "i.ItemID desc";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Image", FieldName = "ItemImage", ImagePath="Uploads/Items/", ShowOnlyOneImg=true  },
                new GridField() { Header = "Barcode", FieldName = "Barcode", DisplayBarcode=true },
                new GridField() { Header = "ItemCode", FieldName = "ItemCode", },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true, },// Style="width: 350px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" },				
                new GridField() { Header = "Item Size", FieldName = "ItemSize" },
                new GridField() { Header = "UOM", FieldName = "UM" },
                //new GridField() { Header = "Serial Number", FieldName = "SerialNumber" },
                 new GridField() { Header = "QTY", FieldName = "QTY", DisplayTextBox = true, },
                new GridField() { Header = "Action", FieldName = "PrintBarcode", TextAlign = "center" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "List Print Item Expire Barcode")]
        public ActionResult PrintItemExpireBarcode()
        {
            HTML html = new HTML
            {
                TableName = "dbo.TblItem",
                PrimaryField = "ItemID"
            };
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                DefaultCheckBok = true,
                OPTLabel = "Item Name",
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i where lower(i.ItemName) like replace(@SearchText, '[', '') and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("ItemCode"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Code",
                SQL = "select distinct top 10 i.ItemCode from TblItem i where isnull(i.ItemCode, '') <> '' and lower(i.ItemCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("ItemCode"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"
with ExpireDateCTE as (
    select i.ItemID, i.ItemCode, i.ItemName, ed.Barcode, format(ed.ExpireDate, 'dd-MM-yyyy') ExpireDate, 1 as QTY,
    '<a href=""javascript:void(0)"" onclick=""PrintItemBarcode(this);"">Print Label</a>' as PrintBarcode, i.SystemID, i.WarehouseID, i.DomainID
    from dbo.TblStockExpireDate ed 
    inner join dbo.TblItem i on i.ItemID = ed.ItemID
    group by i.ItemID, i.ItemCode, i.ItemName, ed.Barcode, ed.ExpireDate, i.SystemID, i.WarehouseID, i.DomainID
)
select row_number() over(order by ItemID asc) as [No], * from ExpireDateCTE";
            html.GH.SQL = sql;
            // html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="SystemID", Value =html.SystemID },
              new GridPara() { ParaName="WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="DomainID", Value =html.DomainID },
            };
            html.OrderBy = "[No]";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", TextAlign = "center"},
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true, },
                new GridField() { Header = "Barcode", FieldName = "Barcode", DisplayBarcode=true },
                new GridField() { Header = "ExpireDate", FieldName = "ExpireDate", ClassName="text-center"},
                new GridField() { Header = "QTY", FieldName = "QTY", DisplayTextBox = true, },
                new GridField() { Header = "Action", FieldName = "PrintBarcode", TextAlign = "center" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult PODetail(string PO)
        {
            HTML html = new HTML();
            string sql = @"select row_number() over(order by pd.ItemName) as [No],
pd.PONumber, pd.PODate, pd.ItemID, pd.ItemName, pd.DeliveryDate, pd.DeliverTo, pd.QTY,
i.Barcode, '<a href=""javascript:void(0)"" onclick=""PrintProductLable('''+i.Barcode+''','+convert(varchar, pd.QTY)+','''+i.ItemCode+''');"">Print Label</a>' PrintBarcode
from dbo.TblPurchaseOrderDetail pd
inner join dbo.TblItem i on i.ItemID = pd.ItemID
where pd.PONumber = '" + PO + "' and pd.SystemID = " + html.SystemID + " and pd.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "pd.ItemName";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Barcode", FieldName = "Barcode", DisplayBarcode = true },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID" },
                new GridField() { Header = "QTY", FieldName = "QTY", Format="#,#0", TextAlign="right" },
                new GridField() { Header = "ETA Date", FieldName = "DeliveryDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "DeliverTo", FieldName = "DeliverTo" },
                new GridField() { Header = "Action", FieldName = "PrintBarcode", TextAlign = "center" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        //[Authorize(Roles = "Show Item Profile")]
        //public new async Task<ActionResult> Profile(int ID)
        //{
        //    using(var ctx = new DBContext())
        //    {
        //        var item = await ctx.Database.SqlQuery<ItemProfileDetailDTO>(
        //            "EXEC SP_ITEM_PROFILE @ItemID",
        //            new SqlParameter("@ItemID", ID)
        //        ).FirstOrDefaultAsync();

        //        var itemExpiredDate = (await ctx.Database
        //            .SqlQuery<ItemProfileDetailDTO>(
        //                "EXEC SP_ITEM_EXPIRATION_BY_ITEMID @ItemID",
        //                new SqlParameter("@ItemID", ID)
        //            )
        //            .ToListAsync())
        //            .Select(x => new
        //            {
        //                x.ExpireDate,
        //                x.WarehouseName,
        //                x.CurrentStockExpired
        //            })
        //            .ToList();


        //        ViewBag.Item = item;
        //        ViewBag.ItemExpiredDate = itemExpiredDate;
        //    }
        //    return View();
        //}
        //[Authorize(Roles = "Show Item Profile")]
        //public async Task<ActionResult> Profile(int ID)
        //{
        //    using (var ctx = new DBContext())
        //    {
        //        var conn = ctx.Database.Connection;
        //        if (conn.State != ConnectionState.Open)
        //            await conn.OpenAsync();

        //        using (var cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "SP_ITEM_PROFILE";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@ItemID", ID));

        //            using (var reader = await cmd.ExecuteReaderAsync())
        //            {
        //                // First result set: item profile
        //                var item = ((IObjectContextAdapter)ctx).ObjectContext
        //                    .Translate<ItemProfileDetailDTO>(reader)
        //                    .FirstOrDefault();

        //                // Move to second result set: item expiration info
        //                await reader.NextResultAsync();
        //                var itemExpiredDate = ((IObjectContextAdapter)ctx).ObjectContext
        //                    .Translate<ItemExpiredDTO>(reader)
        //                    .ToList();

        //                // Assign to ViewBag
        //                ViewBag.Item = item;
        //                ViewBag.ItemExpiredDate = itemExpiredDate;
        //            }
        //        }
        //    }

        //    return View();
        //}
        [Authorize(Roles = "Show Item Profile")]
        public new async Task<ActionResult> Profile(int ID)
        {
            using (var ctx = new DBContext())
            {
                var item = await ctx.Database.SqlQuery<ItemProfileDetailDTO>(
                    "EXEC SP_ITEM_PROFILE @ItemID",
                    new SqlParameter("@ItemID", ID)
                ).FirstOrDefaultAsync();

                var itemExpiredDate = (await ctx.Database
                    .SqlQuery<ItemProfileDetailDTO>(
                        "EXEC SP_ITEM_EXPIRATION_BY_ITEMID @ItemID",
                        new SqlParameter("@ItemID", ID)
                    )
                    .ToListAsync())
                    .Select(x => new
                    {
                        x.ExpireDate,
                        x.WarehouseName,
                        x.CurrentStockExpired
                    })
                    .ToList();

                HTML html = new HTML();
                int telegramAlert = 0;
                var dataSet = html.LoadData($"SELECT TOP 1 TelegramAlert FROM dbo.TblStockExpireDate WHERE ItemID = {ID}");
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var val = dataSet.Tables[0].Rows[0]["TelegramAlert"];
                    if (val != DBNull.Value)
                        telegramAlert = Convert.ToInt32(val);
                }

                ViewBag.Item = item;
                ViewBag.ItemExpiredDate = itemExpiredDate;
                ViewBag.TelegramAlert = telegramAlert;
            }
            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UpdateExpireDateTelegramAlert(int ItemID, int value, string comment)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            var para = new System.Collections.Generic.Dictionary<string, string>
    {
        { "ItemID", ItemID.ToString() },
        { "value",  value.ToString()  }
    };

            string sql = "UPDATE dbo.TblStockExpireDate SET TelegramAlert = @value WHERE ItemID = @ItemID";

            SqlResult result = html.SaveChangeData(sql, false, para);

            // ✅ 2. Notify only if save succeeded
            if (result.Result == ResultType.Successful)
            {
                string status = value == 1
                    ? "turned OFF 🔕"
                    : "turned ON 🔔 ";

                string msg =
                       $"Dear Team,\n" +
                       $"Item ID: <b>{ItemID} </b> notification has been {status} <b>{html.UserFullName}</b>; " +
                       $"Updated Date: <b>{DateTime.Now:dd-MM-yyyy HH:mm:ss tt}</b>\n\n" +
                       $"User Comment: {comment}\n\n" +
                       $"Thank you and best regards,\n" +
                       $"<i>BSM Notification!</i>";

                string botToken = html.GetBotTokenBySystemID(25);
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(
                    msg, -1003110947397, 1640, null, 0, "", newBotClient: botToken);
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public string LoadStockInfo(int ID)
        {
            HTML html = new HTML();
            string sql = @"select row_number() over(order by i.ItemID) as [No], i.ItemID,
w.WarehouseName, sum(isnull(sd.QTY,0)) QTYIN,
sum(isnull(sd.QTYOUT,0)) QTYOUT, 0 OnPurchase,
(isnull(sum(sd.QTY),0) - isnull(sum(sd.QTYOUT), 0)) StockOnHand, i.ReorderPoint Alert, 
(isnull(iv.CostPrice,0) * (isnull(sum(sd.QTY),0) - isnull(sum(sd.QTYOUT), 0))) StockValue, iv.CostPrice
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
left  join dbo.TblUserWarehouse w on w.WID = sd.LocationID
where i.ItemID = " + ID + " group by i.ItemID, w.WarehouseName, i.ReorderPoint, iv.CostPrice";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName", Width = "100%"  },
                new GridField() { Header = "Alert", FieldName = "Alert", ClassName="text-info text-right"  },
                new GridField() { Header = "Cost", FieldName = "CostPrice", TextAlign="right", Format = "#,#0.00", TotalLabel="Total:"},
                new GridField() { Header = "QTY IN", FieldName = "QTYIN", Format = "n0", ClassName=" text-right", IsTotalFooter=true},
                new GridField() { Header = "QTY OUT", FieldName = "QTYOUT", Format = "n0", ClassName="text-danger text-right", IsTotalFooter=true},
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", Format = "n0", ClassName="text-primary text-right", IsTotalFooter=true},
                new GridField() { Header = "Stock Value", FieldName = "StockValue", Format = "#,#0.00", ClassName="text-right",},
            };
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public JsonResult GetStockInfo(int ID)
        {
            HTML html = new HTML();
            string sql = @"select row_number() over(order by i.ItemID) as [No], i.ItemID,
w.WarehouseName, sum(isnull(sd.QTY,0)) QTYIN,
sum(isnull(sd.QTYOUT,0)) QTYOUT, 0 OnPurchase,
(isnull(sum(sd.QTY),0) - isnull(sum(sd.QTYOUT), 0)) StockOnHand, i.ReorderPoint Alert, 
(isnull(iv.CostPrice,0) * (isnull(sum(sd.QTY),0) - isnull(sum(sd.QTYOUT), 0))) StockValue, iv.CostPrice, sd.UM
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
left  join dbo.TblUserWarehouse w on w.WID = sd.LocationID
where i.ItemID = " + ID + " group by i.ItemID, w.WarehouseName, i.ReorderPoint, iv.CostPrice, sd.UM";

            var data = html.JsonData(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public string LoadStockIn(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            string sql = $@"select row_number() over(order by sd.StockInDate desc) as [No], i.ItemID, i.ItemName, i.Barcode, w.WarehouseName, 
format(sd.StockInDate, 'dd-MM-yy') + case when isnull(sd.ItemExpirationDate, '') <> '' then '<br><span class=""text-danger"">Exp.: ' + format(sd.ItemExpirationDate,'dd-MM-yy') + '</span>' else '' end [Date],
isnull(sum(sd.QTY),0) QTYIN, sd.CostPrice, sd.Type, sd.UM
from dbo.TblStockInDetail sd
inner join dbo.TblItem i on i.ItemID = sd.ItemID
inner join dbo.TblUserWarehouse w on w.WID = sd.LocationID
where sd.QTY > 0 and i.ItemID = {ID} 
group by i.ItemID, i.ItemName, w.WarehouseName, sd.StockInDate, i.Barcode, sd.CostPrice, sd.ItemExpirationDate, sd.Type, sd.UM
";
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Date IN", FieldName = "Date", },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName", Width="100%" },
                new GridField() { Header = "Type", FieldName = "Type", TotalLabel="Total:" },
                new GridField() { Header = "QTY IN", FieldName = "QTYIN", Format = "n0", IsTotalFooter = true, ClassName="text-right" },
            };
            html.TblID = "TblStockIn";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public string LoadStockOut(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            string sql = $@"select row_number() over(order by sd.StockOutDate desc) as [No], i.ItemID, i.ItemName, i.Barcode, w.WarehouseName, case when sd.Type = 'Stock Out' then format(sd.StockOutDate, 'dd-MM-yyyy') else format(sd.StockInDate, 'dd-MM-yyyy') end as [Date],
    isnull(sum(sd.QTYOUT),0) QTYOUT, case when isnull(sd.SalePrice, 0) = 0 then i.SellingPrice else sd.SalePrice end as SalePrice, sd.Type, sd.UM
    from dbo.TblStockInDetail sd
    inner join dbo.TblItem i on i.ItemID = sd.ItemID
    inner join dbo.TblUserWarehouse w on w.WID = sd.LocationID
    where i.ItemID = {ID}   AND (
        (sd.Type = 'Stock Out' AND (sd.ItemType IS NULL OR sd.ItemType <> 'Stock Transfers'))
        OR (sd.Type = 'Stock Adjustment' AND sd.QTYOUT > 0)
      )
    group by i.ItemID, i.ItemName, w.WarehouseName, sd.StockInDate, i.Barcode, case when isnull(sd.SalePrice, 0) = 0 then i.SellingPrice else sd.SalePrice end, sd.Type, sd.UM, sd.StockOutDate";
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Date OUT", FieldName = "Date", },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName", Width="100%"},
                new GridField() { Header = "Type", FieldName = "Type",  TotalLabel="Total:"},
                new GridField() { Header = "QTY OUT", FieldName = "QTYOUT", Format = "n0", IsTotalFooter = true, ClassName="text-danger text-right" },
            };
            html.TblID = "TblStockOut";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public string LoadPurchaseDetail(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            string sql = @"select row_number() over(order by pd.PONumber desc) as [No], '<a href=""javascript:void(0);"" onclick=""ShowPOInvoice('+convert(varchar, bd.VendorID)+', '+convert(varchar, bd.PurchaseOrderID)+', '''+bd.PONumber+''')"">'+pd.PONumber+'</a>' as PONumber, 
format(pd.PODate, 'dd-MM-yy') PODate, i.ItemName, pd.QTY, format(pd.DeliveryDate, 'dd-MM-yy') DeliveryDate, pd.DeliverTo, 
'<span class=""badge badge-pill text-uppercase badge-primary"">' + s.[Status] + '</span>' [Status], bd.VendorName,
case when isnull(pd.StatusID,0) = 66 then '<a href=""javascript:void(0);"" onclick=""PrintProductLable('''+i.Barcode+''', '+convert(varchar, pd.QTY)+', '''+i.ItemCode+''');"">Print Lable</a>' else '' end PrintProductLabel,
pd.RequestBy
from dbo.TblItem i
inner join dbo.TblPurchaseOrderDetail pd on pd.ItemID = i.ItemID
inner join dbo.TblPurchaseOrder bd on bd.PurchaseOrderID = pd.PurchaseOrderID
left join dbo.TblStatus s on s.StatusID = pd.StatusID
where pd.ItemID = " + ID;
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "PO Date", FieldName = "PODate",  },
                new GridField() { Header = "PO Number", FieldName = "PONumber", },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName", },
                new GridField() { Header = "ItemName", FieldName = "ItemName" },
                new GridField() { Header = "Request", FieldName = "RequestBy", TotalLabel="Total:" },
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-primary text-right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Delivery Date", FieldName = "DeliveryDate", ClassName="text-danger text-center" },
                new GridField() { Header = "Deliver To", FieldName = "DeliverTo" },
            };
            html.TblID = "TblPurchase";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public ActionResult PrintProductLabel(string Barcode, int QTY, string Code)
        {
            ViewBag.QTY = QTY;
            ViewBag.Barcode = Barcode;
            ViewBag.Code = Code;
            return View();
        }

        public string LoadSale(int ID)
        {
            HTML html = new HTML();
            string sql = $@"select row_number() over(order by ci.InvoiceDate desc) as [No], 
format(ci.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, i.Barcode, i.ItemName, uw.WarehouseName, '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, ci.CustomerID)+','''+ ci.InvoiceNo +''', '+convert(varchar(max), (select top 1 IsIncludedVAT from dbo.TblStockOut where StockOutID = sd.StockOutID and ItemID = sd.ItemID))+', 11, '''', false)"">'+ci.InvoiceNo+'</a>' as InvoiceNo, ci.InvoiceNo InvoiceNo1, ci.QTY, ci.CustomerCode,
ci.CustomerName, ci.Amount, sd.SerialNumber
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblCustomerInvoice ci on ci.ItemID = sd.ItemID and sd.InvoiceNumber = ci.InvoiceNo
inner join dbo.TblUserWarehouse uw on uw.WID = ci.LocationID
where i.ItemID = {ID}
union all 
select row_number() over(order by (select top 1 TranDate from dbo.TblSales where SalesID = ci.SalesID) desc) as [No], 
format((select top 1 TranDate from dbo.TblSales where SalesID = ci.SalesID), 'dd-MM-yyyy') InvoiceDate, i.Barcode, i.ItemName, uw.WarehouseName, '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, ci.CustomerID)+','''+ ci.InvoiceNo +''', '+convert(varchar(max), (select top 1 IsIncludedVAT from dbo.TblStockOut where StockOutID = sd.StockOutID and ItemID = sd.ItemID))+', 11, '''', false)"">'+ci.InvoiceNo+'</a>' as InvoiceNo, case when (select top 1 Type from dbo.TblSales where SalesID = ci.SalesID) = 'Invoices' then '<a href=""javascript:void(0);"" onclick=""showInvoicesForm(' + 
        CONVERT(VARCHAR, (SELECT TOP 1 SalesID FROM dbo.TblSales WHERE SalesID = ci.SalesID)) + ', ''' + 
        (SELECT TOP 1 InvoiceNumber FROM dbo.TblSales WHERE SalesID = ci.SalesID) + ''', ''' + 
        (SELECT TOP 1 s.Type FROM dbo.TblSales s WHERE s.SalesID = ci.SalesID) + ''')"">' + 
        (SELECT TOP 1 InvoiceNumber FROM dbo.TblSales WHERE SalesID = ci.SalesID) + 
    '</a>' else '<a href=""javascript:void(0);"" onclick=""showInvoicesForm(' + 
        CONVERT(VARCHAR, (SELECT TOP 1 SalesID FROM dbo.TblSales WHERE SalesID = ci.SalesID)) + ', ''' + 
        (SELECT TOP 1 DNNumber FROM dbo.TblSales WHERE SalesID = ci.SalesID) + ''', ''' + 
        (SELECT TOP 1 s.Type FROM dbo.TblSales s WHERE s.SalesID = ci.SalesID) + ''')"">' + 
        (SELECT TOP 1 DNNumber FROM dbo.TblSales WHERE SalesID = ci.SalesID) + 
    '</a>' end AS InvoiceNo1, ci.QTY, ci.CustomerCode,
(select top 1 CustomerName from dbo.TblSales where SalesID = ci.SalesID) CustomerName, ci.Amount, sd.SerialNumber
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblCustomerInvoice ci on ci.ItemID = sd.ItemID and sd.SalesID = ci.SalesID
inner join dbo.TblUserWarehouse uw on uw.WID = sd.LocationID
where i.ItemID = {ID}";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", },
                new GridField() { Header = "Invoice No.", FieldName = "InvoiceNo1" },
                new GridField() { Header = "ItemName", FieldName = "ItemName" },
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber", ClassName="text-red" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                //new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName", TotalLabel="Total:" },
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-primary text-right", Format="n0", IsTotalFooter = true, },
                new GridField() { Header = "Price", FieldName = "Amount", TextAlign="right", Format="#,#0.00", IsTotalFooter= true}
            };
            html.TblID = "TblSale";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }

        public ActionResult Vendor(int ID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by v.VendorName) as [No],
i.Barcode, i.ItemName,
sd.VendorID, v.VendorCode, v.VendorName, v.Phone, v.[Address], i.CostPrice,
'<a href=""javascript:void(0);"" onclick=""PurchaseNow('+convert(varchar, i.ItemID)+', '''+
i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)
+''','+convert(varchar, sd.VendorID)+', '''+v.VendorCode+''', '''+v.VendorName+''');"">Purchase</a>' Purchase
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblVendor v on v.VendorID = sd.VendorID
where i.ItemID = " + ID;
            html.GH.SQL = sql;
            html.ShowSearchBox = true;
            html.SearchBoxWithColumnName = "v.VendorCode + v.VendorName + v.Phone + v.[Address], convert(varchar, i.CostPrice)";
            html.OrderBy = " v.VendorName";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right",  },
                new GridField() { Header = "Vendor Code", FieldName = "VendorCode", Link="#!/Profile?ID=", LinkParameterField ="VendorID", NewPage = true },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName", Link="#!/Profile?ID=", LinkParameterField ="VendorID", NewPage = true  },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Cost", FieldName = "CostPrice", TextAlign="right", Format="#,#0.00" },
                new GridField() { Header = "Action", FieldName = "Purchase", }
            };
            html.TblID = "TblVendor";
            html.ShowGridOnly = true;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "TblVendor");
            return View();
        }

        public string LoadStockTransfer(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            string sql = $@"select row_number() over(order by b.StockTransfersNumber desc) as [No], i.ItemID, i.ItemCode, i.ItemName,bd.UM, bd.Reason, b.SystemID, b.[Description], bd.SerialNumber,  '<a href=""javascript:void(0);"" onclick=""showFormTransfer('''+ b.StockTransfersNumber +''', '+convert(varchar, b.StockInID)+')"">'+b.StockTransfersNumber+'</a>' as TransferNo, format(b.CreateDate, 'dd-MM-yyyy') as TransferDate, 
(select top 1 wf.WarehouseName from dbo.TblStockInDetail sd inner join dbo.TblUserWarehouse wf on wf.WID = sd.LocationID where sd.StockInID = b.StockInID and sd.Type = 'Stock Out') as FromWarehouse, 
(select top 1 wt.WarehouseName from dbo.TblStockInDetail sd inner join dbo.TblUserWarehouse wt on wt.WID = sd.LocationID where sd.StockInID = b.StockInID and sd.Type = 'Stock Transfer') as ToWarehouse,
sum(isnull(bd.QTYOUT,0)) QTYOUT
from dbo.TblStockIn b
inner join dbo.TblStockInDetail bd on bd.StockInID = b.StockInID
inner join dbo.TblItem i on i.ItemID = bd.ItemID
left join dbo.TblItemSerialDetail srd on srd.ItemID = i.ItemID and srd.SerialNumber = bd.SerialNumber
inner join dbo.TblUserLogin u on u.UserLoginID = b.UserID
left join dbo.TblUserLogin up on up.UserLoginID = b.PreparedUserID
left join dbo.TblUserLogin uc on uc.UserLoginID = b.CheckByID
left join dbo.TblUserLogin ur on ur.UserLoginID = b.ReceivedByID
where i.ItemID = {ID} and b.StockTransfersNumber is not null and b.SystemID = {html.SystemID} and b.WarehouseID = {html.WarehouseID}
group by i.ItemID, i.ItemName,bd.UM, bd.Reason, i.ItemCode, b.[Description], bd.SerialNumber, b.SystemID, b.StockTransfersNumber, 
format(b.CreateDate, 'dd-MM-yyyy'), b.StockInID";
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Transfer Date", FieldName = "TransferDate", },
                new GridField() { Header = "Transfer No", FieldName = "TransferNo", },
                new GridField() { Header = "Item Code", FieldName = "ItemCode",},
                new GridField() { Header = "Item Name", FieldName = "ItemName", },
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber", },
                new GridField() { Header = "QTY", FieldName = "QTYOUT",},
                new GridField() { Header = "UM", FieldName = "UM", },
                new GridField() { Header = "From Warehouse", FieldName = "FromWarehouse" },
                new GridField() { Header = "To Warehouse", FieldName = "ToWarehouse" },
                new GridField() { Header = "Reason", FieldName = "Reason" },
                new GridField() { Header = "Description", FieldName = "Description" },
            };
            html.TblID = "TblStockTransfer";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public string LoadStockAdjust(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Adjust Date:",
                DBField = "b.CreateDate"
            });

            string sql = @"select row_number() over(order by b.ReferenceNo desc) as [No],'<a href=""javascript:void(0);"" onclick=""showFormAdjust('''+ format(b.CreateDate, 'dd-MM-yyyy') +''', '+convert(varchar, b.StockInID)+')"">'+b.ReferenceNo+'</a>' as ReferenceNo,
format(b.CreateDate, 'dd-MM-yyyy') as AdjustDate, b.StockInID, i.Barcode, i.ItemID, i.ItemName, i.ItemCode, bd.QTY QTYIN, bd.UM, bd.QuantityOnHand, bd.QuantityAvliable, bd.QuantityAdjusted, bd.StockInDate, uw.WarehouseName, i.MaintainStock, bd.Reason,isnull(bd.QTYUnusable,0) QTYUnusable, bd.OrderQTY, 
b.BillNumber, b.PONumber, b.VendorID, b.BillID, ig.ItemGroup, bd.AdjustType, bd.SerialNumber, bd.SerialNumberID
from dbo.TblStockIn b
inner join dbo.TblStockInDetail bd on bd.StockInID = b.StockInID
inner join dbo.TblItem i on i.ItemID = bd.ItemID
inner join dbo.TblUserWarehouse uw on uw.WID = b.WID
left join dbo.TblBranch br on br.BranchID = uw.BranchID
left join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID
where bd.StatusID = 82 and i.ItemID = " + ID;
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Warehouse Name", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 10, DisplayOneRow= true, },
                new GridField() { Header = "Adjust Date", FieldName = "AdjustDate"},
                new GridField() { Header = "Adjust No.", FieldName = "ReferenceNo"},
                new GridField() { Header = "Item Code", FieldName = "ItemCode", TextAlign="center" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber", ClassName="text-red text-center" },
                new GridField() { Header = "Adjust Type", FieldName = "AdjustType" },
                new GridField() { Header = "Reason", FieldName = "Reason" },
                new GridField() { Header = "Quantity Avliable", FieldName = "QuantityAvliable", TextAlign="right", },
                new GridField() { Header = "Quantity OnHand", FieldName = "QuantityOnHand", TextAlign="right", },
                new GridField() { Header = "Quantity Adjusted", FieldName = "QuantityAdjusted", TextAlign="right",},
                new GridField() { Header = "UM", FieldName = "UM", }
            };
            html.ShowPaging = false;
            html.TblID = "TblStockAdjust";
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }

        public ActionResult CreateItemGroup(string PrimaryFieldValue, string Type = "")
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
            string TypeName = "", TypeID = "", InventoryAccountID = "", ExpenseAccountID = "", IncomeAccountID = "";
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    TypeID = dr["TypeID"].ToString();
                    TypeName = dr["TypeName"].ToString();
                    InventoryAccountID = dr["InventoryAccountID"].ToString();
                    ExpenseAccountID = dr["ExpenseAccountID"].ToString();
                    IncomeAccountID = dr["IncomeAccountID"].ToString();
                    string InventoryAccount = dr["InventoryAccount"].ToString();
                    string IncomeAccount = dr["IncomeAccount"].ToString();
                    string ExpenseAccount = dr["ExpenseAccount"].ToString();
                }
            }

            if (Type == "NewItem")
            {
                html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewItem();";
            }
            html.FormID = "CreateItemGroup";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Item Information",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Categories:", ID="StockName",
                            Required=true,
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            AddNewFuntionName ="AddNewStockName()",
                            DivClass="pt-1",
                            DisplayText="StockName",
                            Value="StockNameID",
                            Multiple = true,
                            SQL_CBO=sqlStockName,
                            //FieldName="StockName",
                            //"select StockNameID, StockName from dbo.TblStockName where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by StockName",
                            //isTypeahead=true, HiddenFeild="StockNameID",
                            //SQLCallBack="select ItemGroupID, ItemGroup from dbo.TblItemGroup where StockNameID = @StockNameID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemGroup",
                            //CallBackToControlID="ItemGroup",
							//isApplyTypeaheadOnCallBack=true,
                            //TypeAheadAddNewFuntionName="AddNewStockName()",
							//AddButtonFunction="AddNewStockName()",							
                        },
                        new HtmlTextBox()
                        {
                            Label="Type:", ID="TypeName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, FieldName = "TypeName",
                            SQL="select ServiceID ItemTypeID, ServiceName TypeName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ServiceName",
                            isTypeahead=true, HiddenField="ItemTypeID",
                            HiddenFieldValue=TypeID, Value=TypeName,
                            AddButtonFunction="AddNewService()",
                            TypeAheadAddNewFunctionName="AddNewService()", DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Group Name:", ID="Txt_ItemGroupName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISUniqueField= true, DBField = "ItemGroupName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 ItemGroupName from dbo.TblItem where isnull(ItemGroupName,'') <> '' and lower(ItemGroupName) like @SearchText and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by ItemGroupName"
                        },
                        new HtmlComboBox()
                        {
                            Label="Base On:", ID="BaseOn",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BaseOn",
                            TableName="TblItem",
                            SQL_CBO="select 'Barcode' BaseOn union all select 'Serial Number' BaseOn",
                            DisplayText="BaseOn",
                            Value="BaseOn", //Required = true
                            Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {						
						//new HtmlTextBox()
						//{
						//	Label="Manufacturer:", ID="Txt_Manufacturer", Required=true,
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="Manufacturer",
						//	SQL = "select distinct top 10 i.Manufacturer from dbo.TblManufacturer i where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.Manufacturer",
						//	isTypeahead = true,
						//	AddButtonFunction="AddNewManufacturer()",
						//	TypeAheadAddNewFuntionName="AddNewManufacturer()",
						//	AddValueToTblNameIfNotExistingData="TblManufacturer",
						//	AddValueToTblNameIfNotExistingDataFieldName="Manufacturer"
						//},
						new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID", Hide = true
                        },
                        new HtmlComboBox()
                        {
                            //Label="UM:", ID="Txt_UMName", Required=true,
                            //Class="input2 CustInfo", IsCustomForm = true, FieldName = "UM",
                            //SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                            //isTypeahead=true, HiddenFeild="UomID",
                            //TypeAheadAddNewFuntionName="AddNewUM();",
                            //AddButtonFunction="AddNewUM()",
                            Label="UOM:", ID="Txt_UMName",
                            Required=true,
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            AddNewFuntionName ="AddNewUM()",
                            DivClass="pt-1",
                            DisplayText="UM",
                            Value="UomID",
                            Multiple = true,
                            SQL_CBO="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Brand:", ID="ItemBrand",
                            Class="input2 CustInfo", IsCustomForm = true,
                            HiddenField="ItemBrandID",isTypeahead=true,
                            AddButtonFunction="AddNewItemBrand()", FieldName="ItemBrand",
                            SQL="select ItemBrandID, ItemBrand from dbo.TblItemBrand where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemBrand",
                            TypeAheadAddNewFunctionName="AddNewItemBrand()",
                            AddValueToTblNameIfNotExistingData="TblItemBrand",
                            AddValueToTblNameIfNotExistingDataFieldName="ItemBrand"
                        },
                        new HtmlTextBox()
                        {
                            Label="Model:", ID="Model",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true,
                            DBField="Model",//ISUniqueFiled=true,
                            //CheckExsitingWithControllName="SerialNumber",
                            //CheckExsitingWithField="SerialNumber",
                            TableName="TblItem"
                            //CheckExsitingWithDefaultValue=""
                        },
                        new HtmlTextBox()
                        {
                            Label="Alert Reorder:", ID="ReorderPoint", DivClassCol="col-sm-auto",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISNumeric = true, ISInteger = true,
                        },
                    }
                },new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Returnable Item:", ID="ReturnableItem", //Value = "true",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", DivClass="wrap-input2 mt-2 pt-1",
                            ISCheckBox=true,
                            Type="checkbox",
                            TableName="TblItem", Hide = true
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
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Expiration Date:", ID="ItemExpirationDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISDateTime = true, SingleDatePicker=true,
                            DefaultShowDateValue = false, Hide = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            //Label="Inventory Asset Account:", ID="Inventory",
                            //Required=true,
                            //Class="input2 CustInfo", IsCustomForm = true,
                            //DBField="InventoryAccountName",
                            //isTypeahead=true,
                            //TypeAheadAddNewFuntionName ="AddNewChartOfAccount('Inventory')",
                            //SQL="select ChartOfAccountID InventoryChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) InventoryAccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            //TypeaheadGroupByField= "AccountType",
                            //HiddenFeild="InventoryChartOfAccountID",
                            //HidenFieldValue = InventoryAccountID,
                            CboValue = InventoryAccountID,
                            Label="Inventory Asset Account:", ID="Inventory",
                            Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="InventoryChartOfAccountID",
                            SQL_CBO = @"select distinct coa.ChartOfAccountID InventoryAccountName, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 1 and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by ac.AccountCategory, act.AccountTypes",
                            Value = "InventoryAccountName",
                            DisplayText = "AccountName",
                            GroupByField = "AccountTypes",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                        },

                        new HtmlComboBox()
                        {
                            //Label="Expense Account:", ID="PurchaseChartOfAccountID",
                            //Class="input2 CustInfo", IsCustomForm = true,
                            //DBField="PurchaseChartOfAccount",
                            //SQL="select ChartOfAccountID PurchaseChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) PurchaseChartOfAccount from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            //isTypeahead=true,
                            CboValue=ExpenseAccountID,
                            //HiddenFeild="PurchaseChartOfAccountID",
                            //TypeaheadGroupByField="AccountType",
                            //TypeAheadAddNewFuntionName="AddNewChartOfAccount('PurchaseChartOfAccountID')",
                            //HidenFieldValue=ExpenseAccountID, Required = true
                            Label="Expense Account:", ID="PurchaseChartOfAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PurchaseChartOfAccountID",
                            SQL_CBO = @"select distinct coa.ChartOfAccountID PurchaseChartOfAccount, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 4 and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by ac.AccountCategory, act.AccountTypes",
                            Value = "PurchaseChartOfAccount",
                            DisplayText = "AccountName",
                            GroupByField = "AccountTypes",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                            Required = true
                        },
                        new HtmlComboBox()
                        {
                            Label="Income Account:", ID="SaleChartOfAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SaleChartOfAccountID",
                            SQL_CBO=@"select distinct coa.ChartOfAccountID SaleChartOfAccount, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 3 and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by ac.AccountCategory, act.AccountTypes",
                            Value = "SaleChartOfAccount",
                            DisplayText = "AccountName",
                            GroupByField = "AccountTypes",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "350" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                            Required= true,
                            //Label="Income Account:", ID="SaleChartOfAccountID",
                            //Class="input2 CustInfo", IsCustomForm = true,
                            //DBField="SaleChartOfAccount",
                            //SQL="select ChartOfAccountID SaleChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) SaleChartOfAccount from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            //isTypeahead = true,
                            CboValue=IncomeAccountID,
                            //HiddenFeild="SaleChartOfAccountID",
                            //TypeaheadGroupByField="AccountType",
                            //TypeAheadAddNewFuntionName="AddNewChartOfAccount('SaleChartOfAccountID')",
                            //HidenFieldValue= IncomeAccountID, Required = true
                        }
                    },
                },
                new  FormData() {
                    FormTitle="Multiple Items",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_MultipleItems",
                            InputName="Attribute",
                            TRName="TR_MultipleItems_No",
                            TDName="TD_MultipleItems_No",
                            ScriptCallBackFunction="CallSetAutoComplete_Attribute",
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Attribute", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Options", Class="text-nowrap",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_MultipleItems_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_MultipleItems_No"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Attribute",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Attribute",
                                            DeleteTDRowFunction="RemoveTableRow('TR_MultipleItems_No', 'TD_MultipleItems_No', this,'','Attribute');",
                                            SQL_AutoComplete = "select distinct top 10 [Attribute] from dbo.TblItemAttribute where lower([Attribute]) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID +" order by Attribute",
                                            ISAutoComplete = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Options",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3", onTagsInputAddedFunction = "OnOptionsItemAdded",
                                            placeholder="Enter Options", isTagsInput=true,
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
                new  FormData() {
                    FormTitle="Items Detail",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_ItemsDetail",
                            TRName="TR_ItemDetails",
                            TDName="TD_ItemDetails",
                            PrimaryFieldName=",ItemID",
                            GetIDENTITYByFiled="ItemID",
                            //GetIDENTITYFromFieldName="ItemID",
                            TableName="TblItem, TblItemVariant",
                            PrimaryFieldValue=PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                //new DBField()
                                //{
                                //    DBFieldName="BaseOn", SetValue = "$(\"#BaseOn\").val().toString()"
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="ItemExpirationDate", SetValue = "$(\"#ItemExpirationDate\").val()", IsDate = true
                                //},
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
                                    DBFieldName="ItemGroupName", SetValue="$(\"#Txt_ItemGroupName\").val()",
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="Warehouse", SetValue = "$(\"#Warehouse\").val()"
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="WID", SetValue = "$(\"#Warehouse_WID\").val()"
                                //},
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
                                    DBFieldName="UM", Value="iUOM", GetValueFromInputName = true, TblName=", TblItemVariant"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="iUOMID", GetValueFromInputName = true, TblName=", TblItemVariant"
                                    //DBFieldName="UomID", SetValue="$(\"#Txt_UMName_UomID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="ReorderPoint", SetValue="$(\"#ReorderPoint\").val()",
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="Multiplier", SetValue="1",
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="ReturnableItem", SetValue="$(\"#ReturnableItem\").val()",
                                //},
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
                                //new DBField()
                                //{
                                //    DBFieldName="VendorName", SetValue="$(\"#VendorName\").val()",
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="VendorID", SetValue="$(\"#VendorName_VendorID\").val()",
                                //},
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No.", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Barcode", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Item Code", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Item Name", Class="text-nowrap col-sm-4"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap col-sm-auto",
                                },
                                new TblTHead()
                                {
                                     Label="Multiplier", Class="text-nowrap col-sm-auto",
                                },
                                new TblTHead()
                                {
                                     Label="Cost Price", Class="text-nowrap col-sm-auto",
                                },
                                new TblTHead()
                                {
                                     Label="Selling Price", Class="text-nowrap col-sm-auto",
                                },
                                new TblTHead()
                                {
                                     Label="Opening Stock", Class="text-nowrap col-sm-auto", Hide = true,
                                },
                                new TblTHead()
                                {
                                     Label="Rate/Unit", Class="text-nowrap col-sm-auto", Hide = true,
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
                                    OpenTR="<tr name=\"TR_ItemDetails\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_ItemDetails"
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
                                            DeleteTDRowFunction="RemoveTableRow('TR_ItemDetails', 'TD_Items_Detail', this,'','iBarcode');",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iItemCode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Code",
                                            DBField = "ItemCode"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iItemName",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Name",
                                            DBField="ItemName", Required = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iUOM",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter UOM",
                                            DBField = "UomID", ISNumeric=true, Required = true,
                                            HiddenField="iUOMID", HiddenFieldValue="0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iMultiplier",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Multiplier",
                                            DBField = "Multiplier", ISNumeric=true, Required = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iCostPrice", Value = "0",
                                            Class="input2 CustInfo text-right", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Cost Price",
                                            DBField = "CostPrice", ISNumeric=true, Required = true,
                                            Align="right",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iSellingPrice",
                                            Class="input2 CustInfo text-right", IsCustomForm = true,
                                            DivClass="wrap-input3", Value = "0",
                                            placeholder="Enter Selling Price",
                                            DBField = "SellingPrice", ISNumeric=true, Required = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iOpeningStock",
                                            Class="input2 CustInfo text-right", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Opening Stock", ISNumeric=true, ISInteger =true,
                                            DivClassCol="col-sm-auto",
                                            DBField="OpeningStock", Value = "0",
                                            Hide= true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="iOpeningStockRatePerUnit",
                                            Class="input2 CustInfo text-right", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Opening Stock Rate Per Unit", ISNumeric=true,
                                            DivClassCol="col-sm-auto",
                                            DBField="OpeningStockRatePerUnit", Value = "0",
                                            Hide = true,
                                        },
                                        new HtmlFile()
                                        {
                                            Name="ItemImage",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemImage", IsDragDropFile = true,
                                            AcceptImageOnly=true, DestinationFile="Uploads/Items/",
                                            ShowDropZoneEnabled = false, Multiple = true,
                                            TableName= "TblItem, TblItemVariant", PrimaryFieldName = "ItemID",
                                            PrimaryFieldValue = PrimaryFieldValue,
                                            DivClassCol="col-sm-auto", Align = "right"
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            };
            html.FieldIDFocus = "StockName";
            //html.FocusFieldIDAfterReset = "Txt_ItemGroupName";
            //html.RestFields = "Txt_ItemGroupName";
            html.SaveChangeAndClose = true;
            html.SaveChangeCallbackOnCloseDialog = true;
            html.SaveChangeCallbackOnCloseDialogValue = "Txt_ItemName";
            html.SaveChangeFunction = "SaveChangeItemGroup();";
            //html.SaveChangeCallbackFunction = "addCategoryToItem()";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult SaveChangeItemGroup(dynamic[] Data)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> DataInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(Data[0]);
            dynamic itemAttribute = new JavaScriptSerializer().DeserializeObject(DataInfo["itemAttribute"].ToString());
            dynamic itemDetail = new JavaScriptSerializer().DeserializeObject(DataInfo["itemDetail"].ToString());
            string sql = "";
            int i = 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<Data>");

            foreach (dynamic items in itemDetail)
            {
                sb.AppendLine("<ItemInfo>");
                foreach (var item in items)
                {
                    if (item.Value != null)
                    {
                        string key = item.Key.ToString();
                        string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                        sb.AppendLine("<" + key + ">" + html.EscapeXml(value).ToString().Trim() + "</" + key + ">");
                    }
                }
                sb.AppendLine("<CreateBy>" + html.UserFullName + "</CreateBy>");
                sb.AppendLine("<CreateDate>" + html.GetCurrentDate() + "</CreateDate>");
                sb.AppendLine("<SystemID>" + html.SystemID + "</SystemID>");
                sb.AppendLine("<WarehouseID>" + html.WarehouseID + "</WarehouseID>");
                sb.AppendLine("<DomainID>" + html.DomainID + "</DomainID>");
                sb.AppendLine("</ItemInfo>");
            }
            sb.AppendLine("</Data>");

            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" },
            };
            result = html.SaveChangeData("SP_INSERT_ITEM_GROUP_NAME", true, Para, true, true);


            if (HttpContext.Request.Files.Count > 0)
            {
                List<string[]> list = result.Data.Tables[0].AsEnumerable()
                               .Select(dr => dr.ItemArray.Select(x => x.ToString()).ToArray())
                               .ToList();
                foreach (var ls in list)
                {
                    sql = "select UMItemID from dbo.TblItemVariant where ItemID = " + ls[0].ToString();
                    DataSet ds = html.LoadData(sql);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string UMItemID = dr["UMItemID"].ToString();
                        string storeFileName = "";
                        string path = HttpContext.Server.MapPath("~/Uploads/Items/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        i++;
                        string ControlName = "ItemImage" + i;
                        for (int j = 0; j < HttpContext.Request.Files.Count; j++)
                        {
                            string key = HttpContext.Request.Files.GetKey(j);
                            if (key == ControlName)
                            {
                                foreach (var file in HttpContext.Request.Files.GetMultiple(key))
                                {
                                    string savedFileName = Path.Combine(path, file.FileName);
                                    file.SaveAs(savedFileName);
                                    if (storeFileName == "")
                                        storeFileName = file.FileName;
                                    else
                                        storeFileName += "~" + file.FileName;
                                }
                                sql = @"update dbo.TblItemVariant set DestinationFolder= 'Uploads/Items/', ItemImage =N'" + storeFileName + "' where UMItemID = " + UMItemID;
                                html.SaveChangeData(sql, false, false);
                                break;
                            }
                        }
                    }
                }
            }


            //            foreach (dynamic item in itemDetail)
            //            {
            //                var StockNameID = html.HtmlDecode(item["StockName"].ToString());
            //                var StockName = html.HtmlDecode(item["StockName"].ToString());
            //                var ItemTypeID = html.HtmlDecode(item["ItemTypeID"].ToString());
            //                var TypeName = html.HtmlDecode(item["TypeName"].ToString());
            //                var ItemGroupName = html.HtmlDecode(item["ItemGroupName"].ToString());
            //                var UomID = item["UomID"].ToString();
            //                var UM = html.HtmlDecode(item["UM"].ToString());
            //                var ItemBrandID = (item["ItemBrandID"].ToString() == "" ? "0" : item["ItemBrandID"].ToString());
            //                var ItemBrand = html.HtmlDecode(item["ItemBrand"].ToString());
            //                var ReorderPoint = (item["ReorderPoint"].ToString() == "" ? "0" : item["ReorderPoint"].ToString());
            //                var ReturnableItem = item["ReturnableItem"].ToString();
            //                var InventoryChartOfAccountID = item["InventoryChartOfAccountID"].ToString();
            //                var InventoryAccountName = html.HtmlDecode(item["InventoryAccountName"].ToString());
            //                var PurchaseChartOfAccountID = item["PurchaseChartOfAccountID"].ToString();
            //                var PurchaseChartOfAccount = html.HtmlDecode(item["PurchaseChartOfAccount"].ToString());
            //                var SaleChartOfAccountID = item["SaleChartOfAccountID"].ToString();
            //                var SaleChartOfAccount = html.HtmlDecode(item["SaleChartOfAccount"].ToString());
            //                var Barcode = item["Barcode"].ToString();
            //                var ItemCode = html.HtmlDecode(item["ItemCode"].ToString());
            //                var ItemName = html.HtmlDecode(item["ItemName"].ToString());
            //                var CostPrice = item["CostPrice"].ToString();
            //                var SellingPrice = item["SellingPrice"].ToString();
            //                var OpeningStock = item["OpeningStock"].ToString();
            //                var OpeningStockRatePerUnit = item["OpeningStockRatePerUnit"].ToString();
            //                var CreateBy = html.UserFullName;
            //                var CreateDate = html.GetCurrentDate();
            //                var SystemID = html.SystemID;
            //                var WarehouseID = html.WarehouseID;
            //                string str = string.Concat("insert into dbo.TblItem (StockNameID, StockName, ItemTypeID, TypeName, ItemGroupName,");
            //                str += string.Concat("UomID, UM, ItemBrandID, ItemBrand, ReorderPoint, ReturnableItem, InventoryChartOfAccountID, InventoryAccountName, PurchaseChartOfAccountID,PurchaseChartOfAccount,SaleChartOfAccountID,SaleChartOfAccount,Barcode,ItemCode,ItemName,CostPrice,SellingPrice,OpeningStock,OpeningStockRatePerUnit,CreateBy,CreateDate,SystemID,WarehouseID)");
            //                str += string.Concat("values(N'", StockNameID, "', N'", StockName, "',", ItemTypeID, ", N'", TypeName, "',N'", ItemGroupName, "',", UomID, ", N'", UM, "', ", ItemBrandID, ", N'", ItemBrand, "', ", ReorderPoint, ", '", ReturnableItem, "',", InventoryChartOfAccountID, ", N'", InventoryAccountName, "',", PurchaseChartOfAccountID, ",N'", PurchaseChartOfAccount, "',", SaleChartOfAccountID, ",N'", SaleChartOfAccount, "','", Barcode, "',N'", ItemCode, "',N'", ItemName, "',", CostPrice, ",", SellingPrice, ",", OpeningStock, ",", OpeningStockRatePerUnit, ",N'", CreateBy, "','", CreateDate, "',", SystemID, ",", WarehouseID, ");select @@IDENTITY;");
            //                //                sql = @"insert into dbo.TblItem (StockNameID, StockName, ItemTypeID, TypeName, ItemGroupName, 
            //                //UomID, UM, ItemBrandID, ItemBrand, ReorderPoint, ReturnableItem, InventoryChartOfAccountID, InventoryAccountName,
            //                //PurchaseChartOfAccountID,PurchaseChartOfAccount,SaleChartOfAccountID,SaleChartOfAccount,Barcode,ItemCode,
            //                //ItemName,CostPrice,SellingPrice,OpeningStock,OpeningStockRatePerUnit,CreateBy,CreateDate,SystemID,WarehouseID)
            //                //values(" + StockNameID + ", '" + StockName + "'," + ItemTypeID + ", '" + TypeName + "','" + ItemGroupName + @"', 
            //                //" + UomID + ", '" + UM + "', " + ItemBrandID + ", '" + ItemBrand + "', " + ReorderPoint + ", '" + ReturnableItem + "'," + InventoryChartOfAccountID + ", '" + InventoryAccountName + @"',
            //                //" + PurchaseChartOfAccountID + ",'" + PurchaseChartOfAccount + "'," + SaleChartOfAccountID + ",'" + SaleChartOfAccount + "','" + Barcode + "','" + ItemCode + @"',
            //                //'" + ItemName + "'," + CostPrice + "," + SellingPrice + "," + OpeningStock + "," + OpeningStockRatePerUnit + ",'" + CreateBy + "','" + CreateDate + "'," + SystemID + "," + WarehouseID + @");
            //                //select @@IDENTITY;";
            //                result = html.SaveChangeData(str, false, true);
            //                string ItemID = result.Value;
            //                foreach (dynamic itemAtt in item["itemAttrsDetail"])
            //                {
            //                    string ItemAttribute = itemAtt.Key;
            //                    string ItemAttributeValue = html.HtmlDecode(itemAtt.Value);
            //                    sql = @"declare @ItemAttributeID int; if(not exists(select ItemAttributeID from dbo.TblItemAttribute where lower([Name]) = N'" + ItemAttribute.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @"))
            //begin
            //	insert into dbo.TblItemAttribute ([Name], CreateBy, CreateDate, SystemID, WarehouseID)
            //values (N'" + ItemAttribute + "', N'" + html.UserFullName + "', '" + html.GetCurrentDate() + "'," + html.SystemID + "," + html.WarehouseID + @")
            //select @ItemAttributeID = @@IDENTITY; select @ItemAttributeID;
            //end
            //else
            //begin
            //	select @ItemAttributeID = ItemAttributeID from dbo.TblItemAttribute where lower([Name]) = '" + ItemAttribute.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @";
            //	select @ItemAttributeID;
            //end";
            //                    result = html.SaveChangeData(sql, false, true);
            //                    string ItemAttributeID = result.Value;
            //                    sql = "insert into dbo.TblItemAttributeValue (ItemGroupName, ItemID, ItemAttributeID, ItemAttribute, ItemAttributeValue, CreateBy,CreateDate,SystemID,WarehouseID) values (N'" + ItemGroupName + "', " + ItemID + ", " + ItemAttributeID + ", N'" + ItemAttribute + "', N'" + ItemAttributeValue + "', N'" + CreateBy + "', '" + CreateDate + "'," + SystemID + "," + WarehouseID + ")";
            //                    result = html.SaveChangeData(sql, false, false);
            //                }
            //    string storeFileName = "";
            //    if (HttpContext.Request.Files.Count > 0)
            //    {
            //        string path = HttpContext.Server.MapPath("~/Uploads/Items/");
            //        if (!Directory.Exists(path))
            //            Directory.CreateDirectory(path);
            //        string ControlName = "DItemImage_" + i;
            //        for (int j = 0; j < HttpContext.Request.Files.Count; j++)
            //        {
            //            string key = HttpContext.Request.Files.GetKey(j);
            //            if (key == ControlName)
            //            {
            //                foreach (var file in HttpContext.Request.Files.GetMultiple(key))
            //                {
            //                    string savedFileName = Path.Combine(path, file.FileName);
            //                    file.SaveAs(savedFileName);
            //                    if (storeFileName == "")
            //                        storeFileName = file.FileName;
            //                    else
            //                        storeFileName += "~" + file.FileName;
            //                }
            //                sql = @"update dbo.TblItem set DestinationFolder= 'Uploads/Items/', ItemImage =N'" + storeFileName + "' where ItemID = " + ItemID;
            //                result = html.SaveChangeData(sql, false, false);
            //                break;
            //            }
            //        }
            //    }
            //    i++;
            //}

            return Json(new { Result = result.Result, Data = html.JsonConvertDataTable(result.Data.Tables[0]), Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateBundleItem(string PrimaryFieldValue)
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
            string TypeName = "", TypeID = "", InventoryAccountID = "", ExpenseAccountID = "", IncomeAccountID = "";
            string InventoryAccount = "", IncomeAccount = "", ExpenseAccount = "";
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    TypeID = dr["TypeID"].ToString();
                    TypeName = dr["TypeName"].ToString();
                    InventoryAccountID = dr["InventoryAccountID"].ToString();
                    ExpenseAccountID = dr["ExpenseAccountID"].ToString();
                    IncomeAccountID = dr["IncomeAccountID"].ToString();
                    InventoryAccount = dr["InventoryAccount"].ToString();
                    IncomeAccount = dr["IncomeAccount"].ToString();
                    ExpenseAccount = dr["ExpenseAccount"].ToString();
                }
            }
            html.FormID = "CreateBundleItems";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblItem",
                    FormTitle="Item Information",
                    GetIDENTITYByFiled="ItemID",
                    PrimaryFieldName="ItemID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Categories:", ID="StockName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
							//FieldName="StockName",
							SQL_CBO=sqlStockName,//"select StockNameID, StockName from dbo.TblStockName where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by StockName",
                            //isTypeahead=true, HiddenFeild="StockNameID",
							//SQLCallBack="select ItemGroupID, ItemGroup from dbo.TblItemGroup where StockNameID = @StockNameID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemGroup",
							//CallBackToControlID="ItemGroup", isApplyTypeaheadOnCallBack=true,
							//TypeAheadAddNewFuntionName="AddNewStockName()", AddButtonFunction="AddNewStockName()"
							AddNewFuntionName ="AddNewStockName()",
                            DivClass="pt-1",
                            DisplayText="StockName",
                            Value="StockNameID",
                            Multiple = true,
                            DBField="StockNameID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Type:", ID="TypeName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            FieldName="TypeName", DivClassCol="col-sm-auto",
                            SQL="select ServiceID ItemTypeID, ServiceName TypeName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ServiceName",
                            isTypeahead=true, HiddenField="ItemTypeID", HiddenFieldValue =TypeID,
                            AddButtonFunction="AddNewService()", Value=TypeName,
                            TypeAheadAddNewFunctionName="AddNewService()",
                            DBField = "TypeName"
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Name:", ID="Txt_ItemName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            FieldName="ItemName", //ISUniqueFiled=true,
							SQL_AutoComplete = "select distinct top 10 i.ItemName from dbo.TblItem i where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                            ISAutoComplete = true,
                            DBField = "ItemName"
                        },
                        new HtmlComboBox()
                        {
                            Label="Base On:", ID="BaseOn",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BaseOn",
                            TableName="TblItem",
                            SQL_CBO="select 'Barcode' BaseOn union all select 'Serial Number' BaseOn",
                            DisplayText="BaseOn",
                            Value="BaseOn", Required = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Unit:", ID="Txt_UMName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="UM",
                            SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                            isTypeahead=true, HiddenField="UomID",
                            TypeAheadAddNewFunctionName="AddNewUM();",
                            AddButtonFunction="AddNewUM()",
                        },
						//new HtmlTextBox()
						//{
						//	Label="Manufacturer:", ID="Txt_Manufacturer",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="Manufacturer",
						//	SQL = "select distinct top 10 i.Manufacturer from dbo.TblManufacturer i where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.Manufacturer",
						//	isTypeahead = true,
						//	AddButtonFunction="AddNewManufacturer()",
						//	TypeAheadAddNewFuntionName="AddNewManufacturer()",
						//	AddValueToTblNameIfNotExistingData="TblManufacturer",
						//	AddValueToTblNameIfNotExistingDataFieldName="Manufacturer"
						//},
						new HtmlTextBox()
                        {
                            Label="Item Brand:", ID="ItemBrand",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemBrand",HiddenField="ItemBrandID",isTypeahead=true,
                            AddButtonFunction="AddNewItemBrand()",
                            SQL="select ItemBrandID, ItemBrand from dbo.TblItemBrand where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemBrand",
                            TypeAheadAddNewFunctionName="AddNewItemBrand()",
                            AddValueToTblNameIfNotExistingData="TblItemBrand",
                            AddValueToTblNameIfNotExistingDataFieldName="ItemBrand"
                        },
                        new HtmlTextBox()
                        {
                            Label="Alert Reorder:", ID="ReorderPoint",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReorderPoint", ISNumeric = true, ISInteger = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Cost Price USD:", ID="PurchasePrice",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CostPrice", ISNumeric=true,
                            Required= true, Value="0",
                            DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Selling Price USD:", ID="SellingPrice", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SellingPrice", ISNumeric=true,
                            Value = "0", DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Returnable Item:", ID="ReturnableItem", Value = "true",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", DivClass="wrap-input2 mt-2 pt-1",
                            DBField="ReturnableItem", ISCheckBox=true,
                            Type="checkbox",
                            TableName="TblItem"
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
                            DivClassCol="col-sm-auto",
                        },
                        new HtmlTextBox()
                        {
                            Label="Expiration Date:", ID="ItemExpirationDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISDateTime = true, SingleDatePicker=true,
                            DefaultShowDateValue = false,
                            DBField="ItemExpirationDate"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Inventory Account:", ID="Inventory",
                            Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="InventoryAccountName",
                            isTypeahead=true,
                            TypeAheadAddNewFunctionName ="AddNewChartOfAccount('Inventory')",
                            SQL="select ChartOfAccountID InventoryChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) InventoryAccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            TypeaheadGroupByField= "AccountType",
                            HiddenField="InventoryChartOfAccountID",
                            HiddenFieldValue = InventoryAccountID,
                            Value = InventoryAccount
                        },

                        new HtmlTextBox()
                        {
                            Label="Expense Account:", ID="PurchaseChartOfAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PurchaseChartOfAccount",
                            SQL="select ChartOfAccountID PurchaseChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) PurchaseChartOfAccount from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            isTypeahead=true,
                            Value=ExpenseAccount,
                            HiddenField="PurchaseChartOfAccountID",
                            TypeaheadGroupByField="AccountType",
                            TypeAheadAddNewFunctionName="AddNewChartOfAccount('PurchaseChartOfAccountID')",
                            HiddenFieldValue=ExpenseAccountID, Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Income Account:", ID="SaleChartOfAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SaleChartOfAccount",
                            SQL="select ChartOfAccountID SaleChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) SaleChartOfAccount from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            isTypeahead = true,
                            Value=IncomeAccount,
                            HiddenField="SaleChartOfAccountID",
                            TypeaheadGroupByField="AccountType",
                            TypeAheadAddNewFunctionName="AddNewChartOfAccount('SaleChartOfAccountID')",
                            HiddenFieldValue= IncomeAccountID, Required = true
                        }
                    },
                },
                new  FormData() {
                    //FormTitle="Items Table",
                    //PrimaryFieldName="ItemID",
                    //PrimaryFieldValue=PrimaryFieldValue,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_Items_Detail",
                            InputName="ItemBarcode, ItemCode, SerialNumber",
                            TRName="TR_Items_Detail_No",
                            TDName="TD_Items_Detail_No",
                            GetIDENTITYByFiled="ItemID",
                            PrimaryFieldName="ItemID",
                            TableName="TblItemVariant",
                            PrimaryFieldValue=PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemName", SetValue="$(\"#Txt_ItemName\").val()", Value="Txt_ItemName",
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="StockNameID", SetValue="$(\"#StockName\").val()", Value="StockName",
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="StockName", SetValue ="$(\"#StockName\").val()", Value="StockName",
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="TypeName", SetValue = "$(\"#TypeName\").val()", Value="TypeName",
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="ItemTypeID", SetValue="$(\"#TypeName_ItemTypeID\").val()", Value="TypeName_ItemTypeID",
                                //},
                                new DBField()
                                {
                                    DBFieldName="UomID", SetValue = "$(\"#Txt_UMName_UomID\").val()", Value="Txt_UMName_UomID",
                                },
                                new DBField()
                                {
                                    DBFieldName="UM", SetValue = "$(\"#Txt_UMName\").val()", Value="Txt_UMName",
                                },
								//new DBField()
								//{
								//	DBFieldName="Manufacturer", SetValue ="$(\"#Txt_Manufacturer\").val()", Value="Txt_Manufacturer",
								//},
								//new DBField()
        //                        {
        //                            DBFieldName="ItemBrandID", SetValue ="$(\"#ItemBrand_ItemBrandID\").val()", Value="ItemBrand_ItemBrandID",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="ItemBrand", SetValue ="$(\"#ItemBrand\").val()", Value="ItemBrand",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="ReorderPoint", SetValue ="$(\"#ReorderPoint\").val()", Value="ReorderPoint",
        //                        },
                                new DBField()
                                {
                                    DBFieldName="CostPrice", SetValue ="$(\"#PurchasePrice\").val()", Value="PurchasePrice",
                                },
                                new DBField()
                                {
                                    DBFieldName="SellingPrice", SetValue ="$(\"#SellingPrice\").val()", Value="SellingPrice",
                                },
        //                        new DBField()
        //                        {
        //                            DBFieldName="ReturnableItem", SetValue ="$(\"#ReturnableItem\").val()", Value="ReturnableItem",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="InventoryChartOfAccountID", SetValue ="$(\"#Inventory_InventoryChartOfAccountID\").val()", Value="Inventory_InventoryChartOfAccountID",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="InventoryAccountName", SetValue ="$(\"#Inventory\").val()", Value="Inventory",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="PurchaseChartOfAccountID", SetValue ="$(\"#PurchaseChartOfAccountID_PurchaseChartOfAccountID\").val()", //Value="PurchaseChartOfAccountID_PurchaseChartOfAccountID",
								//},
        //                        new DBField()
        //                        {
        //                            DBFieldName="PurchaseChartOfAccount", SetValue ="$(\"#PurchaseChartOfAccountID\").val()", Value="PurchaseChartOfAccountID",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="SaleChartOfAccountID", SetValue ="$(\"#SaleChartOfAccountID_SaleChartOfAccountID\").val()", Value="SaleChartOfAccountID_SaleChartOfAccountID",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="SaleChartOfAccount", SetValue ="$(\"#SaleChartOfAccountID\").val()", Value="SaleChartOfAccountID",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="VendorID", SetValue ="$(\"#VendorName_VendorID\").val()", Value="VendorName_VendorID",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="VendorName", SetValue ="$(\"#VendorName\").val()", Value="VendorName",
        //                        },
        //                        new DBField()
        //                        {
        //                            DBFieldName="Reference_VendorName", SetValue ="$(\"#VendorName_Reference_VendorName\").val()", Value="VendorName_Reference_VendorName",
        //                        },
                                new DBField()
                                {
                                    DBFieldName="Warehouse", SetValue = "$(\"#Warehouse\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="WID", SetValue = "$(\"#Warehouse_WID\").val()"
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="BaseOn", SetValue = "$(\"#BaseOn\").val()"
                                //},
                                new DBField()
                                {
                                    DBFieldName="ItemExpirationDate", SetValue = "$(\"#ItemExpirationDate\").val()", IsDate = true
                                },
                            },
                            ScriptCallBackFunction="CallSetAutoComplete_ItemSize",
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No.", Class="text-nowrap col-sm-auto"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Barcode", Class="text-nowrap col-sm-auto"
                                //},
                                
                                new TblTHead()
                                {
                                     Label="Item Code", Class="text-nowrap col-sm-auto"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Item Size", Class="text-nowrap col-sm-auto"
                                //},
                                new TblTHead()
                                {
                                     Label="Serial Number", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Model", Class="text-nowrap col-sm-auto",
                                },
                                new TblTHead()
                                {
                                     Label="Opening Stock", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Rate/Unit", Class="text-nowrap col-sm-auto"
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
                                    OpenTR="<tr name=\"TR_Items_Detail_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_Items_Detail_No"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemBarcode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Barcode",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Barcode",
                                            DeleteTDRowFunction="RemoveTableRow('TR_Items_Detail_No', 'TD_Items_Detail_No', this,'','ItemBarcode');",
                                            AutoGenerateBarcodeNumberIfNotEntry = true,
                                            IsDisplayAutoGenerateBarcodeNumber = true,
                                            Hide=true
                                        },
                                        //new HtmlTextBox(){
                                        //    Name="Multiplier",
                                        //    ID="Multiplier",
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DBField="Multiplier", ISNumeric=true,
                                        //    Value = "1", DivClassCol="col-sm",
                                        //    Hide=true
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="ItemCode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Code",
                                            DBField="ItemCode",
                                            DeleteTDRowFunction="RemoveTableRow('TR_Items_Detail_No', 'TD_Items_Detail_No', this,'','ItemCode');",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemSize",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            Hide = true,
                                            DBField="ItemSize",
                                            placeholder="Enter Item Size", ISAutoComplete = true,
                                            SQL_AutoComplete="select distinct top 10 ItemSize from dbo.TblItemSize where lower(ItemSize) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ItemSize",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SerialNumber",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Serial Number",
                                            DBField="SerialNumber", OnKeyupFunction="onEnter(event, this);",
                                            //DeleteTDRowFunction="RemoveTableRow('TR_Items_Detail_No', 'TD_Items_Detail_No', this,'','ItemBarcode');",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Model",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Model",
                                            DBField="Model"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="OpeningStock",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Opening Stock", ISNumeric=true, ISInteger =true,
                                            DivClassCol="col-sm-auto",
                                            DBField="OpeningStock"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="OpeningStockRatePerUnit",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Opening Stock Rate Per Unit", ISNumeric=true,
                                            DivClassCol="col-sm-auto",
                                            DBField="OpeningStockRatePerUnit"
                                        },
                                        new HtmlFile()
                                        {
                                            Name="iItemImage",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemImage", IsDragDropFile = true,
                                            AcceptImageOnly=true, DestinationFile="Uploads/Items/",
                                            ShowDropZoneEnabled = false, Multiple = true,
                                            TableName= "TblItemSerial", PrimaryFieldName = "ItemID",
                                            PrimaryFieldValue = PrimaryFieldValue,
                                            DivClassCol="col-sm-auto", Align = "right"
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            html.FieldIDFocus = "StockName";
            html.FocusFieldIDAfterReset = "ItemCode";
            html.RestFields = "ItemCode, SerialNumber, Model, SKU, MaximumPoint";
            html.SaveChangeCallbackOnCloseDialog = true;
            html.SaveChangeCallbackOnCloseDialogValue = "Txt_ItemName";
            html.SaveChangeCallbackFunction = "addCategoryToItem()";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult ItemGroup()
        {
            HTML html = new HTML();
            string sql = @"select distinct i.ItemGroupName from dbo.TblItem i
where isnull(ItemGroupName, '') <> '' and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + @"
order by i.ItemGroupName";
            ViewBag.Data = html.LoadData(sql).Tables[0];
            return View();
        }

        [Authorize(Roles = "Add Item Request, Edit Item Request, Delete Item Request")]
        public ActionResult InventoryRequest()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Item Request"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item Request"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item Request"))
                html.AllowDelete = true;

            html.AddButton = "Add Stock Request";
            html.AddURL = "Items/AddItemRequest";
            html.DialogBoxWidth = "auto";
            html.TableName = "TblItem_Request";
            html.PrimaryField = "RequestItemID";
            html.OrderBy = "RequestItemID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ir.RequestDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtDepartment",
                //ISAutoComplete = true,
                Label = "Department Name:",
                DBField = "Department",
                SQL_CBO = "select distinct d.DepartmentName Department from dbo.TblDepartment d where d.SystemID = " + html.SystemID + " and d.WarehouseID = " + html.WarehouseID + " and d.DomainID like '%" + html.DomainID + "%' order by d.DepartmentName",
                DisplayText = "Department",
                Value = "Department",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Request No",
                SQL = "select distinct top 10 RequestNo from dbo.TblItem_Request where lower(RequestNo) like @SearchText and RequestMode = 'Item Request' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestNo",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Request Name",
                SQL = "select distinct top 10 RequestName from dbo.TblItem_Request where lower(RequestName) like @SearchText and RequestMode = 'Item Request' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
            });
            string sql = @"select ROW_NUMBER() over(order by ir.RequestItemID desc) as[No], ir.RequestItemID, 
'<a href=""javascript:void(0);"" onclick=""showItemRequestForm('+convert(varchar, ir.RequestItemID)+', false, '''+ ir.RequestNo +''')"">'+ir.RequestNo+'</a>' as RequestNo, format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, ir.NeedToApproval, 
case when ir.[Status] = 'Approved' then '<span class=""badge bg-primary"">Approved</span>' 
when ir.[Status] = 'Cancel' then '<span class=""badge bg-warning"">Cancel</span>' 
else '<span class=""badge bg-danger"">'+ir.[Status]+'</span>' end + case when ir.WaitingConfirmedReceivedStatus = 'Received' then '<br><span class=""badge bg-success"">' + ir.WaitingConfirmedReceivedStatus + '</span>' else '' end
+ case when isnull(IsWaitingReceived, 0) = 1 then '<br><a href=""javascript:void(0);"" onclick=""ConfirmReceivedItem('+convert(varchar, ir.RequestItemID)+', '''+ir.RequestNo+''', '''+ir.ReceivedBy+''', '''+convert(varchar, ir.ReceivedUserID)+''', '''+ir.ReceivedTelegaramUserName+''')"" class=""btn btn-sm btn-primary text-white"">Confirm Received</a>' else '' end
[Status], ir.Status Status1, ir.[Status] CStatus,
case when ir.[Status] = 'Approved' then 'Approved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') 
when ir.[Status] = 'Cancel' then 'Cancel Date: ' + format(ir.CancelDate, 'dd-MM-yy') 
when ir.[Status] = 'Disapproved' then 'Disapproved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy')
when ir.[Status] = 'Rejected' then 'Rejected Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') else
case when ir.NeedToApprovalUserID <> 0 then 
'<a href=""javascript:void(0);"" onclick=""ApproveDisapproveItemRequest('+convert(varchar, ir.RequestItemID)+', ''Approved'');"" class=""btn btn-sm btn-primary text-white"">Approve</a>' + 
'&nbsp;<a href=""javascript:void(0);"" onclick=""ApproveDisapproveItemRequest('+convert(varchar, ir.RequestItemID)+', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' +
'&nbsp;<a href=""javascript:void(0);"" onclick=""ApproveDisapproveItemRequest('+convert(varchar, ir.RequestItemID)+', ''Rejected'');"" class=""btn btn-sm btn-danger text-white"">Reject</a>' else '' end
end ApproveOrDisapprove, ir.Department, t.CustomerType Block
from dbo.TblItem_Request ir
left join dbo.TblCustomerType t on t.CustomerTypeID = ir.BlockID
where ir.RequestMode = 'Item Request'";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="ir.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="ir.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="ir.DomainID", Value =html.DomainID },
            };
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Request Date", FieldName = "RequestDate"},
                new GridField() { Header = "Request No", FieldName = "RequestNo" },
                new GridField() { Header = "Request Name", FieldName = "RequestName" },
                new GridField() { Header = "Department", FieldName = "Department" },
                new GridField() { Header = "Need To Approval", FieldName = "NeedToApproval" },
                new GridField() { Header = "Status", FieldName = "Status1", TextAlign = "center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Approved", "badge-primary" },
                        { "Pending", "badge-danger" }
                    }
                },
                //new GridField() { Header = "Approve", FieldName = "ApproveOrDisapprove", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "RequestItemID", TextAlign = "Center",
                    IsModifyField = true,
                    DeleteWithTableName="TblItem_Request_Detail",
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Request Approval Inventory, Edit Request Approval Inventory, Delete Request Approval Inventory")]
        public ActionResult RequestApprovalItemOut()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Request Approval Inventory"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Request Approval Inventory"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Request Approval Inventory"))
                html.AllowDelete = true;

            html.AddButton = "Add Stock Out";
            html.AddURL = "Items/AddRequestApprovalItemOut";
            html.DialogBoxWidth = "auto";
            html.TableName = "TblItem_Request";
            html.PrimaryField = "RequestItemID";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ir.RequestDate",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Stock Out No",
                SQL = "select distinct top 10 RequestApprovalNo from dbo.TblItem_Request where lower(RequestApprovalNo) like @SearchText and RequestMode = 'Request Approval Item Out' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestApprovalNo",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestApprovalNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Request Name",
                SQL = "select distinct top 10 RequestName from dbo.TblItem_Request where lower(RequestName) like @SearchText and RequestMode = 'Request Approval Item Out' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestApprovalNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
            });
            string sql = @"select ROW_NUMBER() over(order by ir.RequestItemID desc) as[No], ir.RequestItemID, 
'<a href=""javascript:void(0);"" onclick=""showItemRequestForm('+convert(varchar, ir.RequestItemID)+', true, '''+ ir.RequestApprovalNo +''')"">'+ir.RequestApprovalNo+'</a>' as RequestApprovalNo, ir.RequestNo, format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, ir.NeedToApproval, 
case when isnull(IsConfirmPrepared,0) = 0 then case when ir.WaitingConfirmedReceivedStatus='Waiting Stock Confirm' then
'<a href=""javascript:void(0);"" onclick=""ConfirmPreparedItem(this,'+convert(varchar, ir.RequestItemID)+','''+ir.RequestNo+''','''+ir.RequestName+''','''+convert(varchar, ir.RequestNameUserID)+''','''+ir.RequestNameTelegram+''');"" class=""btn btn-sm btn-danger text-white btn-confirm-prepare"">Confirm Prepare</a>'
else case when ir.RequestApprovalItemOutStatus = 'Approved' then '<span class=""badge badge-primary"">Approved</span>' 
+ case when (ir.IsWaitingConfirmedReceived = 1 or ir.IsReceived = 1) then '<br><span class=""badge badge-danger"">' + ir.WaitingConfirmedReceivedStatus + '</span>' else '' end
else '<span class=""badge badge-danger"">'+ir.RequestApprovalItemOutStatus+'</span>' end end else 
case when ir.RequestApprovalItemOutStatus = 'Approved' then
'<span class=""badge badge-primary"">'+ir.RequestApprovalItemOutStatus+'</span>' 
+ case when (ir.IsWaitingConfirmedReceived = 1 or ir.IsReceived = 1) then '<br><span class=""badge badge-success"">' + ir.WaitingConfirmedReceivedStatus + '</span>' else '' end
else
'<span class=""badge badge-danger"">'+ir.RequestApprovalItemOutStatus+'</span>' 
end end
[Status], ir.RequestApprovalItemOutStatus CStatus, ir.PreparedBy, ir.RequestApprovalBy, format(ir.PreparedDate, 'dd-MM-yyyy') PreparedDate,
case when ir.RequestMode = 'Item Request' and ir.RequestApprovalItemOutStatus = 'Pending' and isnull(ir.IsConfirmPrepared,0) = 0 then '' else
case when ir.RequestApprovalItemOutStatus = 'Approved' then 'Approved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') 
when ir.RequestApprovalItemOutStatus = 'Disapproved' then 'Disapproved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') else
'<a href=""javascript:void(0);"" onclick=""ApproveDisapproveItemRequest('+convert(varchar, ir.RequestItemID)+', ''Approved'',''Request Approval Item Out'');"" class=""btn btn-sm btn-primary text-white"">Approve</a>' + 
'&nbsp;<a href=""javascript:void(0);"" onclick=""ApproveDisapproveItemRequest('+convert(varchar, ir.RequestItemID)+', ''Disapproved'',''Request Approval Item Out'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>'
end end ApproveOrDisapprove, ir.CreateBy
from dbo.TblItem_Request ir
where ir.IsRequestApprovalItemOut = 1 and ir.RequestMode <> 'Stock Return' and ir.SystemID = " + html.SystemID + " and ir.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date Prepared", FieldName = "PreparedDate"},
                new GridField() { Header = "Stock Out No", FieldName = "RequestApprovalNo" },
                new GridField() { Header = "Date Request", FieldName = "RequestDate"},
                new GridField() { Header = "Request No", FieldName = "RequestNo" },
                new GridField() { Header = "Request Name", FieldName = "RequestName" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Created By", FieldName = "CreateBy", TextAlign = "center" },
                new GridField() { Header = "Prepared By", FieldName = "PreparedBy", TextAlign = "center" },
                //new GridField() { Header = "Approve", FieldName = "ApproveOrDisapprove", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "RequestItemID",
                    TextAlign = "Center", IsModifyField = true, DeleteWithTableName="TblItem_Request_Detail",
                    HideEditOnActionNameNotMatch=true,
                    HideDeteleOnNotMatch=true,
                    ShowActionNameOnConditionField="CStatus",
                    ShowActionNameOnConditionValue="Pending, Disapproved"
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public ActionResult RequestReturn()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Request Return"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Request Return"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Request Return"))
                html.AllowDelete = true;

            html.AddButton = "Add New";
            html.AddURL = "Items/AddRequestReturn";
            html.DialogBoxWidth = "auto";
            html.TableName = "TblItem_Request";
            html.PrimaryField = "RequestItemID";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ir.RequestDate",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Request No",
                SQL = "select distinct top 10 RequestReturnNo from dbo.TblItem_Request where lower(RequestReturnNo) like @SearchText and RequestMode in ('Request Stock Return', 'Stock Return') and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestReturnNo",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestReturnNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Request Name",
                SQL = "select distinct top 10 RequestName from dbo.TblItem_Request where lower(RequestName) like @SearchText and RequestMode in ('Request Stock Return', 'Stock Return') and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestReturnNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
            });
            string sql = @"select ROW_NUMBER() over(order by ir.RequestItemID desc) as[No], ir.RequestItemID, 
'<a href=""javascript:void(0);"" onclick=""showRequestReturnForm('+convert(varchar, ir.RequestItemID)+', true, '''+ ir.RequestReturnNo +''')"">'+ir.RequestReturnNo+'</a>' as RequestReturnNo, format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, ir.NeedToApproval, 
case when ir.[Status] = 'Approved' then '<span class=""badge bg-primary"">Approved</span>' 
when ir.[Status] = 'Cancel' then '<span class=""badge bg-warning"">Cancel</span>' 
else '<span class=""badge bg-danger"">'+ir.[Status]+'</span>' end [Status], ir.[Status] CStatus,
case when ir.[Status] = 'Approved' then 'Approved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') 
when ir.[Status] = 'Cancel' then 'Cancel Date: ' + format(ir.CancelDate, 'dd-MM-yy') 
when ir.[Status] = 'Disapproved' then 'Disapproved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy')
when ir.[Status] = 'Rejected' then 'Rejected Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') else
case when ir.NeedToApprovalUserID <> 0 then 
'<a href=""javascript:void(0);"" onclick=""ApproveDisapproveRequestReturn('+convert(varchar, ir.RequestItemID)+', ''Approved'');"" class=""btn btn-sm btn-primary text-white"">Approve</a>' + 
'&nbsp;<a href=""javascript:void(0);"" onclick=""ApproveDisapproveRequestReturn('+convert(varchar, ir.RequestItemID)+', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' +
'&nbsp;<a href=""javascript:void(0);"" onclick=""ApproveDisapproveRequestReturn('+convert(varchar, ir.RequestItemID)+', ''Rejected'');"" class=""btn btn-sm btn-danger text-white"">Reject</a>' else '' end
end ApproveOrDisapprove
from dbo.TblItem_Request ir
where ir.RequestMode in ('Request Stock Return', 'Stock Return') and ir.Status is not null";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="ir.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="ir.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="ir.DomainID", Value =html.DomainID },
            };
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "Request Item ID", FieldName = "RequestItemID", IsHiddenField = true },
                new GridField() { Header = "Date", FieldName = "RequestDate", TextAlign = "center" },
                new GridField() { Header = "Return No", FieldName = "RequestReturnNo", TextAlign = "center" },
                new GridField() { Header = "Return Name", FieldName = "RequestName", TextAlign = "center" },
                new GridField() { Header = "Need To Approval", FieldName = "NeedToApproval", TextAlign = "center" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Approve", FieldName = "ApproveOrDisapprove", TextAlign = "center"},
                new GridField() { Header = "Action", FieldName = "RequestItemID", TextAlign = "Center", IsModifyField = true, DeleteWithTableName="TblItem_Request_Detail",
                    //HideDeteleOnNotMatch = true,
                    //HideEditOnActionNameNotMatch = true,
                    //ShowActionNameOnConditionField="CStatus",
                    //ShowActionNameOnConditionValue="Pending, Disapproved"
                }
            };
            html.ShowPaging = false;
            //html.GridEventClick = "onGridRequestReturn(this, 'true')";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult StockReturn()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Stock Return"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Stock Return"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Stock Return"))
                html.AllowDelete = true;

            html.AddButton = "Add Stock Return";
            html.AddURL = "Items/AddStockReturn";
            html.DialogBoxWidth = "auto";
            html.TableName = "TblItem_Request";
            html.PrimaryField = "RequestItemID";
            html.OrderBy = "RequestItemID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Return Date:",
                DBField = "ir.RequestDate",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Return No",
                SQL = "select distinct top 10 RequestApprovalNo from dbo.TblItem_Request where lower(RequestApprovalNo) like @SearchText and RequestMode in ('Request Stock Return','Stock Return') and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestApprovalNo",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestApprovalNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Request Name",
                SQL = "select distinct top 10 RequestName from dbo.TblItem_Request where lower(RequestName) like @SearchText and RequestMode in ('Request Stock Return','Stock Return') and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ir.RequestApprovalNo") + "," + EncryAndDecry.Encryp("ir.RequestName"),
            });
            string sql = @"select ROW_NUMBER() over(order by ir.RequestItemID desc) as[No], ir.RequestItemID, 
'<a href=""javascript:void(0);"" onclick=""showRequestReturnForm('+convert(varchar, ir.RequestItemID)+', false, '''+ ir.RequestApprovalNo +''')"">'+ir.RequestApprovalNo+'</a>' as RequestApprovalNo, ir.RequestReturnNo, format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, ir.NeedToApproval, 
case when ir.WaitingConfirmedReceivedStatus='Waiting Stock Confirm' then
'<a href=""javascript:void(0);"" onclick=""ConfirmItemRequestReturn(this,'+convert(varchar, ir.RequestItemID)+','''+ir.RequestReturnNo+''','''+ir.RequestName+''','''+convert(varchar, ir.RequestNameUserID)+''','''+ir.RequestNameTelegram+''');"" class=""btn btn-sm btn-danger text-white btn-confirm-prepare"">Confirm Request Return</a>'
else case when ir.RequestApprovalItemOutStatus = 'Approved' then '<span class=""badge bg-primary"">Approved</span>' else '<span class=""badge bg-danger"">'+ir.RequestApprovalItemOutStatus+'</span>' end end [Status], ir.RequestApprovalItemOutStatus CStatus, ir.PreparedBy, ir.RequestApprovalBy, format(ir.PreparedDate, 'dd-MM-yyyy') PreparedDate,
case when ir.RequestMode in ('Request Stock Return','Stock Return') and ir.RequestApprovalItemOutStatus = 'Pending' and isnull(IsConfirmPrepared, 0) = 0 then '' else
case when ir.RequestApprovalItemOutStatus = 'Approved' then 'Approved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') 
when ir.RequestApprovalItemOutStatus = 'Disapproved' then 'Disapproved Date: ' + format(ir.ApproveDisapproveDate, 'dd-MM-yy') else
'<a href=""javascript:void(0);"" onclick=""ApproveDisapproveRequestReturn('+convert(varchar, ir.RequestItemID)+', ''Approved'',''Stock Return'');"" class=""btn btn-sm btn-primary text-white"">Approve</a>' + 
'&nbsp;<a href=""javascript:void(0);"" onclick=""ApproveDisapproveRequestReturn('+convert(varchar, ir.RequestItemID)+', ''Disapproved'',''Stock Return'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>'
end end ApproveOrDisapprove
from dbo.TblItem_Request ir
where ir.RequestMode in ('Request Stock Return', 'Stock Return') and (ir.Status='Approved' or ir.Status is null) and ir.SystemID = " + html.SystemID + " and ir.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date Prepared", FieldName = "PreparedDate", TextAlign="center"},
                new GridField() { Header = "Stock Return No", FieldName = "RequestApprovalNo", TextAlign="center" },
                new GridField() { Header = "Request Return No", FieldName = "RequestReturnNo", TextAlign="center" },
                new GridField() { Header = "Request Name", FieldName = "RequestName" },
                new GridField() { Header = "Approved", FieldName = "NeedToApproval" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Prepared By", FieldName = "PreparedBy", TextAlign = "center" },
                new GridField() { Header = "Need To Approval By", FieldName = "RequestApprovalBy", TextAlign = "center" },
                new GridField() { Header = "Approve", FieldName = "ApproveOrDisapprove", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "RequestItemID",
                    TextAlign = "Center", IsModifyField = true, DeleteWithTableName="TblItem_Request_Detail",
                    //HideEditOnActionNameNotMatch=true,
                    //HideDeteleOnNotMatch=true,
                    //ShowActionNameOnConditionField="CStatus",
                    //ShowActionNameOnConditionValue="Pending, Disapproved"
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public JsonResult LoadRequestReturn(string RRTNumber)
        {
            HTML html = new HTML();
            string sql = @"SELECT rd.* FROM dbo.TblItem_Request_Detail rd
                    INNER JOIN dbo.TblItem_Request ir ON ir.RequestItemID = rd.RequestItemID 
                    where ir.RequestReturnNo = @RequestReturnNo;";
            var para = new Dictionary<string, string>()
            {
                { "RequestReturnNo", RRTNumber}
            };
            var ds = html.LoadData(sql, false, para);
            return Json(new { data = ds.Tables[0].ToJsonString() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddRequestReturn(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            string sql = "select PrefixItemRequestNo, Logo from TblCompany where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and PrefixItemRequestNo is not null";
            DataSet ds = html.LoadData(sql);
            string PrefixItemRequestNo;
            if (ds.Tables[0].Rows.Count > 0)
                PrefixItemRequestNo = ds.Tables[0].Rows[0]["PrefixItemRequestNo"].ToString();
            html.FormID = "AddItemRequest";
            html.PrimaryField = "RequestItemID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Request Stock Return Information",
                    TableName="TblItem_Request",
                    PrimaryFieldName = "RequestItemID",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldValue=PrimaryFieldValue,
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName = "Status", SetValue="Pending"
                        },
                        new DBField()
                        {
                            DBFieldName = "RequestMode", SetValue="Request Stock Return"
                        },

                        new DBField()
                        {
                            DBFieldName = "RequestApprovalItemOutStatus", SetValue="Pending"
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Return No:", ID="RequestReturnNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestReturnNo", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Request Return'; select @Prefix = Prefix from dbo.TblPrefix where Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'RT{yy}{MM}-{00000}'",
                            CallBackToControlID="RequestReturnNo, PrefixRequestReturnNo", UpdatePrefixModule="Request Return", Prefix="PrefixRequestReturnNo", SetPrefixToDbFieldName="RequestReturnNo"
                        },
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="RequestDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestDate", SingleDatePicker = true,
                            SingleDatePickerPosition="down",//DivClassCol ="col-sm-auto"
                        },

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request's Name:", ID="RequestName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestName",isTypeahead = true,
                            SQL="select u.UserLoginID RequestNameUserID, u.TelegramUserName RequestNameTelegram, u.FullName RequestName, u.Telephone RequestPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="RequestNameTelegram, RequestNameUserID",
                            HiddenFieldValue="0,0",
							//DisplayOnFields="RequestPhone, NeedToApproval,NeedToApproval_NeedToApprovalTelegram",
							DisplayOnFields="RequestPhone",
                        },
                        new HtmlTextBox()
                        {
                            Label="Request's Phone:", ID="RequestPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Approval By:", ID="NeedToApproval", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToApproval",isTypeahead = true,
                            SQL="select u.UserLoginID NeedToApprovalUserID, u.TelegramUserName NeedToApprovalTelegram, u.FullName NeedToApproval\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="NeedToApprovalUserID,NeedToApprovalTelegram",
                            HiddenFieldValue="0,0",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {

                        new HtmlTextBox()
                        {
                            Label="Department", ID="DepartmentName",
                            //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Department",isTypeahead = true,
                            SQL="select DepartmentID, DepartmentName Department from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by DepartmentName",
                            HiddenField ="DepartmentID",
                            //AddValueToTblNameIfNotExistingData="TblDepartment",
                            //AddValueToTblNameIfNotExistingDataFieldName="DepartmentName",
                            //DivClassCol="col-sm-auto",
                            //AddButtonFunction="AddNewDepartment()",
                            //TypeAheadAddNewFuntionName="AddNewDepartment()"
                        },
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address", isTypeahead =true,
                            SQL="select distinct [Address] from dbo.TblItem_Request where Address is not null and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by [Address]"
                        },
                    }
                },
                new  FormData() {
                    TableName="TblItem_Request_Detail",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldName="RequestItemID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_Request_Item",
                            InputName="ItemName",
                            TRName="TR_Items_Request",
                            TDName="TD_Items_Request",
                            PrimaryFieldName="RequestItemID",
                            GetIDENTITYByFiled="RequestItemID",
                            PrimaryFieldValue=PrimaryFieldValue,
                            TableName="TblItem_Request_Detail",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName, Reload_Situation()",
                            //ScriptCallBackFunction="Reload_ItemName",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", GetValueFromInputName=true, Value = "ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", GetValueFromInputName=true, Value = "UomID"
                                },
                                 new DBField()
                                {
                                    DBFieldName = "SerialNumberID", Value = "SerialNumberID", GetValueFromInputName = true
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No.", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Item Description", Class="text-nowrap col-sm-4"
                                },
                                new TblTHead()
                                {
                                     Label="Code", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Serial Number", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="QTY", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Situation", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Purpose or Reasons", Class="text-nowrap col-sm-2",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_Items_Request\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_Items_Request"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemNameFull", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TR_Items_Request', 'TD_Items_Request', this,'','ItemName');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=$"select top 10 * from dbo.fn_ItemAutoComplete({html.SystemID}, {html.WarehouseID}, {html.DomainID}) where lower(ItemName) like @SearchText;",
                                            SQLCallBack ="select distinct i.ItemCode, i.ItemName ItemNames from dbo.TblItemVariant iv inner join dbo.TblItem i on i.ItemID = iv.ItemID where i.ItemID = @SearchText",
                                            DisplayOnFields="ItemCode, ItemNames, SerialNumber, SerialNumberID",
                                            SQLTypeaheadCallBack = "select distinct iv.ItemID, iv.UomID, iv.UM Unit, iv.CostPrice, iv.SellingPrice SalePrice \r\nfrom dbo.TblItemVariant iv\r\n where iv.ItemID = @ItemID",
                                            DisplayOnFieldsTypeahead="CostPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="Unit", FieldName = "ItemID",
                                            CallBackDisplayText="Unit",
                                            //TypeaheadCallBackFunction="TotalAll()",
                                            //InputDescription = true,
                                            placeholder="Item Name"
                                        },

                                        new HtmlTextBox()
                                        {
                                            Name="ItemNames",Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Name",
                                            DBField="ItemName", HiddenField="ItemIDs", Hide = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemCode",IsReadOnly=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Code",
                                            DBField="ItemCode",
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="ItemIDs",Required=true,
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DivClass="wrap-input3",
                                        //    DBField="ItemID", Hide = true
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="SerialNumber", IsReadOnly=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Serial Number",
                                            DBField="SerialNumber",
                                            HiddenField="CostPrice, SalePrice, LocationID, AvariableQTY, SerialNumberID",
                                            HiddenFieldValue="0,0,0,0,0",
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="Model",
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Model",
                                        //    DBField="Model",
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="QTY",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter QTY",
                                            DBField="QTY", Required = true, Align="center", ISNumeric=true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Unit",Required = true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Unit",
                                            DBField="Unit",HiddenField = "UomID"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Situation", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Situation", isTypeahead = true,Required = true,
                                            SQL="select 'Usable' Situation union all select 'Broken' Situation",
                                            placeholder="Select is Situation"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Remark",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Purpose or Reasons",
                                            DBField="Remark",
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            html.SaveChangeCallbackFunction = "SendNotificationStockReturn()";

            html.SaveChangeAndClose = true;
            //html.CustomValidateFunction = "ValidateQTYOUT";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult AddStockReturn(string PrimaryFieldValue, bool IsConfirmPrepared = false)
        {
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "RequestItemID", PrimaryFieldValue }
            };
            int refId = 0;
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(PrimaryFieldValue))
            {
                string sql = @"select RequestMode, ReferenceRequestItemID from dbo.TblItem_Request where RequestItemID = @RequestItemID";
                ds = html.LoadData(sql, false, para);
            }

            html.FormID = "AddItemRequest";
            html.PrimaryField = "RequestItemID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Stock Return Information",
                    TableName="TblItem_Request",
                    PrimaryFieldName = "RequestItemID",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldValue=PrimaryFieldValue,
                    //SQL="update dbo.TblItem_Request set IsRequestApprovalItemOut = 0, where RequestItemID = @ReferenceRequestItemID",
                    //UpdateFiledName ="ReferenceRequestItemID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="PreparedUserID", SetValue="`+$(\"#PreparedBy_PreparedUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="RequestApprovalUserID", SetValue="`+$(\"#RequestApprovalBy_RequestApprovalUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName = "RequestApprovalItemOutStatus", SetValue="Pending"
                        },
                        new DBField()
                        {
                            DBFieldName = "RequestMode", SetValue="Stock Return"
                        },
                        new DBField()
                        {
                            DBFieldName = "IsRequestApprovalItemOut", SetValue="1"
                        },
                        new DBField()
                        {
                            DBFieldName="ReferenceRequestItemID", SetValue="`+$(\"#ReferenceRequestItemID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="ReceivedUserID", SetValue="`+$(\"#ReceivedBy_ReceivedUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="ReceivedTelegaramUserName", SetValue="`+$(\"#ReceivedBy_ReceivedTelegaramUserName\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsReceived", SetValue="0"
                        },
                        new DBField()
                        {
                            DBFieldName="IsWaitingReceived", SetValue="0"
                        },
                        new DBField()
                        {
                            DBFieldName="IsConfirmPrepared", SetValue="1"
                        },
                        new DBField()
                        {
                            DBFieldName = "AvariableQTY", Value ="AvariableQTY", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName = "WID", Value ="Warehouse_WID", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName = "WaitingConfirmedReceivedStatus", SetValue=null
                        }
                    },

                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request Return No:", ID="RequestReturnNo", //Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead = true,
                            SQL = "select distinct top 10 ir.RequestItemID ReferenceRequestItemID, ir.RequestReturnNo, ir.RequestName ReceivedBy, ir.RequestNameUserID ReceivedUserID, ir.RequestNameTelegram ReceivedTelegaramUserName, ir.Department, ir.[Address] from dbo.TblItem_Request ir where ((ir.NeedToApprovalUserID = 0 and ir.Status = 'Pending') or (ir.NeedToApprovalUserID <> 0 and ir.Status = 'Approved')) and ir.RequestMode in ('Request Stock Return', 'Stock Return') and ir.SystemID = " + html.SystemID + " and ir.WarehouseID = " + html.WarehouseID +"order by RequestItemID desc",
                            DBField="RequestReturnNo", HiddenField="ReferenceRequestItemID, ReceivedUserID, ReceivedTelegaramUserName",
                            HiddenFieldValue= refId+",0,0,0",
                            DisplayOnFields="Department, Address",
                        },
                        new HtmlTextBox()
                        {
                            Label="Stock Return No:", ID="RequestApprovalNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestApprovalNo", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Stock Return'; select @Prefix = Prefix from dbo.TblPrefix where Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'SRT{yy}{MM}-{00000}'",
                            CallBackToControlID="RequestApprovalNo, PrefixRequestNo", UpdatePrefixModule="Stock Return", Prefix="PrefixRequestNo", SetPrefixToDbFieldName="RequestApprovalNo",
                            AllowAddNewPrefixEventPrimaryValueIsNotZeo = IsConfirmPrepared,
                        },
                        new HtmlTextBox()
                        {
                            Label="Department:", ID="Department", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Department",
                            isTypeahead = true,
                            SQL="select distinct Department from dbo.TblItem_Request where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by Department"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Prepared Date:", ID="PreparedDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedDate", SingleDatePicker = true,
                            SingleDatePickerPosition="down",DivClassCol ="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared By:", ID="PreparedBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedBy",isTypeahead = true,
                            SQL="select u.UserLoginID PreparedUserID, u.TelegramUserName PreparedTelegaram, u.FullName PreparedBy, u.Telephone PreparedPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PreparedTelegaram, PreparedUserID",
                            HiddenFieldValue="0,0",
                            DisplayOnFields="PreparedPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared's Phone:", ID="PreparedPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedPhone"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Approval By:", ID="RequestApprovalBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestApprovalBy",isTypeahead = true,
                            SQL="select u.UserLoginID RequestApprovalUserID, u.TelegramUserName RequestApprovalTelegaram, u.FullName RequestApprovalBy\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="RequestApprovalTelegaram, RequestApprovalUserID",
                            HiddenFieldValue="0,0",
                        },
                        new HtmlTextBox()
                        {
                            Label="Received By:", ID="ReceivedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceivedBy",isTypeahead = true,
                            SQL="select u.UserLoginID ReceivedUserID, u.TelegramUserName ReceivedTelegaramUserName, u.FullName ReceivedBy, u.Telephone RequestPhone from dbo.TblUserLogin u inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID where u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ReceivedTelegaramUserName, ReceivedUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address", isTypeahead =true,
                            SQL="select distinct [Address] from dbo.TblItem_Request where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by [Address]"
                        },

                    }
                },
                new  FormData() {
                    TableName="TblItem_Request_Detail",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldName="RequestItemID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_Request_Item",
                            InputName="ItemName",
                            TRName="TR_Items_Request",
                            TDName="TD_Items_Request",
                            PrimaryFieldName="RequestItemID",
                            GetIDENTITYByFiled="RequestItemID",
                            //PrimaryFieldValue=PrimaryFieldValue,
                            TableName="TblItem_Request_Detail",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName, Reload_Situation()",
                            ReturnFieldNameValue="RequestItemID",
                            ReturnFieldNameValueFromTblName = "TblItem_Request",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", GetValueFromInputName=true, Value = "ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", GetValueFromInputName=true, Value = "UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName = "SerialNumberID", Value = "SerialNumberID", GetValueFromInputName = true
                                },
                                new DBField()
                                {
                                    DBFieldName="LocationID", SetValue="$(\"#Warehouse_WID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="CostPrice", GetValueFromInputName = true, Value = "CostPrice", IsNumber=true
                                },
                                new DBField()
                                {
                                    DBFieldName="SallingPrice", GetValueFromInputName = true, Value = "SalePrice", IsNumber = true
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No.", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Item Description", Class="text-nowrap col-sm-4"
                                },
                                new TblTHead()
                                {
                                     Label="Code", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Serial Number", Class="text-nowrap col-sm-auto", Hide = html.SystemID != "13"
                                },
                                new TblTHead()
                                {
                                     Label="Expire Date", Class="text-nowrap col-sm-auto", Hide = html.SystemID == "13"
                                },
                                new TblTHead()
                                {
                                     Label="QTY", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap col-sm-1"
                                },
                                new TblTHead()
                                {
                                     Label="Situation", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Purpose or Reasons", Class="text-nowrap col-sm-auto",
                                      CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_Items_Request\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_Items_Request"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemNameFull", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TR_Items_Request', 'TD_Items_Request', this,'','ItemName');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select distinct top 10 i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then '-' + isnull(i.ItemCode,'') else '' end + case when isnull(srd.SerialNumber,'') <> '' then '-' + isnull(srd.SerialNumber,'') else '' end) ItemName, srd.ItemSerialDetailID SerialNumberID, case when isnull(srd.SerialNumber, '') <> '' then srd.SerialNumber else '' end SerialNumber
from dbo.TblItem i
left join dbo.TblItemVariant iv on iv.ItemID = i.ItemID  
left join dbo.TblItemSerialDetail srd on srd.ItemID = i.ItemID and isnull(srd.SerialNumber, '') <> '' and srd.IsAvailable = 0
where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then i.ItemCode else '' end + case when isnull(srd.SerialNumber,'') <> '' then srd.SerialNumber else '' end)) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID +" and i.DomainID like '%"+html.DomainID+"%'",
                                            SQLCallBack ="select distinct i.ItemCode, i.ItemName ItemNames from dbo.TblItemVariant iv inner join dbo.TblItem i on i.ItemID = iv.ItemID where i.ItemID = @SearchText",
                                            DisplayOnFields="ItemCode, ItemNames, SerialNumber, SerialNumberID",
                                            SQLTypeaheadCallBack = "select distinct iv.ItemID, iv.UomID, iv.UM Unit, iv.CostPrice, iv.SellingPrice SalePrice \r\nfrom dbo.TblItemVariant iv\r\n where iv.ItemID = @ItemID",
                                            DisplayOnFieldsTypeahead="CostPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="Unit", FieldName = "ItemID",
                                            CallBackDisplayText="Unit",
                                            //TypeaheadCallBackFunction="TotalAll()",
                                            //InputDescription = true,
                                            placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemCode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Code",
                                            DBField="ItemCode",//IsReadOnly = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemNames",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Name",
                                            DBField="ItemName", Hide = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SerialNumber",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Serial Number",
                                            DBField="SerialNumber",
                                            HiddenField="CostPrice, SalePrice, LocationID, AvariableQTY, SerialNumberID",
                                            HiddenFieldValue="0,0,0,0,0",IsReadOnly = true, Hide = html.SystemID != "13"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpireDate",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="StockExpireDate",placeholder="Enter Expire Date",
                                            DivClass="wrap-input3", Hide = html.SystemID == "13"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter QTY",
                                            DBField="QTY", Required = true, Align="center", ISNumeric=true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Unit", Required = true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter UOM",
                                            DBField="Unit",HiddenField = "UomID",
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="Situation", IsReadOnly = true,
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Situation",
                                        //    DBField="Situation"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="Situation", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Situation", isTypeahead = true,Required = true,
                                            SQL="select 'Usable' Situation union all select 'Broken' Situation",
                                            placeholder="Select is Situation"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Remark",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Purpose or Reasons",
                                            DBField="Remark",
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
            html.SaveChangeAndClose = true;
            //html.CustomValidateFunction = "ValidateQTYOUT";
            ViewBag.DrawHTMLFormV1 = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult StockReturnForm(int ID, bool IsRequestStockReturn = false, int warehouseId = 0, int systemId = 0)
        {
            HTML html = new HTML();
            string sql = "select Logo from TblWarehouse where WarehouseID = " + (warehouseId.ToString() != "0" ? warehouseId.ToString() : html.WarehouseID) + " and isnull(Logo,'') <> ''";
            string Logo = "";
            DataSet ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
                Logo = ds.Tables[0].Rows[0]["Logo"].ToString();
            ViewBag.ServerRoot = html.ServerRoot;
            ViewBag.Logo = "../Uploads/WarehouseLogo/" + Logo;
            ViewBag.IsRequestStockReturn = IsRequestStockReturn;
            ViewBag.SystemID = systemId.ToString() != "0" ? systemId.ToString() : html.SystemID;

            sql = @"select ua.Job ApprovalByPosition, ul.Job RequestByPosition, ir.RequestItemID, ir.RequestName, ir.RequestPhone, isnull(ir.RequestReturnNo, '') RequestReturnNo, isnull(ir.RequestApprovalNo,'') RequestApprovalNo, ir.Department, ir.[Address],
format(ir.RequestDate, 'dd-MM-yyyy') RequestDate,ird.ItemName, ird.ItemNameFull, ird.ItemSize, ird.QTY, ird.Unit, ird.Remark, ir.RequestNameUserID,
isnull(ua.Signature, '') NeedToApprovalSignature, ir.[Status], ird.ItemCode, ird.SerialNumber, ir.PreparedBy, format(ir.PreparedDate, 'dd-MM-yyyy') PreparedDate, up.Job PrepareByPosition, up.Signature PreparedSignature, uc.Signature ReceivedUserSignature, ir.IsReceived,
ua.FullName ApprovalName, format(ir.ApproveDisapproveDate, 'dd-MM-yyyy') ApproveDisapproveDate, isnull(ul.Signature, '') RequestSignature, w.WarehouseName, uc.FullName ReceivedBy, uc.Job ReceiveByPosition, 
isnull(ir.WaitingConfirmedReceivedStatus,'') WaitingConfirmedReceivedStatus, 
isnull(IsWaitingConfirmedReceived, 0) IsWaitingConfirmedReceived, isnull(ir.RequestApprovalItemOutStatus, '') RequestApprovalItemOutStatus,format(ir.ApprovedRequestDate, 'dd-MM-yyyy') ApprovedRequestDate, format(ir.RequirementDate, 'dd-MM-yyyy') RequirementDate, i.Model, ird.Situation, ird.StockExpireDate
from dbo.TblItem_Request ir
inner join dbo.TblItem_Request_Detail ird on ird.RequestItemID = ir.RequestItemID
inner join dbo.TblItem i on i.ItemID = ird.ItemID
left join dbo.TblUserWarehouse w on w.WID = ir.WID
left join dbo.TblUserLogin up on up.UserLoginID = ir.PreparedUserID
left join dbo.TblUserLogin uc on uc.UserLoginID = ir.ReceivedUserID
left join dbo.TblUserLogin ul on ul.UserLoginID = ir.RequestNameUserID";
            if (IsRequestStockReturn)
            {
                sql += " left join dbo.TblUserLogin ua on ua.UserLoginID = ir.NeedToApprovalUserID";
            }
            else
            {
                sql += " left join dbo.TblUserLogin ua on ua.UserLoginID = ir.RequestApprovalUserID";
            }

            sql += " where ir.RequestItemID = " + ID;

            ds = html.LoadData(sql);
            ViewBag.SRID = ID;
            ViewBag.Status = ds.Tables[0].Rows[0]["Status"];
            ViewBag.Data = ds.Tables[0];
            return View();
        }
        public ActionResult AddItemRequest(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            string sql = "select PrefixItemRequestNo, Logo from TblCompany where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and PrefixItemRequestNo is not null";
            DataSet ds = html.LoadData(sql);
            string PrefixItemRequestNo;
            if (ds.Tables[0].Rows.Count > 0)
                PrefixItemRequestNo = ds.Tables[0].Rows[0]["PrefixItemRequestNo"].ToString();
            html.FormID = "AddItemRequest";
            html.PrimaryField = "RequestItemID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Request Information",
                    TableName="TblItem_Request",
                    PrimaryFieldName = "RequestItemID",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldValue=PrimaryFieldValue,
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="RequestNameUserID", SetValue="`+$(\"#RequestName_RequestNameUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="NeedToApprovalUserID", SetValue="`+$(\"#NeedToApproval_ApproveByUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName = "Status", SetValue="Pending"
                        },
                        new DBField()
                        {
                            DBFieldName = "RequestMode", SetValue="Item Request"
                        },
                        new DBField()
                        {
                            DBFieldName = "IsWaitingConfirmedReceived", SetValue="0"
                        },
                        new DBField()
                        {
                            DBFieldName = "IsRequestApprovalItemOut", SetValue="`+$(\"#IsRequestApprovalItemOut\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName = "WaitingConfirmedReceivedStatus", SetValue="`+$(\"#WaitingConfirmedReceivedStatus\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName = "RequestApprovalItemOutStatus", SetValue="`+$(\"#RequestApprovalItemOutStatus\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName = "EditTelegramMessage", SetValue="1"
                        },
                        new DBField()
                        {
                            DBFieldName = "IsWaitingReceived", SetValue="0"
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request No:", ID="RequestNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestNo", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Stock Request'; select @Prefix = Prefix from dbo.TblPrefix where Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'SR{yy}-{00000}'",
                            CallBackToControlID="RequestNo, PrefixRequestNo", UpdatePrefixModule="Stock Request", Prefix="PrefixRequestNo", SetPrefixToDbFieldName="RequestNo"
                        },
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="RequestDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestDate", SingleDatePicker = true,
                            SingleDatePickerPosition="down",//DivClassCol ="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Requirement Date", ID="RequirementDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequirementDate",
                            SingleDatePicker = true,
                        },

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request's Name:", ID="RequestName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestName",isTypeahead = true,
                            SQL="select u.UserLoginID RequestNameUserID, u.TelegramUserName RequestNameTelegram, u.FullName RequestName, u.Telephone RequestPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="RequestNameTelegram, RequestNameUserID",
                            HiddenFieldValue="0,0",
                            DisplayOnFields="RequestPhone",
                        },
                        new HtmlTextBox()
                        {
                            Label="Request's Phone:", ID="RequestPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Approval By:", ID="NeedToApproval", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToApproval",isTypeahead = true,
                            SQL="select u.UserLoginID NeedToApprovalUserID, u.TelegramUserName NeedToApprovalTelegram, u.FullName NeedToApproval\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="NeedToApprovalUserID,NeedToApprovalTelegram",
                            HiddenFieldValue="0,0",
                        },
                        new HtmlTextBox()
                        {
                            ID= "IsRequestApprovalItemOut",
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            ID= "WaitingConfirmedReceivedStatus",
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            ID= "RequestApprovalItemOutStatus",
                            Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {

                        new HtmlTextBox()
                        {
                            Label="Department", ID="DepartmentName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Department",isTypeahead = true,
                            SQL="select DepartmentID, DepartmentName Department from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by DepartmentName",
                            HiddenField ="DepartmentID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Pick Up:", ID="Address", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address", isTypeahead = true,
                            SQL="select uw.WarehouseName Address, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                        },
                    }
                },
                new  FormData() {
                    TableName="TblItem_Request_Detail",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldName="RequestItemID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_Request_Item",
                            InputName="ItemName",
                            TRName="TR_Items_Request",
                            TDName="TD_Items_Request",
                            PrimaryFieldName="RequestItemID",
                            GetIDENTITYByFiled="RequestItemID",
                            PrimaryFieldValue=PrimaryFieldValue,
                            TableName="TblItem_Request_Detail",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName, Reload_BlockName(), Reload_ProjectName()",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", GetValueFromInputName=true, Value = "ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", GetValueFromInputName=true, Value = "UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="BlockID", GetValueFromInputName=true, Value = "BlockID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ProjectID", GetValueFromInputName=true, Value = "ProjectID"
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No.", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Item Description", Class="text-nowrap col-sm-4"
                                },
                                new TblTHead()
                                {
                                     Label="QTY", Class="text-nowrap col-sm-1"
                                },
                                new TblTHead()
                                {
                                     Label="Unit", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Project", Class="text-nowrap col-sm-auto", Hide = html.SystemID != "13",
                                },
                                new TblTHead()
                                {
                                     Label="Block", Class="text-nowrap col-sm-2", Hide = html.SystemID != "13",
                                },
                                new TblTHead()
                                {
                                     Label="Purpose or Reasons", Class="text-nowrap col-sm-2",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_Items_Request\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_Items_Request"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemNameFull", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TR_Items_Request', 'TD_Items_Request', this,'','ItemName');",
                                            ISAutoComplete = true,
                                            HiddenField="ItemID",
                                            SQL_AutoComplete=$"select top 10 * from dbo.fn_ItemAutoComplete({html.SystemID},{html.WarehouseID},{html.DomainID}) where lower(ItemName) like @SearchText order by ItemName;",
                                            SQLCallBack ="select distinct iv.UM Unit, iv.UomID, i.ItemID ItemIDs, i.ItemName ItemNames \r\nfrom dbo.TblItemVariant iv \r\nleft join dbo.TblStockInDetail sd on sd.ItemID = iv.ItemID \r\ninner join dbo.TblItem i on i.ItemID = iv.ItemID \r\nwhere iv.ItemID = @SearchText",
                                            DisplayOnFields="Unit, UomID, ItemIDs, ItemNames",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM Unit, CostPrice UnitPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            DisplayOnFieldsTypeahead="UnitPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="Unit", FieldName = "ItemID",
                                            CallBackDisplayText="Unit",
                                            placeholder="Search item.. (name, code, serial_no)"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemNames",Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Name",
                                            DBField="ItemName", HiddenField="ItemIDs", Hide = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter QTY",
                                            DBField="QTY", Required = true, Align="center", ISNumeric=true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Unit",Required = true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Unit",
                                            DBField="Unit",HiddenField = "UomID"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ProjectName", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true, Hide = html.SystemID != "13",
                                            DBField="ProjectName", isTypeahead = true, Required = false,
                                            SQL="select t.ProjectID,t.ProjectName from TblProject t where  t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%"+html.DomainID+"%' order by t.ProjectID",
                                            placeholder="Enter Project", HiddenField="ProjectID"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="BlockName", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true, Hide = html.SystemID != "13",
                                            DBField="BlockName", isTypeahead = true, Required = html.SystemID != "25",
                                            SQL="select t.CustomerTypeID BlockID,(t.CustomerType + '-' + t.DistrictCode) BlockName from dbo.TblCustomerType t where t.[Status] = 'Enable' and t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%"+html.DomainID+"%' order by BlockID",
                                            placeholder="Enter Block", HiddenField="BlockID"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Remark",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Purpose or Reasons",
                                            DBField="Remark",
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            html.SaveChangeCallbackFunction = "SendNotificationRequestItem()";

            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult AddRequestApprovalItemOut(string PrimaryFieldValue, bool IsConfirmPrepared = false)
        {
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "RequestItemID", PrimaryFieldValue }
            };
            int refId = 0;
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(PrimaryFieldValue))
            {
                string sql = @"select RequestMode, ReferenceRequestItemID from dbo.TblItem_Request where RequestItemID = @RequestItemID";
                ds = html.LoadData(sql, false, para);
            }

            html.FormID = "AddItemRequest";
            html.PrimaryField = "RequestItemID";
            if (ds.Tables[0].Rows.Count > 0)
            {
                string requestMode = ds.Tables[0].Rows[0]["RequestMode"].ToString();
                if (requestMode == "Request Approval Item Out")
                {
                    html.PrimaryFieldValue = PrimaryFieldValue;
                    PrimaryFieldValue = ds.Tables[0].Rows[0]["ReferenceRequestItemID"].ToString();
                    refId = int.Parse(PrimaryFieldValue);
                }
            }
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Request Information",
                    SplitColumns = new List<SplitColumn>()
                    {
                        new SplitColumn(){
                            Col = "col-8",
                            ID = "divmain"
                        },
                        new SplitColumn(){
                            Col = "col-4",
                            ID = "divmain1",
                            SQL=@"select i.ItemCode 'Item Code', i.ItemName 'Item Name', rd.QTY, rd.Unit UOM, p.ProjectName 'Project', t.CustomerType Block 
from dbo.TblItem_Request_Detail rd
inner join dbo.TblItem i on i.ItemID = rd.ItemID
left join dbo.TblProject p on p.ProjectID = rd.ProjectID
left join dbo.TblCustomerType t on t.CustomerTypeID = rd.BlockID
inner join dbo.TblItem_Request ir on ir.RequestItemID = rd.RequestItemID
where ir.RequestMode = 'Item Request' and ir.RequestItemID = " + PrimaryFieldValue
                        }
                    },
                    TableName="TblItem_Request",
                    PrimaryFieldName = "RequestItemID",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldValue=PrimaryFieldValue,
                    SQL="update dbo.TblItem_Request set IsRequestApprovalItemOut = 0 where RequestItemID = @RequestNo_ReferenceRequestItemID",
                    UpdateFiledName ="RequestNo_ReferenceRequestItemID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="PreparedUserID", SetValue="`+$(\"#PreparedBy_PreparedUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="RequestApprovalUserID", SetValue="`+$(\"#RequestApprovalBy_RequestApprovalUserID\").val()+`"
                        },

                        new DBField()
                        {
                            DBFieldName = "RequestApprovalItemOutStatus", SetValue="Pending"
                        },

                        new DBField()
                        {
                            DBFieldName = "RequestMode", SetValue="Request Approval Item Out"
                        },
                        new DBField()
                        {
                            DBFieldName = "IsRequestApprovalItemOut", SetValue="1"
                        },
                        new DBField()
                        {
                            DBFieldName="ReferenceRequestItemID", SetValue="`+$(\"#RequestNo_ReferenceRequestItemID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="ReceivedUserID", SetValue="`+$(\"#ReceivedBy_ReceivedUserID\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="ReceivedTelegaramUserName", SetValue="`+$(\"#ReceivedBy_ReceivedTelegaramUserName\").val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsReceived", SetValue="0"
                        },
                        new DBField()
                        {
                            DBFieldName="IsWaitingReceived", SetValue="0"
                        },
                        new DBField()
                        {
                            DBFieldName="IsConfirmPrepared", SetValue="1"
                        },
                        new DBField()
                        {
                            DBFieldName = "AvariableQTY", Value ="AvariableQTY", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName = "WID", Value ="Warehouse_WID", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName = "ToWarehouseID", Value ="ToWarehouse_ToWarehouseID", GetValueFromInputName = true
                        }
                    },

                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request No:", ID="RequestNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead = true,
                            SQL = "select distinct ir.RequestItemID ReferenceRequestItemID, ir.RequestNo, ir.RequestName ReceivedBy, ir.RequestNameUserID ReceivedUserID, ir.RequestNameTelegram ReceivedTelegaramUserName, ir.Department, ir.[Address] from dbo.TblItem_Request ir where ((ir.NeedToApprovalUserID = 0 and ir.Status = 'Pending') or (ir.NeedToApprovalUserID <> 0 and ir.Status = 'Approved')) and ir.RequestMode='Item Request' and ir.SystemID = " + html.SystemID + " and ir.WarehouseID = " + html.WarehouseID +"order by RequestItemID desc",
                            DBField="RequestNo", HiddenField="ReferenceRequestItemID",
                            DisplayOnFields="Department, Address",
                            CallBackFunction="ShowRequestItem",
                        },
                        new HtmlTextBox()
                        {
                            Label="Stock Out No:", ID="RequestApprovalNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestApprovalNo", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Stock Out'; select @Prefix = Prefix from dbo.TblPrefix where Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'SI{yy}-{00000}'",
                            CallBackToControlID="RequestApprovalNo, PrefixRequestNo", UpdatePrefixModule="Stock Out", Prefix="PrefixRequestNo", SetPrefixToDbFieldName="RequestApprovalNo",
                            AllowAddNewPrefixEventPrimaryValueIsNotZeo= IsConfirmPrepared == true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared Date:", ID="PreparedDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedDate", SingleDatePicker = true,
                            SingleDatePickerPosition="down",DivClassCol ="col-sm-auto"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Prepared By:", ID="PreparedBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedBy",isTypeahead = true,
                            SQL="select u.UserLoginID PreparedUserID, u.TelegramUserName PreparedTelegaram, u.FullName PreparedBy, u.Telephone PreparedPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PreparedTelegaram, PreparedUserID",
                            HiddenFieldValue="0,0",
                            DisplayOnFields="PreparedPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared's Phone:", ID="PreparedPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Approval By:", ID="RequestApprovalBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestApprovalBy",isTypeahead = true,
                            SQL="select u.UserLoginID RequestApprovalUserID, u.TelegramUserName RequestApprovalTelegaram, u.FullName RequestApprovalBy\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="RequestApprovalTelegaram, RequestApprovalUserID",
                            HiddenFieldValue="0,0",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Department:", ID="Department", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Department",
                            isTypeahead = true,
                            SQL="select distinct Department from dbo.TblItem_Request where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by Department"
                        },
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address", isTypeahead =true,
                            SQL="select distinct [Address] from dbo.TblItem_Request where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by [Address]"
                        },
                        new HtmlTextBox()
                        {
                            Label="Received By:", ID="ReceivedBy", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceivedBy",isTypeahead = true,
                            SQL="select u.UserLoginID ReceivedUserID, u.TelegramUserName ReceivedTelegaramUserName, u.FullName ReceivedBy, u.Telephone RequestPhone from dbo.TblUserLogin u inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID where u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ReceivedTelegaramUserName, ReceivedUserID",
                            HiddenFieldValue="0,0",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="From Warehouse:", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID"
                        },
                        new HtmlTextBox()
                        {
                            Label="To Warehouse:", ID="ToWarehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ToWarehouse", isTypeahead = true,
                            SQL="select uw.WID ToWarehouseID, uw.WarehouseName ToWarehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="ToWarehouseID"
                        },
                    }
                },
                new  FormData() {
                    TableName="TblItem_Request_Detail",
                    GetIDENTITYByFiled="RequestItemID",
                    PrimaryFieldName="RequestItemID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_Request_Item",
                            InputName="ItemName",
                            TRName="TR_Items_Request",
                            TDName="TD_Items_Request",
                            PrimaryFieldName="RequestItemID",
                            GetIDENTITYByFiled="RequestItemID",
                            //PrimaryFieldValue=PrimaryFieldValue,
                            TableName="TblItem_Request_Detail",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName, Reload_BlockName(), Reload_ProjectName(), Reload_IsSpare()",
                            ReturnFieldNameValue="RequestItemID",
                            ReturnFieldNameValueFromTblName = "TblItem_Request",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", GetValueFromInputName=true, Value = "ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", GetValueFromInputName=true, Value = "UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName = "SerialNumberID", Value = "SerialNumberID", GetValueFromInputName = true
                                },
                                new DBField()
                                {
                                    DBFieldName="LocationID", SetValue="$(\"#Warehouse_WID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="ToWarehouseID", SetValue="$(\"#ToWarehouse_ToWarehouseID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="CostPrice", GetValueFromInputName = true, Value = "CostPrice", IsNumber=true
                                },
                                new DBField()
                                {
                                    DBFieldName="SallingPrice", GetValueFromInputName = true, Value = "SalePrice", IsNumber = true
                                },

                                new DBField()
                                {
                                    DBFieldName="ProjectID", GetValueFromInputName=true, Value = "ProjectID"
                                },
                                new DBField()
                                {
                                    DBFieldName="BlockID", GetValueFromInputName=true, Value = "BlockID"
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No.", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Item Description", Class="text-nowrap col-sm-3"
                                },
                                new TblTHead()
                                {
                                     Label="Code", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Serial Number", Class="text-nowrap col-sm-auto", Hide = html.SystemID != "13"
                                },
                                new TblTHead()
                                {
                                     Label="Expire Date", Class="text-nowrap col-sm-auto", Hide = html.SystemID == "13"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Model", Class="text-nowrap col-sm-auto"
                                //},
                                new TblTHead()
                                {
                                     Label="QTY", Class="text-nowrap col-sm-auto"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap col-sm-1"
                                },
                                 new TblTHead()
                                 {
                                      Label="Project", Class="text-nowrap col-sm-2", Hide = html.SystemID != "13",
                                 },
                                new TblTHead()
                                {
                                     Label="Block", Class="text-nowrap col-sm-2", Hide = html.SystemID != "13",
                                },
                                new TblTHead()
                                {
                                     Label="Purpose or Reasons", Class="text-nowrap col-sm-1",
                                     //CloseTR="</tr>"
                                },
                                new TblTHead()
                                {
                                     Label="Is Spare?", Class="text-nowrap col-sm-1",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_Items_Request\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_Items_Request"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemNameFull", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TR_Items_Request', 'TD_Items_Request', this,'','ItemName');",
                                            ISAutoComplete = true,
                                            HiddenField="ItemID",
                                            SQL_AutoComplete=$"select top 10 * from dbo.fn_ItemAutoComplete({html.SystemID},{html.WarehouseID},{html.DomainID}) where lower(ItemName) like @SearchText order by ItemName;",
                                            SQLCallBack ="select distinct i.ItemCode, i.ItemName ItemNames from dbo.TblItemVariant iv inner join dbo.TblItem i on i.ItemID = iv.ItemID where i.ItemID = @SearchText",
                                            CboValue="Warehouse_WID",
                                            DisplayOnFields="ItemCode, ItemNames, SerialNumber, SerialNumberID",
                                            SQLTypeaheadCallBack = "select distinct iv.ItemID, iv.UomID, iv.UM Unit, iv.CostPrice, iv.SellingPrice SalePrice \r\nfrom dbo.TblItemVariant iv\r\n where iv.ItemID = @ItemID",
                                            DisplayOnFieldsTypeahead="CostPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="Unit", FieldName = "ItemID",
                                            CallBackDisplayText="Unit",
                                            placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemCode",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Code",
                                            DBField="ItemCode",IsReadOnly = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemNames",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Item Name",
                                            DBField="ItemName", Hide = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SerialNumber",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Serial Number",
                                            DBField="SerialNumber",
                                            HiddenField="CostPrice, SalePrice, LocationID, AvariableQTY, SerialNumberID",
                                            HiddenFieldValue="0,0,0,0,0",IsReadOnly = true, Hide = html.SystemID != "13"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpireDate",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="StockExpireDate",placeholder="Enter Expire Date",
                                            DivClass="wrap-input3", Hide = html.SystemID == "13"
                                        },
                                        
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="Model",
                                        //    Class="input2 CustInfo", IsCustomForm = true,
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Model",
                                        //    DBField="Model",
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="QTY",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter QTY",
                                            DBField="QTY", Required = true, Align="center", ISNumeric=true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Unit", Required = true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter UOM",
                                            DBField="Unit",HiddenField = "UomID",
                                        },
                                         new HtmlTextBox()
                                        {
                                            Name="ProjectName", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true, Hide = html.SystemID != "13",
                                            DBField="ProjectName", isTypeahead = true, Required = html.SystemID != "25",
                                            SQL="select t.ProjectID,t.ProjectName from TblProject t where t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%"+html.DomainID+"%' order by t.ProjectID",
                                            placeholder="Enter Project", HiddenField="ProjectID"
                                        },
                                         new HtmlTextBox()
                                        {
                                            Name="BlockName", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="BlockName", isTypeahead = true, Required = html.SystemID != "25",
                                            SQL="select t.CustomerTypeID BlockID,(t.CustomerType + '-' + t.DistrictCode) BlockName from dbo.TblCustomerType t where t.[Status] = 'Enable' and t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%"+html.DomainID+"%' order by BlockID",
                                            placeholder="Enter Block", HiddenField="BlockID", Hide = html.SystemID != "13",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Remark",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Purpose or Reasons",
                                            DBField="Remark",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="IsSpare", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="IsSpare", isTypeahead = true,Required = true,
                                            //HiddenFeild="IsStockOut",
                                            SQL="select 'Yes' IsSpare union all select 'No' IsSpare",
                                            //Value="Yes",
                                            placeholder="Select is spare"
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
            html.SaveChangeAndClose = true;
            //html.CustomValidateFunction = "ValidateQTYOUT";
            ViewBag.DrawHTMLFormV1 = html.DrawHTMLFormV1();
            return View();
        }
        public JsonResult LoadItemRequested(bool IsRequestApprovalItemOut = false)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"select ir.RequestItemID, ir.RequestNo, format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, 
isnull(ir.[Status], 'Pending') [Status], u.FullName
from dbo.TblItem_Request ir
inner join dbo.TblUserLogin u on u.UserLoginID = ir.NeedToApprovalUserID
where ir.RequestMode = 'Item Request' and ir.SystemID = " + html.SystemID + " and ir.WarehouseID = " + html.WarehouseID + " order by format(ir.RequestDate, 'dd-MM-yyyy') desc, ir.RequestItemID desc";
            if (IsRequestApprovalItemOut)
            {
                sql = @"select ir.RequestItemID, ir.RequestNo, ir.RequestApprovalNo, format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, 
isnull(ir.[RequestApprovalItemOutStatus], 'Pending') [Status], u.FullName
from dbo.TblItem_Request ir
inner join dbo.TblUserLogin u on u.UserLoginID = ir.NeedToApprovalUserID
where ir.IsRequestApprovalItemOut = 1 and ir.SystemID = " + html.SystemID + " and ir.WarehouseID = " + html.WarehouseID + " order by format(ir.RequestDate, 'dd-MM-yyyy') desc, ir.RequestItemID desc";
            }
            DataSet ds = html.LoadData(sql);
            return Json(new { Data = ds.Tables[0].ToJsonString() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ItemRequestForm(int ID, bool IsRequestApprovalItemOut = false)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "RequestItemID", ID.ToString() }
            };
            string sql = @"select w.Logo, w.WarehouseID from dbo.TblItem_Request ri
inner join dbo.TblWarehouse w on w.WarehouseID = ri.WarehouseID
where RequestItemID = @RequestItemID and isnull(Logo,'') <> ''";
            string Logo = "";
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
                Logo = ds.Tables[0].Rows[0]["Logo"].ToString();
            ViewBag.ServerRoot = html.ServerRoot;
            ViewBag.Logo = "../Uploads/WarehouseLogo/" + Logo;
            ViewBag.IsRequestApprovalItemOut = IsRequestApprovalItemOut;
            sql = !IsRequestApprovalItemOut
                ? @"select ua.Job ApprovalByPosition, ul.Job RequestByPosition, ir.RequestItemID, ir.RequestName, ir.RequestPhone, isnull(ir.RequestNo, '') RequestNo, isnull(ir.RequestApprovalNo,'') RequestApprovalNo, ir.Department, ir.[Address],
format(ir.RequestDate, 'dd-MM-yyyy') RequestDate,ird.ItemName, ird.ItemNameFull, ird.ItemSize, ird.QTY, ird.Unit, ird.Remark, ir.RequestNameUserID,
isnull(ua.Signature, '') NeedToApprovalSignature, ir.[Status], ird.ItemCode, ird.SerialNumber,
ua.FullName ApprovalName, ir.ApproveDisapproveDate, isnull(ul.Signature, '') RequestSignature, 
isnull(ir.WaitingConfirmedReceivedStatus,'') WaitingConfirmedReceivedStatus, ir.SystemID,
isnull(IsWaitingConfirmedReceived, 0) IsWaitingConfirmedReceived, isnull(ir.RequestApprovalItemOutStatus, '') RequestApprovalItemOutStatus,
case when isnull(ir.ApprovedRequestDate,'') <> '' then ir.ApprovedRequestDate else '' end ApprovedRequestDate, format(ir.RequirementDate, 'dd-MM-yyyy') RequirementDate, i.Model, t.CustomerType Block, tp.ProjectName ProjectName
from dbo.TblItem_Request ir
inner join dbo.TblItem_Request_Detail ird on ird.RequestItemID = ir.RequestItemID
inner join dbo.TblItem i on i.ItemID = ird.ItemID
left join dbo.TblCustomerType t on t.CustomerTypeID = ird.BlockID
left join dbo.TblProject tp on tp.ProjectID = ird.ProjectID
inner join dbo.TblUserLogin ul on ul.UserLoginID = ir.RequestNameUserID
left join dbo.TblUserLogin ua on ua.UserLoginID = ir.NeedToApprovalUserID
where ir.RequestMode = 'Item Request' and ir.RequestItemID = @RequestItemID"
                : @"select ua.Job ApprovalByPosition, ul.Job PrepareByPosition, ur.Job ReceiveByPosition, ir.RequestItemID, format(ir.PreparedDate, 'dd-MM-yyyy') PreparedDate, ir.PreparedBy, ir.PreparedPhone, isnull(ir.RequestNo, '') RequestNo, isnull(ir.RequestApprovalNo,'') RequestApprovalNo, ir.Department, ir.[Address],
ird.ItemNameFull,ird.ItemName, ird.ItemSize, ird.QTY, ird.Unit, ird.Remark, ir.PreparedUserID RequestNameUserID,format(ir.RequestDate, 'dd-MM-yyyy') RequestDate, ir.RequestName, ir.RequestPhone,
isnull(ua.[Signature], '') NeedToApprovalSignature, isnull(ir.[Status],'Pending') [Status], ird.ItemCode, ird.SerialNumber,
ua.FullName ApprovalName, ir.ApproveDisapproveDate, isnull(ul.[Signature], '') RequestSignature, 
isnull(ir.WaitingConfirmedReceivedStatus,'') WaitingConfirmedReceivedStatus, isnull(IsWaitingConfirmedReceived, 0) IsWaitingConfirmedReceived,
ir.ReceivedBy, case when ir.IsReceived = 1 then convert(nvarchar, ir.ReceivedDate) else '' end ReceivedDate, isnull(ir.IsReceived, '') IsReceived,
isnull(ur.[Signature], '') ReceivedUserSignature, isnull(ir.RequestApprovalItemOutStatus, '') RequestApprovalItemOutStatus, ir.SystemID,
case when isnull(ir.ApprovedRequestDate,'') <> '' then ir.ApprovedRequestDate else '' end ApprovedRequestDate, format(ir.RequirementDate, 'dd-MM-yyyy') RequirementDate, i.Model, t.CustomerType Block, tp.ProjectName ProjectName
from dbo.TblItem_Request ir
left join dbo.TblItem_Request_Detail ird on ird.RequestItemID = ir.RequestItemID
inner join dbo.TblItem i on i.ItemID = ird.ItemID
left join dbo.TblUserLogin ul on ul.UserLoginID = ir.PreparedUserID
left join dbo.TblCustomerType t on t.CustomerTypeID = ird.BlockID
left join dbo.TblProject tp on tp.ProjectID = ird.ProjectID
left join dbo.TblUserLogin ua on ua.UserLoginID = ir.RequestApprovalUserID
left join dbo.TblUserLogin ur on ur.UserLoginID = ir.ReceivedUserID
where ir.IsRequestApprovalItemOut = 'True' and ir.RequestItemID = @RequestItemID";
            ds = html.LoadData(sql, false, para);
            ViewBag.SRID = ID;
            ViewBag.Status = ds.Tables[0].Rows[0]["Status"];
            ViewBag.Data = ds.Tables[0];
            return View();
        }

        public async Task<SqlResult> GeneratePDF(int requestID, string requestType, string refNo)
        {
            var html = new HTML();
            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);

            var routeData = new RouteData();
            routeData.Values["controller"] = "Items";
            routeData.Values["action"] = "ItemRequestForm";

            var controller = new ItemsController();
            controller.ControllerContext = new ControllerContext(
                httpContext,
                routeData,
                controller);

            int warehouseId = int.Parse(html.WarehouseID);
            int systemId = int.Parse(html.SystemID);

            ViewEngineResult viewResult;
            string partialViewName;

            if (requestType == "Request Stock Return" || requestType == "Stock Return")
            {
                controller.StockReturnForm(
                    requestID,
                    requestType == "Request Stock Return",
                    warehouseId,
                    systemId);

                partialViewName = "StockReturnForm";
            }
            else
            {
                controller.ItemRequestForm(
                    requestID,
                    requestType == "Request Approval Item Out");

                partialViewName = "ItemRequestForm";
            }

            viewResult = ViewEngines.Engines.FindPartialView(
                controller.ControllerContext,
                partialViewName);

            if (viewResult?.View == null)
                throw new InvalidOperationException(
                    $"Could not locate the partial view '{partialViewName}'.");

            string htmlContent;

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    sw);

                viewResult.View.Render(viewContext, sw);
                htmlContent = sw.ToString();
            }

            string templatePath = httpContext.Server.MapPath(
                "~/Templates/HtmlTemplate.html");

            string template = System.IO.File.ReadAllText(templatePath);

            string finalHtml = template
                .Replace("{Title}", "Inventory Request")
                .Replace("{BodyMessage}", htmlContent);

            return await html.HTMLToPDF(
                finalHtml,
                "Uploads/Item/",
                refNo, pageSize: "A3");
        }

        [HttpPost]
        public async Task<JsonResult> SendNotificationRequestItem(int ID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            StringBuilder sb = new StringBuilder();
            NotificationTelegram notification = new NotificationTelegram();

            try
            {
                //** Fetching data entry
                string sql = "select * from dbo.TblItem_Request where RequestItemID = @ID and SystemID = @SystemID;";
                var dicPara = new Dictionary<string, string>()
                {
                    { "ID", ID.ToString() },
                    { "SystemID", html.SystemID }
                };
                DataSet ds = html.LoadData(sql, false, dicPara);

                if (ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Request not found");

                DataRow row = ds.Tables[0].Rows[0];
                string reqType = row["RequestMode"].ToString();
                string reqMode = reqType == "Item Request"
                    ? "stock request"
                    : "stock issue";
                string refNo = reqType == "Item Request"
                    ? row["RequestNo"].ToString()
                    : row["RequestApprovalNo"].ToString();
                string reqBy = reqType == "Item Request"
                    ? row["RequestName"].ToString()
                    : row["PreparedBy"].ToString();
                string needApproveBy = reqType == "Item Request"
                    ? row["NeedToApproval"].ToString()
                    : row["RequestApprovalBy"].ToString();
                string needApproveByUsername = reqType == "Item Request"
                    ? row["NeedToApprovalTelegram"].ToString()
                    : row["RequestApprovalTelegaram"].ToString();

                //** Handle telegram message 
                sb.AppendLine($"{needApproveByUsername}\n");
                sb.AppendLine($"Dear {needApproveBy},\n");
                sb.AppendLine($"Now you have a {reqMode} need to approve.\n");
                sb.AppendLine($"<i><b>Prepared by: {reqBy}</b></i>.\n");
                sb.AppendLine($"Please kindly review and take action, Thanks you.");

                // Get chat groupId
                sql = "select * from dbo.TblTelegramGroupNotification where Module = @Module and SystemID = @SystemID;";
                dicPara.Add("Module", reqType == "Item Request" ? "StockRequest" : "StockIssue");
                ds.Clear();
                ds = html.LoadData(sql, false, dicPara);

                if (ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Group not found");

                row = ds.Tables[0].Rows[0];
                long chatId = Utility.TryParse(row["TelegramGroupID"].ToString(), "long", 0);
                int topicId = Utility.TryParse(row["TelegramGroupTopicID"].ToString(), "int", 0);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveItemRequest~{ID}~{needApproveByUsername}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"ApproveItemRequest~{ID}~{needApproveByUsername}"),
                    }
                });

                // PDF form generate
                var pdfResult = await GeneratePDF(ID, reqType, refNo);
                string msg = sb.ToString();
                string botToken = html.GetBotTokenBySystemID(int.Parse(html.SystemID));
                result = await notification.NotificationTelegramBotToTopic(
                    "",
                    chatId: chatId,
                    topicID: topicId,
                    inlineKeyboard: inlineKeyboard,
                    Files: pdfResult?.OutPutFile,
                    Caption: msg.ToString(),
                    EditMessage: false,
                    newBotClient: botToken
                );

                // Save change current message 
                if (result.Result == ResultType.Successful)
                {
                    System.IO.File.Delete(pdfResult.OutPutFile);

                    sql = @"update dbo.TblItem_Request set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage, TelegramGroupID = @TelegramGroupID where RequestItemID = @ID";
                    dicPara.Add("@TelegramMessageID", result.Value.ToString());
                    dicPara.Add("@TelegramMessage", msg.ToString());
                    dicPara.Add("@TelegramGroupID", chatId.ToString());

                    result = html.SaveChangeData(sql, false, dicPara);
                }
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }

            return Json(new { Result = result.Result, Message = result.Message });
        }

        public async Task<JsonResult> SendNotificationStockReturn(int ID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            StringBuilder sb = new StringBuilder();
            NotificationTelegram notification = new NotificationTelegram();

            try
            {
                //** Fetching data entry
                string sql = "select * from dbo.TblItem_Request where RequestItemID = @ID and SystemID = @SystemID;";
                var dicPara = new Dictionary<string, string>()
                {
                    { "ID", ID.ToString() },
                    { "SystemID", html.SystemID }
                };
                DataSet ds = html.LoadData(sql, false, dicPara);

                if (ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Request not found");

                DataRow row = ds.Tables[0].Rows[0];
                string reqType = row["RequestMode"].ToString();
                string reqMode = reqType == "Request Stock Return"
                    ? "request return"
                    : "issue stock return";
                string refNo = reqType == "Request Stock Return"
                    ? row["RequestReturnNo"].ToString()
                    : row["RequestApprovalNo"].ToString();
                string reqBy = reqType == "Request Stock Return"
                    ? row["RequestName"].ToString()
                    : row["PreparedBy"].ToString();
                string needApproveBy = reqType == "Request Stock Return"
                    ? row["NeedToApproval"].ToString()
                    : row["RequestApprovalBy"].ToString();
                string needApproveByUsername = reqType == "Request Stock Return"
                    ? row["NeedToApprovalTelegram"].ToString()
                    : row["RequestApprovalTelegaram"].ToString();

                //** Handle telegram message 
                sb.AppendLine($"{needApproveByUsername}\n");
                sb.AppendLine($"Dear {needApproveBy},\n");
                sb.AppendLine($"Now you have a {reqMode} need to approve.\n");
                sb.AppendLine($"<i><b>Prepared by: {reqBy}</b></i>.\n");
                sb.AppendLine($"Please kindly review and take action, Thanks you.");

                // Get chat groupId
                sql = "select * from dbo.TblTelegramGroupNotification where Module = @Module and SystemID = @SystemID;";
                dicPara.Add("Module", "StockReturn");
                ds.Clear();
                ds = html.LoadData(sql, false, dicPara);

                if (ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Group not found");

                row = ds.Tables[0].Rows[0];
                long chatId = Utility.TryParse(row["TelegramGroupID"].ToString(), "long", 0);
                int topicId = Utility.TryParse(row["TelegramGroupTopicID"].ToString(), "int", 0);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveStockReturn~{ID}~{needApproveByUsername}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"ApproveStockReturn~{ID}~{needApproveByUsername}"),
                    }
                });

                // PDF form generate
                var pdfResult = await GeneratePDF(ID, reqType, refNo);
                string msg = sb.ToString();
                string botToken = html.GetBotTokenBySystemID(int.Parse(html.SystemID));
                result = await notification.NotificationTelegramBot(
                    "",
                    chatId: chatId,
                    PDFFile: pdfResult?.OutPutFile,
                    Caption: msg.ToString(),
                    replyMarkupInlineKeyboardMarkup: true,
                    inlineKeyboard: inlineKeyboard,
                    newBotClient: botToken
                );

                // Save change current message 
                if (result.Result == ResultType.Successful)
                {
                    System.IO.File.Delete(pdfResult.OutPutFile);

                    sql = @"update dbo.TblItem_Request set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage, TelegramGroupID = @TelegramGroupID where RequestItemID = @ID";
                    dicPara.Add("@TelegramMessageID", result.Value.ToString());
                    dicPara.Add("@TelegramMessage", msg.ToString());
                    dicPara.Add("@TelegramGroupID", chatId.ToString());

                    result = html.SaveChangeData(sql, false, dicPara);
                }
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }

            return Json(new { Result = result.Result, Message = result.Message });
        }

        public async Task<JsonResult> ApproveDisapproveRequestReturn(int ID, string Status, string RequestMode = "Request Stock Return", string FromName = "", string Comment = "")
        {
            bool IsRequestStockReturn = true, updateDB = false;
            DataSet ds = new DataSet();
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"RequestItemID", ID.ToString() },
                {"Status", Status},
            };

            string sql = @"select ir.RequestItemID, u.FullName, ir.ReceivedTelegaramUserName, isnull(ir.ReceivedUserID, 0) ReceivedUserID, ir.ReceivedBy, ir.TelegramMessageID, ir.TelegramMessage, ir.RequestApprovalNo, ir.RequestMode, ir.RequestReturnNo,isnull(ir.NeedToApprovalUserID, 0) NeedToApprovalUserID, ir.NeedToApprovalTelegram, ir.EditTelegramMessage, isnull(ir.ReferenceRequestItemID, 0) ReferenceRequestItemID, ir.TelegramGroupID, ir.WarehouseID, ir.SystemID
from TblItem_Request ir
left join dbo.TblUserLogin u on u.UserLoginID = ir.NeedToApprovalUserID 
where ir.RequestItemID = @RequestItemID";
            if (RequestMode == "Stock Return")
            {
                sql = @"select ir.RequestItemID, u.FullName, ir.ReceivedTelegaramUserName, isnull(ir.ReceivedUserID, 0) ReceivedUserID, ir.ReceivedBy, ir.TelegramMessageID, ir.TelegramMessage, ir.RequestApprovalNo, ir.RequestMode, ir.RequestReturnNo, isnull(ir.RequestApprovalUserID, 0) NeedToApprovalUserID, ir.RequestApprovalTelegaram NeedToApprovalTelegram, ir.EditTelegramMessage, isnull(ir.ReferenceRequestItemID, 0) ReferenceRequestItemID, ir.TelegramGroupID, ir.WarehouseID, ir.SystemID
from TblItem_Request ir
left join dbo.TblUserLogin u on u.UserLoginID = ir.RequestApprovalUserID 
where ir.RequestItemID = @RequestItemID";
            }


            ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                string RequestReturnNo = dr["RequestReturnNo"].ToString();
                string RequestApprovalNo = dr["RequestApprovalNo"].ToString();
                bool EditTelegramMessage = bool.Parse(dr["EditTelegramMessage"].ToString());
                string TelegramGroupID = dr["TelegramGroupID"].ToString();
                string TelegramMessageID = dr["TelegramMessageID"].ToString();
                string TelegramMessage = html.HtmlDecode(dr["TelegramMessage"].ToString());
                string FullName = dr["FullName"].ToString();
                int NeedToApprovalUserID = int.Parse(dr["NeedToApprovalUserID"].ToString());
                string NeedToApprovalTelegram = dr["NeedToApprovalTelegram"].ToString();
                int ReferenceRequestItemID = int.Parse(dr["ReferenceRequestItemID"].ToString());
                string ReceivedTelegaramUserName = dr["ReceivedTelegaramUserName"].ToString();
                string ReceivedBy = dr["ReceivedBy"].ToString();
                int ReceivedUserID = int.Parse(dr["ReceivedUserID"].ToString());
                int WarehouseID = int.Parse(dr["WarehouseID"].ToString());
                int SystemID = int.Parse(dr["SystemID"].ToString());

                if (Status == "Cancel")
                {
                    sql = @"update dbo.TblItem_Request set [Status] = @Status, CancelDate = @CancelDate, CancelByUserID = @CancelByUserID, Comment = @Comment, CancelBy= @CancelBy
where RequestItemID = @RequestItemID";

                    para.Add("CancelDate", html.GetCurrentDate());
                    para.Add("CancelByUserID", html.UserID);
                    para.Add("Comment", Comment);
                    para.Add("CancelBy", html.UserFullName);
                }
                else
                {
                    if (FullName == html.UserFullName || NeedToApprovalTelegram == FromName)
                    {
                        if (RequestMode == "Request Stock Return")
                        {
                            sql = @"update dbo.TblItem_Request set [Status] = @Status, ApproveDisapproveDate = @ApproveDisapproveDate";
                            if (Status == "Approved")
                            {
                                sql += ", IsWaitingConfirmedReceived = 1, WaitingConfirmedReceivedStatus='Waiting Stock Confirm'";
                            }
                            sql += " where RequestItemID = @RequestItemID";

                            para.Add("ApproveDisapproveDate", html.GetCurrentDate());
                        }
                        else if (RequestMode == "Stock Return")
                        {
                            IsRequestStockReturn = false;
                            sql = @"declare @ReferenceRequestItemID int, @TelegramGroupID nvarchar(50), @TelegramGroupMessageID int, @FromTelegramGroupID nvarchar(50), @FromTelegramGroupMessageID int;
                        select @ReferenceRequestItemID = ReferenceRequestItemID, @TelegramGroupID = TelegramGroupID,@TelegramGroupMessageID=TelegramGroupMessageID, @FromTelegramGroupID = FromTelegramGroupID,@FromTelegramGroupMessageID=FromTelegramGroupMessageID  from dbo.TblItem_Request where RequestItemID = @RequestItemID;
                        select @TelegramGroupID = TelegramGroupID,@TelegramGroupMessageID=TelegramGroupMessageID, @FromTelegramGroupID = FromTelegramGroupID,@FromTelegramGroupMessageID=FromTelegramGroupMessageID  from dbo.TblItem_Request where RequestItemID = @ReferenceRequestItemID;
                        update TblItem_Request set IsWaitingReceived = 1, WaitingConfirmedReceivedStatus='Waiting Confirm Received' where RequestItemID = @ReferenceRequestItemID;
                        update dbo.TblItem_Request set ApprovedRequestDate = @ApprovedRequestDate";
                            if (Status == "Approved")
                            {
                                sql += ", [Status] = 'Approved', IsWaitingConfirmedReceived = 1, IsRequestApprovalItemOut = 1, RequestApprovalItemOutStatus = 'Approved', IsWaitingReceived = 1, WaitingConfirmedReceivedStatus='Waiting Confirm Received', ApproveDisapproveDate = @ApprovedRequestDate";
                                sql += ", TelegramGroupID = @TelegramGroupID, TelegramGroupMessageID = @TelegramGroupMessageID, FromTelegramGroupID = @FromTelegramGroupID, FromTelegramGroupMessageID = @FromTelegramGroupMessageID ";
                            }
                            else
                            {
                                sql += ", RequestApprovalItemOutStatus = 'Disapproved'";
                            }
                            sql += " where RequestItemID = @RequestItemID";

                            para.Add("ApprovedRequestDate", html.GetCurrentDate());
                        }
                    }
                    else
                    {
                        return Json(new { Result = sqlresult.Result, Message = "You cannot take this action." }, JsonRequestBehavior.AllowGet);
                    }
                }

                html.SaveChangeData(sql, false, para);

                var controller = DependencyResolver.Current.GetService<ItemsController>();
                var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
                var routeData = new RouteData();
                routeData.Values.Add("controller", "Items");
                routeData.Values.Add("action", "StockReturnForm");
                var controllerContext = new ControllerContext(httpContext, routeData, controller);
                var sw = new StringWriter();
                controller.ControllerContext = controllerContext;
                controller.StockReturnForm(ID, IsRequestStockReturn, WarehouseID, SystemID);
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, "StockReturnForm");
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                string Data = sw.GetStringBuilder().ToString();
                string FilePath = httpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
                StreamReader str = new StreamReader(FilePath);
                string Text = str.ReadToEnd();
                str.Close();

                Text = Text.Replace("{Title}", "Stock Return");
                Text = Text.Replace("{BodyMessage}", Data);
                sqlresult = await html.HTMLToPDF(Text.ToString(), "Uploads/Item", RequestApprovalNo != "" ? RequestApprovalNo : RequestReturnNo);
                string FromTelegramGroupID = "";
                string TelegramGroupMessageID = "0", FromTelegramGroupMessageID = "";

                if (sqlresult.Result == ResultType.Successful)
                {
                    //nsm isp item request = 4117466359; 4152951572 = nsm isp request inventory out
                    NotificationTelegram notificationTelegram = new NotificationTelegram();
                    string OutPutFile = sqlresult.OutPutFile;
                    string msg = "";
                    if (Status == "Cancel")
                    {
                        msg += "Dear Team, <br><br>Now this Stock Return is " + Status + " ❌ by " + (html.UserFullName == "" ? FromName : html.UserFullName) + ".<br><br>";
                        msg += "<b>Remark:</b> " + Comment + ".<br><br>";
                    }
                    else
                    {
                        if (Status == "Approved")
                        {
                            msg += "Dear Team, <br><br>Now this Stock Return is " + Status + " ✅ by " + (html.UserFullName == "" ? FromName : html.UserFullName);
                            if (RequestMode == "Request Stock Return")
                            {
                                msg += " and WAITING STOCK CONFIRM.<br><br>";
                            }
                            else
                            {
                                msg += ".<br><br>";
                            }
                        }
                        else
                        {
                            msg += "Dear Team, <br><br>Now this Stock Return is " + Status + " ❌ by " + (html.UserFullName == "" ? FromName : html.UserFullName) + ".<br><br>";
                        }
                    }
                    msg += "Please check attachment file for detail.<br><br>Thank you and best regards,<br><br>";
                    msg += "NSM ISP BILLING SYSTEM AUTO GENERATED Inventory Request.#" + RequestReturnNo;

                    if (RequestMode == "Request Stock Return" || RequestMode == "Stock Return")
                    {
                        sqlresult = Status == "Cancel"
                            ? await notificationTelegram.NotificationTelegramBot("", long.Parse(TelegramGroupID), "", OutPutFile, msg)
                            : await notificationTelegram.NotificationTelegramBot("", long.Parse(TelegramGroupID), "", OutPutFile, msg, false, false, ID, int.Parse(TelegramMessageID), TelegramMessage, null, EditTelegramMessage);

                        FromTelegramGroupID = TelegramGroupID;
                        FromTelegramGroupMessageID = sqlresult.Value;
                    }

                    sql = "update dbo.TblItem_Request set ReceivedBy = '" + ReceivedBy + "', ReceivedUserID = '" + ReceivedUserID + "', ReceivedTelegaramUserName = '" + ReceivedTelegaramUserName + "' where RequestItemID = " + ReferenceRequestItemID + ";";
                    if (!updateDB)
                    {
                        sql += "update dbo.TblItem_Request set FromTelegramGroupID='" + FromTelegramGroupID + "',FromTelegramGroupMessageID=" + FromTelegramGroupMessageID + ", TelegramGroupID='" + TelegramGroupID + "',TelegramGroupMessageID=" + TelegramGroupMessageID + ",TelegramMessageID = '" + sqlresult.Value + "', TelegramMessage = N'" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(msg)) + "', EditTelegramMessage = 0 where RequestItemID = " + ID;
                    }
                    html.SaveChangeData(sql, false, false);


                    if (sqlresult.Result == ResultType.Successful)
                    {
                        System.IO.File.Delete(OutPutFile);

                        //Insert data into stock in
                        if (!IsRequestStockReturn && Status == "Approved")
                        {
                            SaveItemReturnStockIn(ID);
                        }
                    }
                }
                else
                {
                    return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveItemReturnStockIn(int RequestItemID)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            string sql = "";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "RequestItemID", RequestItemID.ToString() }
            };

            sql += @"insert into dbo.TblStockInDetail(ItemID, ItemName, ItemDescription, StockInDate, Type, UM, UomID, QTY, CostPrice,LocationID, SystemID, WarehouseID,  RequestItemID, SerialNumberID, SerialNumber)
select id.ItemID, id.ItemName, i.ItemDescription, ir.ApprovedRequestDate StockInDate,'Stock Return' Type, id.Unit UM, id.UomID, id.QTY, id.CostPrice, id.LocationID, id.SystemID, id.WarehouseID, id.RequestItemID, id.SerialNumberID, id.SerialNumber
from dbo.TblItem_Request ir
inner join dbo.TblItem_Request_Detail id on id.RequestItemID = ir.RequestItemID
inner join dbo.TblItem i on i.ItemID = id.ItemID
where ir.RequestItemID = @RequestItemID;";

            //Update serial number status
            sql += @"DECLARE @ItemIDTable TABLE (ItemID INT);
INSERT INTO @ItemIDTable (ItemID)
SELECT distinct ItemID
FROM dbo.TblItem_Request_Detail
WHERE RequestItemID = @RequestItemID;
UPDATE srd SET srd.IsAvailable = 1, srd.TransactionType = 'Stock Return'
FROM dbo.TblItemSerialDetail srd
INNER JOIN dbo.TblStockInDetail sd ON sd.ItemID = srd.ItemID 
    AND sd.SerialNumberID = srd.ItemSerialDetailID 
    AND sd.Type = 'Stock Return'
WHERE srd.ItemID in (SELECT ItemID FROM @ItemIDTable);";

            sqlresult = html.SaveChangeData(sql, false, para);
            if (sqlresult.Result == ResultType.Successful)
            {
                UpdateQtyCalStockInDetail(RequestItemID.ToString(), "Stock Return");
                InventoryExpirationDateController expireDate = new InventoryExpirationDateController();
                expireDate.StockExpireOut(RequestItemID, "StockReturn");
            }

            return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteItemRequest(int ID)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            string sql = "delete from dbo.TblItem_Request where RequestItemID = " + ID;
            sql += "delete from dbo.TblItem_Request_Detail where RequestItemID = " + ID;
            sqlresult = html.SaveChangeData(sql, false, false);
            return Json(new { Result = sqlresult.Result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConfirmReceivedItem(int ID)
        {
            HTML html = new HTML
            {
                FormID = "CreateReceived",
                FieldIDFocus = "ReceivedBy"
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Received Date:",
                ID = "ReceivedDate",
                ISDateTime = true,
                SingleDatePicker = true,
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Received By:",
                ID = "ReceivedBy",
                isTypeahead = true,
                DBField = "ReceivedBy",
                SQL = "select u.UserLoginID ReceivedUserID, u.TelegramUserName ReceivedTelegaramUserName, u.FullName ReceivedBy, u.Telephone RequestPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = " + html.SystemID + " and ts.WarehouseID = " + html.WarehouseID + " order by u.FullName",
                HiddenField = "ReceivedUserID",
                DisplayOnFields = "ReceivedTelegaramUserName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Hide = true,
                Label = "Received Telegaram User Name:",
                ID = "ReceivedTelegaramUserName",
                DBField = "ReceivedTelegaramUserName",
            });
            html.SaveChangeFunction = "SaveChangeConfirmItemRequest()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public async Task<JsonResult> SaveChangeConfirmItemRequest(int ID)
        {
            HTML html = new HTML();

            try
            {
                string RequestItemID = ID.ToString();
                var para = new Dictionary<string, string> {
                    { "RequestItemID", RequestItemID },
                };

                string sql = @"insert into dbo.TblStockOut (ActivateCustomerID, CustomerID, ItemID, DateOut, UM, QTY, CostPrice, StatusID, 
InvoiceNumber, [Type], LocationID, ToWarehouseID, StockOutType, SystemID, WarehouseID, UomID, RequestItemID, SerialNumberID, SerialNumber, RequestItemDetailID)
select ir.RequestItemID ActivateCustomerID, ir.ReceivedUserID CustomerID, id.ItemID, getdate() DateOut, 
id.Unit UM, id.QTY, id.CostPrice, 80 StatusID, ir.RequestApprovalNo InvoiceNumber, 'Stock Out' [Type], id.LocationID, id.ToWarehouseID,
'Stock Out By Staff' StockOutType, id.SystemID, id.WarehouseID, id.UomID, id.RequestItemID, id.SerialNumberID, id.SerialNumber, id.RequestItemDetailID
from dbo.TblItem_Request ir
inner join dbo.TblItem_Request_Detail id on id.RequestItemID = ir.RequestItemID
where ir.RequestItemID = @RequestItemID;";
                sql += @"insert into dbo.TblStockInDetail(ActivateCustomerID, StockOutID, ItemID, ItemName, ItemDescription, [Type], StockOutDate, CustomerID, InvoiceNumber, StatusID, StockOutType, SystemID, WarehouseID, UomID, RequestItemID)
select distinct so.RequestItemID ActivateCustomerID, so.StockOutID, so.ItemID, i.ItemName, i.ItemDescription, 'Stock Out' [Type], getdate() StockOutDate, so.CustomerID, so.InvoiceNumber, 80 StatusID,
'Stock Out By Staff' StockOutType, so.SystemID, so.WarehouseID, so.UomID, so.RequestItemID
from dbo.TblStockOut so
inner join dbo.TblItem i on i.ItemID = so.ItemID
where so.RequestItemID = @RequestItemID;
            insert into dbo.TblStockInDetail(ActivateCustomerID, StockOutID, ItemID, ItemName, ItemDescription, [Type], Reason, StockInDate, CustomerID, InvoiceNumber, StatusID, StockOutType, SystemID, WarehouseID, UomID, RequestItemID)
select distinct so.RequestItemID ActivateCustomerID, so.StockOutID, so.ItemID, i.ItemName, i.ItemDescription, 'Stock Transfer' [Type],'Stock Request' Reason, getdate() StockInDate, so.CustomerID, so.InvoiceNumber, 80 StatusID,
'Stock Out By Staff' StockOutType, so.SystemID, so.WarehouseID, so.UomID, so.RequestItemID
from dbo.TblStockOut so
inner join dbo.TblItem i on i.ItemID = so.ItemID
inner join dbo.TblItem_Request_Detail id on id.RequestItemID = so.RequestItemID and id.ItemID = so.ItemID
where id.IsSpare = 'Yes' and so.RequestItemID = @RequestItemID;";
                var result = html.SaveChangeData(sql, false, para);

                if (result.Result == ResultType.Successful)
                {
                    UpdateQtyCalStockInDetail(RequestItemID);
                    UpdateSerialNumberStatus(RequestItemID);
                    UpdateQtyPartialOut(RequestItemID);
                    SaveItemSerialStockIn(RequestItemID);
                }
                return Json(
                    new
                    {
                        Result = ResultType.Successful,
                        Message = result.Message
                    }
                );

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Result = ResultType.Failed,
                        Message = ex.Message
                    }
                );

            }
        }
        public void UpdateQtyCalStockInDetail(string RequestItemID, string Type = "")
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = @"";

            sql += @"select * from dbo.TblStockOut 
where RequestItemID = @RequestItemID";
            para.Add("RequestItemID", RequestItemID.ToString());

            ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int ItemID = int.Parse(dr["ItemID"].ToString());
                    int UomID = int.Parse(dr["UomID"].ToString());
                    double QTY = double.Parse(dr["QTY"].ToString());
                    double CostPrice = double.Parse(dr["CostPrice"].ToString());
                    int WarehouseID = int.Parse(dr["WarehouseID"].ToString());
                    int LocationID = int.Parse(dr["LocationID"].ToString());
                    int ToWarehouseID = int.Parse(dr["ToWarehouseID"].ToString());
                    int RequestItemDetailID = int.Parse(dr["RequestItemDetailID"].ToString());
                    int StockOutID = int.Parse(dr["StockOutID"].ToString());
                    string UM = dr["UM"].ToString();

                    var data = html.GetUnitCal(ItemID, UomID, QTY, 0);

                    para.Clear();
                    para.Add("QtyCal", data["QtyCal"]);
                    para.Add("QtyCalOut", data["QtyCal"]);
                    para.Add("RequestItemID", RequestItemID);
                    para.Add("ItemID", ItemID.ToString());
                    para.Add("QTYOUT", QTY.ToString());
                    para.Add("CostPrice", CostPrice.ToString());
                    para.Add("WarehouseID", WarehouseID.ToString());
                    para.Add("LocationID", LocationID.ToString());
                    para.Add("ToWarehouseID", ToWarehouseID.ToString());
                    para.Add("RequestItemDetailID", RequestItemDetailID.ToString());
                    para.Add("StockOutID", StockOutID.ToString());
                    para.Add("UM", UM);

                    sql = Type == "Stock Return"
                        ? @"update dbo.TblStockInDetail set QtyCal = @QtyCal, CostPrice = @CostPrice, WarehouseID = @WarehouseID, LocationID = @LocationID, UM = @UM
where RequestItemID = @RequestItemID and ItemID = @ItemID;"
                        : @"update dbo.TblStockOut set QtyCalOut = @QtyCalOut
where RequestItemID = @RequestItemID and ItemID = @ItemID and RequestItemDetailID = @RequestItemDetailID;
update dbo.TblStockInDetail set QtyCalOut = @QtyCalOut, QTYOUT = @QTYOUT, CostPrice = @CostPrice, WarehouseID = @WarehouseID, LocationID = @LocationID, UM = @UM 
where RequestItemID = @RequestItemID and ItemID = @ItemID and Type = 'Stock Out' and StockOutID = @StockOutID;
update dbo.TblStockInDetail set QtyCal = @QtyCalOut, QTY = @QTYOUT, CostPrice = @CostPrice, WarehouseID = @WarehouseID, LocationID = @ToWarehouseID, UM = @UM 
where RequestItemID = @RequestItemID and ItemID = @ItemID and Type = 'Stock Transfer' and StockOutID = @StockOutID;";
                    html.SaveChangeData(sql, false, para);
                }

            }

        }
        public void UpdateSerialNumberStatus(string RequestItemID)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = @"select ItemID, SerialNumber from dbo.TblItem_Request_Detail 
where IsStockOut='Yes' and RequestItemID = @RequestItemID";
            para.Add("RequestItemID", RequestItemID);

            ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int ItemID = int.Parse(dr["ItemID"].ToString());
                    string SerialNumber = dr["SerialNumber"].ToString();

                    sql = @"update dbo.TblItemSerialDetail set IsAvailable = 0, TransactionType = 'Request Stock Out'
where ItemID =  @ItemID and SerialNumber = @SerialNumber;
";
                    para.Clear();
                    para.Add("ItemID", ItemID.ToString());
                    para.Add("SerialNumber", SerialNumber);
                    html.SaveChangeData(sql, false, para);
                }

            }
        }
        public void UpdateQtyPartialOut(string requestItemId)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = "";
            sql += @"select ir.ReferenceRequestItemID, rd.ItemID, sum(rd.QTY) TotalQtyOut 
from dbo.TblItem_Request ir
inner join dbo.TblItem_Request_Detail rd on rd.RequestItemID = ir.RequestItemID
where ir.RequestItemID = @RequestItemID
group by ir.ReferenceRequestItemID, rd.ItemID";
            para.Add("RequestItemID", requestItemId);

            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int refRequestItemId = int.Parse(dr["ReferenceRequestItemID"].ToString());
                    int itemId = int.Parse(dr["ItemID"].ToString());
                    double totalQtyOut = double.Parse(dr["TotalQtyOut"].ToString());
                    para.Clear();
                    para.Add("RefRequestItemID", refRequestItemId.ToString());
                    para.Add("ItemID", itemId.ToString());

                    sql = @"select isnull(QtyOut, 0) QtyOut from dbo.TblItem_Request_Detail where RequestItemID = @RefRequestItemID and ItemID = @ItemID";
                    ds = html.LoadData(sql, false, para);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        var TQtyOut = double.Parse(dr["TotalQtyOut"].ToString()) + double.Parse(ds.Tables[0].Rows[0]["QtyOut"].ToString());
                        para.Add("TQtyOut", TQtyOut.ToString());

                        sql = @"update dbo.TblItem_Request_Detail set QtyOut = @TQtyOut
                        where RequestItemID = @RefRequestItemID and ItemID = @ItemID";

                        html.SaveChangeData(sql, false, para);
                    }

                }

            }
        }
        public void SaveItemSerialStockIn(string requestItemId)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "RequestItemID", requestItemId }
            };
            string sql = @"select StockOutID, ItemID, SerialNumberID, SerialNumber 
from dbo.TblStockOut 
where RequestItemID = @RequestItemID";
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    para.Clear();
                    para.Add("StockOutID", dr["StockOutID"].ToString());
                    para.Add("ItemID", dr["ItemID"].ToString());
                    para.Add("SerialNumberID", dr["SerialNumberID"].ToString());
                    para.Add("SerialNumber", dr["SerialNumber"].ToString());
                    if (!string.IsNullOrEmpty(dr["SerialNumber"].ToString()))
                    {
                        sql = @"update dbo.TblStockInDetail set SerialNumberID = @SerialNumberID, SerialNumber = @SerialNumber
where StockOutID = @StockOutID and ItemID = @ItemID";
                        html.SaveChangeData(sql, false, para);
                    }
                }
            }
        }
        [Authorize]
        public JsonResult ShowRequestItem(int ID)
        {
            string sql = @"select i.ItemID, i.ItemName ItemNameFull,
i.ItemCode, i.SerialNumber, i.ItemSize, (id.QTY - isnull(id.QtyOut, 0)) QTY, id.Unit, id.Remark, ir.RequestName,
ir.RequestName ReceivedBy, ir.RequestNameUserID ReceivedUserID, ir.RequestNameTelegram ReceivedTelegaramUserName, ir.Address, ir.Department, t.CustomerType Block, p.ProjectName
from dbo.TblItem_Request ir
inner join dbo.TblItem_Request_Detail id on id.RequestItemID = ir.RequestItemID
inner join dbo.TblItem i on i.ItemID = id.ItemID
left join dbo.TblCustomerType t on t.CustomerTypeID = id.BlockID
left join dbo.TblProject p on p.ProjectID = id.ProjectID
where ir.RequestMode = 'Item Request' and ((ir.NeedToApprovalUserID = 0 and ir.Status = 'Pending') 
        or (ir.NeedToApprovalUserID <> 0 and ir.Status = 'Approved')) and ir.RequestItemID = " + ID;
            HTML html = new HTML();
            return Json(html.JsonData(sql), JsonRequestBehavior.AllowGet);
        }

        public void SaveChangeRequestApprovalItem(int ID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = "";
            sql += @"select ReferenceRequestItemID from dbo.TblItem_Request where RequestItemID = @RequestItemID";
            para.Add("RequestItemID", ID.ToString());
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                int refId = int.Parse(ds.Tables[0].Rows[0]["ReferenceRequestItemID"].ToString());
                para.Add("RefID", refId.ToString());
                sql = @"select RequestApprovalNo, RequestDate, NeedToApproval, NeedToApprovalUserID, NeedToApprovalTelegram, RequestName, RequestPhone, RequestNameUserID, RequestNameTelegram from dbo.TblItem_Request where RequestItemID = @RefID";
                DataSet ds2 = html.LoadData(sql, false, para);
                //para.Add("RequestApprovalNo", ds2.Tables[0].Rows[0]["RequestApprovalNo"].ToString());
                if (ds2.Tables[0].Rows.Count > 0)
                {
                    para.Add("RequestDate", ds2.Tables[0].Rows[0]["RequestDate"].ToString());
                    para.Add("NeedToApproval", ds2.Tables[0].Rows[0]["NeedToApproval"].ToString());
                    para.Add("NeedToApprovalUserID", ds2.Tables[0].Rows[0]["NeedToApprovalUserID"].ToString());
                    para.Add("NeedToApprovalTelegram", ds2.Tables[0].Rows[0]["NeedToApprovalTelegram"].ToString());
                    para.Add("RequestName", ds2.Tables[0].Rows[0]["RequestName"].ToString());
                    para.Add("RequestPhone", ds2.Tables[0].Rows[0]["RequestPhone"].ToString());
                    para.Add("RequestNameUserID", ds2.Tables[0].Rows[0]["RequestNameUserID"].ToString());
                    para.Add("RequestNameTelegram", ds2.Tables[0].Rows[0]["RequestNameTelegram"].ToString());

                    sql = @"update dbo.TblItem_Request set IsRequestApprovalItemOut = 0 where RequestItemID = @RefID;
update dbo.TblItem_Request set RequestDate = @RequestDate, NeedToApproval = @NeedToApproval, NeedToApprovalUserID =@NeedToApprovalUserID, NeedToApprovalTelegram = @NeedToApprovalTelegram, RequestName = @RequestName, RequestPhone = @RequestPhone, RequestNameUserID = @RequestNameUserID, RequestNameTelegram = @RequestNameTelegram
where RequestItemID = @RequestItemID";
                    html.SaveChangeData(sql, false, para);
                }
            }
        }
        public ActionResult SearchItemByBarcode()
        {
            HTML html = new HTML();
            string SQL = "select top 10 i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName from dbo.TblItem i where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName";
            ViewBag.AutoCompleteSQL = EncryAndDecry.Encryp(SQL);

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
            string cbo = html.LoadDataToCBO("CboCategories", EncryAndDecry.Encryp(sqlStockName), EncryAndDecry.Encryp("StockNameID"), EncryAndDecry.Encryp("StockName"), "", false, false, true, false, false, "", false, false, false, false);
            ViewBag.DrawCategories = cbo;
            string sql = @"select distinct b.ItemBrandID, b.ItemBrand from dbo.TblItem i
inner join dbo.TblItemBrand b on b.ItemBrandID = i.ItemBrandID
where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + @"
order by b.ItemBrand";
            cbo = html.LoadDataToCBO("CboItemBrand", EncryAndDecry.Encryp(sql), EncryAndDecry.Encryp("ItemBrandID"), EncryAndDecry.Encryp("ItemBrand"), "", false, false, true, false, false, "", false, false, false, false);
            ViewBag.DrawItemBrand = cbo;

            //			sql = @"select i.ItemID, i.ItemCode, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName, i.ItemImage
            //from dbo.TblItem i where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID;

            //			ViewBag.Item = html.LoadData(sql).Tables[0];

            return View();
        }
        public string filterProducts(string keyword, string Categories, string ItemBrand)
        {
            HTML html = new HTML();
            string sql = "SP_FILTER_PRODUCTS";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"ItemID",keyword.ToLower()},
                {"Categories",Categories},
                {"ItemBrand",ItemBrand},
            };
            return html.JsonData(sql, true, para, true);
        }
        public DataSet ItemDetail(int ItemID)
        {
            HTML html = new HTML();
            string sql = @"select sd.ItemID, isnull(sd.ItemExpirationDate, '') [ExpireDate],
isnull(sum(isnull(sd.QTY,0)),0) - isnull(sum(isnull(sd.QTYOUT,0)),0) QTY 
from dbo.TblStockInDetail sd
where sd.ItemID = " + ItemID + @"
group by sd.ItemID, sd.ItemExpirationDate";
            return html.LoadData(sql);
        }
        public string ItemExpiration(int ItemID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            string sql = @"select sd.ItemID, sd.ItemExpirationDate [ExpireDate],
SUM(ISNULL(sd.QtyCal, 0)) QTY 
from dbo.TblStockInDetail sd
where sd.ItemID = " + ItemID + @"
group by sd.ItemID, sd.ItemExpirationDate";
            sql = @"select se.ItemID, se.ExpireDate, (sum(isnull(se.QTYIN, 0)) - sum(isnull(se.QTYOUT, 0))) as QTY, se.Barcode
from dbo.TblStockExpireDate se 
where se.SystemID = @SystemID and se.WarehouseID = @WarehouseID and se.ItemID = @ItemID
group by se.ItemID, se.ExpireDate, se.Barcode
having (sum(isnull(se.QTYIN, 0)) - sum(isnull(se.QTYOUT, 0))) > 0
order by se.ExpireDate";
            return html.JsonData(sql, false, para);
        }
        public string LoadItemDetail(int ItemID)
        {
            HTML html = new HTML();
            string sql = @"SELECT case when value <> '' then isnull('"" + html.ServerRoot + @""'+i.DestinationFolder + value, '') else '' end ItemImage,
i.ItemName + case when i.ItemCode <> '' then ' - ' + i.ItemCode else '' end ItemName, i.Barcode, i.SellingPrice Price,
i.ItemName [Name],i.ItemCode, i.ItemSize, i.ItemBrand, i.UM, i.SerialNumber, i.Model, i.ReorderPoint,
(select STUFF((SELECT ',' + sn.StockName from dbo.TblStockName sn
where sn.StockNameID 
in(select Data from dbo.Split(i.StockNameID,','))
FOR XML PATH('')), 1, 1, '')) StockName, ca.AccountName, cae.AccountName ExpenseAccount, cas.AccountName IncomeAccount,
format(i.CreateDate, 'dd-MM-yy HH:mm:ss tt') CreateDate, i.CreateBy, i.ItemDescription,
i.CostPrice, i.SellingPrice
FROM TblItem i
inner join dbo.TblChartOfAccount ca on ca.ChartOfAccountID = i.InventoryChartOfAccountID
inner join dbo.TblChartOfAccount cae on cae.ChartOfAccountID = i.PurchaseChartOfAccountID
inner join dbo.TblChartOfAccount cas on cas.ChartOfAccountID = i.SaleChartOfAccountID
CROSS APPLY STRING_SPLIT(isnull(i.ItemImage,''), '~')
where i.ItemID = @ItemID

select (sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))) StockOnHand, w.WID, w.WarehouseName
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblUserWarehouse w on w.WID = sd.LocationID
where i.ItemID = @ItemID
group by w.WID, w.WarehouseName;

select (sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))) StockOnHand
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblUserWarehouse w on w.WID = sd.LocationID
where i.ItemID = @ItemID;

select sd.ItemID, isnull(sd.ItemExpirationDate, '') [ExpireDate],
isnull(sum(isnull(sd.QTY,0)),0) - isnull(sum(isnull(sd.QTYOUT,0)),0) QTY 
from dbo.TblStockInDetail sd
where sd.ItemID = @ItemID
group by sd.ItemID, sd.ItemExpirationDate";

            Dictionary<string, string> para = new Dictionary<string, string> {
                {
                    "ItemID", ItemID.ToString()
                }
            };
            return html.JsonData(sql, false, para, true);
        }
        public string GetConvertUnit(int ItemID, double Qty)
        {
            HTML html = new HTML();
            string strQtyUnit = html.ConvertUnit(ItemID, Qty);
            return strQtyUnit;
        }
        public ActionResult ItemSerial()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Item Serial"
            };
            if (html.HasPermission("Add Item Serial"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item Serial"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item Serial"))
                html.AllowDelete = false;
            html.TableName = "TblItemSerial";
            html.AddButton = "Add Serial Number";
            html.AddURL = "Items/AddItemSerial";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "sr.CreateDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "IsAvailable",
                Label = "Status:",
                DBField = "srd.IsAvailable",
                SQL_CBO = "select distinct IsAvailable [IsAvailableVal], case when IsAvailable = 1 then 'Available' else 'Unavailable' end IsAvailable from dbo.TblItemSerialDetail",
                DisplayText = "IsAvailable",
                Value = "IsAvailableVal",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                SQL = "select distinct i.ItemName from dbo.TblItem i where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + "%' order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("i.ItemName") + "," + EncryAndDecry.Encryp("i.ItemCode") + "," + EncryAndDecry.Encryp("srd.SerialNumber"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Code",
                SQL = "select distinct i.ItemCode from dbo.TblItem i where lower(i.ItemCode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + "%' order by i.ItemCode",
                RadioValue = EncryAndDecry.Encryp("i.ItemName") + "," + EncryAndDecry.Encryp("i.ItemCode") + "," + EncryAndDecry.Encryp("srd.SerialNumber"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Serial Number",
                SQL = "select distinct s.SerialNumber from dbo.TblItemSerialDetail s where lower(s.SerialNumber) like @SearchText;",
                RadioValue = EncryAndDecry.Encryp("i.ItemName") + "," + EncryAndDecry.Encryp("i.ItemCode") + "," + EncryAndDecry.Encryp("srd.SerialNumber"),
                ISAutoComplete = true,
            });

            string sql = $@"select ROW_NUMBER() Over(PARTITION by i.ItemName + (case when isnull(i.ItemCode, '') <> '' then '-' + i.ItemCode else '' end) order by srd.SerialNumber desc) as [No], sr.ItemSerialID, i.ItemCode, Format(sr.CreateDate, 'dd-MM-yyyy') CreateDate,'<span class=""text-primary"" style=""cursor: pointer;"" onclick=""ShowItemSerial('+convert(varchar, sr.ItemSerialID)+', '''+sr.ReceiveNoteNumber+''')"">'+sr.ReceiveNoteNumber+'</span>' ReceiveNoteNumber1, sr.ReceiveNoteNumber ReceiveNoteNumber, sr.UserID, u.FullName [CreateBy], 
srd.SerialNumber, case when srd.IsAvailable = 1 then '<span class=""badge badge-pill badge-primary"">Available</span>' else '<span class=""badge badge-pill badge-danger"">Used</span>' end [Status], srd.ItemID, i.ItemName + (case when isnull(i.ItemCode, '') <> '' then '-' + i.ItemCode else '' end) as ItemName, srd.QTY, srd.UOM
from dbo.TblItemSerial sr
inner join dbo.TblItemSerialDetail srd on srd.ItemSerialID = sr.ItemSerialID
inner join dbo.TblItem i on i.ItemID = srd.ItemID
inner join dbo.TblUserLogin u on u.UserLoginID = sr.UserID
where i.SystemID = {html.SystemID} and i.WarehouseID = {html.WarehouseID}";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Receive Note Number#", FieldName = "ReceiveNoteNumber", IsHiddenField=true },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true},
                new GridField() { Header = "Item Name", FieldName = "ItemName", IsGroupByField = true},
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber"},
                new GridField() { Header = "QTY", FieldName = "QTY", TextAlign = "right"},
                new GridField() { Header = "UOM", FieldName = "UOM", TextAlign = "center"},
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center"},
                new GridField() { Header = "Create Date", FieldName = "CreateDate",TextAlign="center" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", TextAlign ="center" },
                //new GridField() { Header = "Action", FieldName = "ItemSerialID", TextAlign = "Center",  IsModifyField = true,
                //    DeleteWithTableName="TblItemSerialDetail",
                //    HideEditOnActionNameNotMatch = true, //(AdminEdit==false? true:false),
                //    ShowActionNameOnConditionField ="IsAvailable",
                //    ShowActionNameOnConditionValue="60, 91, 90, 92",
                //    HideDeteleOnNotMatch=true
                //}
            };
            html.ShowPaging = false;
            html.DialogBoxWidth = "1200";

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult AddItemSerial(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "AddItemSerial",
                FieldIDFocus = "TxtReceiveNoteNumber",
                TableName = "TblItemSerial",
                PrimaryField = "ItemSerialID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Receive Note Number:",
            //    ID = "ReceiveNoteNumber",
            //    DBField = "ReceiveNoteNumber",
            //    Required = true,
            //    ISAutoComplete = true,
            //    HiddenFeild = "StockInID",
            //    SQL_AutoComplete = "select distinct top 10 StockInID, ReceiveNoteNumber from dbo.TblStockIn where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "order by ReceiveNoteNumber desc",
            //    SQLTypeaheadCallBack = "select i.ItemID ItemName_ItemID, i.ItemName from dbo.TblItem i inner join dbo.TblStockInDetail sd on i.ItemID = sd.ItemID where sd.StockInID = @StockInID",
            //    DisplayOnFields = "ItemName_ItemID",
            //    CallBackWithControllValueID = "ReceiveNoteNumber_StockInID",
            //    isApplyTypeaheadOnCallBack = true,
            //    CallBackToControlID = "ItemName",
            //    DisplayOnFieldsTypeahead = "ItemName_ItemID, ItemName",
            //    FieldName = "StockInID",
            //    CallBackDisplayText = "ItemName",
            //});

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Receive Note Number:",
                ID = "TxtReceiveNoteNumber",
                DBField = "StockInID",
                SQL_CBO = "select StockInID, ReceiveNoteNumber from dbo.TblStockIn where ReceiveNoteNumber is not null and StatusID in (79, 83) and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "order by ReceiveNoteNumber desc",
                DisplayText = "ReceiveNoteNumber",
                Value = "StockInID",
                //Infinity = false,
                Required = true,
                placeholder = "Select receive note number",
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Select Item:",
                ID = "ItemName",
                //DBField = "ItemName",
                //Required = true,
                HiddenField = "ItemID",
            });


            html.SaveChangeAndClose = true;
            html.SaveChangeCallbackFunction = "addSerialNumberToDb()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public JsonResult LoadItem(int ItemID, int StockInID)
        {
            HTML html = new HTML();
            string SQL = @"select i.ItemID, i.ItemName, sd.QTY, sd.UM, sd.UomID 
from dbo.TblItem i 
inner join dbo.TblStockInDetail sd on i.ItemID = sd.ItemID 
where i.ItemID = @ItemID and sd.StockInID = @StockInID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() },
                { "StockInID", StockInID.ToString() },
            };
            var dataItem = html.JsonData(SQL, false, para);

            SQL = @"select distinct srd.ItemID, srd.SerialNumber 
from dbo.TblItemSerialDetail srd 
where ItemID = @ItemID and StockInID = @StockInID and IsAvailable = 1";
            var dataExitsSerialNumber = html.JsonData(SQL, false, para);
            return Json(new { dataItem = dataItem, dataSerialNumber = dataExitsSerialNumber }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult JqueryProduct(int stockInId, string term)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", stockInId.ToString() },
                { "term", "%" + term.ToLower() + "%" }
            };
            string sql = term != ""
                ? @"select i.ItemID, i.ItemName 
from dbo.TblItem i 
inner join dbo.TblStockInDetail sd on i.ItemID = sd.ItemID 
where sd.StockInID = @StockInID and lower(i.ItemName) like @term and StatusID in (79,83)"
                : @"select i.ItemID, i.ItemName 
from dbo.TblItem i 
inner join dbo.TblStockInDetail sd on i.ItemID = sd.ItemID 
where sd.StockInID = @StockInID and StatusID in (79,83)";
            var data = html.JsonData(sql, false, para);

            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult JquerySerialNumberByItem(int itemId, int WarehouseID, string term)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", itemId.ToString() },
                { "WID", WarehouseID.ToString() },
                { "Term", term.ToString() }
            };

            string sql = "SP_FILTER_ITEM_SERIAL";
            var data = html.JsonData(sql, true, para);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQohBySerialNumber(int ItemID, int SerialNumberID, string SerialNumber, int WarehouseID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() },
                { "SerialNumberID", SerialNumberID.ToString() },
                { "SerialNumber", SerialNumber.ToString() },
                { "WarehouseID", WarehouseID.ToString() }
            };
            string sql = @"SELECT 
                    i.ItemID,
                    i.ItemName,
                    srd.ItemSerialDetailID AS SerialNumberID,
                    srd.SerialNumber,
                    CASE 
                        WHEN srd.IsAvailable = 1 THEN 'In Stock'
                        ELSE 'Out of Stock'
                    END AS StockStatus,
                    CASE 
                        WHEN srd.IsAvailable = 1 THEN '1'
                        ELSE '0'
                    END AS QTY,
                    COALESCE(
                        (SELECT TOP 1 sd.LocationID 
                         FROM dbo.TblStockInDetail sd 
                         WHERE sd.ItemID = i.ItemID 
                         AND sd.SerialNumber = srd.SerialNumber
                         ORDER BY sd.StockInDetailID DESC),
                        sd.LocationID
                    ) AS CurrentLocationID
                INTO #temp
                FROM dbo.TblItem i
                INNER JOIN dbo.TblItemSerialDetail srd ON srd.ItemID = i.ItemID
                INNER JOIN dbo.TblStockInDetail sd ON sd.StockInID = srd.StockInID 
                    AND sd.ItemID = srd.ItemID
                WHERE i.ItemID = @ItemID
                ORDER BY srd.SerialNumber;

                -- Step 2: Select from the temporary table with a filter on CurrentLocationID

                SELECT *
                FROM #temp
                WHERE SerialNumberID = @SerialNumberID and CurrentLocationID = @WarehouseID;

                -- Optional: Drop the temporary table when done
                DROP TABLE #temp;
            ";

            var data = html.JsonData(sql, false, para);

            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveChangeSerialNumber(int stockInId, string receiveNoteNumber, int itemSerialId, dynamic[] data)
        {
            HTML html = new HTML();
            dynamic dataSerialNumber = new JavaScriptSerializer().DeserializeObject(data[0]);
            Dictionary<string, string> para = new Dictionary<string, string>();
            SqlResult result = new SqlResult();
            string sql = @"update dbo.TblItemSerial set ReceiveNoteNumber = @ReceiveNoteNumber where ItemSerialID = @ItemSerialID";
            para.Add("ReceiveNoteNumber", receiveNoteNumber);
            para.Add("StockInID", stockInId.ToString());
            para.Add("ItemSerialID", itemSerialId.ToString());
            html.SaveChangeData(sql, false, para);

            foreach (dynamic dataRow in dataSerialNumber)
            {
                int itemId = int.Parse(dataRow["itemId"].ToString());
                string itemName = dataRow["itemName"].ToString();
                int uomId = int.Parse(dataRow["uomId"].ToString());
                string uomName = dataRow["uomName"].ToString();
                dynamic serialNumbers = dataRow["serialNumber"];

                foreach (dynamic lot in serialNumbers)
                {
                    //If lot has already
                    para.Clear();
                    para.Add("SerialNumber", lot);
                    para.Add("ItemID", itemId.ToString());
                    sql = @"select * from dbo.TblItemSerialDetail where SerialNumber = @SerialNumber and ItemID = @ItemID";
                    DataSet ds = html.LoadData(sql, false, para);

                    if (lot != "" && ds.Tables[0].Rows.Count == 0)
                    {
                        sql = @"insert into dbo.TblItemSerialDetail (ItemSerialID, StockInID, SerialNumber, ItemID, ItemName, QTY, UomID, UOM)
values (" + itemSerialId + ", " + stockInId + ",N'" + lot + "', " + itemId + ", N'" + itemName + "', 1, " + uomId + ", N'" + uomName + "')";
                        result = html.SaveChangeData(sql, false, false);
                    }
                }
            }

            return Json(new { Resutl = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckExistSerialNumber(int itemId)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();
            SqlResult result = new SqlResult();
            para.Add("ItemID", itemId.ToString());
            string sql = @"select SerialNumber from dbo.TblItemSerialDetail where ItemID = @ItemID ";
            var data = html.JsonData(sql, false, para);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowFormItemSerial(int itemSerialID)
        {
            return View();
        }
        public string LoadSerialNumberByItem(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "sr.CreateDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Status:",
                DBField = "IsAvailable",
                SQL_CBO = "select 'Available' Status, '1' IsAvailable union all select 'Used' Status, '0' IsAvailable",
                DisplayText = "Status",
                Value = "IsAvailable",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
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
                DefaultCheckBok = true,
                RadioValue = EncryAndDecry.Encryp("srd.SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select row_number() over(order by srd.IsAvailable desc) as [No], i.ItemID, srd.SerialNumber, srd.QTY, srd.UOM, format(sr.CreateDate, 'dd-MM-yyyy') as CreateDate,case when isnull(srd.TransactionType, '') <> '' then srd.TransactionType else 'New Stock' end TransactionType,
case when srd.IsAvailable = 1 then '<span class=""badge badge-pill badge-primary"">Available</span>' else '<span class=""badge badge-pill badge-danger"">Used</span>' end [Status]
from dbo.TblItem i
left join dbo.TblItemSerialDetail srd on srd.ItemID = i.ItemID
inner join dbo.TblItemSerial sr on sr.ItemSerialID = srd.ItemSerialID
where i.ItemID = " + ID;

            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "center",},
                new GridField() { Header = "Lot/Serial Number", FieldName = "SerialNumber", TextAlign = "center" },
                new GridField() { Header = "Status", FieldName = "Status" , TextAlign = "center", TotalLabel="Total:"},
                new GridField() { Header = "Qty", FieldName = "QTY", TextAlign = "right",Format="n0", IsTotalFooter=true,},
                new GridField() { Header = "UOM", FieldName = "UOM", TextAlign = "center"},
                new GridField() { Header = "Create Date", FieldName = "CreateDate", TextAlign  = "center"},
            };
            html.TblID = "TblItemSerial";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public string LoadRequestStockOut(int ID)
        {
            HTML html = new HTML
            {
                TableName = "TblItem",
                PrimaryField = "ItemID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Request Date:",
                DBField = "ir.RequestDate"
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
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
                DefaultCheckBok = true,
                RadioValue = EncryAndDecry.Encryp("sd.SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select row_number() over(order by ir.RequestNo desc) as [No], ir.RequestItemID, format(ir.RequestDate, 'dd-MM-yyyy') as RequestDate, isnull(ir.RequestNo, '') RequestNo, isnull(ir.RequestApprovalNo,'') RequestApprovalNo, 
ir.Department, ir.[Address],i.ItemName, i.ItemSize, sd.QTYOUT, sd.UM, ir.PreparedUserID RequestNameUserID, ir.RequestName, ir.RequestPhone,isnull(ir.[Status],'Pending') [Status], i.ItemCode, sd.SerialNumber,isnull(ir.RequestApprovalItemOutStatus, '') RequestApprovalItemOutStatus, i.Model, t.CustomerType Block, (select top 1 Remark from dbo.TblItem_Request_Detail where RequestItemID = sd.RequestItemID and ItemID = sd.ItemID) as Remark
from dbo.TblItem_Request ir
inner join dbo.TblStockInDetail sd on sd.RequestItemID = ir.RequestItemID
inner join dbo.TblItem i on i.ItemID = sd.ItemID
left join dbo.TblUserLogin ul on ul.UserLoginID = ir.PreparedUserID
left join dbo.TblCustomerType t on t.CustomerTypeID = (select top 1 BlockID from dbo.TblItem_Request_Detail where RequestItemID = sd.RequestItemID and ItemID = sd.ItemID)
left join dbo.TblUserLogin ua on ua.UserLoginID = ir.RequestApprovalUserID
left join dbo.TblUserLogin ur on ur.UserLoginID = ir.ReceivedUserID
where ir.IsRequestApprovalItemOut = 'True' and sd.Type = 'Stock Out' and i.ItemID = " + ID;

            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "center",},
                new GridField() { Header = "Request Date", FieldName = "RequestDate", TextAlign = "center" },
                new GridField() { Header = "Request No.", FieldName = "RequestNo", TextAlign = "center" },
                new GridField() { Header = "Approval No.", FieldName = "RequestApprovalNo", TextAlign = "center" },
                new GridField() { Header = "Item Code", FieldName = "ItemCode" },
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber", ClassName = "text-center text-red" },
                new GridField() { Header = "Block", FieldName = "Block" , ClassName = "text-center", TotalLabel="Total:"},
                new GridField() { Header = "QTYOUT", FieldName = "QTYOUT", TextAlign = "right",Format="n0", IsTotalFooter=true,},
                new GridField() { Header = "UOM", FieldName = "UM", TextAlign = "center"},
                new GridField() { Header = "Request By", FieldName = "RequestName"},
                new GridField() { Header = "Reason", FieldName = "Remark"},
            };
            html.TblID = "TblStockRequest";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public ActionResult ItemExpiredFormHtml(string sql)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>();

            // Load the template file safely using a `using` block
            string filePath = html.httpContext.Server.MapPath("~/Templates/InvoiceItemExpire.html");
            string templateContent;

            using (StreamReader reader = new StreamReader(filePath))
            {
                templateContent = reader.ReadToEnd();
            }

            ds = html.LoadData(sql);

            // Table header
            string tableHeader = @"
                <tr>
                    <th class=""text-center"" width=""10%"">No</th>
                    <th class=""text-center"" width=""10%"">Item Code</th>
                    <th class=""text-center"" width=""45%"">Item Name</th>
                    <th class=""text-center"" width="""">QTYIN</th>
                    <th class=""text-center"" width="""">Expired Date</th>
                    <th class=""text-nowrap text-center"" width="""">Warehouse</th>
                </tr>
            ";

            // Replace the placeholder in the template with the table header
            templateContent = templateContent.Replace("{TblHead}", tableHeader);

            StringBuilder sb = new StringBuilder();
            int i = 0;

            // Loop through the dataset to build the table body
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DateTime expireDate = DateTime.Parse(dr["ItemExpirationDate"].ToString());
                i++;

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td align=\"center\">{i}</td>");
                sb.AppendLine($"<td class=\"text-nowrap-pre\">{HttpUtility.HtmlEncode(dr["ItemCode"])}</td>");
                sb.AppendLine($"<td class=\"text-nowrap-pre\">{HttpUtility.HtmlEncode(dr["ItemName"])}</td>");
                sb.AppendLine($"<td class=\"text-nowrap-pre\">{HttpUtility.HtmlEncode(dr["QTY"]) + " " + HttpUtility.HtmlEncode(dr["UM"])}</td>");
                sb.AppendLine($"<td class=\"text-nowrap-pre\">{expireDate:yyyy-MM-dd}</td>");
                sb.AppendLine($"<td class=\"text-nowrap-pre\">{HttpUtility.HtmlEncode(dr["WarehouseName"])}</td>");
                sb.AppendLine("</tr>");
            }

            // Replace the placeholder in the template with the table body
            templateContent = templateContent.Replace("{TblBody}", sb.ToString());

            // Load and parse the HTML content
            var doc = new HtmlDocument
            {
                OptionDefaultStreamEncoding = Encoding.UTF8
            };
            doc.LoadHtml(templateContent);

            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                sb.Clear();
                sb.Append(bodyNode.InnerHtml);
                ViewBag.Data = sb.ToString();
            }

            return View();
        }
        public void SaveChangeStockExpire(int TranID, string TranType = "")
        {
            InventoryExpirationDateController expireDate = new InventoryExpirationDateController();
            expireDate.StockExpireOut(TranID, TranType);
        }
    }
}