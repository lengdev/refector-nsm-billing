"use strict";
(function () {
    var _gridCache = {};
    var DEFAULT_TTL_MS = 30000;
    function cacheKeyFor(payload) {
        return JSON.stringify(payload);
    }

    function getCached(tblId, key, ttl) {
        var bucket = _gridCache[tblId];
        if (!bucket) return null;
        var entry = bucket[key];
        if (!entry) return null;
        if (Date.now() - entry.time > ttl) {
            delete bucket[key];
            return null;
        }
        return entry.response;
    }

    function setCached(tblId, key, response) {
        if (!_gridCache[tblId]) _gridCache[tblId] = {};
        _gridCache[tblId][key] = { time: Date.now(), response: response };
    }

    // ── Custom column pinning (position: sticky) ────────────────────────
    // We deliberately don't use Kendo's native "locked columns" feature.
    // That feature requires at least one column to already be locked when
    // the grid is first initialized, and it can never go back to zero
    // locked columns afterwards — there's no supported way to start with
    // nothing pinned and let the user freely pin/unpin from the UI. So we
    // build pinning ourselves with plain CSS `position: sticky`: zero
    // columns pinned by default, and "Pin Column"/"Unpin Column" on a
    // column's menu freezes everything from the start of the grid up to
    // (and including) that column — Excel-style "freeze panes", not
    // independent per-column toggling.
    var _pinnedFields = {}; // tblId -> ordered array of pinned field names
    var PIN_CSS_ID = "kendo-pin-column-style";

    function ensurePinStyles() {
        if ($("#" + PIN_CSS_ID).length) return;
        $(
            "<style id='" +
                PIN_CSS_ID +
                "'>" +
                ".pinned-col-edge{box-shadow:2px 0 5px -2px rgba(0,0,0,.25);}" +
                ".pin-indicator-icon{font-size:12px;margin-right:4px;opacity:.9;vertical-align:middle;}" +
                "</style>",
        ).appendTo("head");
    }

    function updatePinnedIndicators($headerCells, pinnedFields) {
        $headerCells.each(function () {
            var $th = $(this);
            $th.find(".pin-indicator-icon").remove();

            var field = $th.attr("data-field");
            if (!field || pinnedFields.indexOf(field) === -1) return;

            var $icon = $(
                '<i class="ti ti-pin pin-indicator-icon" title="Pinned"></i>',
            );
            var $menuBtn = $th
                .find(
                    ".k-header-column-menu, .k-grid-column-menu, [class*='column-menu']",
                )
                .first();

            if ($menuBtn.length) {
                $icon.insertBefore($menuBtn);
            } else {
                $th.find(".k-cell-inner, .k-link").first().append($icon);
            }
        });
    }

    function getPinnedFields(tblId) {
        if (!_pinnedFields[tblId]) _pinnedFields[tblId] = [];
        return _pinnedFields[tblId];
    }

    function isFieldPinned(tblId, field) {
        return getPinnedFields(tblId).indexOf(field) !== -1;
    }

    // Freezes every column from the start of the grid up to and including
    // `field` (Excel-style "freeze panes"), replacing whatever was pinned
    // before.
    function pinUpToField(tblId, field) {
        var $headerRow = $("#" + tblId)
            .find(".k-grid-header thead tr")
            .first();
        var $headerCells = $headerRow.children();
        var targetIndex = $headerCells
            .filter('[data-field="' + field + '"]')
            .index();

        var fieldsUpToTarget = [];
        $headerCells.each(function (i) {
            if (i > targetIndex) return false; // stop once past target
            var f = $(this).attr("data-field");
            if (f) fieldsUpToTarget.push(f);
        });

        _pinnedFields[tblId] = fieldsUpToTarget;
    }

    function unpinAll(tblId) {
        _pinnedFields[tblId] = [];
    }

    // Resolves the background a pinned cell should paint, based on the
    // row's own state (selected / alt-striped / normal) and dark mode —
    // never trusts a computed `background-color` read, since striping is
    // often applied via a stylesheet rule that reports as transparent.
    function pinnedRowBackground($row) {
        var isDark = document.body.classList.contains("dark-mode");
        var isSelected =
            $row.hasClass("k-state-selected") || $row.hasClass("k-selected");
        var isAlt = $row.hasClass("k-alt");

        if (isSelected) {
            return isDark ? "var(--dark-bg-selected)" : "#c1d9f9";
        }
        if (isDark) {
            return isAlt
                ? "var(--dark-bg-secondary)"
                : "var(--dark-bg-primary)";
        }
        return isAlt ? "#f8f8f8" : "#ffffff";
    }

    // Same idea for the header row — hardcode the real header color
    // instead of reading computed style, which can resolve transparent.
    var PINNED_HEADER_BG = "#ff7a1a"; // match your actual header background

    // Re-measures whichever columns are currently pinned (in their current
    // on-screen order/width) and (re)applies sticky positioning + left
    // offsets, so they stay visually put while the rest of the grid
    // scrolls horizontally. Safe to call repeatedly — e.g. after every
    // dataBound, resize, reorder, or selection change — since it always
    // clears old pin styling first.
    function applyPinnedColumns(tblId) {
        var $grid = $("#" + tblId);
        var $headerRow = $grid.find(".k-grid-header thead tr").first();
        if (!$headerRow.length) return;

        $grid.find(".pinned-col").each(function () {
            $(this).removeClass("pinned-col pinned-col-edge").css({
                position: "",
                left: "",
                zIndex: "",
                background: "",
            });
        });

        var pinned = getPinnedFields(tblId);

        var $headerCells = $headerRow.children();

        updatePinnedIndicators($headerCells, pinned);

        if (!pinned.length) return;

        // Sort by actual left-to-right column position, not pin order,
        // so pinned columns always stack in correct visual order.
        var pinnedSorted = pinned.slice().sort(function (a, b) {
            var idxA = $headerCells.filter('[data-field="' + a + '"]').index();
            var idxB = $headerCells.filter('[data-field="' + b + '"]').index();
            return idxA - idxB;
        });

        var $bodyRows = $grid.find(
            ".k-grid-content table > tbody > tr, .k-grid-content-locked table > tbody > tr",
        );
        var offset = 0;

        pinnedSorted.forEach(function (field, i) {
            var $th = $headerCells.filter('[data-field="' + field + '"]');
            if (!$th.length) return;
            var colIndex = $th.index();
            var width = $th.outerWidth();
            var isEdge = i === pinnedSorted.length - 1;

            $th.addClass("pinned-col")
                .toggleClass("pinned-col-edge", isEdge)
                .css({
                    position: "sticky",
                    left: offset + "px",
                    zIndex: 3,
                    background: PINNED_HEADER_BG,
                });

            $bodyRows.each(function () {
                var $row = $(this);
                if (
                    $row.hasClass("k-grouping-row") ||
                    $row.hasClass("k-detail-row")
                )
                    return;
                var $cell = $row.children().eq(colIndex);
                if (!$cell.length) return;
                $cell
                    .addClass("pinned-col")
                    .toggleClass("pinned-col-edge", isEdge)
                    .css({
                        position: "sticky",
                        left: offset + "px",
                        zIndex: 2,
                        background: pinnedRowBackground($row),
                    });
            });

            offset += width;
        });
    }

    // Adds a "Pin Column"/"Unpin Column" item to a column's menu. Wired up
    // per-grid via options.columnMenuInit in initKendoGrid. Clicking pins
    // (freezes) everything up to and including this column; clicking again
    // on the column that's currently the rightmost pinned one unpins all.
    function makeColumnMenuInit(tblId) {
        return function (e) {
            var field = e.field;
            if (!field) return;
            var $menu = e.container.find(".k-menu, .k-menu-group").first();
            var $target = $menu.length ? $menu : e.container;
            var pinned = isFieldPinned(tblId, field);
            var $li = $(
                `<li class="k-item k-menu-item" role="menuitem" tabindex="-1">
                    <span class="k-link k-menu-link">
                        <i class="ti ${pinned ? "ti-pinned-off" : "ti-pin"}"></i>
                        <span class="k-menu-link-text">${pinned ? "Unpin Column" : "Pin Column"}</span>
                    </span>
                </li>`,
            );
            $li.on("click", function () {
                var alreadyPinned = isFieldPinned(tblId, field);

                if (alreadyPinned) {
                    unpinAll(tblId);
                } else {
                    pinUpToField(tblId, field);
                }
                applyPinnedColumns(tblId);

                var nowPinned = isFieldPinned(tblId, field);
                $li.find("i.ti")
                    .removeClass("ti-pin ti-pinned-off")
                    .addClass(nowPinned ? "ti-pinned-off" : "ti-pin");
                $li.find(".k-menu-link-text").text(
                    nowPinned ? "Unpin Column" : "Pin Column",
                );
                $(document.body).trigger("click");
            });
            $target.append($li);
        };
    }

    // Call this after any add/edit/delete affecting a grid's data.
    // Pass no key to wipe the whole grid's cache (safest default).
    window.invalidateGridCache = function (tblId, key) {
        if (!tblId) {
            _gridCache = {};
            return;
        }
        if (key) {
            if (_gridCache[tblId]) delete _gridCache[tblId][key];
        } else {
            delete _gridCache[tblId];
        }
    };

    function esc(s) {
        return s === null || s === undefined ? "" : String(s);
    }
    function buildEditButton(col, ctx, primaryExpr) {
        var onclick = col.editFunction
            ? col.editFunction.replace(
                  /#=\s*(?:dataItem\.)?(\w+)\s*#/g,
                  "#=data.$1#",
              )
            : "EditGrid_" + ctx.rdn + "(this,'" + primaryExpr + "');";
        return (
            '<a name="Edit_' +
            ctx.rdn +
            '" class="btn btn-primary btn-sm text-white" ' +
            'href="javascript:void(0);" onclick="' +
            onclick +
            '" title="Edit" alt="Edit">' +
            '<i class="ti ti-pencil"></i></a>'
        );
    }

    function buildDeleteButton(col, ctx, primaryExpr) {
        var args = [
            primaryExpr,
            "this",
            "'" + col.fieldName + "'",
            "'" + ctx.tableName + "'",
            "'" + esc(col.deleteUseProcedureName) + "'",
            "'" + esc(col.deleteWithTableNameEncrypted) + "'",
            "'" + esc(col.deletePrimaryTblName) + "'",
            "'" + esc(col.deletePrimaryFiledName) + "'",
            "'" + esc(col.deletePrimaryValue) + "'",
            "'" + esc(col.deletePrimaryTblNameDependOnTblName) + "'",
        ].join(", ");
        return (
            '<a name="Delete" class="btn btn-danger btn-sm text-white" ' +
            'href="javascript:void(0);" onclick="DeleteTR_' +
            ctx.rdn +
            "(" +
            args +
            ');" ' +
            'title="Delete" alt="Delete"><i class="ti ti-trash"></i></a>'
        );
    }

    // Custom date-range filter cell (daterangepicker), one per field.
    function betweenFilterTemplate(fieldName, tblId) {
        return function (args) {
            var $cell = args.element.parents(".k-filtercell");
            $cell.empty();
            $cell.html(
                '<span class="k-input k-input-solid k-input-md k-rounded-md" style="display:flex; justify-content:center;">' +
                    '<input id="' +
                    fieldName +
                    '" class="k-input-inner"/>' +
                    '<span style="display:none;" id="span_' +
                    fieldName +
                    '" unselectable="on" class="k-clear-value" title="clear" role="button" tabindex="-1">' +
                    '<span class="k-icon k-i-x"></span></span></span>',
            );
            var $input = $("#" + fieldName);
            var $clear = $("#span_" + fieldName);

            $input.on("keypress", function (e) {
                if (e.which == 13 && $(this).val() === "") {
                    $("#" + tblId)
                        .data("kendoGrid")
                        .dataSource.filter({});
                    $clear.hide();
                }
            });
            $input.daterangepicker({
                autoUpdateInput: false,
                autoApply: true,
                locale: { format: "DD/MM/YY", cancelLabel: "Clear" },
            });
            $input.on("cancel.daterangepicker", function () {
                $("#" + tblId)
                    .data("kendoGrid")
                    .dataSource.filter({});
                $clear.hide();
                $input.val("");
            });
            $input.on("apply.daterangepicker", function (ev, picker) {
                $clear.show();
                $(this).val(
                    picker.startDate.format("DD/MM/YY") +
                        " - " +
                        picker.endDate.format("DD/MM/YY"),
                );
                var filter = { logic: "and", filters: [] };
                filter.filters.push({
                    field: fieldName,
                    operator: "gte",
                    value: picker.startDate.format("YYYY/MM/DD"),
                });
                filter.filters.push({
                    field: fieldName,
                    operator: "lte",
                    value: picker.endDate.format("YYYY/MM/DD"),
                });
                $("#" + tblId)
                    .data("kendoGrid")
                    .dataSource.filter(filter);
            });
            $clear.hide();
            $input.val("");
            $clear.click(function () {
                $("#" + tblId)
                    .data("kendoGrid")
                    .dataSource.filter({});
                $clear.hide();
                $input.val("");
            });
        };
    }

    // Conditional edit/delete template (ShowActionNameOnConditionField).
    function conditionalActionTemplate(col, ctx) {
        return function (dataItem) {
            var primaryId = dataItem[col.fieldName];
            var conditionValues = String(
                col.showActionNameOnConditionValue,
            ).split(",");
            var isMatch = conditionValues.some(function (v) {
                return dataItem[col.showActionNameOnConditionField] == v.trim();
            });
            if (!isMatch) return '<button class="btn-group"></button>';

            var strEdit = "",
                strDelete = "";
            if (col.isModifyField && ctx.allowEdit && !col.hideEdit) {
                strEdit = col.editFunction
                    ? '<a name="Edit_' +
                      ctx.rdn +
                      '" id="' +
                      primaryId +
                      '" class="btn btn-primary btn-sm text-white" href="javascript:void(0);" onclick="' +
                      col.editFunction.replace(
                          /#=\s*(\w+)\s*#/g,
                          function (_, f) {
                              return dataItem[f];
                          },
                      ) +
                      '" title="Edit"><i class="ti ti-pencil"></i></a>'
                    : buildEditButton(col, ctx, primaryId);
            }
            if (col.isModifyField && ctx.allowDelete && !col.hideEdit) {
                strDelete = buildDeleteButton(col, ctx, "'" + primaryId + "'");
            }
            return (
                '<button class="btn-group">' + strEdit + strDelete + "</button>"
            );
        };
    }

    // ── Column builder ───────────────────────────────────────────────────
    // Returns { columnDef, isEdit, isDelete, groupPiece, aggregateField }
    function buildColumn(col, ctx, index) {
        var out = {
            field: col.fieldName,
            hideOnGroup: false,
            headerAttributes: {
                class: "k-text-center !k-justify-content-center",
            },
        };
        if (col.editable === false) {
            out.editable = function () {
                return false;
            };
        }
        if (col.textAlign)
            out.attributes = { style: "text-align: " + col.textAlign };
        if (col.totalLabel) {
            out.footerTemplate = col.totalLabel;
            out.footerAttributes = { class: "text-right text-danger" };
        }

        var isEdit = false,
            isDelete = false;
        var primaryExpr = "#=" + col.fieldName + "#";

        if (col.isModifyField && ctx.allowEdit && !col.hideEdit) isEdit = true;
        if (col.isModifyField && ctx.allowDelete && !col.hideEdit)
            isDelete = true;

        // ── template resolution (priority mirrors the original C# if/else chain) ──
        if (col.formatDateTime) {
            out.type = "date";
            out.format = col.formatDateTime.toUpperCase();
            out.template =
                "#=data." +
                col.fieldName +
                " ? moment(new Date(data." +
                col.fieldName +
                ")).format('" +
                col.formatDateTime.toUpperCase() +
                "'): ''#";
        } else if (col.format) {
            out.type = "number";
            out.format = "{0:n}";
            var fmt =
                col.format === "n0" ? "n0" : col.format === "c" ? "c" : "n2";
            out.template = col.onClickFunction
                ? '<a href="javascript:void(0);" onclick="' +
                  col.onClickFunction +
                  '">#= kendo.toString(data.' +
                  col.fieldName +
                  ", '" +
                  fmt +
                  "') #</a>"
                : "#= kendo.toString(data." +
                  col.fieldName +
                  ", '" +
                  fmt +
                  "') #";
            if (col.isTotalFooter) {
                out.footerTemplate = "#: kendo.toString(sum,'" + fmt + "') #";
                out.footerAttributes = { class: "text-right text-danger" };
            }
        } else if (col.decodeEntities) {
            out.template = "#= decodeEntities(data." + col.fieldName + ") #";
        } else if (col.avatarKendoGrid) {
            out.template =
                '<div class="avatar' +
                index +
                '"></div><div>#= data.' +
                col.avatarKendoGrid +
                " #</div>";
        } else if (col.displayBadge) {
            out.template = function (dataItem) {
                var rawValue = dataItem[col.fieldName];
                var safeValue = esc(rawValue);
                var cssClass =
                    (col.badgeClassMap &&
                        col.badgeClassMap[safeValue.toLowerCase().trim()]) ||
                    col.badgeClassDefault ||
                    "badge-secondary";
                var label = col.badgePrefixField
                    ? kendo.htmlEncode(esc(dataItem[col.badgePrefixField])) +
                      " - "
                    : "";
                return (
                    label +
                    '<span class="badge ' +
                    cssClass +
                    '">' +
                    kendo.htmlEncode(safeValue) +
                    "</span>"
                );
            };
        } else if (isEdit || isDelete) {
            if (
                col.showActionNameOnConditionField &&
                col.showActionNameOnConditionValue
            ) {
                out.template = conditionalActionTemplate(col, ctx);
            } else if (col.isGroupByField) {
                out.groupHeaderTemplate =
                    col.header +
                    ": <button class='btn-group'>" +
                    "<a name='Edit_" +
                    ctx.rdn +
                    "' class='btn btn-primary btn-sm text-white' href='javascript:void(0);' " +
                    'onclick="EditGrid_' +
                    ctx.rdn +
                    "(this, '#= value #');\" title='Edit'><i class='ti ti-pencil'></i></a>" +
                    "<a name='Delete' class='btn btn-danger btn-sm text-white' href='javascript:void(0);' " +
                    'onclick="DeleteTR_' +
                    ctx.rdn +
                    "('#= value #', this, '" +
                    col.fieldName +
                    "', '" +
                    ctx.tableName +
                    "', '" +
                    esc(col.deleteUseProcedureName) +
                    "', '" +
                    esc(col.deleteWithTableNameEncrypted) +
                    "', '" +
                    esc(col.deletePrimaryTblName) +
                    "', '" +
                    esc(col.deletePrimaryFiledName) +
                    "', '" +
                    esc(col.deletePrimaryValue) +
                    "', '" +
                    esc(col.deletePrimaryTblNameDependOnTblName) +
                    "');\" " +
                    "title='Delete'><i class='ti ti-trash'></i></a></button>";
            } else {
                var strEdit = isEdit
                    ? buildEditButton(col, ctx, primaryExpr)
                    : "";
                var strDelete = isDelete
                    ? buildDeleteButton(col, ctx, "'" + primaryExpr + "'")
                    : "";
                out.template =
                    '<button class="btn-group">' +
                    strEdit +
                    strDelete +
                    "</button>";
            }
        } else if (col.link) {
            out.template =
                '<a href="\\\\' +
                col.link +
                "#=" +
                col.linkParameterField +
                '#" target="_blank">#=' +
                col.fieldName +
                "#</a>";
        } else if (col.imagePath) {
            out.template = function (dataItem) {
                var fileNames = (dataItem[col.fieldName] || "")
                    .split(/[,~]/)
                    .map(function (f) {
                        return f.trim();
                    })
                    .filter(Boolean);
                var html = "";
                fileNames.forEach(function (fileName, i) {
                    var ext = fileName.split(".").pop().toLowerCase();
                    var fileUrl = ctx.serverRoot + col.imagePath + fileName;
                    var titleOverlay = col.dataTitleOverlay
                        .split(",")
                        .map(function (f) {
                            var fieldName = f.replace(/#=|#/g, "").trim();
                            return dataItem[fieldName] || "";
                        })
                        .join(" - ");
                    if (
                        ["jpg", "jpeg", "png", "gif", "bmp", "webp"].indexOf(
                            ext,
                        ) !== -1
                    ) {
                        html +=
                            '<a href="' +
                            fileUrl +
                            '" data-toggle="lightbox" data-gallery="example-gallery" data-footer="' +
                            titleOverlay +
                            '">' +
                            '<img src="' +
                            fileUrl +
                            '" loading="lazy" decoding="async" style="height:40px;max-width:60px;object-fit:cover" ' +
                            "onerror='this.onerror=null;src=\"" +
                            ctx.serverRoot +
                            "Content/images/placeholder.jpg\";'/></a>";
                    }
                });
                return html;
            };
        } else if (col.onClickFunction) {
            out.template =
                '<a href="javascript:void(0);" onclick="' +
                col.onClickFunction +
                '">#= kendo.toString(' +
                col.fieldName +
                ", 'n0') #</a>";
        } else if (col.displayCheckBox) {
            out.template =
                "<input type='checkbox' name='" +
                col.fieldName +
                "' value=\"#=" +
                col.fieldName +
                "#\" class='rowCheckbox' />";
        } else if (col.displayBarcode) {
            // Barcode markup is generated server-side (GenerateBarcode) since it's a
            // C# rendering helper; the resolved template (with #=field# placeholder
            // intact) is sent as rawBarcodeTemplate.
            out.template = col.rawBarcodeTemplate || "";
        } else if (col.displayTextBox) {
            out.template =
                "<input onfocus='$(this).select();' type='text' class='form-control text-right' name='" +
                col.fieldName +
                "' value='#=" +
                col.fieldName +
                "' />";
        }

        if (col.className)
            out.attributes = Object.assign({}, out.attributes, {
                class: col.className,
            });

        var autoFitField = null;
        if (
            !col.width &&
            ctx.autoFitColumn &&
            !col.displayCheckBox &&
            !col.isHiddenField
        ) {
            autoFitField = col.fieldName;
        } else if (col.width && col.width.toLowerCase() !== "auto") {
            out.width = col.width;
        } else if (col.displayCheckBox) {
            out.width = "30px";
            out.attributes = { style: "text-align: center;" };
            out.headerAttributes = { style: "text-align: center;" };
        }

        out.hidden = col.isGroupByField ? true : !!col.isHiddenField;

        if (!col.displayCheckBox) {
            out.title = col.header;
            out.encoded = false;
        } else {
            out.title = "<input type='checkbox' id='headerCheckbox' />";
            out.encoded = false;
            out.sortable = false;
            out.filterable = false;
        }

        if (col.sumGroupByFieldName) {
            out.aggregates = ["sum"];
            out.groupHeaderTemplate = function (e) {
                var g = e[col.sumGroupByFieldName];
                return g.group.value + ": " + g.sum;
            };
        }

        // Filters are on for every column by default now (the filter *row* itself
        // is hidden/shown globally via the toolbar toggle button — see
        // addFilterToggleButton). A column only loses its filter cell if:
        //   - the config explicitly opts out (col.filterable === false), or
        //   - it's a column type where filtering never made sense (checkbox,
        //     action buttons, images, avatars).
        var noFilterColumn =
            col.filterable === false ||
            col.displayCheckBox ||
            isEdit ||
            isDelete ||
            col.imagePath ||
            col.avatarKendoGrid;

        if (noFilterColumn) {
            out.filterable = false;
        } else {
            var filterCell = {
                showOperators: false,
                suggestionOperator: "contains",
                operator: "contains",
            };
            if (col.filterableDate) {
                filterCell.template = betweenFilterTemplate(
                    col.fieldName,
                    ctx.tblId,
                );
            } else if (!col.kendoGridAutoComplect) {
                filterCell.template = function (args) {
                    args.element.kendoAutoComplete({
                        serverFiltering: false,
                        valuePrimitive: true,
                        noDataTemplate: "",
                    });
                };
            }
            out.filterable = { cell: filterCell };
        }

        // ── Pinned column (custom sticky pin, not Kendo's native lock) ──
        // col.pinned: true seeds this column into the pinned list so it
        // starts out pinned when the grid loads. See applyPinnedColumns —
        // this only records the intent here; the actual sticky styling is
        // applied after render (and re-applied on every subsequent render,
        // resize, and reorder).
        if (col.pinned && !isFieldPinned(ctx.tblId, col.fieldName)) {
            getPinnedFields(ctx.tblId).push(col.fieldName);
        }
        if (col.pinnedWidth && !out.width) out.width = col.pinnedWidth;

        var groupPiece = null;
        if (col.isGroupByField) {
            groupPiece = { field: col.fieldName, dir: col.orderBy || "desc" };
            if (col.sumGroupByFieldName) {
                groupPiece.aggregates = [
                    { field: col.sumGroupByFieldName, aggregate: "sum" },
                ];
            }

            if (col.displayBadge) {
                out.groupHeaderTemplate = function (e) {
                    // e.items = raw data rows belonging to this group (Kendo provides this
                    // on the group descriptor when the template is a function)
                    var badgeValue =
                        col.badgeSourceField && e.items && e.items.length
                            ? e.items[0][col.badgeSourceField]
                            : e.value;

                    var cssClass =
                        (col.badgeClassMap &&
                            col.badgeClassMap[
                                String(badgeValue).toLowerCase()
                            ]) ||
                        col.badgeClassDefault ||
                        "badge-primary";

                    return (
                        esc(col.header) +
                        ": " +
                        kendo.htmlEncode(esc(e.value)) +
                        " &nbsp;<span class='badge " +
                        cssClass +
                        "'>" +
                        kendo.htmlEncode(esc(badgeValue)) +
                        "</span>"
                    );
                };
            }
        }

        return {
            columnDef: out,
            isEdit: isEdit,
            isDelete: isDelete,
            groupPiece: groupPiece,
            aggregateField: col.isTotalFooter ? col.fieldName : null,
            autoFitField: autoFitField,
            avatarField: col.avatarKendoGrid || null,
        };
    }

    function buildColumns(columns, ctx) {
        var result = {
            columnDefs: [],
            group: [],
            aggregates: [],
            autoFitFields: [],
            avatarFields: [],
            anyEdit: false,
            anyDelete: false,
        };
        (columns || []).forEach(function (col, i) {
            var built = buildColumn(col, ctx, i);
            result.columnDefs.push(built.columnDef);
            if (built.groupPiece) result.group.push(built.groupPiece);
            if (built.aggregateField)
                result.aggregates.push({
                    field: built.aggregateField,
                    aggregate: "sum",
                });
            if (built.autoFitField)
                result.autoFitFields.push(built.autoFitField);
            if (built.avatarField)
                result.avatarFields.push({
                    index: i,
                    field: built.avatarField,
                });
            if (built.isEdit) result.anyEdit = true;
            if (built.isDelete) result.anyDelete = true;

            if (col.editable === true && !result.firstEditableField) {
                result.firstEditableField = col.fieldName;
            }
        });
        return result;
    }

    // ── Edit / Delete functions (generic — bound per grid via closures) ────

    function makeEditFn(ctx) {
        return function (THIS, data) {
            $(THIS).select();
            ShowDialogBox(
                "Edit",
                ctx.dialogBoxWidth,
                ctx.dialogBoxHeight,
                ctx.editUrl,
                data,
                "",
                "",
                "",
                "",
                function () {
                    if (ctx.onDialogCloseCallbackFunction) {
                        $("#modal-default").on("hide.bs.modal", function () {
                            // eslint-disable-next-line no-new-func
                            new Function(
                                "SaveChangeResponse",
                                ctx.onDialogCloseCallbackFunction,
                            )();
                        });
                    }
                },
            );
        };
    }

    function makeDeleteFn(ctx) {
        return function (
            dataId,
            THIS,
            fieldName,
            tableName,
            deleteProc,
            deleteWithTable,
            primTbl,
            primField,
            primValue,
            primTblDep,
        ) {
            fieldName = fieldName || ctx.deleteDefaultFieldNameEncrypted;
            Swal.fire({
                position: "top",
                title: "Are you sure?",
                text: "You won't be able to revert this!",
                icon: "question",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#3085d6",
                confirmButtonText: "Yes, delete it!",
                cancelButtonText: "No, cancel!",
            }).then(function (result) {
                if (result.value) {
                    DialogSweetAlert("deleteted");
                    $.ajax({
                        url: ctx.serverRoot + "DoActionData/DeleteData",
                        dataType: "json",
                        async: true,
                        type: "POST",
                        headers: { Authorization: "Bearer " + access_Token },
                        data: {
                            FieldName: fieldName,
                            DataID: dataId,
                            TableName: tableName,
                            DeleteUseProcedureName: deleteProc,
                            DeleteWithTableName: deleteWithTable,
                            DeletePrimaryTblName: primTbl,
                            DeletePrimaryFiledName: primField,
                            DeletePrimaryValue: primValue,
                            DeletePrimaryTblNameDependOnTblName: primTblDep,
                            FormID: ctx.formId,
                            ClearByDates: Date(),
                        },
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader(
                                "x-custom-header",
                                JSON.stringify(headerPara),
                            );
                            $("button").attr("disabled", true);
                        },
                        error: function (response) {
                            $("button").removeAttr("disabled");
                            DialogSweetAlert(
                                "error",
                                response.status + " " + response.statusText,
                            );
                        },
                        success: function (responseText) {
                            // Wipe this grid's cache so any subsequent reload hits fresh data
                            window.invalidateGridCache(ctx.tblId);
                            window.invalidateLoadGridCache(ctx.formId);

                            if (ctx.deletedCallBackFunction) {
                                $("button").removeAttr("disabled");
                                // eslint-disable-next-line no-new-func
                                new Function(
                                    "responetext",
                                    "DataID",
                                    ctx.deletedCallBackFunction,
                                )(responseText, dataId);
                                return;
                            }
                            var keepSearch = $("#CboSearchText").val() || "";
                            $("input[name='RD']:eq(0)").click();
                            if (responseText.Result != 1) {
                                $("#CboSearchText").val(keepSearch);
                                if (typeof SearchBy_Change === "function")
                                    SearchBy_Change();
                                var fn =
                                    ctx.loadGridFunction || "LoadTableGrid";
                                if (typeof window[fn] === "function")
                                    window[fn](CurrentPage);
                            }
                        },
                    });
                } else if (result.dismiss === Swal.DismissReason.cancel) {
                    Swal.fire({
                        position: "top",
                        title: "Cancelled",
                        text: "",
                        icon: "error",
                        showConfirmButton: false,
                        allowOutsideClick: false,
                        timerProgressBar: true,
                        allowEscapeKey: false,
                        heightAuto: false,
                        timer: 700,
                    });
                }
            });
        };
    }

    // ── DataSource builder ──────────────────────────────────────────────

    function buildDataSource(config, built) {
        var ds = { pageSize: 10 };
        var cacheTTL = config.cacheTTL || DEFAULT_TTL_MS;
        var cacheEnabled = config.enableCache !== false; // opt-out via config

        if (config.serverPaging && config.urlApiGetData) {
            ds.transport = {
                read: function (options) {
                    var reqData = options.data || {};
                    var pageSize = reqData.pageSize;
                    var pageSizeToSend =
                        pageSize === "All" || pageSize === undefined
                            ? reqData.take || 999999
                            : pageSize;

                    var payload = {
                        pageSize: pageSizeToSend,
                        page: reqData.page,
                        skip: reqData.skip,
                        take: reqData.take,
                        SP: config.procedureNameEncrypted,
                        FilterParam: config.fieldWithValue,
                        IsRawSql: config.isRawSql,
                        OrderBy: config.orderBy,
                        SystemID: config.systemId,
                        WarehouseID: config.warehouseId,
                        DomainID: config.domainId,
                    };

                    var key = cacheKeyFor(payload);

                    if (cacheEnabled) {
                        var cached = getCached(config.tblId, key, cacheTTL);
                        if (cached) {
                            options.success(cached);
                            return;
                        }
                    }

                    $.ajax({
                        url: config.serverRoot + config.urlApiGetData,
                        type: "POST",
                        cache: false,
                        contentType: "application/json",
                        dataType: "json",
                        data: kendo.stringify(payload),
                        headers: { Authorization: "Bearer " + access_Token },
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader(
                                "x-custom-header",
                                JSON.stringify(headerPara),
                            );
                        },
                        success: function (response) {
                            if (cacheEnabled)
                                setCached(config.tblId, key, response);
                            options.success(response);
                        },
                        error: function (err) {
                            options.error(err);
                        },
                    });
                },
                update: function (options) {
                    $.ajax({
                        url: config.serverRoot + config.urlApiUpdateData,
                        type: "POST",
                        contentType: "application/json",
                        dataType: "json",
                        data: kendo.stringify(options.data),
                        headers: { Authorization: "Bearer " + access_Token },
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader(
                                "x-custom-header",
                                JSON.stringify(headerPara),
                            );
                        },
                        success: function (response) {
                            options.success(response);
                        },
                        error: function (err) {
                            options.error(err);
                        },
                    });
                },
            };
            ds.schema = { data: "data", total: "total" };
            if (config.isEditable) {
                ds.schema.model = {
                    id: config.editableFieldPrimaryId,
                    fields: config.editableFields,
                };
            }
            ds.serverPaging = true;

            // Any create/update/destroy against this grid should invalidate its cache
            // so the next read reflects the change instead of a stale cached page.
            ds.requestEnd = function (e) {
                if (
                    e.type === "update" ||
                    e.type === "destroy" ||
                    e.type === "create"
                ) {
                    window.invalidateGridCache(config.tblId);
                    window.invalidateLoadGridCache(config.formId);
                }
            };
        } else {
            ds.data = JSON.parse(config.data.replace(/(^\"|\"$)/g, "")) || [];
        }

        if (config.isTreeList) {
            ds.schema = Object.assign({}, ds.schema, {
                model: { id: "ID", parentId: "parentId", expanded: true },
            });
        }
        if (built.aggregates.length) ds.aggregate = built.aggregates;
        if (built.group.length) ds.group = built.group;

        return ds;
    }

    // ── Detail grid (master/detail) ─────────────────────────────────────

    function buildDetailInit(config, ctx) {
        var detailBuilt = buildColumns(config.detail.columns, ctx);
        return function (e) {
            var detailRow = e.detailRow;
            detailRow
                .find(".detailTabstrip")
                .kendoTabStrip({ animation: { open: { effects: "fadeIn" } } });

            var gridOptions;

            if (config.detail.procedureName2) {
                gridOptions = {
                    dataSource: {
                        transport: {
                            read: {
                                url:
                                    config.serverRoot +
                                    config.detail.urlApiGetDetail,
                                type: "POST",
                                contentType: "application/json",
                                dataType: "json",
                                data: function () {
                                    return {
                                        SP: config.detail
                                            .procedureName2Encrypted,
                                        LinkFieldValue:
                                            e.data[config.detail.linkField],
                                    };
                                },
                            },
                            parameterMap: function (data) {
                                return kendo.stringify(data);
                            },
                        },
                        schema: { data: "data", total: "total" },
                        pageSize: 10,
                        serverPaging: true,
                        aggregate: detailBuilt.aggregates,
                    },
                    sortable: true,
                    filterable: true,
                    groupable: false,
                    resizable: true,
                    selectable: "single",
                    pageable: {
                        alwaysVisible: true,
                        pageSizes: [10, 20, 30, 50, 100, "All"],
                    },
                    noRecords: {
                        template: function () {
                            return "<div style='margin-left:10px;padding:4px 0px;'>No data available.</div>";
                        },
                    },
                    columns: detailBuilt.columnDefs,
                    dataBound: function () {
                        var grid = this;
                        setTimeout(function () {
                            detailBuilt.columnDefs.forEach(function (c) {
                                if (c.field) grid.autoFitColumn(c.field);
                            });
                        }, 10);
                    },
                };
            } else {
                gridOptions = {
                    dataSource: {
                        data: config.detail.data || [],
                        pageSize: 10,
                        filter: {
                            field: config.detail.linkField,
                            operator: "eq",
                            value: e.data[config.detail.linkField],
                        },
                    },
                    sortable: true,
                    groupable: false,
                    resizable: true,
                    selectable: "single",
                    pageable: {
                        alwaysVisible: true,
                        pageSizes: [10, 20, 30, 50, 100, "All"],
                    },
                    noRecords: {
                        template: function () {
                            return "<div style='margin-left:10px;padding:4px 0px;'>No data available.</div>";
                        },
                    },
                    columns: detailBuilt.columnDefs,
                    dataBound: function () {
                        var grid = this;
                        setTimeout(function () {
                            detailBuilt.columnDefs.forEach(function (c) {
                                if (c.field) grid.autoFitColumn(c.field);
                            });
                        }, 10);
                    },
                };
            }
            detailRow.find(".orders").kendoGrid(gridOptions);
        };
    }

    // ── Filter row show/hide toggle ─────────────────────────────────────
    // Filter cells are always built into the grid now, but the row that
    // holds them is hidden by default (via CSS) until the user clicks the
    // toggle button we inject into the grid's toolbar.
    var FILTER_TOGGLE_CSS_ID = "kendo-filter-toggle-style";

    function ensureFilterToggleStyles() {
        if ($("#" + FILTER_TOGGLE_CSS_ID).length) return;
        $(
            "<style id='" +
                FILTER_TOGGLE_CSS_ID +
                "'>" +
                ".k-grid.filters-hidden .k-filter-row{display:none !important;}" +
                ".k-filter-toggle-btn.active{background-color:#e9ecef;}" +
                "</style>",
        ).appendTo("head");
    }

    function addFilterToggleButton(tblId) {
        ensureFilterToggleStyles();

        var $grid = $("#" + tblId);
        // Hidden by default.
        $grid.addClass("filters-hidden");

        var $toolbar = $grid.find(".k-grid-toolbar");
        if (!$toolbar.length) {
            // Grid has no built-in toolbar (showToolbar was falsy) — add a
            // minimal one just to host the button.
            $toolbar = $(
                '<div class="k-grid-toolbar custom-filter-toolbar"></div>',
            );
            $grid.prepend($toolbar);
        }

        // Avoid stacking duplicate buttons if this grid gets re-initialized.
        $toolbar.find(".k-filter-toggle-btn").remove();

        var $btn = $(
            '<button type="button" class="btn btn-outline-secondary btn-sm k-filter-toggle-btn" ' +
                'data-toggle="tooltip" data-placement="top" title="Show/Hide Filters"><i class="ti ti-filter"></i></button>',
        );
        $toolbar.append($btn);

        $btn.on("click", function () {
            $grid.toggleClass("filters-hidden");
            $btn.toggleClass("active", !$grid.hasClass("filters-hidden"));
        });
    }

    // ── Main entry point ────────────────────────────────────────────────

    window.initKendoGrid = function (config) {
        // Fresh grid instance for this tblId — any pins from a previous
        // grid that reused this same element id must not carry over.
        _pinnedFields[config.tblId] = [];

        var ctx = {
            rdn: config.rdn,
            tblId: config.tblId,
            allowEdit: config.allowEdit,
            allowDelete: config.allowDelete,
            tableName: config.tableName,
            serverRoot: config.serverRoot,
            autoFitColumn: config.autoFitColumn,
        };

        var built = buildColumns(config.columns, ctx);
        ensurePinStyles();

        var options = {
            selectable: "single",
            resizable: true,
            reorderable: true,
            noRecords: {
                template: function () {
                    return "<div style='margin-left: 10px;padding: 4px 0px;'>No data available.</div>";
                },
            },
            loaderType: "skeleton",
            columns: [],
            dataSource: buildDataSource(config, built),
        };

        if (config.showCheckbox) {
            options.columns.push({
                selectable: true,
                width: 20,
                attributes: { style: "text-align: center;" },
                headerAttributes: { style: "text-align: center;" },
            });
        }
        options.columns = options.columns.concat(built.columnDefs);

        if (config.isEditable) {
            var commands = ["edit"];
            if (config.showBellButton) {
                commands.push({
                    name: "alertBtn",
                    text: '<i class="ti ti-bell-ringing"></i>',
                    click: function (e) {
                        e.preventDefault();
                        var tr = $(e.currentTarget).closest("tr");
                        var dataItem = this.dataItem(tr);
                        // eslint-disable-next-line no-new-func
                        new Function("dataItem", config.onClickBellButton)(
                            dataItem,
                        );
                    },
                });
            }
            options.columns.push({
                command: commands,
                title: "&nbsp;",
                width: "180px",
            });
            options.editable = "inline";
            options.edit = function (e) {
                if (!built.firstEditableField) return;
                var $input = e.container.find(
                    "input[name='" + built.firstEditableField + "']",
                );
                if ($input.length) {
                    // small timeout so Kendo finishes building the edit row first
                    setTimeout(function () {
                        $input.focus().select();
                    }, 0);
                }
            };
        }

        if (config.groupable) options.groupable = true;
        if (config.pageable)
            options.pageable = {
                alwaysVisible: true,
                pageSizes: [10, 20, 30, 50, 100, "All"],
            };
        else options.pageable = false;
        if (config.height) options.height = config.height;
        // Column menu (Filter / Pin / column reordering) is on for every
        // grid by default now — pass columnMenu: false in the config to
        // opt a specific grid out, instead of having to opt every grid in.
        // "Pin Column"/"Unpin Column" is our own custom item (see
        // makeColumnMenuInit) — it works on every column from a cold
        // start, with nothing pinned until the user asks for it.
        if (config.columnMenu !== false) {
            options.columnMenu = { filterable: true };
            options.columnMenuInit = makeColumnMenuInit(config.tblId);
        }
        // Row-mode filtering is now always enabled (per-column filter cells);
        // config.filterable === false is the only way to opt a whole grid out.
        options.filterable = { mode: "row" };
        options.sortable = { mode: "single", allowUnsort: false };
        // Recompute pinned-column positions whenever column widths or
        // order change, so pins stay aligned.
        options.columnResize = function () {
            applyPinnedColumns(config.tblId);
        };
        options.columnReorder = function () {
            setTimeout(function () {
                applyPinnedColumns(config.tblId);
            }, 0);
        };
        // Selecting a row (blue highlight) doesn't trigger dataBound, so
        // the pinned cell's background would otherwise stay stale until
        // the next full re-render — recompute on selection too.
        options.change = function () {
            applyPinnedColumns(config.tblId);
        };

        if (config.detail) {
            options.detailTemplate = kendo.template($("#template").html());
            options.detailInit = buildDetailInit(config, ctx);
            // options.dataBound = function () {
            //     this.expandRow(this.tbody.find("tr.k-master-row").first());
            // };
        }

        if (config.showToolbar) {
            options.toolbar = ["excel", "pdf", "search"];
            options.excel = { allPages: true };
            options.messages = { commands: { pdf: "Print PDF" } };
            options.pdf = {
                autoPrint: false,
                allPages: true,
                repeatHeaders: true,
                title: config.reportTitle,
                fileName:
                    (config.reportTitle
                        ? config.reportTitle.replace(/\s/g, "")
                        : "PrintGrid") + ".pdf",
                font: "",
            };
            options.pdfExport = function (e) {
                kendoGridPdfExport(e, {
                    reportTitle: config.reportTitle,
                    headerReportNameExcel: config.headerReportNameExcel,
                    serverRoot: config.serverRoot,
                });
            };
            options.excelExport = function (e) {
                kendoGridExcelExport(e, {
                    manualHeader: config.manualHeaderExcel,
                    headerReportNameExcel: config.headerReportNameExcel,
                    showPrintDate: config.showPrintDate,
                    showFilterDateRange: config.showFilterDateRange,
                });
            };
        }

        // dataBound: format-unit-variant async fetch, avatar init, autofit, sticky header
        options.dataBound = (function (original) {
            return function (e) {
                if (original) original.call(this, e);

                (config.columns || []).forEach(function (col) {
                    if (col.isShowFormatUnitVariant && col.primaryId) {
                        var columnIndex = this.wrapper
                            .find(
                                ".k-grid-header [data-field=" +
                                    col.fieldName +
                                    "]",
                            )
                            .index();
                        var rows = e.sender.tbody.children();
                        for (var j = 0; j < rows.length; j++) {
                            var row = $(rows[j]);
                            var dataItem = e.sender.dataItem(row);
                            var qty = dataItem.get(col.fieldName);
                            var itemId = dataItem.get(col.primaryId);
                            var cell = row.children().eq(columnIndex);
                            fetch(
                                config.serverRoot +
                                    "Items/GetConvertUnit?ItemID=" +
                                    itemId +
                                    "&Qty=" +
                                    (qty || 0),
                            )
                                .then(function (r) {
                                    return r.text();
                                })
                                .then(function (result) {
                                    cell.html(result);
                                })
                                .catch(function (err) {
                                    console.error("Error:", err);
                                });
                        }
                    }
                }, this);

                if (built.avatarFields.length) {
                    var grid = e.sender;
                    grid.items().each(function () {
                        var dataItem = grid.dataItem(this);
                        var $row = $(this);
                        built.avatarFields.forEach(function (a) {
                            var $el = $row.find(".avatar" + a.index);
                            $el.kendoAvatar({
                                type: "image",
                                size: "large",
                                image: dataItem[a.field],
                            });
                        });
                    });
                }

                if (config.gridEventClick) {
                    if (config.hasKendoGridSetting) {
                        $("#" + config.tblId + " tbody tr td")
                            .off("click")
                            .click(function () {
                                // eslint-disable-next-line no-new-func
                                new Function(config.gridEventClick)();
                            });
                    } else {
                        $("#" + config.tblId + " tbody")
                            .find("tr")
                            .off("click")
                            .click(function () {
                                // eslint-disable-next-line no-new-func
                                new Function(config.gridEventClick)();
                            });
                    }
                }

                $('[title="Clear"]').remove();

                if (built.autoFitFields.length) {
                    var g = $("#" + config.tblId).data(
                        config.isTreeList ? "kendoTreeList" : "kendoGrid",
                    );
                    setTimeout(function () {
                        built.autoFitFields.forEach(function (f) {
                            g.autoFitColumn(f);
                        });
                    }, 10);
                }

                fixLightboxImages();

                // Every dataBound re-renders the body from scratch (paging,
                // sorting, filtering, refresh...), which wipes any sticky
                // styling we'd applied — reapply it so pinned columns stay
                // pinned.
                applyPinnedColumns(config.tblId);
            };
        })(options.dataBound);

        if (config.autoFitColumn) {
            options.columnShow = function (e) {
                var g = $("#" + config.tblId).data("kendoGrid");
                setTimeout(function () {
                    g.autoFitColumn(e.column.field);
                }, 10);
            };
            options.columnHide = function () {
                var g = $("#" + config.tblId).data("kendoGrid");
                setTimeout(function () {
                    g.columns.forEach(function (col) {
                        if (!col.hidden && col.field)
                            g.autoFitColumn(col.field);
                    });
                }, 10);
            };
        }

        // Whether or not autoFitColumn is on, showing/hiding a column
        // shifts every pinned column's offset — recompute after either.
        options.columnShow = (function (original) {
            return function (e) {
                if (original) original.call(this, e);
                applyPinnedColumns(config.tblId);
            };
        })(options.columnShow);
        options.columnHide = (function (original) {
            return function (e) {
                if (original) original.call(this, e);
                applyPinnedColumns(config.tblId);
            };
        })(options.columnHide);

        kendo.pdf.defineFont({
            "Arial Unicode MS":
                config.serverRoot +
                "/Scripts/kendo/css/fonts/arialunicodems.ttf",
        });

        var $grid = $("#" + config.tblId)[
            config.isTreeList ? "kendoTreeList" : "kendoGrid"
        ](options);
        $("#" + config.tblId).addClass("khmerFont1");

        // Sticky header (unconditional in the original code)
        $(".content-wrapper #" + config.tblId + " > div.k-grid-header").css({
            position: "sticky",
            top: "66px",
            "z-index": 999,
        });
        var header = $(
            ".content-wrapper #" + config.tblId + " > div.k-grid-header",
        );
        $(window).on("scroll", function () {
            if (!header[0]) return;
            var rect = header[0].getBoundingClientRect();
            header.toggleClass("sticky-bg", rect.top <= 66);
        });

        // Filter row is on for every column but hidden until toggled.
        addFilterToggleButton(config.tblId);

        // Bind generic edit/delete functions scoped to this grid's config
        if (built.anyEdit) window["EditGrid_" + ctx.rdn] = makeEditFn(config);
        if (built.anyDelete)
            window["DeleteTR_" + ctx.rdn] = makeDeleteFn(config);

        if (config.gridEventClick) {
            $("#" + config.tblId + " tbody tr:hover").css("cursor", "pointer");
        }

        return $grid;
    };
})();

(function () {
    // Kendo icon class (either system) -> Tabler webfont class
    var ICON_MAP = {
        "k-i-filter": "ti-filter",
        "k-svg-i-filter": "ti-filter",
        "k-i-sort-asc-sm": "ti-sort-ascending",
        "k-i-sort-asc": "ti-sort-ascending",
        "k-svg-i-sort-asc-sm": "ti-sort-ascending",
        "k-i-sort-desc-sm": "ti-sort-descending",
        "k-i-sort-desc": "ti-sort-descending",
        "k-svg-i-sort-desc-sm": "ti-sort-descending",
        "k-i-x": "ti-x",
        "k-svg-i-x": "ti-x",
        "k-i-cancel": "ti-x",
        "k-i-search": "ti-search",
        "k-svg-i-search": "ti-search",
        "k-i-excel": "ti-file-spreadsheet",
        "k-i-file-excel": "ti-file-type-xls",
        "k-svg-i-file-excel": "ti-file-spreadsheet",
        "k-i-pdf": "ti-file-type-pdf",
        "k-i-file-pdf": "ti-file-type-pdf",
        "k-svg-i-file-pdf": "ti-file-type-pdf",
        "k-i-arrow-chevron-left": "ti-chevron-left",
        "k-svg-i-arrow-chevron-left": "ti-chevron-left",
        "k-i-arrow-chevron-right": "ti-chevron-right",
        "k-svg-i-arrow-chevron-right": "ti-chevron-right",
        "k-i-caret-alt-down": "ti-chevron-down",
        "k-svg-i-caret-alt-down": "ti-chevron-down",
        "k-i-caret-alt-left": "ti-chevron-left",
        "k-svg-i-caret-alt-left": "ti-chevron-left",
        "k-i-caret-alt-right": "ti-chevron-right",
        "k-svg-i-caret-alt-right": "ti-chevron-right",
        "k-i-columns": "ti-columns",
        "k-svg-i-columns": "ti-columns",
        "k-i-more-vertical": "ti-dots-vertical",
        "k-svg-i-more-vertical": "ti-dots-vertical",
        "k-i-checkmark": "ti-check",
        "k-svg-i-checkmark": "ti-check",
        "k-i-check": "ti-check",
        "k-i-plus": "ti-plus",
        "k-svg-i-plus": "ti-plus",
        "k-i-minus": "ti-minus",
        "k-svg-i-minus": "ti-minus",
        "k-i-calendar": "ti-calendar",
        "k-svg-i-calendar": "ti-calendar",
        "k-i-seek-w": "ti-chevrons-left",
        "k-svg-i-seek-w": "ti-chevrons-left",
        "k-i-seek-e": "ti-chevrons-right",
        "k-svg-i-seek-e": "ti-chevrons-right",
        "k-i-refresh": "ti-refresh",
        "k-svg-i-refresh": "ti-refresh",
        "k-i-collapse": "ti-chevron-down",
        "k-svg-i-collapse": "ti-chevron-down",
        "k-i-expand": "ti-chevron-right",
        "k-svg-i-expand": "ti-chevron-right",
        "k-i-arrow-end-left": "ti-chevrons-left",
        "k-svg-i-arrow-end-left": "ti-chevrons-left",
        "k-i-arrow-60-left": "ti-chevron-left",
        "k-svg-i-arrow-60-left": "ti-chevron-left",
        "k-i-arrow-60-right": "ti-chevron-right",
        "k-svg-i-arrow-60-right": "ti-chevron-right",
        "k-i-arrow-end-right": "ti-chevrons-right",
        "k-svg-i-arrow-end-right": "ti-chevrons-right",
        "k-i-arrow-n": "ti-chevron-up",
        "k-i-arrow-s": "ti-chevron-down",
        "k-svg-i-arrow-s": "ti-chevron-down",
        "k-i-edit": "ti-pencil",
        "k-i-close": "ti-x",
    };

    var SELECTOR = Object.keys(ICON_MAP)
        .map(function (c) {
            return "." + c;
        })
        .join(",");

    function swap(el) {
        var matchClass = Object.keys(ICON_MAP).find(function (c) {
            return el.classList.contains(c);
        });
        if (!matchClass) return;
        var wantedIcon = "ti " + ICON_MAP[matchClass];
        var current = el.querySelector("i.ti");
        if (current && current.className === wantedIcon) return; // already correct
        el.innerHTML = '<i class="' + wantedIcon + '"></i>';
    }

    function swapAll(root) {
        root.querySelectorAll(SELECTOR).forEach(swap);
    }

    swapAll(document);

    var observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (m) {
            if (m.type === "childList") {
                m.addedNodes.forEach(function (node) {
                    if (node.nodeType !== 1) return;
                    if (node.matches && node.matches(SELECTOR)) swap(node);
                    swapAll(node);
                });
            } else if (m.type === "attributes") {
                swap(m.target);
            }
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ["class"],
    });
})();
