(function() {
    "use strict";

    app.factory("ReportTransactionLogDialog", [
        "$http",
        "$templateRequest",
        function($http, $templateRequest) {
            let activeDialog = null;
            let openingId = 0;
            const templateUrl =
                window.apiBaseUrl + "App/views/TrasactionLog/index.html";
            let stylesheetRequest = null;

            const ensureStylesheet = () => {
                if (stylesheetRequest) return stylesheetRequest;
                if (document.getElementById("rp-txlog-dialog-styles"))
                    return Promise.resolve();

                const link = document.createElement("link");
                link.id = "rp-txlog-dialog-styles";
                link.rel = "stylesheet";
                link.href = window.apiBaseUrl + "App/Views/TrasactionLog/style.css?v=2";
                stylesheetRequest = new Promise((resolve, reject) => {
                    link.addEventListener("load", resolve, { once: true });
                    link.addEventListener("error", () => {
                        stylesheetRequest = null;
                        link.remove();
                        reject(new Error("Failed to load the transaction log styles."));
                    }, { once: true });
                });
                document.head.appendChild(link);
                return stylesheetRequest;
            };

            const valueOf = (value, camelName, pascalName) =>
                value && value[camelName] != null ?
                value[camelName] :
                value && value[pascalName];

            const formatDate = (value, includeTime) => {
                if (!value) return "";
                const dotNetDate = /^\/Date\((-?\d+)(?:[+-]\d{4})?\)\/$/.exec(
                    String(value),
                );
                const date = new Date(dotNetDate ? Number(dotNetDate[1]) : value);
                if (Number.isNaN(date.getTime())) return String(value);
                const pad = (part) => String(part).padStart(2, "0");
                const day = pad(date.getDate());
                const month = pad(date.getMonth() + 1);
                if (!includeTime) return `${day}-${month}-${date.getFullYear()}`;
                const hours = date.getHours();
                return `${day}-${month}-${date.getFullYear()} ${pad((hours + 11) % 12 + 1)}:${pad(date.getMinutes())}:${pad(date.getSeconds())} ${hours >= 12 ? "PM" : "AM"}`;
            };

            const formatAmount = (value) => {
                const amount = Number(value);
                return Number.isFinite(amount) ?
                    amount.toLocaleString("en-US", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                    }) :
                    "0.00";
            };

            const close = (dialog) => {
                if (activeDialog !== dialog) return;
                document.removeEventListener("keydown", dialog.onKeyDown);
                dialog.overlay.remove();
                activeDialog = null;
            };

            document.addEventListener("report:dialog-opening", () => {
                openingId++;
                if (activeDialog) close(activeDialog);
            });

            const createCell = (row, value, numeric) => {
                const cell = document.createElement("td");
                cell.textContent = value == null ? "" : String(value);
                if (numeric) cell.className = "rp-txlog-number";
                row.appendChild(cell);
            };

            const render = (dialog, data) => {
                if (activeDialog !== dialog) return;
                dialog.data = data;
                const details = valueOf(data, "details", "Details") || [];
                const rows = details.length ? details : [{}];
                const body = dialog.overlay.querySelector("tbody");
                body.textContent = "";
                let totalDebit = 0;
                let totalCredit = 0;

                rows.forEach((detail, index) => {
                    const debit = Number(valueOf(detail, "debit", "Debit")) || 0;
                    const credit = Number(valueOf(detail, "credit", "Credit")) || 0;
                    totalDebit += debit;
                    totalCredit += credit;
                    const row = document.createElement("tr");
                    [
                        index === 0 ? valueOf(data, "transactionType", "TransactionType") : "",
                        index === 0 ? valueOf(data, "createOrModifyBy", "CreateOrModifyBy") : "",
                        index === 0 ? formatDate(valueOf(data, "transactionDate", "TransactionDate"), false) : "",
                        index === 0 ? formatDate(valueOf(data, "createOrModifyDate", "CreateOrModifyDate"), true) : "",
                        index === 0 ? valueOf(data, "name", "Name") : "",
                        index === 0 ? valueOf(data, "memo", "Memo") : "",
                        valueOf(detail, "itemName", "ItemName"),
                        valueOf(detail, "account", "Account"),
                        formatAmount(debit),
                        formatAmount(credit),
                    ].forEach((value, cellIndex) =>
                        createCell(row, value, cellIndex >= 8),
                    );
                    body.appendChild(row);
                });

                dialog.overlay.querySelector("[data-txlog-debit]").textContent =
                    formatAmount(totalDebit);
                dialog.overlay.querySelector("[data-txlog-credit]").textContent =
                    formatAmount(totalCredit);
            };

            const showError = (dialog, error) => {
                if (activeDialog !== dialog) return;
                console.error("Failed to load the transaction log.", error);
                const body = dialog.overlay.querySelector("tbody");
                body.textContent = "";
                const row = document.createElement("tr");
                const cell = document.createElement("td");
                cell.colSpan = 10;
                cell.className = "rp-txlog-error";
                const message = error && error.data &&
                    (error.data.message || error.data.Message);
                cell.textContent = message ?
                    `Unable to load transaction log: ${message}` :
                    "Unable to load transaction log. Please try again.";
                row.appendChild(cell);
                body.appendChild(row);
            };

            const exportExcel = (dialog, transactionId) => {
                if (!dialog.data) return;
                if (typeof ExcelJS === "undefined") {
                    window.alert("Cannot export because ExcelJS is not loaded.");
                    return;
                }

                const data = dialog.data;
                const details = valueOf(data, "details", "Details") || [];
                const rows = details.length ? details : [{}];
                const workbook = new ExcelJS.Workbook();
                const sheet = workbook.addWorksheet("Transaction Log");
                const headers = [
                    "Tran Type",
                    "Create or Modify By",
                    "Tran Date",
                    "Create or Modify Date",
                    "Name",
                    "Memo",
                    "Item",
                    "Account",
                    "Debit",
                    "Credit",
                ];
                sheet.addRow(["Audit Trail Transaction"]);
                sheet.mergeCells("A1:J1");
                sheet.getRow(1).font = { bold: true, size: 15 };
                sheet.addRow(headers);
                sheet.getRow(2).font = { bold: true };

                let totalDebit = 0;
                let totalCredit = 0;
                rows.forEach((detail, index) => {
                    const debit = Number(valueOf(detail, "debit", "Debit")) || 0;
                    const credit = Number(valueOf(detail, "credit", "Credit")) || 0;
                    totalDebit += debit;
                    totalCredit += credit;
                    sheet.addRow([
                        index === 0 ? valueOf(data, "transactionType", "TransactionType") || "" : "",
                        index === 0 ? valueOf(data, "createOrModifyBy", "CreateOrModifyBy") || "" : "",
                        index === 0 ? formatDate(valueOf(data, "transactionDate", "TransactionDate"), false) : "",
                        index === 0 ? formatDate(valueOf(data, "createOrModifyDate", "CreateOrModifyDate"), true) : "",
                        index === 0 ? valueOf(data, "name", "Name") || "" : "",
                        index === 0 ? valueOf(data, "memo", "Memo") || "" : "",
                        valueOf(detail, "itemName", "ItemName") || "",
                        valueOf(detail, "account", "Account") || "",
                        debit,
                        credit,
                    ]);
                });

                sheet.addRow(["Total", "", "", "", "", "", "", "", totalDebit, totalCredit]);
                sheet.columns = [
                    { width: 24 }, { width: 24 }, { width: 20 }, { width: 28 },
                    { width: 24 }, { width: 30 }, { width: 24 }, { width: 34 },
                    { width: 16 }, { width: 16 },
                ];
                [9, 10].forEach((column) => {
                    sheet.getColumn(column).numFmt = "#,##0.00";
                });

                workbook.xlsx.writeBuffer().then((buffer) => {
                    const blob = new Blob([buffer], {
                        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    });
                    const url = URL.createObjectURL(blob);
                    const link = document.createElement("a");
                    link.href = url;
                    link.download = `TransactionLog-${transactionId}.xlsx`;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                    window.setTimeout(() => URL.revokeObjectURL(url), 1000);
                }).catch((error) => {
                    console.error("Failed to export the transaction log.", error);
                    window.alert("Excel export failed. Please try again.");
                });
            };

            const open = (sourceRow) => {
                const transactionId = Number(
                    sourceRow &&
                    (sourceRow.transactionID != null ?
                        sourceRow.transactionID :
                        sourceRow.TransactionID),
                );
                if (!Number.isInteger(transactionId) || transactionId <= 0) {
                    window.alert("This transaction has an invalid ID.");
                    return;
                }

                document.dispatchEvent(new Event("report:dialog-opening"));
                const requestId = ++openingId;
                ensureStylesheet().then(() => $templateRequest(templateUrl)).then((html) => {
                    if (requestId !== openingId) return;
                    const templateDocument = new DOMParser().parseFromString(
                        html,
                        "text/html",
                    );
                    const template = templateDocument.querySelector(
                        "#rp-txlog-dialog-template",
                    );
                    const dialogContent =
                        template && template.content.firstElementChild;
                    if (!dialogContent) {
                        throw new Error(
                            "The transaction log dialog template is missing.",
                        );
                    }

                    const overlay = dialogContent.cloneNode(true);
                    document.body.appendChild(overlay);
                    const dialog = { overlay, data: null, onKeyDown: null };
                    dialog.onKeyDown = (event) => {
                        if (event.key === "Escape") close(dialog);
                    };
                    activeDialog = dialog;
                    document.addEventListener("keydown", dialog.onKeyDown);
                    overlay.addEventListener("click", (event) => {
                        if (event.target === overlay ||
                            (event.target && event.target.closest("[data-txlog-close]"))
                        )
                            close(dialog);
                    });
                    overlay.querySelector("[data-txlog-print]").addEventListener(
                        "click",
                        () => window.print(),
                    );
                    overlay.querySelector("[data-txlog-export]").addEventListener(
                        "click",
                        () => exportExcel(dialog, transactionId),
                    );
                    overlay.querySelector("[data-txlog-close]").focus();

                    $http.get(
                            window.apiBaseUrl + "api/AllReport/transaction-log", {
                                params: { transactionId },
                            },
                        ).then((response) => {
                            const payload = response.data;
                            const transaction = Array.isArray(payload) ?
                                payload.find((entry) =>
                                    Number(valueOf(entry, "transactionID", "TransactionID")) ===
                                    transactionId,
                                ) :
                                payload;
                            const responseId = Number(
                                valueOf(transaction, "transactionID", "TransactionID"),
                            );
                            if (!transaction ||
                                (Number.isInteger(responseId) && responseId !== transactionId)
                            ) {
                                throw new Error(
                                    "The server returned data for a different transaction.",
                                );
                            }
                            render(dialog, transaction);
                        })
                        .catch((error) => showError(dialog, error));
                }).catch((error) => {
                    console.error("Failed to load the transaction log view.", error);
                    window.alert("Unable to open the transaction log.");
                });
            };

            return { open };
        },
    ]);
})();