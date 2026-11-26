// ~/Scripts/custom/kendoGridExport.js
// Shared PDF/Excel export logic for all Kendo Grids on the page.
// Loaded ONCE via <script src="~/Scripts/custom/kendoGridExport.js"></script>
// in your _Layout, instead of being re-generated inline for every grid.

// ── Loader overlay: injected once into the DOM, reused by every grid ──────
function _kEnsureLoader() {
    if (document.getElementById("_kPdfLoader")) return;

    var style = document.createElement("style");
    style.textContent = [
        "@keyframes _kSpin { to { transform: rotate(360deg); } }",
        "#_kPdfLoader {",
        "  position:fixed; inset:0; z-index:99999;",
        "  background:rgba(0,0,0,.45);",
        "  display:flex; align-items:center; justify-content:center;",
        "  opacity:0; pointer-events:none;",
        "  transition:opacity .2s;",
        "}",
        "#_kPdfLoader.show { opacity:1; pointer-events:all; }",
        "#_kPdfLoaderBox {",
        "  background:#fff; border-radius:14px;",
        "  padding:28px 40px;",
        "  display:flex; flex-direction:column; align-items:center; gap:14px;",
        "  min-width:220px;",
        "  box-shadow:0 20px 60px rgba(0,0,0,.18);",
        "}",
        "#_kPdfSpinner {",
        "  width:38px; height:38px;",
        "  border:3.5px solid #e4e4e7;",
        "  border-top-color:" +
            (localStorage.getItem("BaseSystemBGColor") || "#6c63ff") +
            ";",
        "  border-radius:50%;",
        "  animation:_kSpin .75s linear infinite;",
        "}",
        "#_kPdfLoaderMsg { font-size:13px; color:#71717a; font-family:Inter,sans-serif; text-align:center; }",
        "#_kPdfLoaderSub { font-size:11px; color:#a1a1aa; margin-top:-6px; text-align:center; }",
    ].join("");
    document.head.appendChild(style);

    var el = document.createElement("div");
    el.id = "_kPdfLoader";
    el.innerHTML =
        '<div id="_kPdfLoaderBox">' +
        '<div id="_kPdfSpinner"></div>' +
        '<span id="_kPdfLoaderMsg">Please wait…</span>' +
        '<span id="_kPdfLoaderSub"></span>' +
        "</div>";
    document.body.appendChild(el);
}

function _kShowLoader(msg, sub) {
    _kEnsureLoader();
    document.getElementById("_kPdfLoaderMsg").textContent =
        msg || "Please wait…";
    document.getElementById("_kPdfLoaderSub").textContent = sub || "";
    document.getElementById("_kPdfLoader").classList.add("show");
}
function _kUpdateLoader(msg, sub) {
    if (msg !== undefined)
        document.getElementById("_kPdfLoaderMsg").textContent = msg;
    if (sub !== undefined)
        document.getElementById("_kPdfLoaderSub").textContent = sub;
}
function _kHideLoader() {
    document.getElementById("_kPdfLoader").classList.remove("show");
}

function _flattenKendoGroups(data) {
    var result = [];
    data.forEach(function (item) {
        if (item.items) {
            result = result.concat(_flattenKendoGroups(item.items));
        } else {
            result.push(item.toJSON ? item.toJSON() : item);
        }
    });
    return result;
}

/**
 * PDF export handler shared by every Kendo Grid.
 * @param {Object} e - Kendo pdfExport event
 * @param {Object} config - { reportTitle, serverRoot }
 */
