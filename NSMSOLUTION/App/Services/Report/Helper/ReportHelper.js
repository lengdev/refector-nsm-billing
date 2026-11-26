// ReportHelper.js
// Every Report helper in ONE file (replaces the whole Helper/ folder).
// Angular registration names are unchanged, so the controller and the
// Categories/*.js files keep working without edits to their injections.
//
// Sections
//   1. Config & filter-field catalog
//   2. Pure services      (Format, Totals, Columns, Grouping, Registry, Options, Excel, Sidebar)
//   3. Page features      (Data, Paging, Sorting, Filters, Date, Combo, Layout, Cells,
//                          Grouping, HeaderGroups, Navigation, Favorites, Export)
(function () {
  "use strict";

  /* ════════════════════════════════════════════════════════════════
   * 1. CONFIG & FILTER FIELDS
   * ════════════════════════════════════════════════════════════════ */

  app.constant("ReportConfig", {
    DEFAULT_TITLE: "Report",
    API_BASE: "api/AllReport/",
    SIDEBAR_KEY: "sidebarCollapsed",
    FAVORITES_KEY: "reportFavorites",
    PAGE_SIZES: [25, 50, 100, 0],
    COMBO_LIMIT: 200,
    FIRST_YEAR: 2020,
    ISO_DATE: /^\d{4}-\d{2}-\d{2}T/,
    MONTH_SHORT: [
      "Jan",
      "Feb",
      "Mar",
      "Apr",
      "May",
      "Jun",
      "Jul",
      "Aug",
      "Sep",
      "Oct",
      "Nov",
      "Dec",
    ],
    MONTH_LABELS: [
      "January",
      "February",
      "March",
      "April",
      "May",
      "June",
      "July",
      "August",
      "September",
      "October",
      "November",
      "December",
    ],
    DATE_PRESETS: [
      { value: "all", label: "All Dates" },
      { value: "today", label: "Today" },
      { value: "week", label: "This Week" },
      { value: "mtd", label: "Month to Date" },
      { value: "month", label: "This Month" },
      { value: "lastMonth", label: "Last Month" },
      { value: "ytd", label: "Year to Date" },
      { value: "custom", label: "Custom" },
    ],
  });

  /* ════════════════════════════════════════════════════════════════
   * 2. PURE SERVICES
   * ════════════════════════════════════════════════════════════════ */

  // ── ReportFormat: raw row values -> display text and links ──────
  app.factory("ReportFormat", [
    "ReportConfig",
    function (Config) {
      const decoder = document.createElement("textarea");

      function prefixOf(row, column, value) {
        const p =
          typeof column.prefix === "function"
            ? column.prefix(value, row)
            : column.prefix;
        return p || "";
      }

      function numberText(row, column, value) {
        const n = Number(value);
        return (
          (n < 0 ? "-" : "") +
          prefixOf(row, column, value) +
          formatNumber(Math.abs(n), column.decimals) +
          suffixOf(row, column, value)
        );
      }

      function formatDate(value) {
        const d = new Date(value);
        if (isNaN(d)) return value;
        return `${String(d.getDate()).padStart(2, "0")}-${Config.MONTH_SHORT[d.getMonth()]}-${d.getFullYear()}`;
      }

      function formatNumber(value, decimals) {
        const n = Number(value);
        const fixed = Number.isInteger(decimals);
        return n.toLocaleString("en-US", {
          minimumFractionDigits: fixed ? decimals : Number.isInteger(n) ? 0 : 2,
          maximumFractionDigits: fixed ? decimals : 2,
        });
      }

      // Exact key, then label, then case-insensitive key.
      function readValue(row, column) {
        if (row[column.key] !== undefined) return row[column.key];
        if (row[column.label] !== undefined) return row[column.label];
        const wanted = column.key.toLowerCase();
        const match = Object.keys(row).find((k) => k.toLowerCase() === wanted);
        return match === undefined ? undefined : row[match];
      }

      function cleanText(value) {
        if (typeof value !== "string" || value.indexOf("&") === -1)
          return value;
        decoder.innerHTML = value;
        return decoder.value.replace(/\s*[\r\n]+\s*/g, " ").trim();
      }

      function isBlank(row, column) {
        const value = readValue(row, column);
        return value == null || value === "";
      }

      // Suffix text: a plain string ("%") or a function (value, row) => " Days".
      function suffixOf(row, column, value) {
        const s =
          typeof column.suffix === "function"
            ? column.suffix(value, row)
            : column.suffix;
        return s || "";
      }

      // Screen only: CSS class for a cell (string or function on the column).
      function cellClass(row, column) {
        if (!column.className) return "";
        return typeof column.className === "function"
          ? column.className(readValue(row, column), row)
          : column.className;
      }

      function display(row, column, zeroAsDash) {
        const value = readValue(row, column);
        if (value == null || value === "") return "-";

        if (column.format) {
          const out = column.format(value, row);
          if (out != null) return out;
        }

        if (
          zeroAsDash &&
          (column.num || column.agg) &&
          String(value).trim() !== "" &&
          Number.isFinite(Number(value)) &&
          Number(value) === 0
        ) {
          return "-";
        }

        if (column.num && !isNaN(Number(value)))
          return numberText(row, column, value);

        if (typeof value === "string" && Config.ISO_DATE.test(value))
          return formatDate(value);

        return cleanText(value);
      }

      // Replaces {field} in column.link with the row's value. "" when a value is missing.
      function linkFor(row, column) {
        if (!column.link) return "";
        let missing = false;
        const url = column.link.replace(/\{(\w+)\}/g, function (_, key) {
          const v = readValue(row, { key: key, label: key });
          if (v == null || v === "") {
            missing = true;
            return "";
          }
          return /^https?:\/\//i.test(v) ? v : encodeURIComponent(v);
        });
        return missing ? "" : url;
      }

      function runAction(row, column, $event) {
        if ($event) $event.preventDefault();
        if (column.click) column.click(row, $event && $event.currentTarget);
      }

      return {
        formatDate,
        formatNumber,
        readValue,
        cleanText,
        isBlank,
        display,
        linkFor,
        runAction,
        suffixOf,
        cellClass,
        prefixOf,
      };
    },
  ]);

  // ── ReportTotals: aggregate rows locally or read the server's totals ──
  app.factory("ReportTotals", [
    "ReportFormat",
    function (Format) {
      const isTotalColumn = (column) => column.total;
      const hasKeys = (obj) => !!obj && Object.keys(obj).length > 0;

      function findTotal(totals, column) {
        const lower = (s) => String(s == null ? "" : s).toLowerCase();

        if (column.exactTotal) {
          const byKey = Object.keys(totals).find(
            (k) => lower(k) === lower(column.key),
          );
          return byKey === undefined ? undefined : totals[byKey];
        }
        const wanted = [column.key, column.label].map((k) => k.toLowerCase());
        const key = Object.keys(totals).find((k) =>
          wanted.includes(k.toLowerCase()),
        );
        return key === undefined ? undefined : totals[key];
      }

      function aggregate(rows, column) {
        const present = rows
          .map((row) => Format.readValue(row, column))
          .filter((v) => v != null && v !== "");
        if (column.agg === "count") return present.length;

        const nums = present.map(Number).filter(Number.isFinite);
        if (!nums.length) return 0;
        switch (column.agg) {
          case "avg":
            return nums.reduce((a, b) => a + b, 0) / nums.length;
          case "min":
            return Math.min.apply(null, nums);
          case "max":
            return Math.max.apply(null, nums);
          default:
            return nums.reduce((a, b) => a + b, 0);
        }
      }

      function sumRows(rows, columns) {
        const totals = {};
        columns.filter(isTotalColumn).forEach((column) => {
          totals[column.key] = aggregate(rows, column);
        });
        return totals;
      }

      function resolveTotals(serverTotals, rows, columns, hasAllRows) {
        if (hasKeys(serverTotals)) return serverTotals;
        return hasAllRows ? sumRows(rows, columns) : {};
      }

      // Formatted total for one column ("" when it has none).
      function format(column, totals) {
        if (!isTotalColumn(column)) return "";
        const raw = findTotal(totals || {}, column);
        if (raw == null || raw === "") return "";
        const value = Number(raw);
        if (!Number.isFinite(value)) return "";
        const rounded = Number.isInteger(column.decimals)
          ? value
          : Math.round(value * 100) / 100;
        const prefix = typeof column.prefix === "string" ? column.prefix : "";
        return (
          (rounded < 0 ? "-" : "") +
          prefix +
          Format.formatNumber(Math.abs(rounded), column.decimals)
        );
      }

      // "key:agg,key:agg" sent to the API as totalFields.
      const fieldsFor = (report) =>
        report.columns
          .filter(isTotalColumn)
          .map((c) => c.key + ":" + c.agg)
          .join(",");

      return {
        isTotalColumn,
        findTotal,
        sumRows,
        resolveTotals,
        format,
        fieldsFor,
      };
    },
  ]);

  // ── ReportColumns: column builders + auto-detection ─────────────
  app.factory("ReportColumns", [
    "ReportFormat",
    function (Format) {
      const ID_KEY = /^(id|ID)$|[a-z0-9](ID|Id)$|_id$/;
      const NON_TOTAL =
        /\b(avg|average|price|rate|age|days|percent|pct|unit)\b/i;

      function keyOf(label) {
        const key = label
          .replace(/#/g, " No")
          .replace(/(\d)\s*-\s*(\d)/g, "$1 To $2")
          .split(/[^A-Za-z0-9]+/)
          .filter(Boolean)
          .map((w) => w[0].toUpperCase() + w.slice(1))
          .join("");
        return /^\d/.test(key) ? "Days" + key : key;
      }

      const fnOrNull = (v) => (typeof v === "function" ? v : null);
      const fnOrString = (v) =>
        typeof v === "function" || typeof v === "string" ? v : null;

      // options:
      //   key, width, hidden, parent, icon
      //   link       "{field}" url template
      //   click      (row, el) => {}
      //   format     (value, row) => text | undefined   (plain text, also used by print/export)
      //   suffix     "%"  or  (value, row) => " Days"
      //   className  "rp-bad"  or  (value, row) => "rp-bad"  (screen only)
      //   decimals   0, 2, 4... fixed decimal places (num only)
      //   agg        "sum"|"avg"|"min"|"max"|"count"
      //   total      (num only) false = no total
      function build(label, options, isNum) {
        const agg = isNum
          ? options.total === false
            ? null
            : options.agg || "sum"
          : options.agg || null;

        return {
          label,
          key: options.key || keyOf(label),
          num: isNum,
          agg,
          total: !!agg,
          width: Number(options.width) > 0 ? Number(options.width) : null,
          hidden: !!options.hidden,
          stickyRight: !!options.stickyRight,
          link: options.link || null,
          icon: options.icon || null,
          format: fnOrNull(options.format),
          prefix: fnOrString(options.prefix),
          suffix: fnOrString(options.suffix),
          className: fnOrString(options.className),
          parent: options.parent || null,
          click: fnOrNull(options.click),
          exactTotal: !!options.exactTotal,
          decimals: Number.isInteger(options.decimals)
            ? options.decimals
            : null,
        };
      }

      const text = (label, options = {}) => build(label, options, false);
      const num = (label, options = {}) => build(label, options, true);
      const money = (label, options = {}) =>
        num(label, Object.assign({ decimals: 2 }, options));

      const cols = (...items) => [
        text("No."),
        ...items.map((item) => (typeof item === "string" ? text(item) : item)),
      ];

      const humanize = (key) =>
        key
          .replace(/_/g, " ")
          .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
          .replace(/\s+/g, " ")
          .trim()
          .replace(/\b[a-z]/g, (c) => c.toUpperCase());

      function columnsFromRows(rows) {
        const sample = rows.slice(0, 50);
        const keys = [];
        sample.forEach((row) =>
          Object.keys(row).forEach((k) => {
            if (!keys.includes(k)) keys.push(k);
          }),
        );

        const seen = new Set(["No."]);
        const columns = keys.map((key) => {
          let label = humanize(key);
          if (seen.has(label)) label = key;
          seen.add(label);

          const value = sample
            .map((row) => row[key])
            .find((v) => v != null && v !== "");
          return typeof value === "number"
            ? num(label, {
                key,
                total: !NON_TOTAL.test(label) && !ID_KEY.test(key),
              })
            : text(label, { key });
        });
        return [text("No."), ...columns];
      }

      const columnsMatch = (columns, row) =>
        columns
          .filter((c) => c.label !== "No.")
          .every((c) => Format.readValue(row, c) !== undefined);

      const signature = (columns) => columns.map((c) => c.key).join("|");

      // Columns the report should switch to for these rows, or null to keep them.
      // Strict reports never switch: only the preset columns are shown.
      function resolve(report, rows, strict) {
        if (!rows.length || strict) return null;
        const next = columnsMatch(report.baseColumns, rows[0])
          ? report.baseColumns
          : columnsFromRows(rows);
        return signature(next) === signature(report.columns) ? null : next;
      }

      return {
        ID_KEY,
        keyOf,
        text,
        num,
        money,
        cols,
        columnsFromRows,
        resolve,
      };
    },
  ]);

  // ── ReportGrouping: registry of grouped (parent / sub row) reports ──
  app.factory("ReportGrouping", function () {
    const configs = {};

    const DEFAULTS = {
      by: [], // raw row field(s) to group on; first non-empty one wins
      label: "", // text shown before the group value
      showCount: true, // NEW: false = hide the number at the end
      countLabel: "records", // text shown after the sub-row count
      emptyLabel: "(None)", // group name when the field is empty
      sum: [], // column keys summed into the parent row
      collapsed: false, // start collapsed
      statement: false,
      nestedBy: [], // optional second-level grouping (raw row field(s))
    };

    return {
      register(reports, config) {
        const merged = Object.assign({}, DEFAULTS, config);
        merged.by = [].concat(merged.by);
        merged.nestedBy = [].concat(merged.nestedBy || []);
        merged.sum = [].concat(merged.sum);
        [].concat(reports).forEach(function (name) {
          configs[name] = merged;
        });
      },
      get(name) {
        return configs[name] || null;
      },
    };
  });

  // ── ReportRegistry: collects Categories/*.js definitions ────────
  // A definition looks like:
  //   {
  //     order: 10, category: "Customer",
  //     groups: [{ name, reports: [...names], filters: [...keys], monthPicker, datePreset }],
  //     reportFilters: { "Report Name": [...keys] | { add: [], remove: [] } },
  //     reportDatePresets: { "Report Name": "mtd" },
  //     reportUiSettings: { "Report Name": { totalLabel?, totalLabelAlign?, totalLabelPosition? } },
  //     columns: [{ reports: [...names], columns: cols(...), strict?: true }],
  //   }
  app.factory("ReportRegistry", [
    "ReportConfig",
    "ReportFilterFields",
    "ReportColumns",
    function (Config, Fields, Columns) {
      const { num, cols } = Columns;
      const definitions = [];
      let index = null;

      // Used when a report has no preset of its own.
      const GENERIC_COLUMNS = cols(
        "Date",
        "Reference #",
        "Name",
        "Description",
        num("Qty"),
        num("Amount"),
      );

      const slug = (value) => value.toLowerCase().replace(/[^a-z0-9]+/g, "-");

      function add(definition) {
        definitions.push(definition);
        index = null;
      }

      function build() {
        const sorted = definitions
          .map((def, i) => ({ def, i }))
          .sort((a, b) => (a.def.order || 0) - (b.def.order || 0) || a.i - b.i)
          .map((x) => x.def);

        const idx = {
          sorted,
          columnsByReport: new Map(),
          hideNo: new Set(),
          strict: new Set(),
          groups: new Map(),
          reportFilters: {},
          reportDatePresets: {},
          reportUiSettings: {},
          reportPrintSettings: {}
        };

        sorted.forEach((def) => {
          (def.columns || []).forEach((preset) =>
            preset.reports.forEach((name) => {
              idx.columnsByReport.set(name, preset.columns);
              if (preset.strict) idx.strict.add(name);
              if (preset.hideNo) idx.hideNo.add(name);
            }),
          );
          (def.groups || []).forEach((g) => idx.groups.set(g.name, g));
          Object.assign(idx.reportFilters, def.reportFilters);
          Object.assign(idx.reportDatePresets, def.reportDatePresets);
          Object.assign(idx.reportUiSettings, def.reportUiSettings);
          Object.assign(idx.reportPrintSettings, def.reportPrintSettings);
        });
        return idx;
      }

      const ensure = () => index || (index = build());

      const columnsFor = (name) =>
        ensure().columnsByReport.get(name) || GENERIC_COLUMNS;

      // Strict reports show only their preset columns (no auto-built extras).
      const isStrict = (name) => ensure().strict.has(name);

      function toReport(name, groupName) {
        const id = slug(name);
        const columns = columnsFor(name);
        return {
          id,
          name,
          group: groupName,
          endpoint: Config.API_BASE + id,
          columns,
          baseColumns: columns,
          hideNo: ensure().hideNo.has(name),
          uiSettings: ensure().reportUiSettings[name] || null,
          printSettings: ensure().reportPrintSettings[name] || null,
        };
      }

      // Fresh report objects every call: reports mutate (columns are swapped
      // when the rows don't match the preset), so each page visit starts clean.
      function buildCategories() {
        return ensure().sorted.map((def) => {
          const groups = (def.groups || []).map((g) => ({
            id: slug(def.category + " " + g.name),
            name: g.name,
            reports: g.reports.map((name) => toReport(name, g.name)),
          }));
          const reports = groups.flatMap((g) => g.reports);

          return {
            id: slug(def.category),
            name: def.category,
            groups,
            reports,
            reportCount: reports.length,
          };
        });
      }

      // reportFilters entry -> { add: [], remove: [] }
      function extrasOf(entry) {
        if (!entry) return { add: [], remove: [] };
        if (Array.isArray(entry)) return { add: entry, remove: [] };
        return { add: entry.add || [], remove: entry.remove || [] };
      }

      // group's filters + the report's own add-ons.
      // The date range always comes first (month picker right before it).
      function filtersFor(report) {
        const idx = ensure();
        const group = idx.groups.get(report.group);
        const extra = extrasOf(idx.reportFilters[report.name]);

        const rest = ((group && group.filters) || []).filter(
          (k) => !extra.remove.includes(k),
        );
        extra.add.forEach((key) => {
          if (!rest.includes(key)) rest.push(key);
        });

        const hasDate = rest.includes("dateRange");
        const head = !hasDate
          ? []
          : group && group.monthPicker
            ? ["month", "dateRange"]
            : ["dateRange"];

        const keys = head.concat(rest.filter((k) => k !== "dateRange"));

        return keys
          .map((key) => {
            if (!Fields[key])
              console.warn("Report filter field not defined:", key);
            return Fields[key];
          })
          .filter(Boolean);
      }

      // date a report opens with: its own, else its group's, else Month to Date
      function datePresetFor(report) {
        if (!report) return "mtd";
        const idx = ensure();
        const group = idx.groups.get(report.group);
        return (
          idx.reportDatePresets[report.name] ||
          (group && group.datePreset) ||
          "mtd"
        );
      }

      return {
        add,
        columnsFor,
        isStrict,
        buildCategories,
        filtersFor,
        datePresetFor,
      };
    },
  ]);

  // ── ReportOptions: loads / normalizes combo filter options ──────
  app.factory("ReportOptions", [
    "$http",
    "ReportConfig",
    function ($http, Config) {
      function optionOf(item, field) {
        if (item == null) return null;

        // plain string (suppliers, warehouses, PO numbers)
        if (typeof item !== "object") {
          const s = String(item).trim();
          return s ? { value: s, label: s } : null;
        }

        // object (customers, ...)
        const raw = item[field.valueKey || "value"];
        if (raw == null || String(raw).trim() === "") return null;

        const value = String(raw).trim();
        const label = field.optionLabel
          ? field.optionLabel(item)
          : item[field.labelKey || "label"];
        return {
          value,
          label: String(label == null ? value : label).trim() || value,
        };
      }

      function normalize(data, field) {
        const list = Array.isArray(data) ? data : (data && data.rows) || [];
        const seen = new Set();
        const out = [];
        list.forEach((item) => {
          const o = optionOf(item, field);
          if (o && !seen.has(o.value)) {
            seen.add(o.value);
            out.push(o);
          }
        });
        return out;
      }

      // One loader per page instance. Fills `store[source]` and keeps its own
      // cache so a source is requested only once per page visit.
      function create(store) {
        const cache = {};

        function load(fields) {
          fields
            .filter((f) => f.source)
            .forEach((f) => {
              if (cache[f.source]) {
                store[f.source] = cache[f.source];
                return;
              }
              $http
                .get(Config.API_BASE + f.source, { ignoreLoadingBar: true })
                .then((res) => {
                  const list = normalize(res.data, f);
                  cache[f.source] = list;
                  store[f.source] = list;
                })
                .catch(() => {
                  store[f.source] = [];
                });
            });
        }

        function labelOf(field, value) {
          if (value == null || value === "") return "";
          const o = (store[field.source] || []).find(
            (x) => x.value === String(value),
          );
          return o ? o.label : String(value);
        }

        function search(field, term) {
          const t = (term || "").toLowerCase().trim();
          const list = store[field.source] || [];
          const hits = t
            ? list.filter((o) => o.label.toLowerCase().includes(t))
            : list;
          return hits.slice(0, Config.COMBO_LIMIT);
        }

        return { load, labelOf, search };
      }

      return { create, normalize };
    },
  ]);

  // ── ReportExcel: .xlsx export of the visible columns (ExcelJS) ──
  app.factory("ReportExcel", [
    "ReportConfig",
    "ReportFormat",
    "ReportTotals",
    function (Config, Format, Totals) {
      const BORDER = {
        top: { style: "thin" },
        left: { style: "thin" },
        bottom: { style: "thin" },
        right: { style: "thin" },
      };

      const FONT_NAME = "Khmer OS Siemreap";
      const ROW_HEIGHT = 26;
      const TITLE_FONT = "Khmer UI";
      const MOUL_FONT = "Khmer Muol";
      const TACTEING_FONT = "Khmer OS Tacteing";
      const TACTEING_TEXT = "3";

      const HEADER_FILL = {
        type: "pattern",
        pattern: "solid",
        fgColor: { argb: "FFE9EEF2" },
      };

      function excelValue(row, column, index) {
        if (column.label === "No.") return index + 1;
        const value = Format.readValue(row, column);
        if (value == null || value === "") return "";
        if (column.format) {
          const out = column.format(value, row);
          if (out != null) return out;
        }
        if (typeof value === "string" && Config.ISO_DATE.test(value))
          return new Date(value);
        return typeof value === "string" ? Format.cleanText(value) : value;
      }

      function numFormat(decimals, prefix, zeroAsDash) {
        const base = Number.isInteger(decimals)
          ? "#,##0" + (decimals > 0 ? "." + "0".repeat(decimals) : "")
          : "#,##0.00";
        const format = prefix
          ? '"' + String(prefix).replace(/"/g, '""') + '"' + base
          : base;
        return zeroAsDash ? format + ";-" + format + ";-" : format;
      }

      const prefixOf = (c) => (typeof c.prefix === "string" ? c.prefix : "");

      // Number of leading columns the "Total" label spans.
      function totalSpan(columns, report) {
        const settings = report && report.uiSettings;

        if (
            settings &&
            settings.totalLabelPosition === "first" &&
            columns.length &&
            !Totals.isTotalColumn(columns[0])
        ) {
            return 1;
        }
        const first = columns.findIndex(Totals.isTotalColumn);
        return first > 0 ? first : Math.max(1, columns.length);
      }

      function sheetName(report) {
        return report.name.replace(/[\\\/?*\[\]:]/g, "-").slice(0, 31);
      }

      function styleHeaderCell(cell) {
        cell.font = { name: FONT_NAME, bold: true };
        cell.border = BORDER;
        cell.fill = HEADER_FILL;
        cell.alignment = { horizontal: "center", vertical: "middle" };
      }

      function styleHeaderRow(row, columns) {
        columns.forEach((_, n) => styleHeaderCell(row.getCell(n + 1)));
      }

      // One header row, or two when some columns have a `parent`. Returns the row count.
      function addHeader(ws, columns) {
        if (!columns.some((c) => c.parent)) {
          const only = ws.addRow(columns.map((c) => c.label));
          only.height = ROW_HEIGHT;
          styleHeaderRow(only, columns);
          return 1;
        }

        const top = ws.addRow(columns.map((c) => c.parent || c.label));
        const sub = ws.addRow(columns.map((c) => (c.parent ? c.label : "")));
        styleHeaderRow(top, columns);
        styleHeaderRow(sub, columns);

        let i = 0;
        while (i < columns.length) {
          const parent = columns[i].parent;
          let end = i;
          if (parent) {
            while (
              end + 1 < columns.length &&
              columns[end + 1].parent === parent
            )
              end++;
          }

          if (!parent) ws.mergeCells(top.number, i + 1, sub.number, i + 1);
          else if (end > i)
            ws.mergeCells(top.number, i + 1, top.number, end + 1);

          i = end + 1;
        }
        return 2;
      }

      function addRows(ws, rows, columns, report) {
        const zeroAsDash = !!(
          report &&
          report.uiSettings &&
          report.uiSettings.zeroAsDash
        );
        rows.forEach((row, i) => {
          const r = ws.addRow(columns.map((c) => excelValue(row, c, i)));
          r.height = ROW_HEIGHT;

          columns.forEach((c, n) => {
            const cell = r.getCell(n + 1);
            cell.border = BORDER;
            cell.font = { name: FONT_NAME, size: 11 };

            if (c.num) {
              cell.numFmt = numFormat(c.decimals, prefixOf(c), zeroAsDash);
              cell.alignment = { horizontal: "right", vertical: "middle" };
            } else if (cell.value instanceof Date) {
              cell.numFmt = "dd-mmm-yyyy";
              cell.alignment = { horizontal: "left", vertical: "middle" };
            } else {
              cell.alignment = { horizontal: "left", vertical: "middle" };
            }
          });
        });
      }

      function addTotal(ws, columns, totals, report ) {
        const span = totalSpan(columns, report);
          const settings = report && report.uiSettings;
          const configuredLabel = settings && settings.totalLabel;
          const label =
              typeof configuredLabel === "string" && configuredLabel.trim()
                  ? configuredLabel.trim()
                  : "Total";
          const configuredAlignment = settings && settings.totalLabelAlign;
          const labelAlignment =
              ["left", "center", "right"].indexOf(configuredAlignment) !== -1
                  ? configuredAlignment
                  : "right";
        const row = ws.addRow(
          columns.map((c, n) => {
            if (n === 0) return label;
            if (n < span) return "";
            const raw = Totals.findTotal(totals || {}, c);
            return Totals.isTotalColumn(c) && Number.isFinite(Number(raw))
              ? Number(raw)
              : "";
          }),
        );
        if (span > 1) ws.mergeCells(row.number, 1, row.number, span);

        row.height = ROW_HEIGHT;

        columns.forEach((c, n) => {
          const cell = row.getCell(n + 1);
          cell.font = { name: FONT_NAME, bold: true };
          cell.border = BORDER;
          if (c.num)
            cell.numFmt = numFormat(
              c.decimals,
              prefixOf(c),
              !!(settings && settings.zeroAsDash),
            );
          cell.alignment = {
            horizontal: n === 0 ? labelAlignment : c.num ? "right" : "left",
            vertical: "middle",
          };
        });
      }

      function setColumnWidths(ws, columns) {
        columns.forEach((c, n) => {
          ws.getColumn(n + 1).width = Math.max(
            6,
            Math.round((c.width || 150) / 7),
          );
        });
      }

      function addTitleBlock(wb, ws, columns, info) {
        const source = (info && info.lines) || [];
        const logo = info && info.logo;
        if (!source.length && !logo) return 0;

        const lines = source;

        const STYLE = {
          companyKh: { name: MOUL_FONT, size: 14, height: 36 },
          company: { name: TITLE_FONT, size: 12, height: 22 },
          vat: { name: TITLE_FONT, size: 11, height: 20 },
          title: { name: TITLE_FONT, size: 12, height: 24 },
          period: { name: TITLE_FONT, size: 11, height: 20 },
        };
        const FALLBACK = { name: TITLE_FONT, size: 11, height: 20 };

        // One rich-text run per line, each with its own font, joined by line breaks.
        const richText = lines.map((line, i) => {
          const s = STYLE[line.id] || FALLBACK;
          return {
            text: line.text + (i < lines.length - 1 ? "\n" : ""),
            font: { name: s.name, size: s.size, bold: false },
          };
        });

        const height = lines.reduce(
          (sum, l) => sum + (STYLE[l.id] || FALLBACK).height,
          0,
        );

        const row = ws.addRow([""]);
        row.height = Math.max(height, logo ? 56 : 0);

        const last = Math.max(1, columns.length);
        if (last > 1) ws.mergeCells(row.number, 1, row.number, last);

        const cell = row.getCell(1);
        if (richText.length) cell.value = { richText };
        cell.alignment = {
          horizontal: "center",
          vertical: "middle",
          wrapText: true,
        };

        if (logo) {
          const id = wb.addImage({
            buffer: logo.buffer,
            extension: logo.extension,
          });
          ws.addImage(id, {
            tl: { col: 0.1, row: row.number - 1 + 0.1 },
            ext: { width: 130, height: 48 },
          });
        }

        ws.addRow([]); // spacer before the table
        return 2;
      }

      function buildWorkbook(report, columns, rows, totals, headerInfo) {
        const wb = new ExcelJS.Workbook();
        const ws = wb.addWorksheet(sheetName(report));

        const titleRows = addTitleBlock(wb, ws, columns, headerInfo);
        const headerRows = addHeader(ws, columns);
        addRows(ws, rows, columns, report);
        if (rows.length && columns.some(Totals.isTotalColumn))
          addTotal(ws, columns, totals, report);

        setColumnWidths(ws, columns);
        ws.views = [{ state: "frozen", ySplit: titleRows + headerRows }];

        return wb.xlsx.writeBuffer();
      }

      function downloadFile(filename, content, mimeType) {
        const url = URL.createObjectURL(
          new Blob([content], { type: mimeType }),
        );
        const link = document.createElement("a");
        link.href = url;
        link.download = filename;
        link.click();
        URL.revokeObjectURL(url);
      }

      return { totalSpan, buildWorkbook, downloadFile };
    },
  ]);

  // ── ReportSidebar: collapses the app sidebar on the report page ──
  app.factory("ReportSidebar", [
    "ReportConfig",
    function (Config) {
      function collapse() {
        $("body").addClass("sidebar-collapse");
        $("#menuIcon").attr("class", "ti ti-chevrons-right");

        const brand = $(".brand-image");
        if (!brand.length) return;
        brand[0].style.setProperty("width", "34px", "important");
        brand.css({ height: "34px", "object-fit": "contain" });
      }

      function init($scope) {
        const stored = sessionStorage.getItem(Config.SIDEBAR_KEY);
        if (stored === null) sessionStorage.setItem(Config.SIDEBAR_KEY, "true");
        if (stored !== "false") collapse();

        $("[data-widget=pushmenu]")
          .off("click.report")
          .on("click.report", function () {
            const willBeOpen = $("body").hasClass("sidebar-collapse");
            sessionStorage.setItem(
              Config.SIDEBAR_KEY,
              willBeOpen ? "false" : "true",
            );
          });

        $scope.$on("$destroy", () =>
          $("[data-widget=pushmenu]").off("click.report"),
        );
      }

      return { init };
    },
  ]);

  /* ════════════════════════════════════════════════════════════════
   * 3. PAGE FEATURES  (each: factory -> install($scope, ctx))
   * ════════════════════════════════════════════════════════════════ */

  // ── Data: loads a report page from the API ──────────────────────
  app.factory("ReportDataFeature", [
    "$http",
    "ReportRegistry",
    "ReportColumns",
    "ReportTotals",
    function ($http, Registry, Columns, Totals) {
      function errorMessage(err, fallback) {
        if (err instanceof Error) return `${fallback}: ${err.message}`;
        if (err && err.status === 404)
          return "This report is not available yet.";
        return (
          (err && err.data && (err.data.Message || err.data.message)) ||
          `${fallback} (${(err && err.status) || "network error"})`
        );
      }

      // Accepts a plain array or { rows, totalCount, page, totals }.
      function parseResponse(data, report, page, pageSize) {
        const all = pageSize === 0;

        if (Array.isArray(data)) {
          return {
            rows: all
              ? data
              : data.slice((page - 1) * pageSize, page * pageSize),
            totalCount: data.length,
            totals: Totals.sumRows(data, report.columns),
            page: all ? 1 : page,
            pageSize,
          };
        }

        const payload = data || {};
        const incoming = payload.rows || [];
        const totalCount =
          payload.totalCount != null
            ? Number(payload.totalCount) || 0
            : incoming.length;

        const sliced = !all && incoming.length > pageSize;
        const rows = sliced
          ? incoming.slice((page - 1) * pageSize, page * pageSize)
          : incoming;

        return {
          rows,
          totalCount,
          totals: Totals.resolveTotals(
            payload.totals,
            incoming,
            report.columns,
            incoming.length >= totalCount,
          ),
          page: all ? 1 : sliced ? page : Number(payload.page) || page,
          pageSize,
        };
      }

      return function install($scope, ctx) {
        let requestId = 0;

        $scope.reportRows = [];
        $scope.totalRows = 0;
        $scope.reportTotals = {};
        $scope.pageTotals = {};
        $scope.reportError = "";
        $scope.isLoading = false;
        $scope.previewed = false;

        $scope.hasReportTotals = () =>
          !!$scope.reportTotals && Object.keys($scope.reportTotals).length > 0;

        function clearResult(preserveRows) {
          requestId++;
          $scope.isLoading = false;
          $scope.reportError = "";
          if (preserveRows) return;

          $scope.reportRows = [];
          $scope.totalRows = 0;
          $scope.reportTotals = {};
          $scope.pageTotals = {};
          $scope.previewed = false;
          $scope.currentPage = 1;
        }

        function loadReport(report, preservePage, retried) {
          report = report || $scope.selectedReport;
          if (!report) return;

          const page = preservePage ? $scope.currentPage : 1;
          const pageSize = ctx.pageSize();
          clearResult(true);
          $scope.currentPage = page;
          const thisRequest = requestId;
          $scope.isLoading = true;
          const params = Object.assign(ctx.activeFilters(), ctx.sortParams(), {
            totalFields: Totals.fieldsFor(report),
          });
          if (pageSize === 0) {
            params.allRows = true;
          } else {
            params.page = page;
            params.pageSize = pageSize;
          }

          $http
            .get(report.endpoint, {
              params,
              ignoreLoadingBar: true,
              skipErrorRedirect: true,
            })
            .then(function (res) {
              if (thisRequest !== requestId) return;

              const result = parseResponse(res.data, report, page, pageSize);

              // Switch columns when rows don't match the preset (never for strict reports).
              // const next = Columns.resolve(
              //   report,
              //   result.rows,
              //   Registry.isStrict(report.name),
              // );
              // if (next) {
              //   report.columns = next;
              //   ctx.applyDefaultLayout(report);
              // }
            let next = null;
            const isProfitLossAccount = (row) =>
              row &&
              row.rowType === "account" &&
              row.accountCode &&
              row.accountName;
            const profitLossAccountClass = (value, row) =>
              [
                row.level > 0
                  ? `rp-pl-indent-${Math.min(row.level, 10)}`
                  : "",
                ["header", "parent", "subtotal", "total", "grand", "net"].includes(
                  row.rowType,
                )
                  ? "font-weight-bold"
                  : "",
                isProfitLossAccount(row)
                  ? "rp-pl-account-link"
                  : "rp-pl-account-static",
              ]
                .filter(Boolean)
                .join(" ");
            const drillToProfitLossAccount = (row) => {
              if (!isProfitLossAccount(row)) return;
              const $root = angular
                .element(document.body)
                .injector()
                .get("$rootScope");
              const filters = $scope.filters || {};
              $root.$broadcast("report:drill", {
                report: "Transaction Detail by Account",
                filters: {
                  account: row.accountCode + "-" + row.accountName,
                  from: filters.from,
                  to: filters.to,
                },
              });
            };
            if (report.name === "Profit/Loss by Month") {
                let monthKeys = Object.keys(result.rows[0] || {})
                    .filter((key) => /^\d{6}$/.test(key))
                    .sort();
                if (!monthKeys.length) {
                    const filterDate = (value) => {
                        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(
                            String(ctx.toIsoDate(value) || ""),
                        );
                        return match
                            ? new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]))
                            : null;
                    };
                    const selectedMonth = /^(\d{4})(?:-(\d{2}))?$/.exec(
                        String($scope.filters.month || ""),
                    );
                    if (selectedMonth) {
                        const year = Number(selectedMonth[1]);
                        const firstMonth = Number(selectedMonth[2] || 1);
                        const lastMonth = selectedMonth[2] ? firstMonth : 12;
                        for (let month = firstMonth; month <= lastMonth; month++)
                            monthKeys.push(`${year}${String(month).padStart(2, "0")}`);
                    } else {
                        const selectedMonthOnly = /^(\d{2})$/.exec(
                            String($scope.filters.month || ""),
                        );
                        const from = filterDate($scope.filters.from);
                        const to = filterDate($scope.filters.to);
                        if (selectedMonthOnly) {
                            const year = (to || from || new Date()).getFullYear();
                            monthKeys.push(`${year}${selectedMonthOnly[1]}`);
                        } else if (from && to && from <= to) {
                            for (
                                let month = new Date(from.getFullYear(), from.getMonth(), 1);
                                month <= new Date(to.getFullYear(), to.getMonth(), 1);
                                month.setMonth(month.getMonth() + 1)
                            )
                                monthKeys.push(
                                    `${month.getFullYear()}${String(month.getMonth() + 1).padStart(2, "0")}`,
                                );
                        }
                    }
                }
                const monthNames = [
                    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
                ];
                const amountClass = (v, row) =>
                    [
                        ["subtotal", "total", "grand", "net"].includes(row.rowType)
                            ? "rp-subtotal-line"
                            : "",
                        v < 0 ? "rp-bad" : "",
                    ]
                        .filter(Boolean)
                        .join(" ");
                const hideHeaderAmounts = (v, row) =>
                    row.rowType === "header" || row.rowType === "parent"
                        ? ""
                        : undefined;

                report.columns = Columns.cols(
                    Columns.text("Account", {
                        key: "display",
                        className: profitLossAccountClass,
                        click: drillToProfitLossAccount,
                    }),
                    ...monthKeys.map((key) => {
                        const month = Number(key.slice(4, 6)) - 1;
                        return Columns.money(
                            `${monthNames[month]}-${key.slice(0, 4)}`,
                            {
                                key,
                                total: false,
                                className: amountClass,
                                format: hideHeaderAmounts,
                            },
                        );
                    }),
                    Columns.money("Total", {
                        key: "amount",
                        total: false,
                        className: amountClass,
                        format: hideHeaderAmounts,
                    }),
                );
                report.baseColumns = report.columns;
                ctx.applyDefaultLayout(report);


            } else if (report.name === "Profit/Loss by Year") {
                let yearKeys = Object.keys(result.rows[0] || {})
                    .filter((key) => /^\d{4}$/.test(key))
                    .sort();
                if (!yearKeys.length) {
                    const filterDate = (value) => {
                        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(
                            String(ctx.toIsoDate(value) || ""),
                        );
                        return match
                            ? new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]))
                            : null;
                    };
                    const selectedPeriod = /^(\d{4})(?:-\d{2})?$/.exec(
                        String($scope.filters.month || ""),
                    );
                    const from = filterDate($scope.filters.from);
                    const to = filterDate($scope.filters.to);
                    if (selectedPeriod) {
                        yearKeys.push(selectedPeriod[1]);
                    } else if (/^\d{2}$/.test(String($scope.filters.month || ""))) {
                        yearKeys.push(String((to || from || new Date()).getFullYear()));
                    } else if (from && to && from <= to) {
                        for (let year = from.getFullYear(); year <= to.getFullYear(); year++)
                            yearKeys.push(String(year));
                    }
                }
                const amountClass = (v, row) =>
                    [
                        ["subtotal", "total", "grand", "net"].includes(row.rowType)
                            ? "rp-subtotal-line"
                            : "",
                        v < 0 ? "rp-bad" : "",
                    ]
                        .filter(Boolean)
                        .join(" ");
                const hideHeaderAmounts = (v, row) =>
                    row.rowType === "header" || row.rowType === "parent"
                        ? ""
                        : undefined;

                report.columns = Columns.cols(
                    Columns.text("Account", {
                        key: "display",
                        className: profitLossAccountClass,
                        click: drillToProfitLossAccount,
                    }),
                    ...yearKeys.map((key) =>
                        Columns.money(key, {
                            key,
                            total: false,
                            className: amountClass,
                            format: hideHeaderAmounts,
                        }),
                    ),
                    Columns.money("Total", {
                        key: "amount",
                        total: false,
                        className: amountClass,
                        format: hideHeaderAmounts,
                    }),
                );
                report.baseColumns = report.columns;
                ctx.applyDefaultLayout(report);
            } else if (
                report.name === "Profit/Loss by Class" ||
                report.name === "Profit/Loss by Customer"
            ) {
                const fixedKeys = new Set([
                    "group",
                    "section",
                    "accountCode",
                    "accountName",
                    "amount",
                    "rowType",
                    "level",
                    "percent",
                    "display",
                    "yearToDate",
                    "previousMonth",
                    "thisMonth",
                    "thisMonthLastYear",
                ]);
                const groupKeys = Object.keys(result.rows[0] || {})
                    .filter((key) => !fixedKeys.has(key))
                    .sort((a, b) => a.localeCompare(b));
                const amountClass = (v, row) =>
                    [
                        ["subtotal", "total", "grand", "net"].includes(row.rowType)
                            ? "rp-subtotal-line"
                            : "",
                        v < 0 ? "rp-bad" : "",
                    ]
                        .filter(Boolean)
                        .join(" ");
                const hideHeaderAmounts = (v, row) =>
                    row.rowType === "header" || row.rowType === "parent"
                        ? ""
                        : undefined;

                report.columns = Columns.cols(
                    Columns.text("Account", {
                        key: "display",
                        className: profitLossAccountClass,
                        click: drillToProfitLossAccount,
                    }),
                    ...groupKeys.map((key) =>
                        Columns.money(key, {
                            key,
                            total: false,
                            className: amountClass,
                            format: hideHeaderAmounts,
                        }),
                    ),
                    Columns.money("Total", {
                        key: "amount",
                        total: false,
                        className: amountClass,
                        format: hideHeaderAmounts,
                    }),
                );
                report.baseColumns = report.columns;
                ctx.applyDefaultLayout(report);
            }

            else {
                // Switch columns when rows don't match the preset (never for strict reports).
                next = Columns.resolve(
                    report,
                    result.rows,
                    Registry.isStrict(report.name),
                );
                if (next) {
                    report.columns = next;
                    ctx.applyDefaultLayout(report);
                }
            }
              //////

              $scope.reportRows = result.rows;
              $scope.totalRows = result.totalCount;
              $scope.reportTotals = result.totals;
              $scope.pageTotals = Totals.sumRows(result.rows, report.columns);
              $scope.currentPage = result.page;
              $scope.pagination.pageSize = result.pageSize;
              $scope.previewed = true;

              // New columns may have totals the server didn't compute yet: ask once more.
              if (next && !retried && report.columns.some(Totals.isTotalColumn))
                loadReport(report, true, true);
            })
            .catch(function (err) {
              if (thisRequest !== requestId) return;
              $scope.reportRows = [];
              $scope.totalRows = 0;
              $scope.reportTotals = {};
              $scope.pageTotals = {};
              $scope.reportError = errorMessage(err, "Failed to load report");
            })
            .finally(function () {
              if (thisRequest === requestId) $scope.isLoading = false;
            });
        }

        ctx.clearResult = clearResult;
        ctx.errorMessage = errorMessage;
        ctx.loadReport = loadReport;
        $scope.loadReport = loadReport;
      };
    },
  ]);

  // ── Paging ──────────────────────────────────────────────────────
  app.factory("ReportPagingFeature", [
    "ReportConfig",
    function (Config) {
      const SIZES = Config.PAGE_SIZES;
      const ALL = 0;

      return function install($scope, ctx) {
        $scope.currentPage = 1;
        $scope.pagination = { pageSize: SIZES[0] };
        $scope.pageSizeOptions = SIZES;
        $scope.pageSizeLabel = (size) => (size === ALL ? "All" : size);

        function pageSizeValue() {
          const size = Number($scope.pagination.pageSize);
          return SIZES.includes(size) ? size : SIZES[0];
        }

        const pageCount = () => {
          const size = pageSizeValue();
          return size === ALL
            ? 1
            : Math.max(1, Math.ceil($scope.totalRows / size));
        };

        const pageRows = () => $scope.reportRows;

        function pageSummary() {
          const total = $scope.totalRows;
          if (!total) return "0 rows";
          const pageSize = pageSizeValue();
          if (pageSize === ALL) return `Showing 1-${total} of ${total}`;
          const start = ($scope.currentPage - 1) * pageSize + 1;
          const end = Math.min(start + pageSize - 1, total);
          return `Showing ${start}-${end} of ${total}`;
        }

        function setPage(page) {
          const next = Math.min(Math.max(page, 1), pageCount());
          if (next === $scope.currentPage) return;
          $scope.currentPage = next;
          ctx.loadReport($scope.selectedReport, true);
        }

        function changePageSize() {
          $scope.pagination.pageSize = pageSizeValue();
          $scope.currentPage = 1;
          if ($scope.selectedReport)
            ctx.loadReport($scope.selectedReport, true);
        }

        ctx.pageSize = pageSizeValue;

        Object.assign($scope, {
          pageRows,
          pageCount,
          pageSummary,
          setPage,
          changePageSize,
        });
      };
    },
  ]);

  // ── Sorting (server side) ───────────────────────────────────────
  app.factory("ReportSortingFeature", function () {
    return function install($scope, ctx) {
      $scope.sort = { key: null, dir: "asc" };

      function toggleSort(column) {
        if (column.label === "No." || !$scope.previewed) return;
        const s = $scope.sort;
        if (s.key !== column.key) {
          s.key = column.key;
          s.dir = "asc";
        } else if (s.dir === "asc") {
          s.dir = "desc";
        } else {
          s.key = null;
          s.dir = "asc";
        }
        ctx.loadReport($scope.selectedReport);
      }

      function sortIcon(column) {
        const s = $scope.sort;
        if (s.key !== column.key) return "ti-arrows-sort";
        return s.dir === "asc" ? "ti-sort-ascending" : "ti-sort-descending";
      }

      const sortParams = () =>
        $scope.sort.key
          ? { sortBy: $scope.sort.key, sortDir: $scope.sort.dir }
          : {};

      ctx.sortParams = sortParams;

      Object.assign($scope, { toggleSort, sortIcon });
    };
  });

  // ── Filters: values, active params, summary, drawer ─────────────
  app.factory("ReportFiltersFeature", function () {
    return function install($scope, ctx) {
      $scope.filters = {};
      $scope.visibleFilterFields = [];

      let summaryCache = { sig: "", list: [] };

      // Params sent to the API: only visible fields with a value.
      function activeFilters() {
        const params = {};
        const hasMonth = /^(\d{4}(-\d{2})?|\d{2})$/.test(
          $scope.filters.month || "",
        );
        const visibleKeys = new Set(
          $scope.visibleFilterFields.flatMap(
            (field) => field.keys || [field.key],
          ),
        );

        Object.entries($scope.filters).forEach(([key, value]) => {
          if (hasMonth && (key === "from" || key === "to")) return;
          if (key === "from" || key === "to") value = ctx.toIsoDate(value);
          if (
            key === "account" &&
            Array.isArray(value)
          )
            value = value.join(",");
          if (
            visibleKeys.has(key) &&
            value !== "" &&
            value != null &&
            value !== false
          )
            params[key] = value;
        });
        return params;
      }

      const filterCount = () => Object.keys(activeFilters()).length;

      // Same array instance while nothing changed, so ng-repeat doesn't re-render.
      function filterSummary() {
        const list = $scope.visibleFilterFields
          .filter(
            (f) =>
              f.type === "text" || f.type === "select" || f.type === "combo",
          )
          .map((f) => {
            let value = $scope.filters[f.key];
            if (f.type === "select") {
              const opt = (f.options || []).find((o) => o.value === value);
              value = opt ? opt.label : value;
            } else if (f.type === "combo") {
              //value = ctx.optionLabelOf(f, value);
              value = Array.isArray(value)
                ? value.map((item) => ctx.optionLabelOf(f, item)).filter(Boolean).join(", ")
                : ctx.optionLabelOf(f, value);
            }
            return {
              label: f.label,
              value: value || f.placeholder || "All",
            };
          });

        const sig = JSON.stringify(list);
        if (sig !== summaryCache.sig) summaryCache = { sig, list };
        return summaryCache.list;
      }

      function openDrawer() {
        $scope.ui.drawerOpen = true;
      }

      function closeDrawer() {
        $scope.ui.drawerOpen = false;
        $scope.ui.typeOpen = false;
        $scope.combo.open = null;
      }

      function submitFilters() {
        ctx.loadReport();
        closeDrawer();
      }

      function resetFilters() {
        $scope.filters = {};
        $scope.dateState.preset = ctx.defaultDatePreset();
        ctx.applyDatePreset();
      }

      ctx.activeFilters = activeFilters;

      Object.assign($scope, {
        filterCount,
        filterSummary,
        openDrawer,
        closeDrawer,
        submitFilters,
        resetFilters,
      });
    };
  });

  // ── Date: presets, custom range, month/year picker ──────────────
  // Picking a date loads the report immediately (no Apply needed).
  // from/to are kept as dd-mm-yyyy (what the user sees); ctx.toIsoDate()
  // converts them to yyyy-mm-dd when the request is sent to the API.
  app.factory("ReportDateFeature", [
    "$timeout",
    "ReportConfig",
    function ($timeout, Config) {
      const pad = (n) => String(n).padStart(2, "0");

      // Date object -> "dd-mm-yyyy"
      const dmyDate = (d) =>
        `${pad(d.getDate())}-${pad(d.getMonth() + 1)}-${d.getFullYear()}`;

      // "dd-mm-yyyy" or "yyyy-mm-dd" -> "yyyy-mm-dd". Anything else is returned unchanged.
      function toIsoDate(value) {
        const v = String(value == null ? "" : value).trim();
        let m = /^(\d{2})-(\d{2})-(\d{4})$/.exec(v);
        if (m) return `${m[3]}-${m[2]}-${m[1]}`;
        m = /^(\d{4})-(\d{2})-(\d{2})/.exec(v);
        return m ? `${m[1]}-${m[2]}-${m[3]}` : value;
      }

      // "yyyy-mm-dd" or "dd-mm-yyyy" -> "dd-mm-yyyy" for display.
      function toDmy(value) {
        const v = String(value == null ? "" : value).trim();
        const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(v);
        return m ? `${m[3]}-${m[2]}-${m[1]}` : v;
      }

      function presetRange(preset) {
        const t = new Date();
        const y = t.getFullYear();
        const m = t.getMonth();
        const d = t.getDate();
        switch (preset) {
          case "today":
            return [t, t];
          case "week":
            return [new Date(y, m, d - ((t.getDay() + 6) % 7)), t];
          case "mtd":
            return [new Date(y, m, 1), t];
          case "month":
            return [new Date(y, m, 1), new Date(y, m + 1, 0)];
          case "lastMonth":
            return [new Date(y, m - 1, 1), new Date(y, m, 0)];
          case "ytd":
            return [new Date(y, 0, 1), t];
          default:
            return null;
        }
      }

      return function install($scope, ctx) {
        $scope.datePresets = Config.DATE_PRESETS;
        $scope.dateState = { preset: "mtd" };
        $scope.monthOptions = Config.MONTH_LABELS.map((label, i) => ({
          value: pad(i + 1),
          label,
        }));
        $scope.yearOptions = [];
        for (let y = new Date().getFullYear(); y >= Config.FIRST_YEAR; y--) {
          $scope.yearOptions.push(String(y));
        }
        $scope.monthSelection = { month: null, year: null };

        // Reload the report with the new date. Debounced so typing a custom
        // date or clicking quickly doesn't fire a request for every change.
        let autoTimer = null;
        function autoLoad(delay) {
          if (autoTimer) $timeout.cancel(autoTimer);
          autoTimer = $timeout(
            function () {
              autoTimer = null;
              if ($scope.selectedReport) ctx.loadReport($scope.selectedReport);
            },
            delay == null ? 0 : delay,
          );
        }

        function resetMonthSelection() {
          $scope.monthSelection.month = null;
          $scope.monthSelection.year = null;
          $scope.filters.month = "";
        }

        function updateMonthFilterOnly() {
          const { month, year } = $scope.monthSelection;

          $scope.filters.month = year
            ? month
              ? `${year}-${month}`
              : year
            : month || "";

          if ($scope.filters.month) {
            $scope.filters.from = "";
            $scope.filters.to = "";
            $scope.dateState.preset = "custom";
          }
        }

        // Fills from/to only (used by Reset and when opening a report; no reload).
        function applyDatePresetOnly() {
          resetMonthSelection();
          const preset = $scope.dateState.preset;
          const range = presetRange(preset);
          if (range) {
            $scope.filters.from = dmyDate(range[0]);
            $scope.filters.to = dmyDate(range[1]);
          } else if (preset === "all") {
            $scope.filters.from = "";
            $scope.filters.to = "";
          }
        }

        // Used by the template (ng-change): fill + reload.
        function applyDatePreset() {
          applyDatePresetOnly();
          autoLoad(0);
        }

        function updateMonthFilter() {
          updateMonthFilterOnly();
          autoLoad(0);
        }

        // Custom from/to typed or picked: reload shortly after the last change.
        function markCustom() {
          resetMonthSelection();
          $scope.dateState.preset = "custom";
          autoLoad(400);
        }

        function dateText() {
          const { from, to, month } = $scope.filters;
          if (month) {
            const parts = month.split("-");
            if (parts.length === 2)
              return `Month: ${Config.MONTH_LABELS[+parts[1] - 1]} ${parts[0]}`;
            return parts[0].length === 4
              ? `Year: ${parts[0]}`
              : `Month: ${Config.MONTH_LABELS[+parts[0] - 1]}`;
          }
          if (!from && !to) return "All Dates";
          return `From ${toDmy(from) || "…"} To ${toDmy(to) || "…"}`;
        }

        function pickMonthYear(part, value) {
          $scope.monthSelection[part] = value || null;
          $scope.combo.open = null;
          $scope.combo.search = "";
          updateMonthFilterOnly();
          autoLoad(0);
        }

        function pickPreset(value) {
          $scope.dateState.preset = value;
          $scope.combo.open = null;
          applyDatePresetOnly();
          // "Custom" has no range yet: wait until the user picks dates.
          if (value !== "custom") autoLoad(0);
        }

        function clearMonthYear(part, $event) {
          if ($event) $event.stopPropagation();
          pickMonthYear(part, null);
        }

        const presetLabel = () => {
          const p = Config.DATE_PRESETS.find(
            (x) => x.value === $scope.dateState.preset,
          );
          return p ? p.label : "Custom";
        };

        function monthLabel() {
          const m = $scope.monthOptions.find(
            (o) => o.value === $scope.monthSelection.month,
          );
          return m ? m.label : "Select month";
        }

        const yearLabel = () => $scope.monthSelection.year || "Select year";

        // Internal callers (Reset, opening a report) must NOT trigger a second load.
        ctx.applyDatePreset = applyDatePresetOnly;
        ctx.toIsoDate = toIsoDate;

        Object.assign($scope, {
          updateMonthFilter,
          applyDatePreset,
          markCustom,
          dateText,
          pickMonthYear,
          pickPreset,
          clearMonthYear,
          presetLabel,
          monthLabel,
          yearLabel,
        });

        // Clicking a day applies it at once and closes the popup.
        // daterangepicker selects on mousedown (not click) and redraws the cell,
        // so listen to mousedown, let the picker handle it first, then press Apply.
        // For a range, Apply is disabled until both dates are chosen, so the
        // first click only sets the start and the second click applies.
        const pickerSelector = ".daterangepicker.rp-report-picker";
        $(document).off("mousedown.reportDatePick");
        $(document).on(
          "mousedown.reportDatePick",
          pickerSelector + " td.available",
          function () {
            const picker = $(this).closest(pickerSelector);
            setTimeout(function () {
              picker.find(".applyBtn").trigger("click");
            }, 0);
          },
        );

        $scope.$on("$destroy", function () {
          $(document).off("mousedown.reportDatePick");
          if (autoTimer) $timeout.cancel(autoTimer);
        });
      };
    },
  ]);

  // ── Combo: searchable dropdown filters ──────────────────────────
  // A field with `source` loads its list from the API (ReportOptions).
  // A field with `options` and no `source` uses that fixed list directly.
  app.factory("ReportComboFeature", [
    "ReportOptions",
    function (Options) {
      const isStatic = (field) =>
        !!field && !field.source && Array.isArray(field.options);

      // Normalized once per options array. Fresh objects on every call would make
      // ng-repeat see a changed list on every digest ($rootScope:infdig).
      const cache = new WeakMap();

      function staticList(field) {
        let list = cache.get(field.options);
        if (!list) {
          list = field.options.map((o) =>
            o !== null && typeof o === "object"
              ? { value: o.value, label: o.label != null ? o.label : o.value }
              : { value: o, label: o },
          );
          cache.set(field.options, list);
        }
        return list;
      }

      return function install($scope, ctx) {
        $scope.comboOptions = {};
        $scope.combo = { open: null, search: "" };

        const options = Options.create($scope.comboOptions);

        function comboList(field) {
          if (!isStatic(field))
            return options.search(field, $scope.combo.search);

          const q = String($scope.combo.search || "")
            .trim()
            .toLowerCase();
          const list = staticList(field);
          return q
            ? list.filter((o) => String(o.label).toLowerCase().includes(q))
            : list;
        }

        function labelOf(field, value) {
          if (!isStatic(field)) return options.labelOf(field, value);
          const hit = staticList(field).find((o) => o.value === value);
          return hit ? hit.label : "";
        }

        //const comboLabel = (field) =>
        //  labelOf(field, $scope.filters[field.key]) ||
        //  field.placeholder ||
        //  "All";

        function isComboMultiple(field) {
            if (!field.multiple) return false;

            const report = $scope.selectedReport;
            if (
                !Array.isArray(field.multipleReports) &&
                !Array.isArray(field.multipleGroups)
            )
                return true;

            return !!report &&
                ((Array.isArray(field.multipleReports) &&
                    field.multipleReports.includes(report.name)) ||
                    (Array.isArray(field.multipleGroups) &&
                        field.multipleGroups.includes(report.group)));
        }

        function hasComboValue(field) {
            const value = $scope.filters[field.key];
            return Array.isArray(value)
                ? value.length > 0
                : value != null && value !== "";
        }

        function isComboSelected(field, value) {
            const selected = $scope.filters[field.key];
            if (!isComboMultiple(field)) return selected === value;

            return (Array.isArray(selected) ? selected : selected ? [selected] : [])
                .includes(value);
        }

        function comboLabel(field) {
            const value = $scope.filters[field.key];
            if (isComboMultiple(field)) {
                const selected = Array.isArray(value) ? value : value ? [value] : [];
                return (
                    selected.map((item) => labelOf(field, item)).filter(Boolean).join(", ") ||
                    field.placeholder ||
                    "All"
                );
            }
            return labelOf(field, value) || field.placeholder || "All";
        }

        function pickCombo(field, value) {
            if (isComboMultiple(field)) {
                const selected = Array.isArray($scope.filters[field.key])
                    ? $scope.filters[field.key].slice()
                    : $scope.filters[field.key]
                        ? [$scope.filters[field.key]]
                        : [];

                if (!value) {
                    $scope.filters[field.key] = [];
                    return;
                }

                const index = selected.indexOf(value);
                if (index >= 0) selected.splice(index, 1);
                else selected.push(value);

                $scope.filters[field.key] = selected;
                return;
            }

            $scope.filters[field.key] = value || "";
            closeCombo();
        }

        function clearCombo(field, $event) {
            if ($event) $event.stopPropagation();
            $scope.filters[field.key] = isComboMultiple(field) ? [] : "";
            closeCombo();
        }

        function closeCombo() {
          $scope.combo.open = null;
          $scope.combo.search = "";
        }

        function toggleCombo(field) {
          $scope.combo.open =
            $scope.combo.open === field.key ? null : field.key;
          $scope.combo.search = "";
        }

        // Fixed-list fields are skipped when options are loaded from the API.
        ctx.loadFieldOptions = function (arg) {
          if (Array.isArray(arg)) {
            const rest = arg.filter((f) => !isStatic(f));
            return rest.length
              ? options.load.apply(
                  null,
                  [rest].concat([].slice.call(arguments, 1)),
                )
              : undefined;
          }
          if (isStatic(arg)) return undefined;
          return options.load.apply(null, arguments);
        };
        ctx.optionLabelOf = labelOf;

        Object.assign($scope, {
          comboList,
          comboLabel,
          isComboMultiple,
          hasComboValue,
          isComboSelected,
          toggleCombo,
          closeCombo,
          pickCombo,
          clearCombo,
        });
      };
    },
  ]);

  // ── Layout: visible columns, header block, widths, zoom, @page ──
  app.factory("ReportLayoutFeature", [
    "$rootScope",
    "$http",
    "ReportConfig",
    "ReportColumns",
      function ($rootScope, $http, Config, Columns) {
      const clamp = (n, min, max) =>
        Math.min(max, Math.max(min, Number.isFinite(n) ? n : min));

      //const defaultSettings = () => ({
      //  landscape: true,
      //  align: "center",
      //  padding: 6,
      //});

    const defaultSettings = (report) => {
        const print = report && report.printSettings;
        return {
            paperSize: (print && print.paperSize) || "A3",
            landscape:
                print && typeof print.landscape === "boolean"
                    ? print.landscape
                    : true,
            align: "center",
            padding: 6,
        };
    };

      function defaultHeader(report) {
        return [
          { id: "logo", label: "Logo", show: true, editable: false },
          {
            id: "companyKh",
            label: "Company Name (Khmer)",
            show: true,
            editable: false,
          },
          { id: "company", label: "Company Name", show: true, editable: false },
          { id: "vat", label: "Company VAT", show: false, editable: false },
          { id: "title", label: report.name, show: true, editable: true },
          { id: "period", label: "Report Period", show: true, editable: false },
        ];
      }

      // Hidden columns start unchecked. Auto-built columns also hide ID-like keys.
      function defaultColConfig(report) {
        const dynamic = report.columns !== report.baseColumns;
        return report.columns.map((c) => {
          const rowNo = c.label === "No.";
          return {
            key: c.key,
            label: c.label,
            num: c.num,
            rowNo,
            // show:
            //   rowNo || !(c.hidden || (dynamic && Columns.ID_KEY.test(c.key))),
            show: rowNo
              ? !report.hideNo
              : !(c.hidden || (dynamic && Columns.ID_KEY.test(c.key))),
            width: rowNo ? 50 : c.width || (c.num ? 120 : 150),
            decimals: Number.isInteger(c.decimals) ? c.decimals : null,
            fixed: !rowNo && !!c.width,
          };
        });
      }

      function moveItem(list, from, to) {
        if (from === to || from < 0 || to < 0) return;
        if (from >= list.length || to >= list.length) return;
        list.splice(to, 0, list.splice(from, 1)[0]);
      }
      return function install($scope, ctx) {
        $scope.companyName =
          $rootScope.CompanyName || $rootScope.companyName || "Company Name";
        $scope.companyNameKh =
          $rootScope.CompanyNameKH || $rootScope.companyNameKh || "";
        $scope.companyVat = $rootScope.companyVat || "";

        $scope.zoom = 100;
        $scope.visibleColumns = [];
        $scope.colConfig = [];
        $scope.headerConfig = [];
        $scope.settings = defaultSettings();
        $scope.decimalChoices = [0, 1, 2, 3, 4, 5];
        $scope.cellPad = {};

        let companyLookupRequest = 0;
        $scope.$watch("filters.class", (className) => {
            const requestId = ++companyLookupRequest;

            $http
                .get(Config.API_BASE + "get-company-name", {
                    params: { className: className || "" },
                    ignoreLoadingBar: true,
                })
                .then((response) => {
                    if (requestId !== companyLookupRequest) return;

                    const rows = Array.isArray(response.data)
                        ? response.data
                        : (response.data && response.data.rows) || [];
                    const company = rows[0];

                    $scope.companyName = company
                        ? company.companyName || ""
                        : $rootScope.CompanyName || $rootScope.companyName || "Company Name";
                    $scope.companyNameKh = company
                        ? company.companyNameKH || ""
                        : $rootScope.CompanyNameKH || $rootScope.companyNameKh || "";
                })
                .catch((error) => {
                    console.error("Failed to load the report company name.", error);
                });
        });

        function resetCustomization(report) {
          $scope.headerConfig = defaultHeader(report);
          $scope.colConfig = defaultColConfig(report);
          //$scope.settings = defaultSettings();
          $scope.settings = defaultSettings(report);
        }

        function updateVisibleColumns() {
          const report = $scope.selectedReport;

          if (!report) {
            $scope.visibleColumns = [];
            $scope.cellPad = {};
            return;
          }

          const padPx = clamp(Number($scope.settings.padding), 0, 40);
          const padStyle = {
            paddingTop: padPx + "px",
            paddingBottom: padPx + "px",
          };
          $scope.cellPad = padStyle;

          const shown = [];
          $scope.colConfig.forEach((cfg) => {
            if (!cfg.show) return;
            const base = report.columns.find((c) => c.key === cfg.key);
            if (!base) return;

            const width = Number(cfg.width) > 0 ? Number(cfg.width) : null;
            const style = Object.assign({}, padStyle);
            if (width) style.width = width + "px";

            if (cfg.rowNo && width) {
              Object.assign(style, {
                minWidth: width + "px",
                maxWidth: width + "px",
                paddingLeft: "4px",
                paddingRight: "4px",
                textAlign: "center",
              });
            }

            if (cfg.fixed && width) {
              Object.assign(style, {
                minWidth: width + "px",
                maxWidth: width + "px",
                overflow: "hidden",
                textOverflow: "ellipsis",
              });
            }

            shown.push(
              Object.assign({}, base, {
                label: cfg.rowNo
                  ? base.label
                  : (cfg.label || "").trim() || base.label,
                width,
                decimals: Number.isInteger(cfg.decimals) ? cfg.decimals : null,
                style,
                fixed: !!cfg.fixed,
              }),
            );
          });

          $scope.visibleColumns = shown;
        }

        function applyDefaultLayout(report) {
          resetCustomization(report);
          updateVisibleColumns();
        }

        const moveColumn = (from, to) => moveItem($scope.colConfig, from, to);
        const moveHeader = (from, to) =>
          moveItem($scope.headerConfig, from, to);

        function companyNameKhWithPrefix() {
            const name = String($scope.companyNameKh || "").replace(
                /^[\s\u200B\u00A0]+|[\s\u200B\u00A0]+$/g,
                "",
            );
            return name && name.indexOf("ក្រុមហ៊ុន") !== 0
                ? "ក្រុមហ៊ុន " + name
                : name;
        }

        function headerText(h) {
          switch (h.id) {
            case "companyKh":
              return companyNameKhWithPrefix();
            case "company":
              return $scope.companyName;
            case "vat":
              return $scope.companyVat ? `VAT : ${$scope.companyVat}` : "";
            case "title":
              return (
                h.label ||
                ($scope.selectedReport && $scope.selectedReport.name) ||
                ""
              );
            case "period":
              return $scope.dateText();
            default:
              return "";
          }
        }

        function logoSrc() {
          return $rootScope.companyLogo || $(".brand-image").attr("src") || "";
        }

        function logoVisible() {
          const h = $scope.headerConfig.find((x) => x.id === "logo");
          return !!h && h.show && !!logoSrc();
        }

        const zoomIn = () => ($scope.zoom = Math.min(200, $scope.zoom + 10));
        const zoomOut = () => ($scope.zoom = Math.max(50, $scope.zoom - 10));

        // Drag the right edge of a header cell to resize the column.
        function startResize(e, column) {
          e.preventDefault();
          e.stopPropagation();

          const cfg = $scope.colConfig.find((c) => c.key === column.key);
          if (!cfg) return;

          const th = e.currentTarget.parentElement;
          const scale = $scope.zoom / 100;
          const startX = e.clientX;
          const startW = th.getBoundingClientRect().width / scale;

          document.body.classList.add("rp-resizing");

          function onMove(ev) {
            const w = startW + (ev.clientX - startX) / scale;
            cfg.width = Math.round(clamp(w, 20, 800));
            cfg.fixed = true;
            $scope.$applyAsync();
          }

          function onUp() {
            document.removeEventListener("mousemove", onMove);
            document.removeEventListener("mouseup", onUp);
            document.body.classList.remove("rp-resizing");
          }

          document.addEventListener("mousemove", onMove);
          document.addEventListener("mouseup", onUp);
        }

        // @page size follows the Landscape setting.
        let pageStyleEl = null;
        function updatePageStyle() {
          if (!pageStyleEl) {
            pageStyleEl = document.createElement("style");
            pageStyleEl.id = "rp-page-style";
            document.head.appendChild(pageStyleEl);
          }
          //const landscape = !$scope.settings || $scope.settings.landscape;
          // pageStyleEl.textContent = `@page { size: A3 ${landscape ? "landscape" : "portrait"}; }`;

        const landscape = !$scope.settings || $scope.settings.landscape;
        const report = $scope.selectedReport;
        const printSettings = report && report.printSettings;
        const paperSize =
            ($scope.settings && $scope.settings.paperSize) || "A3";
        const screenWidth =
            report && report.uiSettings && report.uiSettings.width;
        const printWidth = printSettings && printSettings.width;

        const screenStyle = screenWidth
                ? `@media screen {
                   #report-result {
                     width: ${screenWidth};
                     max-width: 100%;
                     margin-left: auto;
                     margin-right: auto;
                   }
                 }`
                : "";

            const printStyle = printWidth
                ? `@media print {
                   #report-result {
                     width: ${printWidth} !important;
                     max-width: 100% !important;
                     margin-left: auto !important;
                     margin-right: auto !important;
                   }
                 }`
                : "";

            pageStyleEl.textContent = `${screenStyle}${printStyle}
              @page { size: ${paperSize} ${landscape ? "landscape" : "portrait"}; }
            `;
        }

        ctx.applyDefaultLayout = applyDefaultLayout;

        Object.assign($scope, {
          zoomIn,
          zoomOut,
          moveColumn,
          moveHeader,
          headerText,
          logoSrc,
          logoVisible,
          startResize,
        });

        $scope.$watch("colConfig", updateVisibleColumns, true);
        $scope.$watch("settings.padding", updateVisibleColumns);
        $scope.$watch("settings.landscape", updatePageStyle);
        $scope.$watch("selectedReport", updatePageStyle);
        $scope.$watch("settings.paperSize", updatePageStyle);
        $scope.$on("$destroy", function () {
          if (pageStyleEl) pageStyleEl.remove();
        });
      };
    },
  ]);

  // ── Cells: what table cells and the totals row show ─────────────
  app.factory("ReportCellsFeature", [
    "ReportFormat",
    "ReportTotals",
    "ReportGrouping",
    function (Format, Totals, Grouping) {
      return function install($scope) {
        //function cell(row, column) {
        //  $scope.isEmptyField = Format.isBlank(row, column);
        //  return Format.display(row, column);
        //}
        function cell(row, column) {
            $scope.isEmptyField = Format.isBlank(row, column);

            const cfg =
                $scope.selectedReport &&
                Grouping.get($scope.selectedReport.name);
            const option = cfg && cfg.subtotalDisplay;

            if (
                option &&
                row &&
                row.rowType === (option.rowType || "subtotal")
            ) {
                if (option.labelColumn === column.key) return option.label || "Total";

                if (
                    option.hideOtherColumns &&
                    !(option.lineColumns || []).includes(column.key)
                ) {
                    return "";
                }
            }

            const settings =
              $scope.selectedReport && $scope.selectedReport.uiSettings;
            return Format.display(
              row,
              column,
              !!(settings && settings.zeroAsDash),
            );
        }

        $scope.cellStyle = (row, column, columnStyle) => {
            const style = Object.assign({}, columnStyle || {});
            const cfg =
                $scope.selectedReport &&
                Grouping.get($scope.selectedReport.name);
            const option = cfg && cfg.subtotalDisplay;

            if (
                !option ||
                !row ||
                row.rowType !== (option.rowType || "subtotal") ||
                !(option.lineColumns || []).includes(column.key)
            ) {
                return style;
            }

            const inset = Number.isFinite(option.inset) ? option.inset : 12;

            return Object.assign(style, {
                backgroundImage:
                    `linear-gradient(to right, transparent ${inset}px, var(--rp-fg) ${inset}px, var(--rp-fg) calc(100% - ${inset}px), transparent calc(100% - ${inset}px))`,
                backgroundRepeat: "no-repeat",
                backgroundSize: "100% 1px",
                backgroundPosition: option.linePosition === "bottom" ? "bottom" : "top",
            });
        };
        const totalForColumn = (column, totals) => {
          const currentTotals = totals || $scope.reportTotals;
          const settings =
            $scope.selectedReport && $scope.selectedReport.uiSettings;
          const rawTotal = Totals.findTotal(currentTotals || {}, column);
          if (
            settings &&
            settings.zeroAsDash &&
            Totals.isTotalColumn(column) &&
            (column.num || column.agg) &&
            rawTotal != null &&
            String(rawTotal).trim() !== "" &&
            Number.isFinite(Number(rawTotal)) &&
            Number(rawTotal) === 0
          ) {
            return "-";
          }
          return Totals.format(column, currentTotals);
        };
          function totalLabel() {
              const settings =
                  $scope.selectedReport && $scope.selectedReport.uiSettings;
              const label = settings && settings.totalLabel;
              return typeof label === "string" && label.trim()
                  ? label.trim()
                  : "Total";
          }

          function totalLabelClass() {
              const settings =
                  $scope.selectedReport && $scope.selectedReport.uiSettings;
              const alignment = settings && settings.totalLabelAlign;
              return alignment === "left" || alignment === "center"
                  ? "rp-total-label-" + alignment
                  : "";
          }

        $scope.rowClass = (row) =>
          row && row.rowType ? "rp-stmt-" + row.rowType : "";

        Object.assign($scope, {
          cell,
          totalLabel,
          totalLabelClass,
          totalForColumn,
          cellClass: Format.cellClass,
          linkFor: Format.linkFor,
          runAction: Format.runAction,
        });
      };
    },
  ]);

  // ── Grouping: page rows -> parent (group) rows + sub rows ───────
  // Reports without a registered grouping get a plain list, so one template serves all.
  app.factory("ReportGroupingFeature", [
    "ReportGrouping",
    function (Grouping) {
      return function ($scope, ctx) {
        const collapsed = {};

        $scope.viewRows = [];
        $scope.printRows = [];

        // function config() {
        //   return $scope.selectedReport
        //     ? Grouping.get($scope.selectedReport.name)
        //     : null;
          // }

        function config() {
            if (!$scope.selectedReport) return null;

            const cfg = Grouping.get($scope.selectedReport.name);
            if (!cfg || cfg.by.length === 0) return null;
            return cfg && (!cfg.statement || cfg.nestedBy.length > 0) ? cfg : null;
        }
        function keyOf(row, cfg) {
          for (let i = 0; i < cfg.by.length; i++) {
            const v = row[cfg.by[i]];
            if (v !== undefined && v !== null && String(v).trim() !== "")
              return String(v).trim();
          }
          return cfg.emptyLabel;
        }

        function toNumber(v) {
          const n = Number(String(v == null ? "" : v).replace(/,/g, ""));
          return isFinite(n) ? n : 0;
        }

        // function stateKey(key) {
        //   return $scope.selectedReport.name + "|" + key;
        // }
        function stateKey(key, path) {
            return $scope.selectedReport.name + "|" + (path ? JSON.stringify(path) : key);
        }

        //function isCollapsed(cfg, key) {
        //  const k = stateKey(key);
        //  return k in collapsed ? collapsed[k] : cfg.collapsed;
        //}
        function isCollapsed(cfg, key, path) {
            const k = stateKey(key, path);
            return k in collapsed ? collapsed[k] : cfg.collapsed;
        }

        function pageOffset() {
          const size = $scope.pagination && $scope.pagination.pageSize;
          return (($scope.currentPage || 1) - 1) * (size || 0);
        }

        function build() {
          const rows = ($scope.pageRows && $scope.pageRows()) || [];
          const cfg = config();
          const offset = pageOffset();
          const view = [];

          if (!cfg) {
            rows.forEach(function (row, i) {
              view.push({
                id: "r" + i,
                type: "row",
                row: row,
                no: offset + i + 1,
              });
            });
            $scope.viewRows = view;
            $scope.printRows = view;
            return;
          }

          if (cfg.nestedBy.length) {
            const levels = cfg.by.slice(0, 1).concat(cfg.nestedBy);
            const print = [];
            let n = offset;

            function renderLevel(levelRows, parentPath, ancestorsExpanded) {
                const field = levels[parentPath.length];
                const levelGroups = [];
                const levelIndex = new Map();

                levelRows.forEach(function(row) {
                    const key = keyOf(row, {
                        by: [field],
                        emptyLabel: cfg.emptyLabel,
                    });
                    let group = levelIndex.get(key);
                    if (!group) {
                        group = { key: key, rows: [], sums: {} };
                        levelIndex.set(key, group);
                        levelGroups.push(group);
                    }
                    group.rows.push(row);
                    cfg.sum.forEach(function(sumField) {
                        group.sums[sumField] =
                            (group.sums[sumField] || 0) + toNumber(row[sumField]);
                    });
                });

                levelGroups.forEach(function(group) {
                    const path = parentPath.concat(group.key);
                    const normalize = (value) =>
                        String(value).replace(/\s*-\s*/g, "-");
                    const duplicateParent =
                        parentPath.length > 0 &&
                        normalize(group.key) ===
                        normalize(parentPath[parentPath.length - 1]);

                    const head = {
                        id: "g:" + JSON.stringify(path),
                        type: "group",
                        key: group.key,
                        path: path,
                        level: path.length,
                        label: cfg.label,
                        title: cfg.label ? cfg.label + ": " + group.key : group.key,
                        showCount: cfg.showCount !== false,
                        count: group.rows.length,
                        sums: group.sums,
                        collapsed: duplicateParent
                            ? false
                            : isCollapsed(cfg, group.key, path),
                    };
                    const expanded = ancestorsExpanded && !head.collapsed;

                    if (!duplicateParent) {
                        print.push(head);
                        if (ancestorsExpanded) view.push(head);
                    }

                    if (path.length < levels.length) {
                        renderLevel(group.rows, path, expanded);
                        return;
                    }

                    group.rows.forEach(function(row) {
                        n++;
                        const item = {
                            id: "r" + n,
                            type: "row",
                            row: row,
                            no: n,
                            group: group.key,
                        };
                        print.push(item);
                        if (expanded) view.push(item);
                    });
                });
            }

            renderLevel(rows, [], true);
            $scope.viewRows = view;
            $scope.printRows = print;
            return;
          }

          const groups = [];
          const index = {};
          rows.forEach(function (row) {
            const key = keyOf(row, cfg);
            let g = index[key];
            if (!g) {
              g = index[key] = { key: key, rows: [], sums: {} };
              groups.push(g);
            }
            g.rows.push(row);
            cfg.sum.forEach(function (f) {
              g.sums[f] = (g.sums[f] || 0) + toNumber(row[f]);
            });
          });

          const print = [];
          let n = offset;
          groups.forEach(function (g) {
            const head = {
              id: "g:" + g.key,
              type: "group",
              key: g.key,
              label: cfg.label,
              title: cfg.label ? cfg.label + ": " + g.key : g.key,
              showCount: cfg.showCount !== false, // must exist
              count: g.rows.length,
              sums: g.sums,
              collapsed: isCollapsed(cfg, g.key),
            };
            view.push(head);
            print.push(head);
            g.rows.forEach(function (row) {
              n++;
              const item = {
                id: "r" + n,
                type: "row",
                row: row,
                no: n,
                group: g.key,
              };
              if (!head.collapsed) view.push(item);
              print.push(item);
            });
          });

          $scope.viewRows = view;
          $scope.printRows = print;
        }

        $scope.groupConfig = config;

        $scope.toggleGroup = function (item) {
          collapsed[stateKey(item.key, item.path)] = !item.collapsed;
          build();
        };

        function setAll(value) {
          $scope.printRows.forEach(function (item) {
            if (item.type === "group")
              collapsed[stateKey(item.key, item.path)] = value;
          });
          build();
        }
        $scope.collapseAll = function () {
          setAll(true);
        };
        $scope.expandAll = function () {
          setAll(false);
        };

        // Parent label spans every column before the first summed column.
        $scope.groupSpan = function () {
          const cfg = config();
          const cols = $scope.visibleColumns || [];
          if (!cfg || !cfg.sum.length) return cols.length || 1;
          for (let i = 0; i < cols.length; i++) {
            if (cfg.sum.indexOf(cols[i].key) !== -1) return Math.max(i, 1);
          }
          return cols.length || 1;
        };

        // Format a parent-row sum with the same formatter the sub rows use.
        $scope.groupSum = function (item, col) {
          const cfg = config();
          if (!cfg || cfg.sum.indexOf(col.key) === -1) return "";
          const pseudo = {};
          pseudo[col.key] = item.sums[col.key];
          return $scope.cell(pseudo, col);
        };

        $scope.$watchCollection(function () {
          return $scope.pageRows && $scope.pageRows();
        }, build);
        $scope.$watch("selectedReport.id", build);
        $scope.$watch("currentPage", build);
      };
    },
  ]);

  // ── HeaderGroups: spanning parent headers ───────────────────────
  app.factory("ReportHeaderGroupsFeature", function () {
    return function ($scope) {
      $scope.headerGroups = [];
      $scope.hasHeaderGroups = false;

      function build(columns) {
        const groups = [];
        let any = false;
        (columns || []).forEach(function (c) {
          if (!c.parent) {
            groups.push({ id: "c:" + c.key, column: c, span: 1 });
            return;
          }
          any = true;
          const last = groups[groups.length - 1];
          if (last && !last.column && last.label === c.parent) last.span++;
          else
            groups.push({
              id: "g:" + groups.length + c.parent,
              label: c.parent,
              span: 1,
            });
        });
        $scope.headerGroups = groups;
        $scope.hasHeaderGroups = any;
      }

      $scope.$watchCollection("visibleColumns", build);
    };
  });

  // ── Navigation: categories, report list, open / close a report ──
  app.factory("ReportNavigationFeature", [
    "$rootScope",
    "ReportConfig",
    "ReportRegistry",
    function ($rootScope, Config, Registry) {
      function reportIcon(name) {
        const n = (name || "").toLowerCase();
        if (/reconcil/.test(n)) return "ti-checklist";
        if (/variance/.test(n)) return "ti-file-diff";
        if (/detail/.test(n)) return "ti-list";
        if (/summary/.test(n)) return "ti-archive";
        if (/aging/.test(n)) return "ti-clock-hour-4";
        if (/list/.test(n)) return "ti-list-details";
        return "ti-file-text";
      }

      return function install($scope, ctx) {
        $rootScope.title = Config.DEFAULT_TITLE;

        $scope.reportCategories = Registry.buildCategories();
        ctx.allReports = $scope.reportCategories.flatMap((c) => c.reports);

        $scope.selectedCategory = null;
        $scope.collapsed = {};
        $scope.selectedReport = null;
        $scope.currentCategory = null;
        $scope.currentGroup = null;
        $scope.visibleReports = [];
        $scope.ui = {
          typeOpen: false,
          filterOpen: true,
          customizeOpen: false,
          drawerOpen: false,
          custTab: "column",
          reportSearch: "",
        };

        const shownCategories = () =>
          $scope.selectedCategory
            ? [$scope.selectedCategory]
            : $scope.reportCategories;

        function toggleSection(id) {
          $scope.collapsed[id] = !$scope.collapsed[id];
        }

        function selectCategory(selected) {
          $scope.selectedCategory = selected;
        }

        function updateVisibleReports() {
          const term = ($scope.ui.reportSearch || "").toLowerCase().trim();
          const source = $scope.currentCategory
            ? $scope.currentCategory.reports.filter(
                (r) => r.group === $scope.currentGroup,
              )
            : [];
          $scope.visibleReports = source.filter(
            (r) => !term || r.name.toLowerCase().includes(term),
          );
        }

        const categoryOf = (report) =>
          $scope.reportCategories.find((c) => c.reports.includes(report)) ||
          null;

        function openReport(report, drillFilters, origin) {
          $scope.currentCategory = categoryOf(report);
          $scope.currentGroup = report.group;
          updateVisibleReports();
          $scope.ui.typeOpen = false;
          $scope.ui.drawerOpen = false;
          $scope.ui.filterOpen = true;
          $scope.ui.customizeOpen = false;
          $scope.ui.reportSearch = "";
          $scope.zoom = 100;
          selectReport(report, drillFilters, origin);
        }

        function closeViewer() {
          $scope.drillOrigin = null;
          $scope.ui.drawerOpen = false;
          $scope.selectedReport = null;
          $scope.visibleFilterFields = [];
          $scope.visibleColumns = [];
          $scope.colConfig = [];
          $scope.headerConfig = [];
          $rootScope.title = Config.DEFAULT_TITLE;
          ctx.clearResult();
        }

        function selectReport(report, drillFilters, origin) {
          if (!report) return;

        const previousDate =
        $scope.selectedReport && !drillFilters
            ? {
                filters: {
                    from: $scope.filters.from,
                    to: $scope.filters.to,
                    month: $scope.filters.month,
                },
                preset: $scope.dateState.preset,
                monthSelection: Object.assign({}, $scope.monthSelection),
            }
            : null;

          $scope.drillOrigin = origin || null;
          $scope.selectedReport = report;
          $scope.visibleFilterFields = Registry.filtersFor(report);
          ctx.loadFieldOptions($scope.visibleFilterFields);
          ctx.applyDefaultLayout(report);
          $scope.resetFilters();
          if (previousDate) {
            Object.assign($scope.filters, previousDate.filters);
            $scope.dateState.preset = previousDate.preset;
            Object.assign($scope.monthSelection, previousDate.monthSelection);
          }
          if (drillFilters) Object.assign($scope.filters, drillFilters);
          $rootScope.title = report.name;
          ctx.clearResult();
          $scope.sort = { key: null, dir: "asc" };
          ctx.loadReport(report);
        }

        function drillBack() {
          const origin = $scope.drillOrigin;
          if (!origin) return;
          openReport(origin.report, origin.filters); // restores the report and its date range
        }

        ctx.defaultDatePreset = () =>
          Registry.datePresetFor($scope.selectedReport);

        $scope.$watch("ui.reportSearch", updateVisibleReports);

        $scope.$on("report:drill", (event, { report: name, filters }) => {
          const target = ctx.allReports.find((r) => r.name === name);
          if (!target) return;

          const origin = {
            report: $scope.selectedReport,
            filters: angular.copy($scope.filters),
          };
          $scope.$evalAsync(() => openReport(target, filters, origin));
        });

        Object.assign($scope, {
          shownCategories,
          toggleSection,
          selectCategory,
          openReport,
          closeViewer,
          selectReport,
          reportIcon,
          drillBack,
        });
      };
    },
  ]);

  // ── Favorites: saved in localStorage ────────────────────────────
  app.factory("ReportFavoritesFeature", [
    "ReportConfig",
    function (Config) {
      function readIds() {
        try {
          const ids = JSON.parse(localStorage.getItem(Config.FAVORITES_KEY));
          return Array.isArray(ids) ? ids : [];
        } catch (e) {
          return [];
        }
      }

      return function install($scope, ctx) {
        let ids = readIds();
        $scope.favorites = [];

        const isFavorite = (report) => ids.includes(report.id);

        function refresh() {
          $scope.favorites = ctx.allReports.filter((r) => ids.includes(r.id));
        }

        function toggleFavorite(report, $event) {
          if ($event) $event.stopPropagation();
          ids = isFavorite(report)
            ? ids.filter((id) => id !== report.id)
            : ids.concat(report.id);
          try {
            localStorage.setItem(Config.FAVORITES_KEY, JSON.stringify(ids));
          } catch (e) {}
          refresh();
        }

        Object.assign($scope, { isFavorite, toggleFavorite });

        refresh();
      };
    },
  ]);

  // ── Export: print + Excel (export fetches ALL rows) ─────────────
  app.factory("ReportExportFeature", [
    "$http",
    "$timeout",
    "ReportTotals",
    "ReportExcel",
    function ($http, $timeout, Totals, Excel) {
      return function install($scope, ctx) {
        const canExport = () =>
          !!$scope.selectedReport && !$scope.isLoading && !$scope.reportError;

        function fetchAllRows(report) {
          return $http
            .get(report.endpoint, {
              params: Object.assign(ctx.activeFilters(), ctx.sortParams(), {
                allRows: true,
                totalFields: Totals.fieldsFor(report),
              }),
              ignoreLoadingBar: true,
            })
            .then(function (res) {
              const data = res.data;
              const rows = Array.isArray(data)
                ? data
                : (data && data.rows) || [];
              return {
                rows,
                totals: Totals.resolveTotals(
                  data && data.totals,
                  rows,
                  report.columns,
                  true,
                ),
              };
            });
        }

        function exportAll(report, onReady) {
          fetchAllRows(report)
            .then(function (result) {
              if ($scope.selectedReport === report) onReady(result);
            })
            .catch(function (err) {
              window.alert(ctx.errorMessage(err, "Export failed"));
            });
        }

        function printReport() {
          if (!canExport() || !$scope.previewed) return;
          $timeout(function () {
            window.print();
          });
        }

        // Same lines the printed header shows, in the user's order, hidden ones skipped.
        function headerLines() {
          return $scope.headerConfig
            .filter((h) => h.show && h.id !== "logo")
            .map((h) => ({
              id: h.id,
              text: ($scope.headerText(h) || "").trim(),
            }))
            .filter((l) => l.text);
        }

        // Logo as { buffer, extension }, or null (hidden, SVG, or failed to load).
        function loadLogo() {
          if (!$scope.logoVisible()) return Promise.resolve(null);
          return fetch($scope.logoSrc())
            .then((r) => r.blob())
            .then((blob) => {
              const ext = {
                "image/png": "png",
                "image/jpeg": "jpeg",
                "image/gif": "gif",
              }[blob.type];
              if (!ext) return null; // Excel can't embed SVG/WebP
              return blob
                .arrayBuffer()
                .then((buffer) => ({ buffer, extension: ext }));
            })
            .catch(() => null);
        }

        function exportExcel() {
          if (!canExport()) return;
          if (typeof ExcelJS === "undefined") {
            window.alert(
              "ExcelJS is not loaded. Add the script to the layout page.",
            );
            return;
          }

          const report = $scope.selectedReport;
          exportAll(report, function (result) {
            loadLogo()
              .then((logo) =>
                Excel.buildWorkbook(
                  report,
                  $scope.visibleColumns,
                  result.rows,
                  result.totals,
                  { lines: headerLines(), logo },
                ),
              )
              .then(function (buffer) {
                Excel.downloadFile(
                  `${report.name}.xlsx`,
                  buffer,
                  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                );
              })
              .catch(function (err) {
                window.alert("Export failed: " + err.message);
              });
          });
        }


        const totalSpan = () =>
              Excel.totalSpan($scope.visibleColumns, $scope.selectedReport);
        //const totalSpan = () => Excel.totalSpan($scope.visibleColumns);

        Object.assign($scope, {
          canExport,
          printReport,
          exportExcel,
          totalSpan,
        });
      };
    },
  ]);
})();
