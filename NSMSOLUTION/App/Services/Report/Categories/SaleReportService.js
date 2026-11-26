// Sale & Receivable category: Quotation, Sale and A/R reports.
(function () {
  "use strict";

  app.run([
    "$rootScope",
    "ReportRegistry",
    "ReportColumns",
    "ReportGrouping",
    function ($rootScope, Registry, Columns, Grouping) {
      const { text, num, money, cols } = Columns;
      const profileLink = "#!/CustomerProfile?ID={customerID}";

      const customerName = text("Customer Name", {
        key: "customerName",
        link: profileLink,
      });

      // Opens "Sale By Customer Detail" for the clicked customer.
      const customerSalesDrill = (row) => {
        $rootScope.$broadcast("report:drill", {
          report: "Sale By Customer Detail",
          filters: { customerID: row.customerID },
        });
      };

      // A/R aging buckets share the same money columns.
      const agingBuckets = () => [
        money("Current", { key: "current" }),
        money("1-30", { key: "days1To30" }),
        money("31-60", { key: "days31To60" }),
        money("61-90", { key: "days61To90" }),
        money("Over 90", { key: "over90" }),
        money("Total", { key: "total" }),
      ];

      // ───── collapsible groups ─────
      Grouping.register("Sale By Saleman Detail", {
        by: ["salesman"],
        label: "Salesman",
        countLabel: "invoices",
        emptyLabel: "No Salesman",
      });
      Grouping.register("Sale By Item Detail", {
        by: ["itemName"],
        label: "Item",
        countLabel: "lines",
        emptyLabel: "No Item",
      });

      Registry.add({
        order: 20,
        category: "Sale & Receivable",
        groups: [
          {
            name: "Receivable",
            monthPicker: true,
            filters: ["dateRange", "customer", "block", "class"],
            reports: ["Collection", "Daily Income", "Revenue Estimate"],
          },
          {
            name: "A/R Report",
            monthPicker: true,
            filters: ["dateRange", "customer", "block", "class"],
            reports: [
              "A/R Aging Summary",
              "A/R Aging Detail",
              "A/R Aging By Saleman",
              "A/R Aging By Customer",
            ],
          },
          {
            name: "Quotation Report",
            monthPicker: true,
            filters: ["dateRange", "customer"],
            reports: ["Quotation List", "Quotation Detail", "PI List"],
          },
          {
            // the APIs only apply the date range; reports add customer / product below
            name: "Sale Report",
            monthPicker: true,
            filters: ["dateRange"],
            reports: [
              "Sale By Customer Summary",
              "Sale By Customer Detail",
              "Sale By Item Summary",
              "Sale By Item Detail",
              "Sale By Invoice",
              "Sale By Block Summary",
              "Sale By Saleman Summary",
              "Sale By Saleman Detail",
              "Sale Summary By Month",
              "Sale Order List",
              "Sale Order Detail",
              // "Sale Delivery Summary",
              // "Sale Delivery Detail",
              // "Open Sale Delivery",
              // "Open Sale Order by Customer",
              // "Open Sale Order by Item",
            ],
          },
        ],
        reportDatePresets: {
          "Sale Summary By Month": "all",
          "Sale Order List": "all",
          "Sale Order Detail": "all",
          Collection: "today",
          "Daily Income": "today",
        },
        reportFilters: {
          "Revenue Estimate": { remove: ["block"] },

          // customer must be visible, or the drill's customerID is dropped
          "Sale By Customer Summary": { add: ["customer"] },
          "Sale By Customer Detail": { add: ["customer"] },
          "Sale By Invoice": { add: ["customer"] },
          "Sale By Saleman Detail": { add: ["customer"] },
          "Sale By Item Summary": { add: ["product"] },
          "Sale By Item Detail": { add: ["customer", "product"] },
          "Sale Delivery Summary": { add: ["customer"] },
          "Sale Delivery Detail": { add: ["customer"] },
          "Open Sale Delivery": { add: ["customer"] },
          "Sale Order List": { add: ["customer"] },
          "Sale Order Detail": { add: ["customer"] },
        },
        columns: [
          // ───── Quotation Report ─────
          {
            reports: ["Quotation List"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Quotation #", { key: "quotationNo" }),
              text("Customer", { key: "customer" }),
              text("Salesman", { key: "salesman" }),
              money("Amount", { key: "amount" }),
            ),
          },
          {
            reports: ["Quotation Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Quotation #", { key: "quotationNo" }),
              customerName,
              text("Description", { key: "description" }),
              num("Qty", { key: "qty" }),
              money("Unit Price", { key: "unitPrice", total: false }),
              money("Amount", { key: "amount" }),
            ),
          },
          {
            reports: ["PI List"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("PI #", { key: "piNo" }),
              customerName,
              text("Salesman", { key: "salesman" }),
              text("Status", {
                key: "status",
                className: (v) => (v === "Paid" ? "rp-good" : "rp-warn"),
              }),
              money("Amount", { key: "amount" }),
            ),
          },

          // ───── Sale Report: by customer / invoice ─────
          {
            reports: ["Sale By Customer Summary"],
            strict: true,
            columns: cols(
              text("CID", { key: "customerCode" }),
              customerName,
              num("Invoices", {
                key: "invoices",
                icon: "ti-arrow-right",
                click: customerSalesDrill,
              }),
              money("Amount", { key: "amount" }),
              money("Paid", { key: "paid" }),
              money("Balance", { key: "balance" }),
            ),
          },
          {
            reports: ["Sale By Customer Detail", "Sale By Invoice"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Invoice #", { key: "invoiceNo" }),
              customerName,
              text("Block", { key: "block", hidden: true }),
              text("Salesman", { key: "salesman" }),
              money("Amount", { key: "amount" }),
              money("Paid", { key: "paid" }),
              money("Balance", { key: "balance" }),
            ),
          },
          {
            reports: ["Sale By Saleman Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Invoice #", { key: "invoiceNo" }),
              customerName,
              money("Amount", { key: "amount" }),
              money("Paid", { key: "paid" }),
              money("Balance", { key: "balance" }),
            ),
          },

          // ───── Sale Report: summaries ─────
          {
            reports: ["Sale By Block Summary"],
            strict: true,
            columns: cols(
              text("Block", { key: "block" }),
              num("Invoices", { key: "invoices" }),
              num("Customers", { key: "customers", total: false }),
              money("Amount", { key: "amount" }),
              money("Paid", { key: "paid" }),
              money("Balance", { key: "balance" }),
            ),
          },
          {
            reports: ["Sale By Saleman Summary"],
            strict: true,
            columns: cols(
              text("Salesman", { key: "salesman" }),
              num("Invoices", { key: "invoices" }),
              num("Customers", { key: "customers", total: false }),
              money("Amount", { key: "amount" }),
            ),
          },
          {
            reports: ["Sale Summary By Month"],
            strict: true,
            columns: cols(
              text("Month", { key: "month" }),
              num("Invoices", { key: "invoices" }),
              money("Amount", { key: "amount" }),
            ),
          },

          // ───── Sale Report: items ─────
          {
            reports: ["Sale By Item Summary"],
            strict: true,
            columns: cols(
              text("Item", { key: "itemName" }),
              num("Qty", { key: "qty" }),
              money("Avg Price", { key: "avgPrice", total: false }),
              money("Amount", { key: "amount" }),
            ),
          },
          {
            reports: ["Sale By Item Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Invoice #", { key: "invoiceNo" }),
              customerName,
              num("Qty", { key: "qty" }),
              money("Unit Price", { key: "unitPrice", total: false }),
              money("Amount", { key: "amount" }),
            ),
          },

          // ───── Sale Report: delivery ─────
          {
            reports: ["Sale Delivery Summary"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Delivery #", { key: "deliveryNo" }),
              customerName,
              num("Qty", { key: "qty" }),
              text("Status", {
                key: "status",
                className: (v) => (v === "Open" ? "rp-warn" : "rp-good"),
              }),
            ),
          },
          {
            reports: ["Sale Delivery Detail", "Open Sale Delivery"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Delivery #", { key: "deliveryNo" }),
              customerName,
              text("Item", { key: "itemName" }),
              num("Qty", { key: "qty" }),
              text("Status", {
                key: "status",
                className: (v) => (v === "Open" ? "rp-warn" : "rp-good"),
              }),
            ),
          },

          // ───── Sale Report: orders ─────
          {
            reports: ["Sale Order List"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Order #", { key: "orderNo" }),
              customerName,
              text("Salesman", { key: "salesman" }),
              text("Status", {
                key: "status",
                className: (v) => (v === "Open" ? "rp-warn" : "rp-good"),
              }),
              money("Amount", { key: "amount" }),
            ),
          },
          {
            reports: ["Sale Order Detail"],
            strict: true,
            columns: cols(
              text("Date", { key: "date" }),
              text("Order #", { key: "orderNo" }),
              customerName,
              text("Item", { key: "itemName" }),
              num("Qty", { key: "qty" }),
              money("Amount", { key: "amount" }),
            ),
          },
          {
            reports: ["Open Sale Order by Customer", "Open Sale Order by Item"],
            strict: true,
            columns: cols(
              text("Name", { key: "name" }),
              num("Orders", { key: "orders" }),
              num("Ordered Qty", { key: "orderedQty" }),
              money("Open Amount", { key: "openAmount" }),
            ),
          },

          // ───── A/R Report ─────
          {
            reports: ["A/R Aging Summary"],
            columns: cols(...agingBuckets()),
          },
          {
            reports: ["A/R Aging By Customer"],
            columns: cols(
              text("CID", { key: "customerCode" }),
              customerName,
              ...agingBuckets(),
            ),
          },
          {
            reports: ["A/R Aging By Saleman"],
            columns: cols(
              text("Saleman", { key: "saleMan" }),
              ...agingBuckets(),
            ),
          },
          {
            reports: ["A/R Aging Detail"],
            columns: cols(
              customerName,
              text("Invoice #", { key: "invoiceNo" }),
              text("Invoice Date", { key: "invoiceDate" }),
              text("Due Date", { key: "dueDate" }),
              text("Class", { key: "class" }),
              num("Age (Days)", { key: "ageDays", total: false }),
              money("Balance", { key: "balance" }),
            ),
          },

          // ───── Receivable ─────
          {
            reports: ["Collection"],
            columns: cols(
              text("Received Date", { key: "receivedDate" }),
              text("Invoice Date", { key: "invoiceDate" }),
              text("Invoice #", {
                key: "invoiceNo",
                click: (row, el) =>
                  window.PrintInvoice(
                    row.customerID,
                    row.invoiceNo,
                    "1",
                    "10",
                    "",
                    false,
                    el,
                  ),
              }),
              text("Receipt #", { key: "receiptNo" }),
              text("CID", { key: "customerCode", hidden: true }),
              text("Customer", { key: "customer", link: profileLink }),
              text("Block", { key: "block", hidden: true }),
              text("Class", { key: "class" }),
              text("Account Name", { key: "account" }),
              text("Memo", { key: "memo" }),
              money("Amount", { key: "amount" }),
              text("Received By", { key: "receivedBy" }),
            ),
          },
          {
            reports: ["Daily Income"],
            columns: cols(
              text("Receipt No.", { key: "receiptNo" }),
              text("Invoice No.", {
                key: "invoiceNo",
                click: (row, el) =>
                  window.PrintInvoice(
                    row.customerID,
                    row.invoiceNo,
                    "1",
                    "10",
                    "",
                    false,
                    el,
                  ),
              }),
              text("Invoice Date", { key: "invoiceDate" }),
              text("Customer", { key: "customer", link: profileLink }),
              text("Class", { key: "class" }),
              text("Account Name", { key: "account" }),
              text("Paid Date", { key: "receivedDate" }),
              money("Amount", { key: "amount" }),
              text("Paid By", { key: "receivedBy" }),
            ),
          },
          {
            reports: ["Revenue Estimate"],
            columns: cols(
              text("Date", { key: "date" }),
              customerName,
              text("Class", { key: "class" }),
              text("Package", { key: "package" }),
              money("Amount", { key: "amount" }),
              text("Expire Date", { key: "endDate" }),
              text("Remark", { key: "remark" }),
            ),
          },
        ],
      });
    },
  ]);
})();