function kendoGridPdfExport(e, config) {
    e.preventDefault();
    var grid = e.sender;
    var systemId = localStorage.getItem("SystemID");
    var dataSource = grid.dataSource;

    var gridCols = grid.columns
        .filter(function (c) {
            return (
                !c.hidden &&
                c.field &&
                c.title.toLowerCase().trim() !== "action"
            );
        })
        .map(function (c) {
            return {
                field: c.field,
                title: c.title || c.field,
                type: c.type || null,
                format: c.format || null,
            };
        });

    localStorage.setItem("kendo_grid_columns", JSON.stringify(gridCols));

    var reportLinks = $(".breadcrumb").find("li:nth-child(3) span").text();
    reportLinks = reportLinks.split(/(?=[A-Z])/).join(" ");
    var titleToUse =
        config.reportTitle || config.headerReportNameExcel || reportLinks;
    localStorage.setItem("report_title", titleToUse);

    _kShowLoader("Opening report…", "Fetching all records");

    _kFetchAllRows(dataSource)
        .then(function (allData) {
            var flatData = _flattenKendoGroups(allData);
            var gridDataJson = JSON.stringify(flatData);
            localStorage.setItem("kendo_grid_export", gridDataJson);

            _kUpdateLoader(
                "Generating report…",
                "Please do not close this page",
            );

            return $.ajax({
                url: config.serverRoot + "KendoPDFExport",
                type: "POST",
                headers: { Authorization: "Bearer " + access_Token },
                data: { data: gridDataJson, systemId: systemId },
            });
        })
        .then(function (response) {
            _kUpdateLoader("Done!", "Opening in new tab…");
            var newTab = window.open("", "_blank");
            if (newTab) {
                newTab.document.open();
                newTab.document.write(response);
                newTab.document.close();
            } else {
                console.error("Popup blocked by browser");
            }
            setTimeout(_kHideLoader, 800);
        })
        .catch(function (err) {
            _kHideLoader();
            console.error("Error:", err);
            alert("Failed to generate report. Please try again.");
        });
}

function _kFetchAllRows(dataSource) {
    var d = $.Deferred();

    if (!dataSource.options.serverPaging) {
        try {
            // Grouping (client-side) makes data() return group wrapper
            // objects ({ items: [...] }) instead of flat row objects.
            // Query.filter()/.sort() expect real fields, so they throw
            // (or silently corrupt results) if we hand them groups.
            var rawData = dataSource.data();
            var isGrouped = dataSource.group() && dataSource.group().length > 0;

            var flatSource = isGrouped ? _flattenKendoGroups(rawData) : rawData;

            var Query = kendo.data.Query;
            var query = new Query(flatSource);

            var filter = dataSource.filter();
            var sort = dataSource.sort();

            // Older Kendo builds throw if filter()/sort() are called
            // with undefined instead of just being skipped.
            if (filter) query = query.filter(filter);
            if (sort) query = query.sort(sort);

            var allRows = query.toArray();
            d.resolve(allRows);
        } catch (err) {
            console.error("_kFetchAllRows (local) failed:", err);
            d.reject(err);
        }
        return d.promise();
    }

    dataSource.transport.read({
        data: {
            page: 1,
            pageSize: dataSource.total() || 999999,
            skip: 0,
            take: dataSource.total() || 999999,
            sort: dataSource.sort(),
            filter: dataSource.filter(),
        },
        success: function (response) {
            var data = response.data || response;
            d.resolve(data);
        },
        error: function (err) {
            d.reject(err);
        },
    });

    return d.promise();
}

/**
 * Excel export handler shared by every Kendo Grid.
 * @param {Object} e - Kendo excelExport event
 * @param {Object} config - {
 *     manualHeader: bool,           // KendoGridExportExcelByManualCode && !KendoTreeList && ShowHeaderInfoExcel
 *     headerReportNameExcel: string,
 *     showPrintDate: string,        // "True"/"False" as sent from server
 *     showFilterDateRange: string
 * }
 */
