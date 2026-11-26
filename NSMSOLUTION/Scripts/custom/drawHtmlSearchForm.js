(function (window, $) {
    "use strict";

    var _loadGridCache = {};
    var LOAD_GRID_TTL_MS = 30000; // tune per endpoint volatility

    function _cacheKey(data) {
        return JSON.stringify(data);
    }

    function _getCachedGrid(formId, key, ttl) {
        var bucket = _loadGridCache[formId];
        if (!bucket) return null;
        var entry = bucket[key];
        if (!entry) return null;
        if (Date.now() - entry.time > ttl) {
            delete bucket[key];
            return null;
        }
        return entry.response;
    }

    function _setCachedGrid(formId, key, response) {
        if (!_loadGridCache[formId]) _loadGridCache[formId] = {};
        _loadGridCache[formId][key] = { time: Date.now(), response: response };
    }

    // Call after any add/edit/delete affecting this form's grid
    window.invalidateLoadGridCache = function (formId) {
        if (!formId) {
            _loadGridCache = {};
            return;
        }
        delete _loadGridCache[formId];
    };

    var TableGridWidget = {
        // formId -> config
        registry: {},

        init: function (config) {
            this.registry[config.formId] = config;
            this._registerLegacyGlobal(config);
            this._wireEvents(config);

            if (config.hasRadioSearch) {
                this._searchByChange(config.formId);
            }
            this.loadGrid(config.formId, 1);
        },

        // ---- event wiring -------------------------------------------------

        _wireEvents: function (config) {
            var self = this;

            if (!config.showGridOnly && config.showSearchButton !== false) {
                $("#" + config.searchButtonId).on("click", function () {
                    $(this).blur();
                    self._runSearch(config.formId);
                });
            }

            if (config.searchInputId) {
                $("#" + config.searchInputId).on("keypress", function (e) {
                    if (e.which === 13) {
                        self._runSearch(config.formId);
                        e.preventDefault();
                    }
                });
            }

            if (config.hasRadioSearch) {
                $('input[name="' + config.radioName + '"]').on(
                    "click",
                    function () {
                        self._searchByChange(config.formId);
                    },
                );
            }

            if (config.addButtonId) {
                $("#Bun" + config.addButtonId).on("click", function () {
                    if (!config.allowAdd) {
                        DialogSweetAlert(
                            "nopermission",
                            "Sorry, you don't have permission for accessing this page. Please contact administrator.",
                        );
                        return;
                    }
                    if (!config.addUrl) {
                        if (
                            typeof config.addFunctionName === "string" &&
                            window[config.addFunctionName]
                        ) {
                            window[config.addFunctionName]();
                        }
                        return;
                    }
                    ShowDialogBox(
                        config.addButtonLabel,
                        config.dialogBoxWidth,
                        config.dialogBoxHeight,
                        config.serverRoot + config.addUrl,
                        "0",
                        "",
                        "",
                        config.modalWidth,
                        config.modalHeight,
                        function (SaveChangeResponse) {
                            $("#modal-default").one(
                                "hide.bs.modal",
                                function () {
                                    window.invalidateLoadGridCache(
                                        config.formId,
                                    );
                                    if (config.onDialogCloseCallbackFunction) {
                                        window[
                                            config.onDialogCloseCallbackFunction
                                        ](SaveChangeResponse);
                                    }
                                },
                            );
                        },
                    );
                });
            }

            if (config.drawPrintButton || config.showPrintOnTopTable) {
                $("#PrintButton").on("click", function () {
                    self.showPrintDialog(config.formId);
                    $(this).blur();
                });
            }

            if (config.drawSendEmailButton) {
                $("#SendEmailButton").on("click", function () {
                    self.sendEmail(config.formId);
                    $(this).blur();
                });
            }

            if (config.showSearchBox && config.rnd != null) {
                $("#inputSearchTextBox_" + config.rnd).on("keyup", function () {
                    config.keySearchBoxData = $(this).val();
                    self._runSearch(config.formId);
                    $("#inputSearchTextBox_" + config.rnd)
                        .val(config.keySearchBoxData)
                        .focus();
                });
            }
        },

        _runSearch: function (formId) {
            var config = this.registry[formId];
            window.invalidateLoadGridCache(formId);
            if (config.overrideSearch) {
                // legacy escape hatch: arbitrary JS the caller supplied
                // eslint-disable-next-line no-new-func
                new Function(config.overrideSearch).call(window);
            } else {
                this.loadGrid(formId, 1);
            }
        },

        // Keeps old markup like onclick="LoadTableGrid(1)" working.
        _registerLegacyGlobal: function (config) {
            if (!config.loadGridFunctionName) return;
            var self = this;
            window[config.loadGridFunctionName] = function (
                page,
                status,
                title,
            ) {
                self.loadGrid(config.formId, page, status, title);
            };
        },

        // ---- radio-driven search field (was SearchBy_Change<rnd>) --------

        _searchByChange: function (formId) {
            var config = this.registry[formId];
            var checked = $(
                'input[name="' + config.radioName + '"]:checked',
            ).attr("alt");
            var opt = null;
            for (var i = 0; i < config.radioOptions.length; i++) {
                if (config.radioOptions[i].optLabel === checked) {
                    opt = config.radioOptions[i];
                    break;
                }
            }
            if (!opt) return;

            var fieldId = opt.resolvedFieldId || "CboSearchText";

            $("#LblSearchText" + config.randomNumber).html(opt.optLabel + ":");

            if (!opt.isAutoComplete) {
                LoadDataToComboBox(
                    "DivShowSearchText" + config.randomNumber,
                    fieldId,
                    opt.sql,
                    opt.selectValue,
                    opt.displayField,
                    config.overrideSearch ||
                        config.loadGridFunctionName + "(1);",
                    opt.showAll,
                    opt.value,
                    true,
                    false,
                    false,
                    opt.isUseConnection2,
                    opt.conditionFieldValue,
                    null,
                    null,
                    null,
                    null,
                    String(!!opt.isAutoCbo),
                    String(!!opt.multiple),
                );
                $("#" + fieldId).addClass(config.cboSearchStyle || "");
                $("#" + fieldId).attr("data-style", config.cboDataStyle || "");
            } else {
                $("#DivShowSearchText" + config.randomNumber).html(
                    '<input type="text" name="' +
                        fieldId +
                        '" id="' +
                        fieldId +
                        '" class="form-control" placeholder="' +
                        (opt.placeholder || "") +
                        '"/>',
                );
                SetAutoComplete(
                    fieldId,
                    opt.sql,
                    "",
                    "",
                    "",
                    "",
                    opt.isUseConnection2,
                    "",
                    opt.conditionFieldValue,
                );
                $("#" + fieldId).focus();
            }

            $("#" + fieldId)
                .off("keypress.tgw")
                .on(
                    "keypress.tgw",
                    function (e) {
                        if (e.which === 13) {
                            this._runSearch(formId);
                            e.preventDefault();
                        }
                    }.bind(this),
                );
        },

        // ---- FieldWithValue / WithDate assembly ---------------------------
        // Replaces the big string-concatenation block that built
        // FieldWithValue<FormID> and WithDate on the client.

        _buildDateFilter: function (config) {
            var parts = [];
            (config.dateRanges || []).forEach(function (dr) {
                var $from = $("#" + dr.fromDateId);
                if (!$from.val()) return;

                var fromDate, toDate;

                if (dr.dbFieldEncrypted) {
                    fromDate = $from
                        .data("daterangepicker")
                        .startDate.format("YYYY/MM/DD");
                    toDate = $from
                        .data("daterangepicker")
                        .endDate.format("YYYY/MM/DD");
                    parts.push(
                        dr.betweenKeyEncrypted +
                            "|format(" +
                            dr.dbFieldEncrypted +
                            ",'yyyy/MM/dd') between '" +
                            fromDate +
                            "' and '" +
                            toDate +
                            "'",
                    );
                    return;
                }

                if (dr.isRangePicker && $from.data("daterangepicker")) {
                    fromDate = $from
                        .data("daterangepicker")
                        .startDate.format("DD/MM/YYYY");
                    toDate = $from
                        .data("daterangepicker")
                        .endDate.format("DD/MM/YYYY");
                } else {
                    fromDate = $from.val();
                    toDate = $("#" + dr.toDateId).val();
                }

                fromDate =
                    fromDate.substring(6, 10) +
                    "-" +
                    fromDate.substring(3, 5) +
                    "-" +
                    fromDate.substring(0, 2);
                toDate =
                    toDate.substring(6, 10) +
                    "-" +
                    toDate.substring(3, 5) +
                    "-" +
                    toDate.substring(0, 2);

                parts.push("FromDate|" + fromDate);
                parts.push("ToDate|" + toDate);
            });
            return parts.join("^");
        },

        _buildCustomFields: function (config) {
            var parts = [];
            (config.customFields || []).forEach(function (cf) {
                var val = $("#" + cf.id).val();
                if (val !== undefined) parts.push(cf.dbField + "|" + val);
            });
            return parts.join("^");
        },

        _buildFieldWithValue: function (config) {
            var base = config.staticFieldWithValue || "";

            var textSearch = "";
            if (config.hasRadioSearch) {
                var $input = $("#" + (config.searchInputId || ""));
                if ($input.length && $input.val()) textSearch = $input.val();

                var checkedVal = $(
                    'input[name="' + config.radioName + '"]:checked',
                ).val();
                var parts = [];
                if (checkedVal) {
                    checkedVal.split(",").forEach(function (fieldName, i) {
                        var isChecked = $(
                            'input[name="' +
                                config.radioName +
                                '"]:eq(' +
                                i +
                                ")",
                        ).is(":checked");
                        parts.push(
                            fieldName.trim() +
                                "|" +
                                (isChecked ? textSearch : ""),
                        );
                    });
                }
                var result = parts.join("^");
                var dateFilter = this._buildDateFilter(config);
                if (dateFilter) result += (result ? "^" : "") + dateFilter;
                var customFields = this._buildCustomFields(config);
                if (customFields) result += (result ? "^" : "") + customFields;

                return base + (base && result ? "^" : "") + result;
            }

            var dateOnly = this._buildDateFilter(config);
            return base + (base && dateOnly ? "^" : "") + dateOnly;
        },

        // ---- main grid load (was the big $.ajax block) --------------------

        loadGrid: function (formId, page, status, title) {
            var config = this.registry[formId];
            if (!config) {
                console.error("TableGridWidget: unknown formId", formId);
                return;
            }

            window.CurrentPage = page;

            status = status || "";
            title = title || config.reportTitle || "Print Data";

            if (
                config.hasRadioSearch &&
                !$('input[name="' + config.radioName + '"]:checked').val()
            ) {
                return false;
            }

            if (config.isShowLoadingGrid) DialogSweetAlert("load");
            $("button").attr("disabled", "disabled");

            var fieldWithValue = this._buildFieldWithValue(config);

            var requestData = {
                SQL: config.sql || "",
                GridProcedureName: config.gridProcedureName || "",
                LoadGridFunction: config.loadGridFunctionName,
                Status: status,
                ISPrintOut: status === "" ? "False" : "True",
                PageSize: status === "" ? config.pageSize : 0,
                Page: status === "" ? page : 0,
                AllowAdd: config.allowAdd,
                AllowEdit: config.allowEdit,
                AllowDelete: config.allowDelete,
                UserID: config.userId,
                SystemID: config.systemId,
                WarehouseID: config.warehouseId,
                DomainID: config.domainId,
                IsDrawKendoGrid: config.isDrawKendoGrid,
                GridTable: config.gridTable,

                kendoGridSetting: config.kendoGridSetting,
                GridTable2: config.gridTable2,
                SQL2: config.sql2Encrypted || "",
                LinkField: config.linkFieldEncrypted || "",
                JsonData: config.jsonData,

                ShowPrintOnTopTable: config.showPrintOnTopTable,
                ShowSearchBox: config.showSearchBox,
                SearchBoxWithColumnName:
                    config.searchBoxWithColumnNameEncrypted || "",
                SearchBoxData: config.showSearchBox
                    ? $("#inputSearchTextBox_" + config.rnd).val()
                    : undefined,
                Title: title,
                ReportTitle: config.reportTitle,
                rnd: config.rnd,
                GData: config.gridDataEncrypted,
                GridHeight: config.gridHeight,
                OrderBy: config.orderBy,
                IsUseHavingCondition: config.isUseHavingCondition,
                IsHaveWhereCondition: config.isHaveWhereCondition,
                TableName: config.tableNameEncrypted,
                PrimaryField: config.primaryFieldEncrypted,
                ModifyField: config.modifyFieldEncrypted,
                FieldWithValue: fieldWithValue,
                EditTitle: config.editTitle,
                EditURL: status === "" ? config.serverRoot + config.addUrl : "",
                OnDialogCloseCallbackFunction:
                    config.onDialogCloseCallbackFunction,
                DialogBoxWidth: config.dialogBoxWidth,
                DialogBoxHeight: config.dialogBoxHeight,
                FormID: config.formId,
                RowSeleted: window.RowSeleted,
                dataTableNumber: config.dataTableNumber,
                isfixedTableHeader: config.isFixedTableHeader,
                TableClassName: config.isFixedTableHeader
                    ? "table mytable-bordered"
                    : config.tableClassName,
                TblID: config.tblId,
                ShowPaging: status === "" ? config.showPaging : "False",
                NotShowWithNoRecord: config.notShowWithNoRecord,
                DrawTotalHeaderTbl: config.drawTotalHeaderTbl,
                DrawTotalAllHeader: config.drawTotalAllHeader,
                DrawTotalAllFooter: config.drawTotalAllFooter,
                DrawTotalAllHeaderDataTableNumber:
                    config.drawTotalAllHeaderDataTableNumber,
                DrawTotalAllFooterDataTableNumber:
                    config.drawTotalAllFooterDataTableNumber,
                GridUseConnection2: config.gridUseConnection2,
                FooterSummaryDataTable: config.footerSummaryDataTable,
                LinkSummaryField: config.linkSummaryField,
                LinkSummaryColumn: config.linkSummaryColumn,
            };

            var cacheable =
                status === "" && config.enableLoadGridCache !== false;
            var cacheKey = _cacheKey(
                Object.assign({}, requestData, { RowSeleted: undefined }),
            );

            if (cacheable) {
                var cached = _getCachedGrid(
                    config.formId,
                    cacheKey,
                    config.loadGridCacheTTL || LOAD_GRID_TTL_MS,
                );

                // console.log(cached);
                if (cached) {
                    this._onLoadGridSuccess(config, status, title, cached);
                    return;
                }
            }

            if (config.isShowLoadingGrid) DialogSweetAlert("load");
            $("button").attr("disabled", "disabled");

            var self = this;
            $.ajax({
                url: config.serverRoot + "DoActionData/LoadTableGrid",
                data: requestData,
                headers: { Authorization: "Bearer " + window.access_Token },
                dataType: "html",
                type: "POST",
                cache: false,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader(
                        "x-custom-header",
                        JSON.stringify(window.headerPara || {}),
                    );
                    $("button").attr("disabled", true);
                },
                error: function () {
                    $("button").attr("disabled", false);
                    DialogSweetAlert("error");
                },
                success: function (responseText) {
                    if (cacheable) {
                        _setCachedGrid(formId, cacheKey, responseText);
                    }
                    self._onLoadGridSuccess(
                        config,
                        status,
                        title,
                        responseText,
                    );
                },
            });
        },

        _onLoadGridSuccess: function (config, status, title, responseText) {
            $("button").prop("disabled", false);
            if (window.Swal) Swal.close();

            if (config.isFixedTableHeader) {
                setTimeout(function () {
                    $("#" + config.tblId).fixedTableHeader();
                }, 100);
            }
            if (config.isShowLoadingGrid) DialogSweetAlert("loadsuccess");

            if (config.jqueryDataTable) {
                setTimeout(function () {
                    $("#" + config.tblId).DataTable({
                        responsive: true,
                        lengthChange: false,
                    });
                }, 100);
            }
            if (config.usingJqueryDataTable) {
                this._setupFullDataTable(config);
            }

            $("button").removeAttr("disabled");

            if (status === "Print") {
                downloadInnerHtml(responseText);
            } else if (status === "Excel") {
                ExportToExcel(responseText, config.tableIdToExcel);
            } else if (status === "ViewPDF") {
                OpenNewTap(responseText);
            } else if (status === "Download") {
                var link = document.createElement("a");
                link.setAttribute("target", "_blank");
                link.href = responseText;
                link.download = title + ".pdf";
                link.click();
                link.remove();
            } else {
                $("#" + config.showGridTableId).html(responseText);
                if (config.pageSize === 0) $(".divbody").removeAttr("style");
                window.clearTimeout(window["ClearTimeOut" + config.formId]);

                if (config.gridEventDblClick) {
                    $("#" + config.tblId + " tr td")
                        .dblclick(function () {
                            new Function(config.gridEventDblClick).call(this);
                        })
                        .css("cursor", "pointer");
                }
                if (config.gridEventClick) {
                    $("#" + config.tblId + " tr td")
                        .click(function () {
                            new Function(config.gridEventClick).call(this);
                        })
                        .css("cursor", "pointer");
                }
                if (config.showTblTitle) {
                    $("#" + config.tblId + " tr td").each(function () {
                        $(this).attr("title", $(this).text());
                    });
                }
            }
        },

        _setupFullDataTable: function (config) {
            var self = this;
            setTimeout(function () {
                if (config.showJqueryDataTablePrint)
                    $("#divPrintBtnTop").remove();

                var dtOptions = {
                    responsive: !!config.jqueryDataTableResponsive,
                    lengthMenu: [
                        [config.pageRecord, 50, 100, -1],
                        [String(config.pageRecord), "50", "100", "Show all"],
                    ],
                    lengthChange: true,
                    paging: true,
                    language: {
                        emptyTable: "No items to display.",
                        info: "Showing _START_ to _END_ of _TOTAL_ entries",
                        infoEmpty: "Showing 0 to 0 of 0 entries",
                        searchPlaceholder: "Search",
                    },
                };
                if (config.hiddenJqueryDataColumns) {
                    dtOptions.columnDefs = config.hiddenJqueryDataColumns
                        .split(",")
                        .map(function (t) {
                            return { target: t, visible: false };
                        });
                }
                if (config.showJqueryDataTablePrint) {
                    dtOptions.dom =
                        "<'row'<'col-sm-12 col-md-6 d-flex align-items-center justify-content-start'f>" +
                        "<'col-sm-12 col-md-6 d-flex align-items-center justify-content-end'B>>" +
                        "<'row'<'col-sm-12'tr>><'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>";
                    dtOptions.buttons = [
                        "pageLength",
                        {
                            extend: "colvis",
                            text: "Column visibility",
                            exportOptions: { columns: ":visible" },
                        },
                        {
                            extend: "excelHtml5",
                            text: '<i class="fa-regular fa-file-excel"></i> <span class="d-none d-md-inline">Excel</span>',
                            titleAttr: "Generate Excel",
                            className: "btn btn-primary btn-sm",
                        },
                        {
                            extend: "print",
                            exportOptions: { columns: ":visible" },
                            text: '<i class="ti ti-printer"></i><span class="d-none d-md-inline">Print</span>',
                            titleAttr: "Print Table",
                            className: "btn btn-default btn-sm",
                        },
                    ];
                }

                var table = $("#" + config.tblId).DataTable(dtOptions);

                var headerRow = $('<div class="card-header pl-0 mb-3"></div>');
                if (config.caption)
                    headerRow.append(
                        '<h2 class="card-title mr-auto p-2">' +
                            config.caption +
                            "</h2>",
                    );
                if (config.addButtonLabel) {
                    var tools = $('<div class="card-tools"></div>');
                    tools.append(
                        '<button id="Bun' +
                            config.addButtonId +
                            '" type="button" class="' +
                            (config.btnSearchStyle || "") +
                            '"><i class="fas fa-plus"></i> ' +
                            config.addButtonLabel +
                            "</button>",
                    );
                    headerRow.append(tools);
                }
                if (config.caption || config.addButtonLabel) {
                    $("div.dataTables_filter")
                        .parent()
                        .parent()
                        .parent()
                        .before(headerRow);
                }

                var searchRow = $(
                    '<div class="form-group"><div class="input-group">' +
                        '<div class="input-group-prepend"><label for="myInputSearch" class="input-group-text">' +
                        '<i class="fas fa-search"></i></label></div>' +
                        '<input id="myInputSearch" type="search" class="form-control" placeholder="Search" />' +
                        "</div></div>",
                );
                $("#TableGride_filter")
                    .html(searchRow)
                    .removeClass("dataTables_filter");

                $("#myInputSearch").on("keyup", function () {
                    table.search(this.value).draw();
                });

                if (config.addButtonLabel) {
                    $("#Bun" + config.addButtonId)
                        .off("click")
                        .on("click", function () {
                            if (!config.allowAdd) {
                                DialogSweetAlert(
                                    "nopermission",
                                    "Sorry, you don't have permission for accessing this page. Please contact administrator.",
                                );
                                return;
                            }
                            ShowDialogBox(
                                config.addButtonLabel,
                                config.dialogBoxWidth,
                                config.dialogBoxHeight,
                                config.serverRoot + config.addUrl,
                                "0",
                                "",
                                config.modalWidth,
                                config.modalHeight,
                            );
                        });
                }

                $("div.dt-buttons").addClass("btn-group flex-wrap");
                $("div.btn-group").removeClass("dt-buttons");
            }, 100);
        },

        // ---- print dialog (was ShowPrintDialogOption_<rnd>) ---------------

        showPrintDialog: function (formId, status, title) {
            var config = this.registry[formId];
            status = status || "Print";
            title = title || "Print Data";

            ShowToastLoading.fire({
                title: "Loading...",
                didOpen: function () {
                    Swal.showLoading();
                    setTimeout(function () {
                        $.ajax({
                            url: config.serverRoot + "Print",
                            data: {
                                SQL: config.sql || "",
                                GridProcedureName:
                                    config.gridProcedureName || "",
                                PageSize: 0,
                                Page: 0,
                                ISPrintOut: "True",
                                UserID: config.userId,
                                SystemID: config.systemId,
                                WarehouseID: config.warehouseId,
                                DomainID: config.domainId,
                                ShowPrintOnTopTable: "False",
                                rnd: config.rnd,
                                OrderBy: config.orderBy,
                                IsUseHavingCondition:
                                    config.isUseHavingCondition,
                                IsHaveWhereCondition:
                                    config.isHaveWhereCondition,
                                TableName: config.tableNameEncrypted,
                                PrimaryField: config.primaryFieldEncrypted,
                                GridData: config.gridDataEncrypted,
                                ModifyField: config.modifyFieldEncrypted,
                                FieldWithValue:
                                    TableGridWidget._buildFieldWithValue(
                                        config,
                                    ),
                                DialogBoxWidth: config.dialogBoxWidth,
                                DialogBoxHeight: config.dialogBoxHeight,
                                FormID: config.formId,
                                RowSeleted: window.RowSeleted,
                                dataTableNumber: config.dataTableNumber,
                                isfixedTableHeader: config.isFixedTableHeader,
                                TableClassName: config.isFixedTableHeader
                                    ? "table mytable-bordered"
                                    : config.tableClassName,
                                TblID: config.tblId,
                                PrintHeader: config.printHeader,
                                PrintFooter: config.printFooter,
                                SendEmail: "False",
                                EmailSubject: config.emailSubject,
                                SendEmailType: config.sendEmailType,
                                HideOnPrintTotalFooter:
                                    config.hideOnPrintTotalFooter,
                                NotShowWithNoRecord: config.notShowWithNoRecord,
                                DrawTotalHeaderTbl: config.drawTotalHeaderTbl,
                                DrawTotalAllHeader: config.drawTotalAllHeader,
                                DrawTotalAllFooter: config.drawTotalAllFooter,
                                DrawTotalAllHeaderDataTableNumber:
                                    config.drawTotalAllHeaderDataTableNumber,
                                DrawTotalAllFooterDataTableNumber:
                                    config.drawTotalAllFooterDataTableNumber,
                                GridUseConnection2: config.gridUseConnection2,
                                FooterSummaryDataTable:
                                    config.footerSummaryDataTable,
                                LinkSummaryField: config.linkSummaryField,
                                LinkSummaryColumn: config.linkSummaryColumn,
                                Title: title,
                                Status: status,
                                ReportTitle: config.reportTitle,
                            },
                            headers: {
                                Authorization: "Bearer " + window.access_Token,
                            },
                            dataType: "html",
                            type: "POST",
                            beforeSend: function (xhr) {
                                xhr.setRequestHeader(
                                    "x-custom-header",
                                    JSON.stringify(window.headerPara || {}),
                                );
                                $("button").attr("disabled", true);
                            },
                            error: function () {
                                $("button").attr("disabled", false);
                                DialogSweetAlert("error");
                            },
                            success: function (responseText) {
                                Swal.close();
                                if (status === "Print") {
                                    var win = window.open("", "_blank");
                                    win.document.writeln(responseText);
                                    setTimeout(function () {
                                        win.focus();
                                        win.print();
                                    }, 400);
                                } else if (status === "ViewPDF") {
                                    OpenNewTap(responseText);
                                } else {
                                    var link = document.createElement("a");
                                    link.href = responseText;
                                    link.download = title + ".pdf";
                                    link.click();
                                    link.remove();
                                }
                                $("button").attr("disabled", false);
                            },
                        });
                    }, 100);
                },
                willClose: function () {
                    Swal.hideLoading();
                },
            });
        },

        // ---- send email -----------------------------------------------------

        sendEmail: function (formId) {
            var config = this.registry[formId];
            MessageBox(
                '<div align="center"><h4>Loading... </h4><img width="36" src="' +
                    config.serverRoot +
                    'Content/TableGrid/Images/loadinfo.gif"></div>',
            );
            $("#MessageBox")
                .parent()
                .children()
                .children(".ui-dialog-titlebar-close")
                .hide();
            $("#MessageBox")
                .dialog("widget")
                .find(".ui-dialog-buttonpane .ui-button:first")
                .hide();
            $("#SendEmailButton").button({ label: "Sending..." });

            $.ajax({
                url: config.serverRoot + "Print",
                data: {
                    SystemID: config.systemId,
                    WarehouseID: config.warehouseId,
                    DomainID: config.domainId,
                    ISPrintOut: "True",
                    SendEmail: "True",
                    EmailSubject: config.emailSubject,
                    SendEmailType: config.sendEmailType,
                    DrawTotalAllHeaderDataTableNumber:
                        config.drawTotalAllHeaderDataTableNumber,
                    DrawTotalAllHeader: config.drawTotalAllHeader,
                    PageSize: 0,
                    Page: 0,
                    GridProcedureName: config.gridProcedureName || "",
                    GridData: config.gridDataEncrypted,
                    ModifyField: config.modifyFieldEncrypted,
                    FieldWithValue: this._buildFieldWithValue(config),
                    DialogBoxWidth: config.dialogBoxWidth,
                    DialogBoxHeight: config.dialogBoxHeight,
                    FormID: config.formId,
                    RowSeleted: window.RowSeleted,
                    GridUseConnection2: config.gridUseConnection2,
                    PrintHeader: config.printHeader,
                    FooterSummaryDataTable: config.footerSummaryDataTable,
                    LinkSummaryField: config.linkSummaryField,
                    LinkSummaryColumn: config.linkSummaryColumn,
                    PrintFooter: config.printFooter,
                    HideOnPrintTotalFooter: config.hideOnPrintTotalFooter,
                },
                dataType: "html",
                async: true,
                type: "POST",
                success: function (responseText) {
                    $("#SendEmailButton").button({ label: "Send Email" });
                    $("button").attr("disabled", false);
                    MessageBox(
                        '<div align="center"><h4>' +
                            responseText +
                            "</h4></div>",
                    );
                },
            });
        },
    };

    window.TableGridWidget = TableGridWidget;
})(window, jQuery);
