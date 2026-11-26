// Accounting category: ledger, trial balance and profit/loss reports.
(function() {
    "use strict";

    if (typeof document !== "undefined") {
        var _st = document.getElementById("rp-subtotal-style");
        if (!_st) {
            _st = document.createElement("style");
            _st.id = "rp-subtotal-style";
            document.head.appendChild(_st);
        }
        var accountIndentStyles = Array.from({ length: 10 }, (_, index) =>
            `.rp-bs-indent-${index + 1}, .rp-pl-indent-${index + 1}, .rp-ledger-indent-${index + 1} { padding-left: ${(index + 1) * 22}px; display: inline-block; }`,
        ).join(" ");
        _st.textContent =
            ".rp-table td:has(.rp-subtotal-line) { overflow: visible !important; } .rp-subtotal-line { display: block; margin-top: -8px; padding-top: 7px; border-top: 1px solid var(--rp-fg, #212529) !important; font-weight: 700 !important; } " +
            ".rp-table td a.rp-pl-account-link { color: #0d6efd !important; cursor: pointer; } .rp-table td a.rp-pl-account-link:hover { text-decoration: underline !important; } " +
            ".rp-table td a.rp-pl-account-static, .rp-table td a.rp-pl-account-static:hover { color: #000 !important; text-decoration: none !important; cursor: default; pointer-events: none; } .rp-table td a.rp-pl-account-static .ti { display: none !important; } " +
            accountIndentStyles;

    }

    app.run([
        "ReportRegistry",
        "ReportColumns",
        "ReportGrouping",
        "ReportTransactionLogDialog",
        "ReportJournalViewDialog",
        function(Registry, Columns, Grouping, TransactionLogDialog, JournalViewDialog) {
            const { text, num, money, cols } = Columns;

            const drillToAccountReport = (row, el, report, includeFromDate = false) => {
                const $root = angular
                    .element(document.body)
                    .injector()
                    .get("$rootScope");
                const s = el && angular.element(el).scope();
                const f = (s && s.filters) || {};

                $root.$broadcast("report:drill", {
                    report,
                    filters: {
                        account: row.accountCode + "-" + row.accountName,
                        from: includeFromDate ? f.from : "",
                        to: f.to,
                    },
                });
            };
            const drillToLedger = (row, el) =>
                drillToAccountReport(row, el, "General Ledger");

            // row action menu
            let activeRowActionMenu = null;
            const closeRowActionMenu = () => {
                if (!activeRowActionMenu) return;
                document.removeEventListener(
                    "mousedown",
                    activeRowActionMenu.onOutsideClick,
                );
                document.removeEventListener("keydown", activeRowActionMenu.onKeyDown);
                activeRowActionMenu.menu.remove();
                activeRowActionMenu = null;
            };

            document.addEventListener("click", (event) => {
                const target = event.target;
                if (!target || typeof target.closest !== "function") return;
                const rowElement = target.closest(
                    ".rp-table tbody tr:not(.rp-group-row):not(.rp-loading-row):not(.rp-error-row)",
                );
                if (!rowElement) return;

                document
                    .querySelectorAll(".rp-table tbody tr.rp-action-selected-row")
                    .forEach((row) => row.classList.remove("rp-action-selected-row"));
                rowElement.classList.add("rp-action-selected-row");
            });

            const rowValue = (row, camelName, pascalName) =>
                row[camelName] != null ? row[camelName] : row[pascalName];

            const editTargetFor = (row) => {
                const type = String(
                        rowValue(row, "transaction", "Transaction") ||
                        rowValue(row, "typeLabel", "TypeLabel") ||
                        "",
                    )
                    .trim()
                    .toLowerCase();
                const normalizedType = type.replace(/[\s_-]/g, "");
                const transactionId = Number(
                    rowValue(row, "transactionID", "TransactionID"),
                );
                const customerId = Number(rowValue(row, "customerID", "CustomerID"));
                const number = String(rowValue(row, "number", "Number") || "");
                const openingBalanceId = Number(
                    rowValue(row, "openingBalanceID", "OpeningBalanceID"),
                );
                const billId = Number(rowValue(row, "billID", "BillID"));
                const vendorPaymentId = Number(
                    rowValue(row, "vendorPaymentID", "VendorPaymentID"),
                );

                if (type === "journal" || type === "journal entry")
                    return transactionId > 0 ? { type: "journal", id: transactionId } : null;

                if (type.includes("invoice") && customerId > 0 && number)
                    return { type: "invoice", id: customerId, number };

                if (normalizedType.includes("openingbalance"))
                    return openingBalanceId > 0 ? { type: "openingBalance", id: openingBalanceId } :
                        null;

                if (normalizedType.includes("enterbill") || normalizedType === "bill")
                    return billId > 0 ? { type: "enterBill", id: billId } : null;

                if (
                    normalizedType.includes("paybill") ||
                    normalizedType.includes("billpayment")
                )
                    return vendorPaymentId > 0 ? { type: "payBill", id: vendorPaymentId } :
                        null;

                if (type === "expense" || type === "write check") {
                    const expenseId = Number(rowValue(row, "expenseID", "ExpenseID"));
                    return expenseId > 0 ? { type: "expense", id: expenseId } : null;
                }

                if (type === "transfer" || type === "fund transfer") {
                    const match = /^FT-(\d+)$/i.exec(number);
                    return match ? { type: "fundTransfer", id: Number(match[1]) } : null;
                }

                return null;
            };

            const editTransaction = (row) => {
                const target = editTargetFor(row);
                if (!target) return;
                if (typeof window.ShowDialogBox !== "function") {
                    console.error("The edit dialog handler is not available.");
                    return;
                }

                if (target.type === "invoice") {
                    if (typeof window.EditCustomerWithRefNumber !== "function") {
                        console.error("The customer invoice edit handler is not available.");
                        return;
                    }
                    window.EditCustomerWithRefNumber(
                        target.id,
                        target.number,
                        "Invoice",
                        false,
                    );
                } else if (target.type === "journal") {
                    window.ShowDialogBox(
                        "Edit Journal Entry",
                        "Auto",
                        "Auto",
                        `JournalEntry/Create?id=${target.id}`,
                    );
                } else if (target.type === "expense") {
                    window.ShowDialogBox(
                        "Edit Expense",
                        "Auto",
                        500,
                        "ExpenseTransactions/AddExpenses",
                        target.id,
                        "",
                        false,
                        0,
                        0,
                    );
                } else if (target.type === "fundTransfer") {
                    window.ShowDialogBox(
                        "Edit Fund Transfer",
                        "Auto",
                        600,
                        `FundTransfer/CreateFundTransfer?PrimaryFieldValue=${target.id}`,
                        target.id,
                    );
                } else if (target.type === "openingBalance") {
                    window.ShowDialogBox(
                        "Edit Opening Balance",
                        "Auto",
                        "Auto",
                        `Account/AddOpeningBalance?PrimaryFieldValue=${target.id}`,
                        target.id,
                    );
                } else if (target.type === "enterBill") {
                    window.ShowDialogBox(
                        "Edit Bill",
                        "Auto",
                        "Auto",
                        `Bills/CreateBill?PrimaryFieldValue=${target.id}&Type=EditBill&Status=Bill`,
                        target.id,
                    );
                } else if (target.type === "payBill") {
                    window.ShowDialogBox(
                        "Edit Pay Bill",
                        "Auto",
                        "Auto",
                        `PayBill/Create?paymentId=${target.id}`,
                        target.id,
                    );
                }
            };

            const toggleRowActionMenu = (row, trigger, options) => {
                const triggerElement =
                    trigger && typeof trigger.getBoundingClientRect === "function" ?
                    trigger :
                    document.activeElement;
                if (!row ||
                    row.rowType === "subtotal" ||
                    !triggerElement ||
                    typeof triggerElement.getBoundingClientRect !== "function"
                )
                    return;
                if (
                    activeRowActionMenu &&
                    activeRowActionMenu.trigger === triggerElement
                ) {
                    closeRowActionMenu();
                    return;
                }
                closeRowActionMenu();
                const rowElement = triggerElement.closest("tr");
                if (rowElement) rowElement.classList.add("rp-action-selected-row");

                const menu = document.createElement("div");
                menu.className = "rp-row-action-menu";
                menu.setAttribute("role", "menu");
                menu.setAttribute("aria-label", "Transaction actions");
                const canEdit = !!editTargetFor(row);
                const transactionId = Number(
                    rowValue(row, "transactionID", "TransactionID"),
                );
                const canView = Number.isInteger(transactionId) && transactionId > 0;
                const actions = [
                    { label: "Edit", action: "edit", enabled: canEdit },
                    { label: "Delete", enabled: false },
                    ...(!options || !options.hideJournalView ? [
                        { label: "Journal Report", action: "journal-view", enabled: canView },
                    ] : []),
                    { label: "Transaction Log", action: "transaction-log", enabled: canView },
                ];
                menu.innerHTML = actions.map(({ label, action, enabled }) => {
                    const itemAttributes = enabled && action ?
                        ` tabindex="0" data-action="${action}"` :
                        ' title="Not available for this transaction type"';
                    return `<div class="rp-row-action-item" role="menuitem" aria-disabled="${!enabled}"${itemAttributes}>` +
                        `<span>${label}</span></div>`;
                }).join("");

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

                menu.querySelectorAll("[data-action]").forEach((item) => {
                    const runAction = (event) => {
                        event.preventDefault();
                        closeRowActionMenu();
                        switch (item.getAttribute("data-action")) {
                            case "edit":
                                editTransaction(row);
                                break;
                            case "journal-view":
                                JournalViewDialog.open(row);
                                break;
                            case "transaction-log":
                                TransactionLogDialog.open(row);
                                break;
                        }
                    };
                    item.addEventListener("click", runAction);
                    item.addEventListener("keydown", (event) => {
                        if (event.key === "Enter" || event.key === " ") runAction(event);
                    });
                });


                const onOutsideClick = (event) => {
                    if (!menu.contains(event.target) &&
                        !triggerElement.contains(event.target)
                    )
                        closeRowActionMenu();
                };
                const onKeyDown = (event) => {
                    if (event.key === "Escape") closeRowActionMenu();
                };
                activeRowActionMenu = {
                    menu,
                    trigger: triggerElement,
                    onOutsideClick,
                    onKeyDown,
                };
                document.addEventListener("mousedown", onOutsideClick);
                document.addEventListener("keydown", onKeyDown);
            };

            // collapsible groups
            Grouping.register("Balance Sheet", {
                by: ["section"],
                nestedBy: ["subsection", "category", "accountCategory"],
                showCount: false,
            });

            const negative = (v) => (v < 0 ? "rp-bad" : "");

            const isPlTotal = (row) =>
                row && ["subtotal", "total", "grand", "net"].includes(row.rowType);

            const plAccountClass = (v, row) => {
                if (!row) return "";
                const isMain = [
                    "header",
                    "parent",
                    "subtotal",
                    "total",
                    "grand",
                    "net",
                ].includes(row.rowType);
                return [
                        row.level > 0 ?
                        `rp-pl-indent-${Math.min(row.level, 10)}` :
                        "",
                        isMain ? "font-weight-bold" : "",
                    ]
                    .filter(Boolean)
                    .join(" ");
            };

            const isPlAccountDrillRow = (row) =>
                row && ["account", "line"].includes(row.rowType) &&
                row.accountCode &&
                row.accountName;

            const plAccountDrillClass = (v, row) => [
                    plAccountClass(v, row),
                    isPlAccountDrillRow(row) ?
                    "rp-pl-account-link" :
                    "rp-pl-account-static",
                ]
                .filter(Boolean)
                .join(" ");

            const drillToProfitLossAccount = (row, el) => {
                if (!isPlAccountDrillRow(row)) return;
                drillToAccountReport(
                    row,
                    el,
                    "Transaction Detail by Account",
                    true,
                );
            };

            const plAmountClass = (v, row) => {
                const classes = [];
                if (isPlTotal(row)) classes.push("rp-subtotal-line");
                if (v < 0) classes.push("rp-bad");
                return classes.join(" ");
            };

            const plPercentClass = (v, row) =>
                isPlTotal(row) ? "rp-subtotal-line" : "";

            const hidePlHeaderAmounts = (v, row) =>
                row.rowType === "header" || row.rowType === "parent" ? "" : undefined;

            const blankJournalSubtotal = (value, row) =>
                row.rowType === "subtotal" ? "" : undefined;
            const blankLedgerSubtotal = (value, row) => ["header", "parent", "account", "subtotal"].includes(row.rowType) ?
                "" :
                undefined;
            const ledgerHierarchyClass = (value, row) => [
                    row.level > 0 ?
                    `rp-ledger-indent-${Math.min(row.level, 10)}` :
                    "", ["header", "parent", "account", "subtotal"].includes(row.rowType) ?
                    "font-weight-bold" :
                    "",
                ]
                .filter(Boolean)
                .join(" ");
            const blankLedgerHeaderAmount = (value, row) => ["header", "parent", "account"].includes(row.rowType) ? "" : undefined;

            // shared column sets
            const ledgerCols = (withBalance, showAccount, drillBalance = false) => [
                text("Transaction", {
                    key: "transaction",
                    width: showAccount ? "260" : "300",
                    format: (value, row) => {
                        if (row.rowType === "header")
                            return row.display || row.account;
                        if (["parent", "account"].includes(row.rowType))
                            return row.account;
                        if (row.rowType === "subtotal")
                            return "Total " + row.account;
                        return value;
                    },
                    className: ledgerHierarchyClass,
                }),
                text("Date", { key: "date", width: "110", format: blankLedgerSubtotal }),
                ...(showAccount ? [
                    text("Item Name", {
                        key: "itemName",
                        width: "160",
                        format: blankLedgerSubtotal,
                    }),
                ] : []),
                text("Number", { key: "number", width: "130", format: blankLedgerSubtotal }),
                text("Name", {
                    key: "name",
                    width: "180",
                    format: blankLedgerSubtotal,
                }),
                money("Debit", {
                    key: "debit",
                    width: "110",
                    format: blankLedgerHeaderAmount,
                    className: (v, row) =>
                        row.rowType === "subtotal" ? "rp-subtotal-line" : "",
                }),
                money("Credit", {
                    key: "credit",
                    width: "110",
                    format: blankLedgerHeaderAmount,
                    className: (v, row) =>
                        row.rowType === "subtotal" ? "rp-subtotal-line" : "",
                }),
                ...(withBalance ? [
                    money("Balance", {
                        key: "balance",
                        width: "115",
                        total: false,
                        format: drillBalance ?
                            (value, row) => row.rowType === "header" ? "" : undefined :
                            blankLedgerHeaderAmount,
                        click: drillBalance ?
                            (row, el) => drillToAccountReport(
                                row,
                                el,
                                "Transaction Detail by Account",
                                true,
                            ) : null,
                        clickWhen: drillBalance ?
                            (row) => row.rowType === "line" &&
                                row.accountCode &&
                                row.accountName : null,
                        icon: drillBalance ? "ti-external-link" : null,
                        className: (v, row) =>
                            row.rowType === "subtotal" ? "rp-subtotal-line" : "",
                    }),
                ] : []),
                text("Memo", {
                    key: "memo",
                    width: "220",
                    format: blankLedgerSubtotal,
                }),
                text("", {
                    key: "transactionID",
                    width: "50",
                    icon: "ti-dots-vertical",
                    className: "rp-row-action-column rp-row-action-trigger",
                    stickyRight: true,
                    format: (value, row) => ["header", "parent", "account", "subtotal"].includes(row.rowType) ?
                        "" : " ",
                    click: toggleRowActionMenu,
                }),

            ];

            const plCols = () => [
                text("Account", {
                    key: "display",
                    className: plAccountDrillClass,
                    click: drillToProfitLossAccount,
                    format: (v, row) =>
                        row.display ||
                        (row.accountCode ?
                            row.accountCode + " - " + row.accountName :
                            row.accountName || ""),
                }),
                money("Amount", {
                    key: "amount",
                    total: false,
                    className: plAmountClass,
                }),
            ];

            const asOf = [
                "Trial Balance",
                "Balance Sheet",
                "Adjusted Trial Balance",
                "Working Trial Balance",
            ];

            const grandTotalSettings = {
                totalLabel: "Grand Total",
                totalLabelAlign: "left",
                totalLabelPosition: "first",
                zeroAsDash: true,
            };

            Registry.add({
                order: 70,
                category: "Accounting",
                groups: [{
                    name: "Accounting",
                    monthPicker: true,
                    filters: ["dateRange", "transactionType", "account", "class"],
                    reports: [
                        "General Ledger",
                        "Balance Sheet",
                        "Trial Balance",
                        "Adjusted Trial Balance",
                        "Working Trial Balance",
                        "Journal Report",
                        "Transaction Detail by Account",
                        "Profit/Loss Standard",
                        "Profit/Loss Detail",
                        "Profit/Loss Year to Date",
                        "Profit/Loss by Month",
                        "Profit/Loss by Year",
                        "Profit/Loss by Class",
                        "Profit/Loss by Customer",
                    ],
                }, ],
                reportDatePresets: {
                    "Trial Balance": "all",
                    "Profit/Loss by Month": "all",
                    "Profit/Loss by Year": "all",
                },
                reportUiSettings: {
                    "General Ledger": Object.assign({}, grandTotalSettings),
                    "Balance Sheet": Object.assign({ width: "min(100%, 1000px)" },
                        grandTotalSettings,
                    ),
                    "Trial Balance": Object.assign({ width: "min(100%, 1200px)" },
                        grandTotalSettings,
                    ),
                    "Adjusted Trial Balance": Object.assign({}, grandTotalSettings),
                    "Working Trial Balance": Object.assign({}, grandTotalSettings),
                    "Journal Report": Object.assign({}, grandTotalSettings),
                    "Transaction Detail by Account": Object.assign({}, grandTotalSettings),
                    "Profit/Loss Standard": Object.assign({ width: "min(100%, 1200px)" },
                        grandTotalSettings,
                    ),
                    "Profit/Loss Detail": Object.assign({}, grandTotalSettings),
                    "Profit/Loss Year to Date": Object.assign({}, grandTotalSettings),
                    "Profit/Loss by Month": Object.assign({}, grandTotalSettings),
                    "Profit/Loss by Year": Object.assign({}, grandTotalSettings),
                    "Profit/Loss by Class": Object.assign({}, grandTotalSettings),
                    "Profit/Loss by Customer": Object.assign({}, grandTotalSettings),
                },
                reportPrintSettings: {
                    "Trial Balance": {
                        paperSize: "A4",
                        landscape: false,
                        width: "200mm",
                    },
                    "Profit/Loss Standard": {
                        paperSize: "A4",
                        landscape: false,
                        width: "200mm",
                    },
                    "Profit/Loss Year to Date": {
                        paperSize: "A4",
                        landscape: false,
                        width: "200mm",
                    },
                    "Profit/Loss by Class": {
                        paperSize: "A4",
                        landscape: false,
                        width: "200mm",
                    },
                    "Profit/Loss by Year": {
                        paperSize: "A4",
                        landscape: false,
                        width: "230mm",
                    },
                    "Balance Sheet": {
                        paperSize: "A4",
                        landscape: false,
                        width: "200mm",
                    },
                },
                reportFilters: {
                    "General Ledger": { add: ["showAllAccounts"] },
                },
                columns: [{
                        reports: ["General Ledger"],
                        strict: true,
                        hideNo: true,
                    columns: cols(...ledgerCols(true, false, true)),
                    }, {
                        reports: ["Transaction Detail by Account"],
                        strict: true,
                        hideNo: true,
                        columns: cols(...ledgerCols(true, true)),
                    },

                    {
                        reports: ["Balance Sheet"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Section", { key: "section", hidden: true }),

                            text("Account", {
                                key: "accountName",
                                format: (value, row) =>
                                    row.accountCode ?
                                    row.accountCode + "-" + row.accountName : row.accountName,
                                className: (value, row) => [
                                        row.level > 0 ?
                                        `rp-bs-indent-${Math.min(row.level, 10)}` :
                                        "", ["parent", "subtotal", "total", "grand"].includes(
                                            row.rowType,
                                        ) ?
                                        "font-weight-bold" :
                                        "",
                                    ]
                                    .filter(Boolean)
                                    .join(" "),
                            }),
                            text("", {
                                key: "rowType",
                                width: 32,
                                icon: "ti-external-link",
                                format: (value, row) => ["account", "parent"].includes(row.rowType) &&
                                    row.accountCode ?
                                    " " : "",
                                click: drillToLedger,
                            }),
                            num("Amount", {
                                key: "amount",
                                decimals: 2,
                                total: false,
                                format: (v, row) => ["header", "parent"].includes(row.rowType) ?
                                    "" : undefined,
                                className: (v, row) => [
                                        v < 0 ? "rp-bad" : "",
                                        row.rowType === "subtotal" ?
                                        "rp-subtotal-line" :
                                        "",
                                    ]
                                    .filter(Boolean)
                                    .join(" "),
                            }),
                        ),
                    },

                    {
                        reports: ["Profit/Loss Standard"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account", {
                                key: "display",
                                className: plAccountDrillClass,
                                click: drillToProfitLossAccount,
                            }),
                            money("Amount", {
                                key: "amount",
                                dateLabel: true,
                                total: false,
                                className: plAmountClass,
                                format: (v, row) =>
                                    row.rowType === "header" || row.rowType === "parent" ?
                                    "" : undefined,
                            }),
                            num("Percent %", {
                                key: "percent",
                                total: false,
                                className: plPercentClass,
                                format: (v, row) =>
                                    row.rowType === "header" || row.rowType === "parent" ?
                                    "" : `${Number(v).toFixed(2)} %`,
                            }),
                        ),
                    },

                    {
                        reports: ["Profit/Loss Detail"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account", {
                                key: "display",
                                className: plAccountDrillClass,
                                click: drillToProfitLossAccount,
                            }),
                            text("Transaction", {
                                key: "transaction",
                            }),
                            text("Date", {
                                key: "date",
                            }),
                            text("Number", {
                                key: "number",
                            }),
                            text("name", {
                                key: "name",
                            }),
                            money("Amount", {
                                key: "amount",
                                total: false,
                                className: plAmountClass,
                                format: (v, row) =>
                                    row.rowType === "header" ||
                                    row.rowType === "parent" ||
                                    row.rowType === "account" ?
                                    "" : undefined,
                            }),

                            text("memo", {
                                key: "memo",
                                width: "300",
                            }),
                        ),
                    },
                    {
                        reports: ["Profit/Loss Year to Date"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account", {
                                key: "display",
                                className: plAccountDrillClass,
                                click: drillToProfitLossAccount,
                            }),
                            money("Year to Date", {
                                key: "yearToDate",
                                className: plAmountClass,
                                format: (v, row) =>
                                    row.rowType === "header" || row.rowType === "parent" ?
                                    "" : undefined,
                            }),
                            money("Previous Month", {
                                key: "previousMonth",
                                className: plAmountClass,
                                format: (v, row) =>
                                    row.rowType === "header" || row.rowType === "parent" ?
                                    "" : undefined,
                            }),
                            money("This Month", {
                                key: "thisMonth",
                                className: plAmountClass,
                                format: (v, row) =>
                                    row.rowType === "header" || row.rowType === "parent" ?
                                    "" : undefined,
                            }),
                            money("This Month Last Year", {
                                key: "thisMonthLastYear",
                                className: plAmountClass,
                                format: (v, row) =>
                                    row.rowType === "header" || row.rowType === "parent" ?
                                    "" : undefined,
                            }),
                        ),
                    },
                    {
                        reports: ["Profit/Loss by Month"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account", {
                                key: "display",
                                className: plAccountDrillClass,
                                click: drillToProfitLossAccount,
                            }),
                            money("Total", {
                                key: "amount",
                                total: false,
                                className: plAmountClass,
                                format: hidePlHeaderAmounts,
                            }),
                        ),
                    },

                    {
                        reports: ["Trial Balance", ""],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account Code", { key: "accountCode" }),
                            text("Account Name", {
                                key: "accountName",
                                click: drillToLedger,
                            }),
                            money("Debit", { key: "debit" }),
                            money("Credit", { key: "credit" }),
                        ),
                    },
                    {
                        reports: ["Working Trial Balance", ""],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account Code", { key: "accountCode" }),
                            text("Account Name", {
                                key: "accountName",
                                click: drillToLedger,
                            }),
                            money("Beginning ", { key: "begin", exactTotal: true }),
                            money("Debit", { key: "debit" }),
                            money("Credit", { key: "credit" }),
                            money("Change", { key: "change", exactTotal: true }),
                            money("Ending ", { key: "ending", exactTotal: true }),
                        ),
                    },
                    {
                        reports: ["Adjusted Trial Balance"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account Code", { key: "accountCode" }),
                            text("Account Name", { key: "accountName" }),
                            money("Debit", { key: "debit", parent: "Unadjusted" }),
                            money("Credit", { key: "credit", parent: "Unadjusted" }),
                            money("Debit", {
                                key: "adjDebit",
                                parent: "Adjustments",
                                exactTotal: true,
                            }),
                            money("Credit", {
                                key: "adjCredit",
                                parent: "Adjustments",
                                exactTotal: true,
                            }),
                            money("Debit", { key: "adjustedDebit", parent: "Adjusted" }),
                            money("Credit", { key: "adjustedCredit", parent: "Adjusted" }),
                        ),
                    },
                    {
                        reports: ["Profit/Loss by Year"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Account", {
                                key: "display",
                                className: plAccountDrillClass,
                                click: drillToProfitLossAccount,
                            }),
                            money("Total", {
                                key: "amount",
                                total: false,
                                className: plAmountClass,
                                format: hidePlHeaderAmounts,
                            }),
                        ),
                    },

                    {
                        reports: [
                            "Profit/Loss by Class",
                            "Profit/Loss by Customer",
                        ],
                        strict: true,
                        hideNo: true,
                        columns: cols(...plCols()),
                    },

                    {
                        reports: ["Journal Report"],
                        strict: true,
                        hideNo: true,
                        columns: cols(
                            text("Type", {
                                key: "transaction",
                                format: (value, row) =>
                                    row.rowType === "subtotal" ? "Total" : undefined,
                                className: (v, row) =>
                                    row.rowType === "subtotal" ? "font-weight-bold" : "",
                            }),
                            text("Date", {
                                key: "date",
                                format: blankJournalSubtotal,
                            }),
                            text("Number", {
                                key: "number",
                                format: blankJournalSubtotal,
                                click: (row) => {
                                    if (row.rowType === "subtotal") return;
                                    if (row.expenseID > 0)
                                        window.showPaymentVoucher(
                                            row.expenseID,
                                            1,
                                            row.status,
                                            row.paymentNo,
                                        );
                                    else if (row.number)
                                        window.PrintInvoice(
                                            row.customerID || 0,
                                            row.number,
                                            0,
                                            10,
                                            "",
                                            false,
                                        );
                                },
                            }),
                            text("Account Name", {
                                key: "accountName",
                                format: blankJournalSubtotal,
                            }),
                            text("Name", {
                                key: "name",
                                format: blankJournalSubtotal,
                            }),
                            money("Debit", {
                                key: "debit",
                                className: (v, row) =>
                                    row.rowType === "subtotal" ? "rp-subtotal-line" : "",
                            }),
                            money("Credit", {
                                key: "credit",

                                className: (v, row) =>
                                    row.rowType === "subtotal" ? "rp-subtotal-line" : "",
                            }),

                            text("Memo", {
                                key: "memo",
                                width: "300",
                                format: blankJournalSubtotal,
                            }),
                            text("", {
                                key: "transactionID",
                                width: "50",
                                icon: "ti-dots-vertical",
                                className: "rp-row-action-column rp-row-action-trigger",
                                stickyRight: true,
                                format: (value, row) =>
                                    row.rowType === "subtotal" ? "" : " ",
                                click: (row, trigger) =>
                                    toggleRowActionMenu(row, trigger, {
                                        hideJournalView: true,
                                    }),
                            }),
                        ),
                    },
                ],
            });
        },
    ]);
})();