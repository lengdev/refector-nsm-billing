// Inventory & Stock category: stock, transfer, consignment and lot reports.
(function () {
  "use strict";

  app.run([
    "ReportRegistry",
    "ReportColumns",
    "ReportGrouping",
    function (Registry, Columns, Grouping) {
      const { text, num, money, cols } = Columns;

      const AS_OF = [
        "Stock Summary",
        "Stock Movement",
        "Stock Value Summary",
        "Stock Work Sheet",
        "Stock Value and Balance Sheet Account Variance",
        "FIFO Layer Balance",
        "FIFO Layer Balance Detail",
        "Consignment Balance",
        "Consignment Aging",
        "Lot Balance",
        "Lot Expiry Aging",
        "Lot Reconciliation",
        "Stock Reconciliation",
      ];
      const asOfPresets = {};
      AS_OF.forEach((n) => (asOfPresets[n] = "today"));

      const status = (v) => (v === "In stock" ? "rp-good" : "rp-warn");
      const stockValueCols = () => [
        text("Item Code", { key: "itemCode" }),
        text("Item Name", { key: "itemName" }),
        text("Warehouse", { key: "warehouse" }),
        text("UM", { key: "um" }),
        num("Qty", { key: "qty", decimals: 2 }),
        money("Avg Cost", { key: "avgCost", total: false }),
        money("Value", { key: "value" }),
      ];

      Grouping.register("Stock Movement", {
        by: ["moveGroup"],
        label: "Movement",
        countLabel: "lines",
        emptyLabel: "Other",
      });

      Registry.add({
        order: 30,
        category: "Inventory & Stock",
        groups: [
          {
            name: "Item",
            monthPicker: false,
            filters: [
              "dateRange",
              "product",
              "category",
              "itemGroup",
              "brand",
              "warehouse",
            ],
            reports: [
              "Stock Summary",
              "Stock Movement",
              "Stock Value Summary",
              "Stock Value Detail",
              "Stock Reconciliation",
              "Stock Value and Balance Sheet Account Variance",
              "FIFO Layer Balance",
              "FIFO Layer Balance Detail",
              "Stock Transfer Summary",
              "Stock Transfer Detail",
              "Consignment Balance",
              "Consignment Movement",
              "Consignment Aging",
              "Consignment Settlement",
              "Lot Balance",
              "Lot Expiry Aging",
              "Lot Movement Detail",
              "Lot / Serial Traceability",
              "Lot Reconciliation",
              "Serial Register",
              "Stock Work Sheet",
              "Reorder List",
            ],
          },
        ],
        reportDatePresets: {
          ...asOfPresets,
          "Stock Summary": "all",
          "Stock Movement": "mtd",
        },
        reportFilters: {
          "Stock Reconciliation": {
            remove: ["dateRange", "category", "brand", "warehouse"],
          },
          "Reorder List": {
            remove: ["dateRange", "category", "brand", "warehouse"],
          },
          "Serial Register": { remove: ["dateRange", "category", "brand"] },
          "Lot Reconciliation": {
            remove: ["dateRange", "category", "brand", "warehouse"],
          },
          "Consignment Balance": { remove: ["category", "brand", "warehouse"] },
          "Consignment Movement": {
            remove: ["category", "brand", "warehouse"],
          },
          "Consignment Aging": { remove: ["category", "brand", "warehouse"] },
          "Consignment Settlement": {
            remove: ["category", "brand", "warehouse"],
          },
          "Stock Value and Balance Sheet Account Variance": {
            remove: ["product", "brand", "warehouse"],
          },
          "Stock Summary": { remove: ["brand"] },
          "Stock Movement": { remove: ["brand"] },
        },
        columns: [
          {
            reports: ["Stock Summary"],
            strict: true,
            columns: cols(
              text("Item Code", { key: "itemCode" }),
              text("Barcode", { key: "barcode", hidden: true }),
              text("Item Name", { key: "itemName" }),
              text("Serial #", { key: "serialNumber" }),
              text("Group", { key: "itemGroup" }),
              text("UM", { key: "um" }),
              text("Warehouse", { key: "warehouse" }),
              num("Qty In", { key: "qtyIn" }),
              num("Qty Out", { key: "qtyOut" }),
              num("Stock On Hand", { key: "stockOnHand" }),
              num("Return", { key: "stockReturn" }),
              text("Status", {
                key: "status",
                className: (v) =>
                  v === "Out of stock"
                    ? "rp-bad"
                    : v === "Nearly out of stock"
                      ? "rp-warn"
                      : "rp-good",
              }),
            ),
          },
          {
            reports: ["Stock Movement"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Type", { key: "type" }),
              text("Item Code", { key: "itemCode", hidden: true }),
              text("Item Name", { key: "itemName" }),
              text("Serial #", { key: "serialNumber" }),
              text("Warehouse", { key: "warehouse" }),
              num("Qty In", { key: "qtyIn" }),
              num("Qty Out", { key: "qtyOut" }),
            ),
          },
          {
            reports: [
              "Stock Value Summary",
              "Stock Work Sheet",
              "FIFO Layer Balance",
            ],
            strict: true,
            columns: cols(...stockValueCols()),
          },
          {
            reports: ["Stock Value Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Ref #", { key: "refNo" }),
              text("Type", { key: "moveType" }),
              text("Item Code", { key: "itemCode", hidden: true }),
              text("Item Name", { key: "itemName" }),
              text("Warehouse", { key: "warehouse" }),
              text("UM", { key: "um" }),
              num("Qty", { key: "qty", decimals: 2 }),
              money("Unit Cost", { key: "unitCost", total: false }),
              money("Value", { key: "value" }),
            ),
          },
          {
            reports: ["Stock Reconciliation"],
            strict: true,
            columns: cols(
              text("Item Code", { key: "itemCode" }),
              text("Item Name", { key: "itemName" }),
              num("Ledger Qty", { key: "ledgerQty", decimals: 2 }),
              num("Item Qty", { key: "itemQty", decimals: 2 }),
              num("Difference", {
                key: "difference",
                decimals: 2,
                className: (v) => (v ? "rp-bad" : ""),
              }),
            ),
          },
          {
            reports: ["Stock Value and Balance Sheet Account Variance"],
            strict: true,
            columns: cols(
              text("Category", { key: "category" }),
              num("Qty", { key: "qty", decimals: 2 }),
              money("Stock Value", { key: "value" }),
            ),
          },
          {
            reports: ["FIFO Layer Balance Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Ref #", { key: "refNo" }),
              text("Item Code", { key: "itemCode", hidden: true }),
              text("Item Name", { key: "itemName" }),
              text("Warehouse", { key: "warehouse" }),
              num("Received", {
                key: "receivedQty",
                decimals: 2,
                total: false,
              }),
              num("Remaining", { key: "remainingQty", decimals: 2 }),
              money("Unit Cost", { key: "unitCost", total: false }),
              money("Value", { key: "value" }),
            ),
          },
          {
            reports: ["Stock Transfer Summary"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Transfer #", { key: "transferNo" }),
              text("From Warehouse", { key: "fromWarehouse" }),
              text("To Warehouse", { key: "toWarehouse" }),
              num("Items", { key: "items", total: false }),
              num("Qty", { key: "qty", decimals: 2 }),
            ),
          },
          {
            reports: ["Stock Transfer Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Transfer #", { key: "transferNo" }),
              text("From Warehouse", { key: "fromWarehouse" }),
              text("To Warehouse", { key: "toWarehouse" }),
              text("Item", { key: "itemName" }),
              num("Qty", { key: "qty", decimals: 2 }),
            ),
          },
          {
            reports: ["Consignment Balance"],
            strict: true,
            columns: cols(
              text("Customer", { key: "customer" }),
              text("Item", { key: "itemName" }),
              num("Qty Out", { key: "qtyOut", decimals: 2 }),
              num("Qty Returned", { key: "qtyReturned", decimals: 2 }),
              num("Qty Balance", { key: "qtyBalance", decimals: 2 }),
              money("Value", { key: "value" }),
            ),
          },
          {
            reports: ["Consignment Movement", "Consignment Settlement"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Ref #", { key: "refNo" }),
              text("Customer", { key: "customer" }),
              text("Item", { key: "itemName" }),
              text("Type", { key: "moveType" }),
              num("Qty", { key: "qty", decimals: 2 }),
            ),
          },
          {
            reports: ["Consignment Aging"],
            strict: true,
            columns: cols(
              text("Customer", { key: "customer" }),
              text("Item", { key: "itemName" }),
              num("Current", { key: "current", decimals: 2 }),
              num("1-30", { key: "days1To30", decimals: 2 }),
              num("31-60", { key: "days31To60", decimals: 2 }),
              num("61-90", { key: "days61To90", decimals: 2 }),
              num("Over 90", { key: "over90", decimals: 2 }),
              num("Total", { key: "total", decimals: 2 }),
            ),
          },
          {
            reports: ["Lot Balance"],
            strict: true,
            columns: cols(
              text("Item", { key: "itemName" }),
              text("Lot #", { key: "lotNo" }),
              text("Expiry Date", { key: "expiryDate" }),
              text("Warehouse", { key: "warehouse" }),
              num("Qty", { key: "qty", decimals: 2 }),
            ),
          },
          {
            reports: ["Lot Expiry Aging"],
            strict: true,
            columns: cols(
              text("Item", { key: "itemName" }),
              text("Lot #", { key: "lotNo" }),
              text("Expiry Date", { key: "expiryDate" }),
              num("Expired", {
                key: "expired",
                decimals: 2,
                className: "rp-bad",
              }),
              num("0-30", { key: "days30", decimals: 2, className: "rp-warn" }),
              num("31-60", { key: "days60", decimals: 2 }),
              num("61-90", { key: "days90", decimals: 2 }),
              num("Over 90", { key: "over90", decimals: 2 }),
              num("Total", { key: "total", decimals: 2 }),
            ),
          },
          {
            reports: ["Lot Movement Detail", "Lot / Serial Traceability"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Ref #", { key: "refNo" }),
              text("Type", { key: "moveType" }),
              text("Item", { key: "itemName" }),
              text("Lot #", { key: "lotNo" }),
              text("Warehouse", { key: "warehouse" }),
              text("Customer", { key: "customer" }),
              num("Qty", { key: "qty", decimals: 2 }),
            ),
          },
          {
            reports: ["Lot Reconciliation"],
            strict: true,
            columns: cols(
              text("Item", { key: "itemName" }),
              text("Lot #", { key: "lotNo" }),
              num("Ledger Qty", { key: "ledgerQty", decimals: 2 }),
              num("Serial Qty", { key: "serialQty", decimals: 2 }),
              num("Difference", {
                key: "difference",
                decimals: 2,
                className: (v) => (v ? "rp-bad" : ""),
              }),
            ),
          },
          {
            reports: ["Serial Register"],
            strict: true,
            columns: cols(
              text("Item", { key: "itemName" }),
              text("Serial #", { key: "serialNo" }),
              text("Lot #", { key: "lotNo" }),
              text("Expiry Date", { key: "expiryDate" }),
              text("Warehouse", { key: "warehouse" }),
              text("Status", { key: "status", className: status }),
            ),
          },
          {
            reports: ["Reorder List"],
            strict: true,
            columns: cols(
              text("Item Code", { key: "itemCode" }),
              text("Item Name", { key: "itemName" }),
              num("On Hand", { key: "onHand", decimals: 2 }),
              num("Reorder Point", {
                key: "reorderPoint",
                decimals: 2,
                total: false,
              }),
              num("Min Qty", { key: "minQty", decimals: 2, total: false }),
              num("Suggested Qty", { key: "suggestedQty", decimals: 2 }),
            ),
          },
        ],
      });
    },
  ]);
})();
