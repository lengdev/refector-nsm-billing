(function() {
    "use strict";

    app.factory("ReportJournalViewDialog", [
        "$rootScope",
        "$http",
        "$templateRequest",
        function($rootScope, $http, $templateRequest) {
            let activeDialog = null;
            let openingId = 0;
            const templateUrl = window.apiBaseUrl + "App/views/JournalView/index.html";
            let stylesheetRequest = null;

            const ensureStylesheet = () => {
                if (stylesheetRequest) return stylesheetRequest;
                if (document.getElementById("rp-journal-view-dialog-styles"))
                    return Promise.resolve();

                const link = document.createElement("link");
                link.id = "rp-journal-view-dialog-styles";
                link.rel = "stylesheet";
                link.href = window.apiBaseUrl + "App/Views/JournalView/style.css";
                stylesheetRequest = new Promise((resolve, reject) => {
                    link.addEventListener("load", resolve, { once: true });
                    link.addEventListener("error", () => {
                        stylesheetRequest = null;
                        link.remove();
                        reject(new Error("Failed to load the journal view styles."));
                    }, { once: true });
                });
                document.head.appendChild(link);
                return stylesheetRequest;
            };

            const valueOf = (value, camelName, pascalName) =>
                value && value[camelName] != null ?
                value[camelName] :
                value && value[pascalName];

            const formatDate = (value) => {
                if (!value) return "";
                const dotNetDate = /^\/Date\((-?\d+)(?:[+-]\d{4})?\)\/$/.exec(
                    String(value),
                );
                const date = new Date(dotNetDate ? Number(dotNetDate[1]) : value);
                if (Number.isNaN(date.getTime())) return String(value);
                const pad = (part) => String(part).padStart(2, "0");
                return `${pad(date.getDate())}-${pad(date.getMonth() + 1)}-${date.getFullYear()}`;
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

            const createCell = (row, value, className, columnIndex) => {
                const cell = document.createElement("td");
                cell.textContent = value == null ? "" : String(value);
                if (className) cell.className = className;
                cell.dataset.jvColumn = String(columnIndex);
                row.appendChild(cell);
            };

            const reportRowValues = (data, sourceRow, detail, transactionId) => [
                valueOf(data, "transactionType", "TransactionType") ||
                    valueOf(sourceRow, "transaction", "Transaction") ||
                    valueOf(sourceRow, "typeLabel", "TypeLabel") ||
                    "",
                valueOf(sourceRow, "number", "Number") || transactionId,
                formatDate(
                    valueOf(sourceRow, "date", "Date") ||
                        valueOf(data, "transactionDate", "TransactionDate"),
                ),
                valueOf(sourceRow, "name", "Name") ||
                    valueOf(data, "name", "Name") ||
                    "",
                valueOf(detail, "itemName", "ItemName") || "",
                valueOf(detail, "account", "Account") || "",
                Number(valueOf(detail, "debit", "Debit")) || 0,
                Number(valueOf(detail, "credit", "Credit")) || 0,
                valueOf(detail, "memo", "Memo") ||
                    valueOf(data, "memo", "Memo") ||
                    "",
            ];

            const setupCustomize = (overlay) => {
                const panel = overlay.querySelector(
                    "[data-journal-view-customize-panel]",
                );
                const button = overlay.querySelector(
                    "[data-journal-view-customize]",
                );
                const headers = overlay.querySelectorAll("thead th");

                Array.from(headers).forEach((header, index) => {
                    const label = document.createElement("label");
                    const checkbox = document.createElement("input");
                    checkbox.type = "checkbox";
                    checkbox.checked = true;
                    checkbox.addEventListener("change", () => {
                        overlay
                            .querySelectorAll(`[data-jv-column="${index}"]`)
                            .forEach((cell) => {
                                cell.style.display = checkbox.checked ? "" : "none";
                            });
                    });
                    label.appendChild(checkbox);
                    label.appendChild(
                        document.createTextNode(
                            header.textContent.trim() || `Column ${index + 1}`,
                        ),
                    );
                    panel.appendChild(label);
                });

                button.addEventListener("click", () => {
                    panel.hidden = !panel.hidden;
                    button.setAttribute("aria-expanded", String(!panel.hidden));
                });
            };

            const render = (dialog, data) => {
                if (activeDialog !== dialog) return;
                dialog.data = data;
                const sourceRow = dialog.sourceRow;
                const details = valueOf(data, "details", "Details") || [];
                const body = dialog.overlay.querySelector("tbody");
                body.textContent = "";
                let totalDebit = 0;
                let totalCredit = 0;
                dialog.overlay.querySelector(
                    "[data-journal-view-company]",
                ).textContent = dialog.companyName;

                details.forEach((detail) => {
                    const values = reportRowValues(
                        data,
                        sourceRow,
                        detail,
                        dialog.transactionId,
                    );
                    const debit = values[6];
                    const credit = values[7];
                    totalDebit += debit;
                    totalCredit += credit;
                    const row = document.createElement("tr");
                    values.forEach((value, cellIndex) =>
                        createCell(
                            row,
                            cellIndex === 6 || cellIndex === 7
                                ? formatAmount(value)
                                : value,
                            cellIndex === 6 || cellIndex === 7
                                ? "rp-jv-number"
                                : "",
                            cellIndex,
                        ),
                    );
                    body.appendChild(row);
                });

                if (!details.length) {
                    const row = document.createElement("tr");
                    const cell = document.createElement("td");
                    cell.colSpan = 9;
                    cell.className = "rp-jv-error";
                    cell.textContent = "No journal lines are available.";
                    row.appendChild(cell);
                    body.appendChild(row);
                }

                dialog.overlay.querySelector("[data-journal-view-debit]").textContent =
                    formatAmount(totalDebit);
                dialog.overlay.querySelector("[data-journal-view-credit]").textContent =
                    formatAmount(totalCredit);
            };

            const showError = (dialog, error) => {
                if (activeDialog !== dialog) return;
                console.error("Failed to load the journal report.", error);
                const body = dialog.overlay.querySelector("tbody");
                body.textContent = "";
                const row = document.createElement("tr");
                const cell = document.createElement("td");
                cell.colSpan = 9;
                cell.className = "rp-jv-error";
                const message = error && error.data &&
                    (error.data.message || error.data.Message);
                cell.textContent = message ?
                    `Unable to load journal report: ${message}` :
                    "Unable to load journal report. Please try again.";
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
                const workbook = new ExcelJS.Workbook();
                const sheet = workbook.addWorksheet("Journal View");
                sheet.addRow([dialog.companyName]);
                sheet.mergeCells("A1:I1");
                sheet.getRow(1).font = { bold: true, size: 15 };
                sheet.addRow(["Journal Report"]);
                sheet.mergeCells("A2:I2");
                sheet.getRow(2).font = { bold: true };
                const headers = [
                    "Tran Type",
                    "Number",
                    "Date",
                    "Name",
                    "Item",
                    "Account",
                    "Debit",
                    "Credit",
                    "Memo",
                ];
                sheet.addRow(headers);
                sheet.getRow(3).font = { bold: true };

                let totalDebit = 0;
                let totalCredit = 0;
                details.forEach((detail) => {
                    const values = reportRowValues(
                        data,
                        dialog.sourceRow,
                        detail,
                        transactionId,
                    );
                    const debit = values[6];
                    const credit = values[7];
                    totalDebit += debit;
                    totalCredit += credit;
                    sheet.addRow(values);
                });
                const totalRow = sheet.addRow([
                    "Grand Total",
                    "",
                    "",
                    "",
                    "",
                    "",
                    totalDebit,
                    totalCredit,
                    "",
                ]);
                totalRow.font = { bold: true };
                sheet.columns = [
                    { width: 18 },
                    { width: 22 },
                    { width: 16 },
                    { width: 26 },
                    { width: 22 },
                    { width: 38 },
                    { width: 16 },
                    { width: 16 },
                    { width: 48 },
                ];
                [7, 8].forEach((column) => {
                    sheet.getColumn(column).numFmt = "#,##0.00";
                });

                workbook.xlsx.writeBuffer().then((buffer) => {
                    const blob = new Blob([buffer], {
                        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    });
                    const url = URL.createObjectURL(blob);
                    const link = document.createElement("a");
                    link.href = url;
                    link.download = `JournalReport-${transactionId}.xlsx`;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                    window.setTimeout(() => URL.revokeObjectURL(url), 1000);
                }).catch((error) => {
                    console.error("Failed to export the journal report.", error);
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
                    window.alert("This journal entry has an invalid ID.");
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
                        "#rp-journal-view-dialog-template",
                    );
                    const dialogContent =
                        template && template.content.firstElementChild;
                    if (!dialogContent)
                        throw new Error("The journal view dialog template is missing.");

                    const overlay = dialogContent.cloneNode(true);
                    document.body.appendChild(overlay);
                    const dialog = {
                        overlay,
                        sourceRow,
                        transactionId,
                        companyName:
                            $rootScope.CompanyName || $rootScope.companyName || "",
                        data: null,
                        onKeyDown: null,
                    };
                    dialog.onKeyDown = (event) => {
                        if (event.key === "Escape") close(dialog);
                    };
                    activeDialog = dialog;
                    document.addEventListener("keydown", dialog.onKeyDown);
                    overlay.addEventListener("click", (event) => {
                        if (event.target === overlay ||
                            (event.target && event.target.closest("[data-journal-view-close]"))
                        )
                            close(dialog);
                    });
                    overlay.querySelector("[data-journal-view-export]").addEventListener(
                        "click",
                        () => exportExcel(dialog, transactionId),
                    );
                    overlay.querySelector("[data-journal-view-print]").addEventListener(
                        "click",
                        () => window.print(),
                    );
                    setupCustomize(overlay);
                    overlay.querySelector("[data-journal-view-close]").focus();

                    $http.get(
                        window.apiBaseUrl + "api/AllReport/transaction-log",
                        { params: { transactionId } },
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
                        if (!transaction || responseId !== transactionId)
                            throw new Error(
                                "The server returned data for a different transaction.",
                            );
                        render(dialog, transaction);
                    }).catch((error) => showError(dialog, error));
                }).catch((error) => {
                    console.error("Failed to load the journal report dialog.", error);
                    window.alert("Unable to open the journal report.");
                });
            };

            return { open };
        },
    ]);
})();
