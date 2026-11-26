// Purchase & Payable category: PO, receive, expense, A/P and payment reports.
(function() {
    "use strict";

    app.run([
        "ReportRegistry",
        "ReportColumns",
        "ReportGrouping",
        function(Registry, Columns, Grouping) {
            const { text, num, cols, money } = Columns;
            let activePurchaseRowActionMenu = null;

            const closePurchaseRowActionMenu = () => {
                if (!activePurchaseRowActionMenu) return;
                document.removeEventListener(
                    "mousedown",
                    activePurchaseRowActionMenu.onOutsideClick,
                );
                document.removeEventListener(
                    "keydown",
                    activePurchaseRowActionMenu.onKeyDown,
                );
                activePurchaseRowActionMenu.menu.remove();
                activePurchaseRowActionMenu = null;
            };

            const togglePurchaseRowActionMenu = (row, trigger) => {
                const triggerElement =
                    trigger && typeof trigger.getBoundingClientRect === "function" ?
                    trigger :
                    document.activeElement;
                const expenseID = Number(
                    row &&
                    (row.expenseID != null ? row.expenseID : row.ExpenseID),
                );
                if (!row ||
                    row.rowType === "total" ||
                    expenseID <= 0 ||
                    !triggerElement ||
                    typeof triggerElement.getBoundingClientRect !== "function"
                )
                    return;

                if (
                    activePurchaseRowActionMenu &&
                    activePurchaseRowActionMenu.trigger === triggerElement
                ) {
                    closePurchaseRowActionMenu();
                    return;
                }
                closePurchaseRowActionMenu();

                const rowElement = triggerElement.closest("tr");
                if (rowElement) rowElement.classList.add("rp-action-selected-row");

                const menu = document.createElement("div");
                menu.className = "rp-row-action-menu";
                menu.setAttribute("role", "menu");
                menu.setAttribute("aria-label", "Expense and payment actions");
                menu.innerHTML = [
                    '<div class="rp-row-action-item" role="menuitem" tabindex="0" data-action="edit">Edit</div>',
                    '<div class="rp-row-action-item" role="menuitem" aria-disabled="true" title="Not available for this transaction type">Delete</div>',
                    '<div class="rp-row-action-item" role="menuitem" aria-disabled="true" title="Not available for this transaction type">Journal Report</div>',
                    '<div class="rp-row-action-item" role="menuitem" aria-disabled="true" title="Not available for this transaction type">Transaction Log</div>',
                ].join("");

                document.body.appendChild(menu);
                menu.style.position = "fixed";
                menu.style.display = "block";
                menu.style.visibility = "visible";
                menu.style.zIndex = "1100";
                const bounds = triggerElement.getBoundingClientRect();
                const left = Math.max(
                    8,
                    Math.min(
                        bounds.right - menu.offsetWidth,
                        window.innerWidth - menu.offsetWidth - 8,
                    ),
                );
                let top = bounds.bottom + 4;
                if (top + menu.offsetHeight > window.innerHeight - 8)
                    top = Math.max(8, bounds.top - menu.offsetHeight - 4);
                menu.style.left = `${left}px`;
                menu.style.top = `${top}px`;

                const editItem = menu.querySelector('[data-action="edit"]');
                if (editItem) {
                    const runEdit = (event) => {
                        event.preventDefault();
                        closePurchaseRowActionMenu();
                        if (typeof window.ShowDialogBox !== "function") {
                            console.error("The expense edit dialog handler is not available.");
                            return;
                        }
                        window.ShowDialogBox(
                            "Edit Expense",
                            "Auto",
                            500,
                            "ExpenseTransactions/AddExpenses",
                            expenseID,
                            "",
                            false,
                            0,
                            0,
                        );
                    };
                    editItem.addEventListener("click", runEdit);
                    editItem.addEventListener("keydown", (event) => {
                        if (event.key === "Enter" || event.key === " ") runEdit(event);
                    });
                }

                const onOutsideClick = (event) => {
                    if (!menu.contains(event.target) &&
                        !triggerElement.contains(event.target)
                    )
                        closePurchaseRowActionMenu();
                };
                const onKeyDown = (event) => {
                    if (event.key === "Escape") closePurchaseRowActionMenu();
                };
                activePurchaseRowActionMenu = {
                    menu,
                    trigger: triggerElement,
                    onOutsideClick,
                    onKeyDown,
                };
                document.addEventListener("mousedown", onOutsideClick);
                document.addEventListener("keydown", onKeyDown);
            };

            const expensePaymentActionColumn = (totalRowType, stickyRight = false) =>
                text("", {
                    key: "expenseID",
                    width: "50",
                    stickyRight,
                    icon: "ti-dots-vertical",
                    className: "rp-row-action-column rp-row-action-trigger",
                    format: (value, row) =>
                        row.rowType === totalRowType ? "" : " ",
                    click: togglePurchaseRowActionMenu,
                });

            const blankPaymentTotal = (value, row) =>
                row.rowType === "total" ? "" : undefined;
            const paymentVendorTotal = (value, row) =>
                row.rowType === "total" ?
                value == null || value === "" ?
                "Total" :
                "Total " + value :
                undefined;

            Grouping.register("Payment Detail", {
                by: ["vendor"],
                showCount: false,
            });
            Registry.add({
                order: 40,
                category: "Purchase & Payable",
                groups: [{
                        name: "Purchase",
                        monthPicker: true,
                        filters: [
                            "dateRange",
                            "supplier",
                            "category",
                            "product",
                            "warehouse",
                            "branch",
                            "brand",
                        ],
                        reports: [
                            "Purchase Order List",
                            "Purchase Order Detail",
                            "Open Purchase Order by Vendor",
                            "Open Purchase Order by Item",
                            "Purchase Receive Summary",
                            "Purchase Receive Detail",
                            "Open Purchase Receive",
                            "Purchase Item by Vendor Summary",
                            "Purchase Item by Vendor Detail",
                            "Purchase by Item Summary",
                            "Purchase by Item Detail",
                            "Purchase by Invoice",
                        ],
                    },

                    {
                        name: "A/P",
                        monthPicker: true,
                        filters: [
                            "dateRange",
                            "supplier",
                            "category",
                            "product",
                            "warehouse",
                            "branch",
                            "brand",
                        ],
                        reports: [
                            "A/P Aging Summary",
                            "A/P Aging Detail",
                            "A/P Aging Summary & Detail",
                            "Outstanding A/P by Vendor",
                            "A/P Reconciliation Summary",
                            "A/P Reconciliation Detail",
                        ],
                    },
                    {
                        name: "Payment",
                        monthPicker: true,
                        reports: ["Payment Summary", "Payment Detail"],
                    },
                    {
                        name: "Expense",
                        monthPicker: true,
                        reports: ["Expense Summary", "Expense Detail"],
                    },
                ],
                reportFilters: {
                    "Purchase Order List": ["dateRange", "supplier", "poNumber"],
                    "Purchase Order Detail": [
                        "dateRange",
                        "supplier",
                        "product",
                        "category",
                    ],
                    "Expense Summary": ["dateRange", "payee", "class", "account"],
                    "Expense Detail": ["dateRange", "payee", "class", "account"],
                    "Payment Summary": ["dateRange", "class", "account", "sortOption"],
                    "Payment Detail": ["dateRange", "class", "account", "sortOption"],
                },
                reportUiSettings: {
                    "Expense Summary": {
                        totalLabel: "Grand Total",
                        totalLabelAlign: "left",
                        totalLabelPosition: "first",
                        zeroAsDash: true,
                    },
                    "Expense Detail": {
                        totalLabel: "Grand Total",
                        totalLabelAlign: "left",
                        totalLabelPosition: "first",
                        zeroAsDash: true,
                    },
                    "Payment Summary": {
                        totalLabel: "Grand Total",
                        totalLabelAlign: "left",
                        totalLabelPosition: "first",
                        zeroAsDash: true,
                    },
                    "Payment Detail": {
                        totalLabel: "Grand Total",
                        totalLabelAlign: "left",
                        totalLabelPosition: "first",
                        zeroAsDash: true,
                    },
                },

                columns: [{
                        reports: ["Purchase Order List"],
                        columns: cols(
                            text("Date", { key: "date", width: "80" }),
                            text("PO #", { key: "poNo", width: "80" }),
                            text("Vendor", { key: "vendor" }),
                            text("Status", { key: "status", width: "50" }),
                            num("Amount", { key: "amount" }),
                        ),
                    },
                    {
                        reports: ["Purchase Order Detail"],
                        columns: cols(
                            text("Date", { key: "date" }),
                            text("PO #", { key: "poNo" }),
                            text("Item Name", { key: "itemName" }),
                            text("Description", { key: "description", width: "500" }),
                            num("Qty", { key: "qty" }),
                            num("Cost", { key: "cost" }),
                            num("Amount", { key: "amount" }),
                        ),
                    },
                    {
                        reports: [
                            "Purchase Receive Summary",
                            "Purchase Receive Detail",
                            "Open Purchase Receive",
                        ],
                        columns: cols(
                            "Date",
                            "Receive #",
                            "Vendor",
                            "Warehouse",
                            "Item",
                            num("Qty"),
                            num("Amount"),
                        ),
                    },
                    {
                        reports: [
                            "Purchase Item by Vendor Summary",
                            "Purchase Item by Vendor Detail",
                        ],
                        columns: cols("Vendor", "Item", num("Qty"), num("Amount")),
                    },
                    {
                        reports: ["Purchase by Item Summary", "Purchase by Item Detail"],
                        columns: cols(
                            "Item Code",
                            "Item Name",
                            num("Qty"),
                            "UM",
                            num("Avg Cost", { total: false }),
                            num("Amount"),
                        ),
                    },
                    {
                        reports: ["Purchase by Invoice"],
                        columns: cols(
                            "Date",
                            "Bill #",
                            "Vendor",
                            num("Amount"),
                            num("Paid"),
                            num("Balance"),
                        ),
                    },
                    {
                        reports: ["A/P Aging Summary", "Outstanding A/P by Vendor"],
                        columns: cols(
                            "Vendor Code",
                            "Vendor",
                            num("Current"),
                            num("1-30"),
                            num("31-60"),
                            num("61-90"),
                            num("Over 90"),
                            num("Total"),
                        ),
                    },
                    {
                        reports: ["A/P Aging Detail", "A/P Aging Summary & Detail"],
                        columns: cols(
                            "Vendor",
                            "Bill #",
                            "Bill Date",
                            "Due Date",
                            num("Age (Days)", { total: false }),
                            num("Balance"),
                        ),
                    },
                    {
                        reports: [
                            "A/P Reconciliation Summary",
                            "A/P Reconciliation Detail",
                        ],
                        columns: cols(
                            "Vendor",
                            "Bill #",
                            num("Bill Amount"),
                            num("Paid"),
                            num("Balance"),
                        ),
                    },

                    {
                        reports: [
                            "Open Purchase Order by Vendor",
                            "Open Purchase Order by Item",
                        ],
                        columns: cols(
                            "Name",
                            num("Ordered Qty"),
                            num("Delivered Qty"),
                            num("Open Qty"),
                            num("Open Amount"),
                        ),
                    },
                    {
                        reports: ["Expense Summary"],
                        hideNo: true,
                        strict: true,
                        columns: cols(
                            text("Transaction", { key: "tranType", width: "100" }),
                            text("PV. Number", {
                                key: "pvNumber",
                                click: (row) =>
                                    window.showPaymentVoucher(
                                        row.expenseID,
                                        1,
                                        row.status,
                                        row.paymentNo,
                                    ),
                            }),
                            text("Class", { key: "className" }),
                            text("Expense Date", { key: "expenseDate" }),
                            text("Payee", { key: "payee" }),
                            text("Account Code", { key: "accountCode", hidden: true }),
                            text("Account Name", { key: "accountName", hidden: true }),
                            text("Payment Method", {
                                key: "paymentMethod",
                                hidden: true,
                            }),
                            text("Memo", { key: "memo", width: "300" }),
                            num("Amount", { key: "amount", prefix: "$ ", decimals: 2 }),
                            expensePaymentActionColumn("total"),
                        ),
                    },

                    {
                        reports: ["Expense Detail"],
                        hideNo: true,
                        strict: true,
                        columns: cols(
                            text("Transaction", { key: "tranType", width: "110" }),
                            text("Number", { key: "pvNumber", hidden: true }),
                            text("Class", { key: "className" }),
                            text("Expense Date", { key: "expenseDate" }),
                            text("Payee", { key: "payee" }),
                            text("Account Code", { key: "accountCode", hidden: true }),
                            text("Account Name", { key: "accountName", hidden: true }),
                            text("Account", { key: "account" }),
                            num("Amount", { key: "amount", prefix: "$ ", decimals: 2 }),
                            text("Vat", { key: "vat", hidden: true }),
                            text("Memo", { key: "memo", width: "300" }),
                            expensePaymentActionColumn("total"),
                        ),
                    },

                    {
                        reports: ["Payment Summary"],
                        hideNo: true,
                        strict: true,
                        columns: cols(
                            text("Transaction", { key: "transactionType", width: "110" }),
                            text("Number", { key: "number" }),
                            text("Class", { key: "className", hidden: true }),
                            text("Date", { key: "paymentDate" }),
                            text("Vendor", { key: "vendor" }),
                            text("Currency", { key: "currency" }),
                            num("Discount", { key: "discount", prefix: "$ ", decimals: 2 }),
                            text("Account Code", { key: "accountCode", hidden: true }),
                            text("Account Name", { key: "accountName", hidden: true }),
                            num("Amount", { key: "amount", prefix: "$ ", decimals: 2 }),
                            text("Memo", { key: "memo", width: "300" }),
                            expensePaymentActionColumn("total"),
                        ),
                    },

                    {
                        reports: ["Payment Detail"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Vendor", {
                                key: "vendor",
                                width: 300,
                                format: paymentVendorTotal,
                                className: (value, row) =>
                                    row.rowType === "total" ? "font-weight-bold" : "",
                            }),
                            text("Transaction", {
                                key: "transactionType",
                                width: "80",
                                format: blankPaymentTotal,
                            }),
                            text("Bill No.", {
                                key: "billNo",
                                width: "100",
                                format: blankPaymentTotal,
                            }),
                            text("Purchase Date", {
                                key: "purchaseDate",
                                width: "150",
                                format: blankPaymentTotal,
                            }),
                            text("Date", {
                                key: "paymentDate",
                                width: "150",
                                format: blankPaymentTotal,
                            }),
                            text("Payment No", {
                                key: "paymentNo",
                                width: "140",
                                format: blankPaymentTotal,
                            }),
                            num("Pay Amount", {
                                key: "payAmount",
                                prefix: "$ ",
                                decimals: 2,
                                className: (v, row) =>
                                    row.rowType === "total" ? "rp-subtotal-line" : "",
                            }),
                            text("Account Code", {
                                key: "accountCode",
                                format: blankPaymentTotal,
                            }),
                            text("Account Name", {
                                key: "accountName",
                                width: "250",
                                format: blankPaymentTotal,
                            }),
                            text("Payment Method", {
                                key: "paymentMethod",
                                width: "110",
                                format: blankPaymentTotal,
                            }),
                            text("Currency", {
                                key: "currency",
                                width: "100",
                                format: blankPaymentTotal,
                            }),
                            num("Discount Amount", {
                                key: "discountAmount",
                                prefix: "$ ",
                                decimals: 2,
                                className: (v, row) =>
                                    row.rowType === "total" ? "rp-subtotal-line" : "",
                            }),
                            num("Grand Amount", {
                                key: "grandAmount",
                                prefix: "$ ",
                                decimals: 2,
                                className: (v, row) =>
                                    row.rowType === "total" ? "rp-subtotal-line" : "",
                            }),
                            text("Detail Memo", {
                                key: "memoDetail",
                                width: "250",
                                format: blankPaymentTotal,
                            }),
                            text("Memo", {
                                key: "memo",
                                width: "250",
                                format: blankPaymentTotal,
                            }),
                            expensePaymentActionColumn("total", true),
                        ),
                    },
                ],
            });
        },
    ]);
})();