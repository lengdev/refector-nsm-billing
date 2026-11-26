using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace AllReportAPi.Services
{
    public interface IInventoryReportService
    {
        IQueryable<StockSummaryDto> StockSummary(ReportFilter filter);
        IQueryable<StockMovementDto> StockMovement(ReportFilter filter);

        IQueryable<StockValueDto> StockValueSummary(ReportFilter filter);
        IQueryable<StockMoveDto> StockValueDetail(ReportFilter filter);
        IQueryable<StockReconDto> StockReconciliation(ReportFilter filter);
        IQueryable<CategoryValueDto> StockValueVariance(ReportFilter filter);

        IQueryable<FifoLayerDto> FifoLayerDetail(ReportFilter filter);
        IQueryable<StockValueDto> FifoLayerBalance(ReportFilter filter);

        IQueryable<TransferDto> StockTransferDetail(ReportFilter filter);
        IQueryable<TransferSummaryDto> StockTransferSummary(ReportFilter filter);

        IQueryable<ConsignmentDto> ConsignmentBalance(ReportFilter filter);
        IQueryable<ConsignmentMoveDto> ConsignmentMovement(ReportFilter filter);
        IQueryable<ConsignmentMoveDto> ConsignmentSettlement(ReportFilter filter);
        IQueryable<ConsignmentAgingDto> ConsignmentAging(ReportFilter filter);

        IQueryable<SerialDto> SerialRegister(ReportFilter filter);
        IQueryable<LotDto> LotBalance(ReportFilter filter);
        IQueryable<LotAgingDto> LotExpiryAging(ReportFilter filter);
        IQueryable<LotReconDto> LotReconciliation(ReportFilter filter);
        IQueryable<LotMoveDto> LotMovementDetail(ReportFilter filter);

        IQueryable<ReorderDto> ReorderList(ReportFilter filter);
    }

    public class InventoryReportService : IInventoryReportService
    {
        private const string TransferType = "Transfer";
        private const string ConsOut = "Consignment Out";
        private const string ConsReturn = "Consignment Return";
        private const string ConsSettle = "Consignment Settlement";

        private readonly DBContext db;
        private readonly ReportScope scope;

        public InventoryReportService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;
        }

        // ═════════════ row types ═════════════

        // what the database returns (numbers as double: EF can't cast to decimal)
        private class RawMove
        {
            public int ItemID { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string Brand { get; set; }
            public string Warehouse { get; set; }
            public string ToWarehouse { get; set; }
            public string Customer { get; set; }
            public string UM { get; set; }
            public DateTime? Date { get; set; }
            public string MoveType { get; set; }
            public string LotNo { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public double Qty { get; set; }
            public double UnitCost { get; set; }
        }

        // what the reports use (decimal, built in memory)
        private class MoveRow
        {
            public int ItemID { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string Brand { get; set; }
            public string Warehouse { get; set; }
            public string ToWarehouse { get; set; }
            public string Customer { get; set; }
            public string UM { get; set; }
            public DateTime? Date { get; set; }
            public string RefNo { get; set; }
            public string MoveType { get; set; }
            public string LotNo { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public decimal Qty { get; set; }        // signed: + in, - out
            public decimal UnitCost { get; set; }
        }

        private class StockLine
        {
            public int ItemID { get; set; }
            public string ItemCode { get; set; }
            public string Barcode { get; set; }
            public string ItemName { get; set; }
            public string ItemGroup { get; set; }
            public string UM { get; set; }
            public string Warehouse { get; set; }
            public string SerialNumber { get; set; }
            public string Type { get; set; }
            public int? StockInID { get; set; }
            public DateTime? Date { get; set; }
            public double QtyIn { get; set; }
            public double QtyOut { get; set; }
        }

        private class SerialRow
        {
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public string SerialNumber { get; set; }
            public DateTime? Expiry { get; set; }
            public string Warehouse { get; set; }
            public decimal Qty { get; set; }
            public bool Available { get; set; }
        }

        private class LedgerRow
        {
            public int? ItemID { get; set; }
            public string ItemName { get; set; }
            public string SerialNumber { get; set; }
            public decimal Qty { get; set; }
        }

        // ═════════════ data loaders ═════════════

        // asOf = true: balance reports, everything up to the "To" date.
        // asOf = false: movement inside the period.
        private List<StockLine> LoadStockLines(ReportFilter filter, bool asOf)
        {
            var q =
                from sd in db.TblStockInDetail.AsNoTracking()
                join i in db.TblItem.AsNoTracking().WhereScope(scope)
                    on sd.ItemID equals (int?)i.ItemID
                join g in db.TblItemGroup.AsNoTracking()
                    on i.ItemGroupID equals (int?)g.ItemGroupID into gs
                from g in gs.DefaultIfEmpty()
                join w in db.TblWarehouse.AsNoTracking()
                    on sd.LocationID equals (int?)w.WID into ws
                from w in ws.DefaultIfEmpty()
                where i.ItemCode != null && !i.ItemCode.Contains("N/A")
                select new StockLine
                {
                    ItemID = i.ItemID,
                    ItemCode = i.ItemCode,
                    Barcode = i.Barcode,
                    ItemName = i.ItemName,
                    ItemGroup = g.GroupName,
                    UM = db.TblItemVariant.Where(v => v.ItemID == i.ItemID)
                            .Select(v => v.UM).FirstOrDefault(),
                    Warehouse = w.WarehouseName,
                    SerialNumber = sd.SerialNumber == "N/A" ? null : sd.SerialNumber,
                    Type = sd.Type,
                    StockInID = sd.StockInID,
                    Date = sd.StockInDate,
                    QtyIn = (double?)sd.QTY ?? 0d,
                    QtyOut = (double?)sd.QTYOUT ?? 0d
                };

            if (!string.IsNullOrEmpty(filter.Category)) q = q.Where(r => r.ItemGroup == filter.Category);
            if (!string.IsNullOrEmpty(filter.Warehouse)) q = q.Where(r => r.Warehouse == filter.Warehouse);
            if (!string.IsNullOrEmpty(filter.GroupName)) q = q.Where(r => r.ItemGroup == filter.GroupName);
            if (filter.ItemID != 0) q = q.Where(r => r.ItemID == filter.ItemID);

            if (asOf)
            {
                if (filter.To.HasValue)
                {
                    var toExclusive = filter.To.Value.Date.AddDays(1);
                    q = q.Where(r => r.Date < toExclusive);
                }
            }
            else q = q.WherePeriod(r => r.Date, filter);

            return q.ToList();
        }

        private IQueryable<MoveRow> LoadMoves(ReportFilter filter, bool asOf)
        {
            var q =
                from d in db.TblStockInDetail.AsNoTracking().WhereScope(scope)
                join it in db.TblItem.AsNoTracking() on d.ItemID equals it.ItemID
                join v in db.TblItemVariant.AsNoTracking() on d.ItemVariantID equals (int?)v.ItemVariantID into vs
                from v in vs.DefaultIfEmpty()
                join w in db.TblWarehouse.AsNoTracking() on d.WarehouseID equals (int?)w.WarehouseID into ws
                from w in ws.DefaultIfEmpty()
                join u in db.TblUom.AsNoTracking() on d.UomID equals (int?)u.UomID into us
                from u in us.DefaultIfEmpty()
                join so in db.TblStockOut.AsNoTracking() on d.StockOutID equals (int?)so.StockOutID into sos
                from so in sos.DefaultIfEmpty()
                join tw in db.TblWarehouse.AsNoTracking() on so.ToWarehouseID equals (int?)tw.WarehouseID into tws
                from tw in tws.DefaultIfEmpty()
                join c in db.TblCustomer.AsNoTracking() on so.CustomerID equals (int?)c.CustomerID into cs
                from c in cs.DefaultIfEmpty()
                select new RawMove
                {
                    ItemID = it.ItemID,
                    ItemCode = it.ItemCode,
                    ItemName = v == null ? it.ItemName : it.ItemName + " - " + v.UM,
                    Category = it.StockName,
                    Brand = it.ItemBrand,
                    Warehouse = w.WarehouseName,
                    ToWarehouse = tw.WarehouseName,
                    Customer = string.IsNullOrEmpty(c.CustomerName) ? c.CustomerNameLatin : c.CustomerName,
                    UM = u.Name,
                    Date = d.StockInDate,
                    MoveType = d.Type,
                    LotNo = d.SerialNumber,
                    ExpiryDate = d.ItemExpirationDate,
                    Qty = (double?)d.QTY ?? 0d,
                    UnitCost = (double?)d.CostPrice ?? 0d
                };

            if (!string.IsNullOrEmpty(filter.Category)) q = q.Where(r => r.Category == filter.Category);
            if (!string.IsNullOrEmpty(filter.Brand)) q = q.Where(r => r.Brand == filter.Brand);
            if (!string.IsNullOrEmpty(filter.Warehouse)) q = q.Where(r => r.Warehouse == filter.Warehouse);
            if (filter.ItemID != 0) q = q.Where(r => r.ItemID == filter.ItemID);

            if (asOf)
            {
                if (filter.To.HasValue)
                {
                    var toExclusive = filter.To.Value.Date.AddDays(1);   // include the whole "To" day
                    q = q.Where(r => r.Date < toExclusive);
                }
            }
            else
            {
                q = q.WherePeriod(r => r.Date, filter);
            }

            // convert double -> decimal in memory
            return q.ToList().Select(r => new MoveRow
            {
                ItemID = r.ItemID,
                ItemCode = r.ItemCode,
                ItemName = r.ItemName,
                Category = r.Category,
                Brand = r.Brand,
                Warehouse = r.Warehouse,
                ToWarehouse = r.ToWarehouse,
                Customer = r.Customer,
                UM = r.UM,
                Date = r.Date,
                RefNo = null,                       // no reference column on StockDetail
                MoveType = r.MoveType,
                LotNo = r.LotNo,
                ExpiryDate = r.ExpiryDate,
                Qty = (decimal)r.Qty,
                UnitCost = (decimal)r.UnitCost
            }).AsQueryable();
        }

        // SerialNumber is both the serial and the lot; QTY can be more than 1.
        // withLedger = true also loads the StockDetail balance per item + serial (for reconciliation)
        private void LoadSerials(ReportFilter filter, bool withLedger,
            out List<SerialRow> serials, out List<LedgerRow> ledger)
        {
            var q =
                from sr in db.TblItemSerialDetail.AsNoTracking()
                join s in db.TblStockIn.AsNoTracking().WhereScope(scope)
                    on sr.StockInID equals (int?)s.StockInID
                join it in db.TblItem.AsNoTracking()
                    on sr.ItemID equals (int?)it.ItemID
                join w in db.TblWarehouse.AsNoTracking()
                    on s.WarehouseID equals (int?)w.WarehouseID into ws
                from w in ws.DefaultIfEmpty()
                where sr.SerialNumber != null && sr.SerialNumber != ""
                select new
                {
                    it.ItemID,
                    it.ItemName,
                    sr.SerialNumber,
                    Expiry = sr.ItemSerialExpireDate,
                    Warehouse = w.WarehouseName,
                    sr.QTY,
                    sr.IsAvailable
                };

            if (!string.IsNullOrEmpty(filter.Warehouse)) q = q.Where(r => r.Warehouse == filter.Warehouse);
            if (filter.ItemID != 0) q = q.Where(r => r.ItemID == filter.ItemID);

            serials = q.ToList().Select(r => new SerialRow
            {
                ItemID = r.ItemID,
                ItemName = r.ItemName,
                SerialNumber = r.SerialNumber,
                Expiry = r.Expiry,
                Warehouse = r.Warehouse,
                Qty = (decimal)(r.QTY ?? 0d),
                Available = r.IsAvailable == true
            }).ToList();

            ledger = new List<LedgerRow>();
            if (withLedger)
            {
                ledger = (from d in db.TblStockInDetail.AsNoTracking().WhereScope(scope)
                          join it in db.TblItem.AsNoTracking() on d.ItemID equals it.ItemID
                          where d.SerialNumber != null && d.SerialNumber != ""
                          group d by new { d.ItemID, it.ItemName, d.SerialNumber } into g
                          select new
                          {
                              g.Key.ItemID,
                              g.Key.ItemName,
                              g.Key.SerialNumber,
                              Qty = g.Sum(x => (double?)x.QTY) ?? 0d
                          })
                         .ToList()
                         .Select(x => new LedgerRow
                         {
                             ItemID = x.ItemID,
                             ItemName = x.ItemName,
                             SerialNumber = x.SerialNumber,
                             Qty = (decimal)x.Qty
                         }).ToList();
            }
        }

        private static decimal Safe(decimal a, decimal b) => b == 0m ? 0m : a / b;

        // ───────────── Stock Summary (per item, warehouse, serial) ─────────────
        public IQueryable<StockSummaryDto> StockSummary(ReportFilter filter)
        {
            return LoadStockLines(filter, true)
                .GroupBy(r => new
                {
                    r.ItemID,
                    r.ItemCode,
                    r.Barcode,
                    r.ItemName,
                    r.ItemGroup,
                    r.UM,
                    r.Warehouse,
                    r.SerialNumber
                })
                .Select(g =>
                {
                    var qtyIn = (decimal)g.Sum(x => x.QtyIn);
                    var qtyOut = (decimal)g.Sum(x => x.QtyOut);
                    var onHand = qtyIn - qtyOut;
                    return new StockSummaryDto
                    {
                        ItemID = g.Key.ItemID,
                        ItemCode = g.Key.ItemCode,
                        Barcode = g.Key.Barcode,
                        ItemName = g.Key.ItemName,
                        ItemGroup = g.Key.ItemGroup,
                        UM = g.Key.UM,
                        Warehouse = g.Key.Warehouse,
                        SerialNumber = g.Key.SerialNumber,
                        QtyIn = qtyIn,
                        QtyOut = qtyOut,
                        StockOnHand = onHand,
                        StockReturn = (decimal)g.Where(x => x.Type == "Stock Return").Sum(x => x.QtyIn),
                        Status = onHand <= 0m ? "Out of stock" : onHand < 10m ? "Nearly out of stock" : "Available stock"
                    };
                })
                .Where(x => x.QtyIn > 0m)                       // the SP keeps only rows with stock in
                .OrderBy(x => x.ItemGroup).ThenBy(x => x.ItemName).ThenBy(x => x.SerialNumber)
                .AsQueryable();
        }

        // ───────────── Stock Movement (one row per ledger line) ─────────────
        private static string MoveGroupOf(string type)
        {
            var t = (type ?? "").ToLowerInvariant();
            if (t.Contains("transfer")) return "Transfer";
            if (t.Contains("request")) return "Request";
            if (t.Contains("issue") || t.Contains("stock out")) return "Issue";
            if (t.Contains("return")) return "Return";
            if (t.StartsWith("stock in") || t.Contains("opening") || t.Contains("receive")) return "Receive";
            return "Other";
        }

        public IQueryable<StockMovementDto> StockMovement(ReportFilter filter)
        {
            return LoadStockLines(filter, false)
                .OrderBy(r => r.Date).ThenBy(r => r.StockInID)
                .Select(r => new StockMovementDto
                {
                    Date = r.Date,
                    MoveGroup = MoveGroupOf(r.Type),
                    Type = r.Type,
                    StockInID = r.StockInID,
                    ItemCode = r.ItemCode,
                    ItemName = r.ItemName,
                    SerialNumber = r.SerialNumber,
                    Warehouse = r.Warehouse,
                    QtyIn = (decimal)r.QtyIn,
                    QtyOut = (decimal)r.QtyOut
                })
                .AsQueryable();
        }

        // ───────────── Stock value ─────────────
        public IQueryable<StockValueDto> StockValueSummary(ReportFilter filter)
        {
            return LoadMoves(filter, true)
                .GroupBy(r => new { r.ItemCode, r.ItemName, r.Warehouse, r.UM })
                .Select(g => new StockValueDto
                {
                    ItemCode = g.Key.ItemCode,
                    ItemName = g.Key.ItemName,
                    Warehouse = g.Key.Warehouse,
                    UM = g.Key.UM,
                    Qty = g.Sum(x => x.Qty),
                    Value = g.Sum(x => x.Qty * x.UnitCost),
                    AvgCost = g.Sum(x => x.Qty) == 0m ? 0m : g.Sum(x => x.Qty * x.UnitCost) / g.Sum(x => x.Qty)
                })
                .Where(x => x.Qty != 0m)
                .OrderBy(x => x.ItemName);
        }

        public IQueryable<StockMoveDto> StockValueDetail(ReportFilter filter)
        {
            return LoadMoves(filter, false)
                .OrderBy(r => r.ItemName).ThenBy(r => r.Date)
                .Select(r => new StockMoveDto
                {
                    Date = r.Date,
                    RefNo = r.RefNo,
                    MoveType = r.MoveType,
                    ItemCode = r.ItemCode,
                    ItemName = r.ItemName,
                    Warehouse = r.Warehouse,
                    UM = r.UM,
                    Qty = r.Qty,
                    UnitCost = r.UnitCost,
                    Value = r.Qty * r.UnitCost
                });
        }

        // ledger qty vs the qty stored on the item
        public IQueryable<StockReconDto> StockReconciliation(ReportFilter filter)
        {
            var ledger = db.TblStockInDetail.AsNoTracking().WhereScope(scope)
                .GroupBy(d => d.ItemID)
                .Select(g => new { ItemID = g.Key, Qty = g.Sum(x => (double?)x.QTY) ?? 0d });

            var raw = (from it in db.TblItem.AsNoTracking()
                       join l in ledger on it.ItemID equals l.ItemID into ls
                       from l in ls.DefaultIfEmpty()
                       select new
                       {
                           it.ItemCode,
                           it.ItemName,
                           Ledger = (double?)l.Qty,
                           OnHand = (double?)it.QTYOnHand
                       })
                      .ToList();

            return raw
                .Select(x => new StockReconDto
                {
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    LedgerQty = (decimal)(x.Ledger ?? 0d),
                    ItemQty = (decimal)(x.OnHand ?? 0d),
                    Difference = (decimal)((x.Ledger ?? 0d) - (x.OnHand ?? 0d))
                })
                .Where(x => x.Difference != 0m)
                .OrderBy(x => x.ItemName)
                .AsQueryable();
        }

        // stock side only (no general-ledger table available)
        public IQueryable<CategoryValueDto> StockValueVariance(ReportFilter filter)
        {
            return LoadMoves(filter, true)
                .GroupBy(r => r.Category ?? "(No category)")
                .Select(g => new CategoryValueDto
                {
                    Category = g.Key,
                    Qty = g.Sum(x => x.Qty),
                    Value = g.Sum(x => x.Qty * x.UnitCost)
                })
                .OrderBy(x => x.Category);
        }

        // ───────────── FIFO layers ─────────────
        // layers = receipts (Qty > 0); issues (Qty < 0) consume the oldest layers first
        private static List<FifoLayerDto> Layers(IQueryable<MoveRow> rows)
        {
            var data = rows.Select(r => new
            {
                r.ItemID,
                r.ItemCode,
                r.ItemName,
                r.Warehouse,
                r.Date,
                r.RefNo,
                r.Qty,
                r.UnitCost
            }).ToList();

            var result = new List<FifoLayerDto>();
            foreach (var g in data.GroupBy(x => new { x.ItemID, x.Warehouse }))
            {
                var issued = -g.Where(x => x.Qty < 0).Sum(x => x.Qty);
                foreach (var l in g.Where(x => x.Qty > 0).OrderBy(x => x.Date))
                {
                    var used = Math.Min(l.Qty, issued);
                    issued -= used;
                    var remaining = l.Qty - used;
                    if (remaining <= 0m) continue;
                    result.Add(new FifoLayerDto
                    {
                        Date = l.Date,
                        RefNo = l.RefNo,
                        ItemCode = l.ItemCode,
                        ItemName = l.ItemName,
                        Warehouse = l.Warehouse,
                        ReceivedQty = l.Qty,
                        RemainingQty = remaining,
                        UnitCost = l.UnitCost,
                        Value = remaining * l.UnitCost
                    });
                }
            }
            return result.OrderBy(x => x.ItemName).ThenBy(x => x.Date).ToList();
        }

        public IQueryable<FifoLayerDto> FifoLayerDetail(ReportFilter filter)
        {
            return Layers(LoadMoves(filter, true)).AsQueryable();
        }

        public IQueryable<StockValueDto> FifoLayerBalance(ReportFilter filter)
        {
            return Layers(LoadMoves(filter, true))
                .GroupBy(l => new { l.ItemCode, l.ItemName, l.Warehouse })
                .Select(g => new StockValueDto
                {
                    ItemCode = g.Key.ItemCode,
                    ItemName = g.Key.ItemName,
                    Warehouse = g.Key.Warehouse,
                    Qty = g.Sum(x => x.RemainingQty),
                    Value = g.Sum(x => x.Value),
                    AvgCost = Safe(g.Sum(x => x.Value), g.Sum(x => x.RemainingQty))
                })
                .OrderBy(x => x.ItemName).AsQueryable();
        }

        // ───────────── Transfers (one row per outgoing line) ─────────────
        public IQueryable<TransferDto> StockTransferDetail(ReportFilter filter)
        {
            return LoadMoves(filter, false)
                .Where(r => r.MoveType == TransferType && r.Qty < 0)
                .OrderBy(r => r.Date).ThenBy(r => r.RefNo)
                .Select(r => new TransferDto
                {
                    Date = r.Date,
                    TransferNo = r.RefNo,
                    FromWarehouse = r.Warehouse,
                    ToWarehouse = r.ToWarehouse,
                    ItemName = r.ItemName,
                    Qty = -r.Qty
                });
        }

        public IQueryable<TransferSummaryDto> StockTransferSummary(ReportFilter filter)
        {
            return LoadMoves(filter, false)
                .Where(r => r.MoveType == TransferType && r.Qty < 0)
                .GroupBy(r => new { r.Date, r.RefNo, r.Warehouse, r.ToWarehouse })
                .Select(g => new TransferSummaryDto
                {
                    Date = g.Key.Date,
                    TransferNo = g.Key.RefNo,
                    FromWarehouse = g.Key.Warehouse,
                    ToWarehouse = g.Key.ToWarehouse,
                    Items = g.Select(x => x.ItemID).Distinct().Count(),
                    Qty = -g.Sum(x => x.Qty)
                })
                .OrderBy(x => x.Date).ThenBy(x => x.TransferNo);
        }

        // ───────────── Consignment ─────────────
        private static IQueryable<MoveRow> Cons(IQueryable<MoveRow> rows) =>
            rows.Where(r => r.MoveType == ConsOut || r.MoveType == ConsReturn || r.MoveType == ConsSettle);

        public IQueryable<ConsignmentDto> ConsignmentBalance(ReportFilter filter)
        {
            return Cons(LoadMoves(filter, true))
                .GroupBy(r => new { r.Customer, r.ItemName })
                .Select(g => new ConsignmentDto
                {
                    Customer = g.Key.Customer,
                    ItemName = g.Key.ItemName,
                    QtyOut = g.Sum(x => x.MoveType == ConsOut ? (x.Qty < 0 ? -x.Qty : x.Qty) : 0m),
                    QtyReturned = g.Sum(x => x.MoveType != ConsOut ? (x.Qty < 0 ? -x.Qty : x.Qty) : 0m),
                    QtyBalance = g.Sum(x => x.MoveType == ConsOut
                        ? (x.Qty < 0 ? -x.Qty : x.Qty)
                        : -(x.Qty < 0 ? -x.Qty : x.Qty)),
                    Value = g.Sum(x => (x.MoveType == ConsOut ? 1m : -1m) * (x.Qty < 0 ? -x.Qty : x.Qty) * x.UnitCost)
                })
                .Where(x => x.QtyBalance != 0m)
                .OrderBy(x => x.Customer).ThenBy(x => x.ItemName);
        }

        public IQueryable<ConsignmentMoveDto> ConsignmentMovement(ReportFilter filter)
        {
            return Cons(LoadMoves(filter, false))
                .OrderBy(r => r.Date).ThenBy(r => r.RefNo)
                .Select(r => new ConsignmentMoveDto
                {
                    Date = r.Date,
                    RefNo = r.RefNo,
                    Customer = r.Customer,
                    ItemName = r.ItemName,
                    MoveType = r.MoveType,
                    Qty = r.Qty < 0 ? -r.Qty : r.Qty
                });
        }

        public IQueryable<ConsignmentMoveDto> ConsignmentSettlement(ReportFilter filter)
        {
            return LoadMoves(filter, false)
                .Where(r => r.MoveType == ConsSettle)
                .OrderBy(r => r.Date).ThenBy(r => r.RefNo)
                .Select(r => new ConsignmentMoveDto
                {
                    Date = r.Date,
                    RefNo = r.RefNo,
                    Customer = r.Customer,
                    ItemName = r.ItemName,
                    MoveType = r.MoveType,
                    Qty = r.Qty < 0 ? -r.Qty : r.Qty
                });
        }

        // open consignment qty by age of the consignment-out date (returns close the oldest first)
        public IQueryable<ConsignmentAgingDto> ConsignmentAging(ReportFilter filter)
        {
            var today = DateTime.Today;
            var data = Cons(LoadMoves(filter, true)).Select(r => new
            { r.Customer, r.ItemName, r.Date, r.MoveType, Qty = r.Qty < 0 ? -r.Qty : r.Qty }).ToList();

            var list = new List<ConsignmentAgingDto>();
            foreach (var g in data.GroupBy(x => new { x.Customer, x.ItemName }))
            {
                var closed = g.Where(x => x.MoveType != ConsOut).Sum(x => x.Qty);
                var row = new ConsignmentAgingDto { Customer = g.Key.Customer, ItemName = g.Key.ItemName };
                foreach (var o in g.Where(x => x.MoveType == ConsOut).OrderBy(x => x.Date))
                {
                    var used = Math.Min(o.Qty, closed); closed -= used;
                    var open = o.Qty - used;
                    if (open <= 0m) continue;
                    var age = o.Date == null ? 0 : (int)(today - o.Date.Value.Date).TotalDays;
                    if (age <= 0) row.Current += open;
                    else if (age <= 30) row.Days1To30 += open;
                    else if (age <= 60) row.Days31To60 += open;
                    else if (age <= 90) row.Days61To90 += open;
                    else row.Over90 += open;
                }
                row.Total = row.Current + row.Days1To30 + row.Days31To60 + row.Days61To90 + row.Over90;
                if (row.Total > 0m) list.Add(row);
            }
            return list.OrderBy(x => x.Customer).ThenBy(x => x.ItemName).AsQueryable();
        }

        // ───────────── Lot / Serial (from TblItemSerialDetail) ─────────────
        public IQueryable<SerialDto> SerialRegister(ReportFilter filter)
        {
            List<SerialRow> serials; List<LedgerRow> ledger;
            LoadSerials(filter, false, out serials, out ledger);

            return serials
                .OrderBy(r => r.ItemName).ThenBy(r => r.SerialNumber)
                .Select(r => new SerialDto
                {
                    ItemName = r.ItemName,
                    SerialNo = r.SerialNumber,
                    ExpiryDate = r.Expiry,
                    Warehouse = r.Warehouse,
                    Status = r.Available ? "In stock" : "Sold"
                })
                .AsQueryable();
        }

        public IQueryable<LotDto> LotBalance(ReportFilter filter)
        {
            List<SerialRow> serials; List<LedgerRow> ledger;
            LoadSerials(filter, false, out serials, out ledger);

            return serials
                .Where(r => r.Available)
                .GroupBy(r => new { r.ItemName, r.SerialNumber, r.Expiry, r.Warehouse })
                .Select(g => new LotDto
                {
                    ItemName = g.Key.ItemName,
                    LotNo = g.Key.SerialNumber,
                    ExpiryDate = g.Key.Expiry,
                    Warehouse = g.Key.Warehouse,
                    Qty = g.Sum(x => x.Qty)
                })
                .Where(x => x.Qty > 0m)
                .OrderBy(x => x.ItemName).ThenBy(x => x.ExpiryDate)
                .AsQueryable();
        }

        public IQueryable<LotAgingDto> LotExpiryAging(ReportFilter filter)
        {
            List<SerialRow> serials; List<LedgerRow> ledger;
            LoadSerials(filter, false, out serials, out ledger);

            var today = DateTime.Today;
            return serials
                .Where(r => r.Available)
                .GroupBy(r => new { r.ItemName, r.SerialNumber, r.Expiry })
                .Select(g => new { g.Key, Qty = g.Sum(x => x.Qty) })
                .Where(x => x.Qty > 0m)
                .Select(x =>
                {
                    var days = x.Key.Expiry == null ? 9999 : (int)(x.Key.Expiry.Value.Date - today).TotalDays;
                    return new LotAgingDto
                    {
                        ItemName = x.Key.ItemName,
                        LotNo = x.Key.SerialNumber,
                        ExpiryDate = x.Key.Expiry,
                        Expired = days < 0 ? x.Qty : 0m,
                        Days30 = days >= 0 && days <= 30 ? x.Qty : 0m,
                        Days60 = days > 30 && days <= 60 ? x.Qty : 0m,
                        Days90 = days > 60 && days <= 90 ? x.Qty : 0m,
                        Over90 = days > 90 ? x.Qty : 0m,
                        Total = x.Qty
                    };
                })
                .OrderBy(x => x.ExpiryDate)
                .AsQueryable();
        }

        // ledger balance (StockDetail) vs available qty on the serial records
        public IQueryable<LotReconDto> LotReconciliation(ReportFilter filter)
        {
            List<SerialRow> serials; List<LedgerRow> ledger;
            LoadSerials(filter, true, out serials, out ledger);

            var serialQty = serials
                .Where(r => r.Available)
                .GroupBy(r => r.ItemID + "|" + r.SerialNumber)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));

            return ledger
                .Select(l =>
                {
                    decimal sq;
                    serialQty.TryGetValue(l.ItemID + "|" + l.SerialNumber, out sq);
                    return new LotReconDto
                    {
                        ItemName = l.ItemName,
                        LotNo = l.SerialNumber,
                        LedgerQty = l.Qty,
                        SerialQty = sq,
                        Difference = l.Qty - sq
                    };
                })
                .Where(x => x.Difference != 0m)
                .OrderBy(x => x.ItemName).ThenBy(x => x.LotNo)
                .AsQueryable();
        }

        // movement of each lot / serial through the stock ledger
        // (also used for lot-serial-traceability)
        public IQueryable<LotMoveDto> LotMovementDetail(ReportFilter filter)
        {
            return LoadMoves(filter, false)
                .Where(r => r.LotNo != null && r.LotNo != "")
                .OrderBy(r => r.ItemName).ThenBy(r => r.LotNo).ThenBy(r => r.Date)
                .Select(r => new LotMoveDto
                {
                    Date = r.Date,
                    RefNo = r.RefNo,
                    MoveType = r.MoveType,
                    ItemName = r.ItemName,
                    LotNo = r.LotNo,
                    Warehouse = r.Warehouse,
                    Customer = r.Customer,
                    Qty = r.Qty
                });
        }

        // ───────────── Reorder ─────────────
        public IQueryable<ReorderDto> ReorderList(ReportFilter filter)
        {
            var onHand = db.TblStockInDetail.AsNoTracking().WhereScope(scope)
                .GroupBy(d => d.ItemID)
                .Select(g => new { ItemID = g.Key, Qty = g.Sum(x => (double?)x.QTY) ?? 0d });

            var raw = (from it in db.TblItem.AsNoTracking()
                       join o in onHand on it.ItemID equals o.ItemID into os
                       from o in os.DefaultIfEmpty()
                       select new
                       {
                           it.ItemID,
                           it.ItemCode,
                           it.ItemName,
                           OnHand = (double?)o.Qty,
                           Point = (double?)it.ReorderPoint
                       })
                      .ToList();

            var q = raw.Select(r => new
            {
                r.ItemID,
                r.ItemCode,
                r.ItemName,
                OnHand = (decimal)(r.OnHand ?? 0d),
                ReorderPoint = (decimal)(r.Point ?? 0d),
                MinQty = 0m     // replace with the item's minimum-order column if you have one
            });

            if (filter.ItemID != 0) q = q.Where(r => r.ItemID == filter.ItemID);

            return q.Where(r => r.ReorderPoint > 0m && r.OnHand <= r.ReorderPoint)
                .OrderBy(r => r.ItemName)
                .Select(r => new ReorderDto
                {
                    ItemCode = r.ItemCode,
                    ItemName = r.ItemName,
                    OnHand = r.OnHand,
                    ReorderPoint = r.ReorderPoint,
                    MinQty = r.MinQty,
                    SuggestedQty = (r.ReorderPoint - r.OnHand) > r.MinQty
                        ? (r.ReorderPoint - r.OnHand) : r.MinQty
                })
                .AsQueryable();
        }
    }
}