function kendoGridExcelExport(e, config) {
    _kShowLoader("Exporting to Excel…", "Please do not close this page");

    var sheet = e.workbook.sheets[0];
    var rows = sheet.rows;
    var grid = e.sender;
    var fields = grid.dataSource.options.fields;

    // Strip HTML tags from a value — handles both raw <a href> and encoded &lt;a href&gt;
    function stripHtml(val) {
        if (!val || typeof val !== "string") return val;
        return val.replace(/<[^>]*>/g, "");
    }

    // 1. Decode cell values + format dates/numbers
    for (var ri = 0; ri < rows.length; ri++) {
        var row = rows[ri];
        for (var ci = 0; ci < row.cells.length; ci++) {
            var cell = row.cells[ci];

            // Always decode then strip HTML first — fixes raw <a href> from SQL showing in export
            if (typeof cell.value === "string") {
                cell.value = stripHtml(decodeEntities(cell.value));
            }

            if (fields[ci] && fields[ci].type) {
                if (fields[ci].type === "date") {
                    var timestamp = Date.parse(cell.value);
                    if (!isNaN(timestamp))
                        cell.value = moment(cell.value).format(
                            fields[ci].format,
                        );
                } else if (fields[ci].type === "number") {
                    cell.value = detectType(decodeEntities(cell.value));
                } else {
                    cell.value = decodeEntities(cell.value);
                }
            } else {
                cell.value = decodeEntities(cell.value);
            }
        }
    }

    // 2. Column auto-width (header + data aware)
    var canvas = document.createElement("canvas");
    var ctx = canvas.getContext("2d");
    sheet.columns.forEach(function (col, colIndex) {
        var maxLen = 0;
        rows.forEach(function (row) {
            var cell = row.cells && row.cells[colIndex];
            if (!cell) return;
            var val =
                cell.value != null ? cell.value.toString() : cell.title || "";
            ctx.font = row.type === "header" ? "bold 14px Arial" : "14px Arial";
            var w = ctx.measureText(val).width + 10;
            if (w > maxLen) maxLen = w;
        });
        col.width = Math.max(maxLen, 60);
    });

    // 3. Row height + Khmer font + alignment
    rows.forEach(function (row) {
        if (!row.cells) return;
        row.height = 30;
        row.cells.forEach(function (cell) {
            cell.fontName = "Khmer OS Siemreap";
            cell.vAlign = "top";
            if (row.type === "header") {
                cell.textAlign = "center";
                cell.bold = true;
                cell.fontSize = 13.5;
                cell.borderTop = { color: "#ccc", size: 1 };
                cell.borderBottom = { color: "#ccc", size: 1 };
                cell.borderLeft = { color: "#ccc", size: 1 };
                cell.borderRight = { color: "#ccc", size: 1 };
            } else {
                cell.fontSize = 13.5;
            }
        });
    });

    // 4. Button label feedback
    $("span.k-button-text").eq(0).html("Exporting...");
    setTimeout(function () {
        $("span.k-button-text").eq(0).html("Export to Excel");
    }, 1000);

    // 5. Optional manual header rows (title / date range / print date)
    if (config.manualHeader) {
        var startLetter = "C";
        var customTitle = config.headerReportNameExcel;
        if (customTitle) {
            localStorage.setItem("custom-excel-title", customTitle);
        }
        var showPrintDate = config.showPrintDate;
        var showFilterDate = config.showFilterDateRange;
        var totalColumns = e.sender.columns.length;
        var hiddenColumnsCount = e.sender.columns.filter(function (c) {
            return c.hidden;
        }).length;
        var visibleColumns = totalColumns - hiddenColumnsCount;
        var titleInfo = getExcelTitleExport();
        var title = titleInfo.title,
            printDate = titleInfo.printDate,
            dateRange = titleInfo.dateRange;
        var endLetter = getExcelColumnLetter(visibleColumns - 2);
        var mergeRange = startLetter + "1:" + endLetter + "1";

        sheet.frozenRows = 2;
        sheet.name = "Sheet1";

        var row1 = [
            { value: "", background: "#ffffff", colSpan: 2 },
            {
                value: title.nameKh,
                fontSize: 18,
                fontFamily: "Khmer OS Muol Light",
                textAlign: "center",
                background: "#ffffff",
                colSpan: visibleColumns - 4,
            },
            { value: "", background: "#ffffff", colSpan: 2 },
        ];

        var row2 = [
            {
                value:
                    (dateRange[0] && showFilterDate
                        ? "From: " + dateRange[0].trim()
                        : "") +
                    "  " +
                    (dateRange[1] && showFilterDate
                        ? "- To: " + dateRange[1].trim()
                        : ""),
                background: "#ffffff",
                textAlign: "left",
                colSpan: 2,
            },
            {
                value: title.subTitle,
                fontSize: 15,
                fontFamily: "Khmer OS Muol Light",
                textAlign: "center",
                wrap: true,
                background: "#ffffff",
                colSpan: visibleColumns - 4,
            },
            {
                value: showPrintDate ? printDate : "",
                textAlign: "right",
                background: "#ffffff",
                colSpan: 2,
            },
        ];

        sheet.rows.splice(0, 0, { cells: row1, type: "header", height: 59 });
        sheet.rows.splice(1, 0, { cells: row2, type: "header", height: 89 });

        sheet.frozenRows = 3;
    }

    // Kendo takes over and triggers the file download right after this handler
    // returns, so hide here once our processing is done.
    setTimeout(_kHideLoader, 400);
}
