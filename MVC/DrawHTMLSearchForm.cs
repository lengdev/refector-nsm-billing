using System.Collections.Generic;
using System.Web.Script.Serialization;
using Utilities;

namespace MVC
{
    public class DrawHTMLSearchForm
    {
        public static string BuildScriptBlock(ScriptBlockParams p)
        {
            var cfg = BuildConfig(p);
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(cfg);

            return "<script>TableGridWidget.init(" + json + ");</script>";
        }

        private static GridConfig BuildConfig(ScriptBlockParams p)
        {
            var cfg = new GridConfig
            {
                formId = p.FormID,
                tblId = p.TblID,
                randomNumber = p.randomNumber,
                rnd = p.rnd,
                serverRoot = p.ServerRoot,
                loadGridFunctionName = p.LoadGridFunction,
                overrideSearch = p.OverrideSearch,
                showGridTableId = p.ShowGridTableID + "_" + p.FormID,
                searchButtonId = p.BunSearch,
                searchInputId = p.SearchInputId,
                addButtonId = p.AddButton != null ? p.AddButton.ToString().Replace(" ", "").Replace("/", "") : null,
                addButtonLabel = p.AddButton?.ToString(),
                addFunctionName = p.AddFunctionName,
                addUrl = p.AddURL,
                allowAdd = p.AllowAdd,
                allowEdit = p.AllowEdit,
                allowDelete = p.AllowDelete,
                dialogBoxWidth = p.DialogBoxWidth,
                dialogBoxHeight = p.DialogBoxHeight,
                modalWidth = p.ModalWidth,
                modalHeight = p.ModalHeight,
                onDialogCloseCallbackFunction = p.OnDialogCloseCallbackFunction,
                drawPrintButton = p.DrawPrintButton,
                showPrintOnTopTable = p.ShowPrintOnTopTable,
                drawSendEmailButton = p.DrawSendEmailButton,
                overridePrint = p.OverridePrint,
                sql = !string.IsNullOrEmpty(p.GH_SQL) ? p.Encrypt(p.GH_SQL) : "",
                gridProcedureName = !string.IsNullOrEmpty(p.GH_GridProcedureName) ? p.Encrypt(p.GH_GridProcedureName) : "",
                gridDataEncrypted = p.Encrypt(p.GridData),
                modifyFieldEncrypted = p.Encrypt(p.ModifyField),
                tableNameEncrypted = p.Encrypt(p.TableName),
                primaryFieldEncrypted = p.Encrypt(p.PrimaryField),

                userId = p.UserID,
                systemId = p.SystemID,
                warehouseId = p.WarehouseID,
                domainId = p.DomainID,

                isDrawKendoGrid = p.IsDrawKendoGrid,
                gridTable = p.GridTable,
                kendoGridSetting = p.KendoGridSetting,
                gridTable2 = p.GridTable2,
                sql2Encrypted = !string.IsNullOrEmpty(p.SQL2) ? p.Encrypt(p.SQL2) : "",
                linkFieldEncrypted = !string.IsNullOrEmpty(p.LinkField) ? p.Encrypt(p.LinkField) : "",

                showSearchBox = p.ShowSearchBox,
                searchBoxWithColumnNameEncrypted = !string.IsNullOrEmpty(p.SearchBoxWithColumnName)
                    ? p.Encrypt(p.SearchBoxWithColumnName) : "",
                pageSize = p.PageSize,

                reportTitle = p.ReportTitle,
                gridHeight = p.GridHeight,
                orderBy = p.OrderBy,
                isUseHavingCondition = p.IsUseHavingCondition,
                isHaveWhereCondition = p.IsHaveWhereCondition,
                editTitle = p.AddButton != null ? p.AddButton.ToString().Replace("Add", "Edit") : null,
                dataTableNumber = p.dataTableNumber,
                isFixedTableHeader = p.IsFixedTableHeader,
                tableClassName = p.TableClassName,
                showPaging = p.ShowPaging,
                notShowWithNoRecord = p.NotShowWithNoRecord,
                drawTotalHeaderTbl = p.DrawTotalHeaderTbl,
                drawTotalAllHeader = p.DrawTotalAllHeader,
                drawTotalAllFooter = p.DrawTotalAllFooter,
                drawTotalAllHeaderDataTableNumber = p.DrawTotalAllHeaderDataTableNumber,
                drawTotalAllFooterDataTableNumber = p.DrawTotalAllFooterDataTableNumber,
                gridUseConnection2 = p.GridUseConnection2,
                footerSummaryDataTable = p.FooterSummaryDataTable,
                linkSummaryField = p.LinkSummaryField,
                linkSummaryColumn = p.LinkSummaryColumn,
                printHeader = p.PrintHeader,
                printFooter = p.PrintFooter,
                hideOnPrintTotalFooter = p.HideOnPrintTotalFooter,
                emailSubject = p.EmailSubject,
                sendEmailType = p.SendEmailType,
                jqueryDataTable = p.JqueryDataTable,
                usingJqueryDataTable = p.UsingJqueryDataTable,
                jqueryDataTableResponsive = p.JqueryDataTableResponsive,
                showJqueryDataTablePrint = p.IsShowJqueryDataTablePrint,
                hiddenJqueryDataColumns = p.HiddenJqueryDataColumns,
                pageRecord = p.PageRecord,
                gridEventClick = p.GridEventClick,
                gridEventDblClick = p.GridEventDblClick,
                showTblTitle = p.ShowTblTitle,
                isShowLoadingGrid = p.isShowLoadingGrid,
                defaultSearch = p.DefaultSearch,
                showGridOnly = p.ShowGridOnly,
                showSearchButton = p.ShowSearchButton,
                caption = p.Caption?.ToString(),
                tableIdToExcel = p.TableIDToExcel,
                btnSearchStyle = p.btnSearchStyle,
                cboSearchStyle = p.CboSearchStyle,
                cboDataStyle = p.CboDataStyle,
                staticFieldWithValue = p.StaticFieldWithValue ?? "",
                hasRadioSearch = p.RadioOptions != null && p.RadioOptions.Count > 0,
                jsonData = p.JsonData,
            };

            if (cfg.hasRadioSearch)
            {
                cfg.radioName = "RD" + p.RadioOptions[0].ID;
                foreach (var o in p.RadioOptions)
                {
                    cfg.radioOptions.Add(new RadioOptionConfig
                    {
                        optLabel = o.OPTLabel,
                        id = o.ID,
                        sql = p.Encrypt(o.SQL),
                        selectValue = p.Encrypt(o.SelectValue),
                        displayField = p.Encrypt(o.DisplayField),
                        showAll = o.ShowAll,
                        value = o.Value,
                        isAutoComplete = o.ISAutoComplete,
                        isAutoCbo = o.ISAutoCbo,
                        multiple = o.Multiple,
                        isUseConnection2 = o.IsUseConnection2,
                        conditionFieldValue = o.ConditionFieldVale,
                        placeholder = o.placeholder
                    });
                }
            }

            if (p.CustomFields != null)
            {
                foreach (var o in p.CustomFields)
                    cfg.customFields.Add(new CustomFieldConfig { id = o.ID, dbField = o.DBField });
            }

            if (p.DateRanges != null)
            {
                foreach (var o in p.DateRanges)
                {
                    cfg.dateRanges.Add(new DateRangeConfig
                    {
                        fromDateId = o.FromDateID,
                        toDateId = o.ToDateID,
                        dbFieldEncrypted = EncryAndDecry.Decryp(!string.IsNullOrEmpty(o.DBField) ? o.DBField : ""), // DBField itself is a raw column name used inside a format() call server-side; not encrypted historically
                        betweenKeyEncrypted = p.Encrypt("between"),
                        isRangePicker = o.DateRangePicker,
                        dateTimePickerWithRanges = o.DateTimePickerWithRanges
                    });
                }
            }

            return cfg;
        }
    }

    public class ScriptBlockParams
    {
        public System.Func<string, string> Encrypt = s => s; // wire up as: p.Encrypt = EncryAndDecry.Encryp;

        public string FormID, TblID, ServerRoot, LoadGridFunction, OverrideSearch;
        public string ShowGridTableID, BunSearch, SearchInputId;
        public object AddButton;
        public string AddURL, AddFunctionName;
        public bool AllowAdd, AllowEdit, AllowDelete;
        public string DialogBoxWidth, DialogBoxHeight, ModalWidth, ModalHeight;
        public string OnDialogCloseCallbackFunction;
        public bool DrawPrintButton, ShowPrintOnTopTable, DrawSendEmailButton;
        public string OverridePrint;
        public string GH_SQL, GH_GridProcedureName;
        public string GridData, ModifyField, TableName, PrimaryField;

        // These four were referenced by the JS but never had a C# source before.
        public string UserID, SystemID, WarehouseID, DomainID;

        // Method parameters from the original DrawHTMLSearchForm(...) signature -
        // also referenced by the JS but never wired through.
        public bool IsDrawKendoGrid;
        public string GridTable, KendoGridSetting, GridTable2, SQL2, LinkField;

        public bool ShowSearchBox;
        public string SearchBoxWithColumnName;
        public int PageSize;

        public string ReportTitle, GridHeight, OrderBy;
        public bool IsUseHavingCondition, IsHaveWhereCondition;
        public int dataTableNumber;
        public bool IsFixedTableHeader;
        public string TableClassName;
        public bool ShowPaging, NotShowWithNoRecord;
        public bool DrawTotalHeaderTbl, DrawTotalAllHeader, DrawTotalAllFooter;
        public int DrawTotalAllHeaderDataTableNumber, DrawTotalAllFooterDataTableNumber;
        public bool GridUseConnection2;
        public string LinkSummaryField, LinkSummaryColumn;
        public int FooterSummaryDataTable;
        public bool PrintHeader, PrintFooter, HideOnPrintTotalFooter;
        public string EmailSubject;
        public int SendEmailType;
        public bool JqueryDataTable, UsingJqueryDataTable, JqueryDataTableResponsive, IsShowJqueryDataTablePrint;
        public string HiddenJqueryDataColumns;
        public int PageRecord;
        public string GridEventClick, GridEventDblClick;
        public bool ShowTblTitle, isShowLoadingGrid, DefaultSearch, ShowGridOnly, ShowSearchButton;
        public object Caption;
        public string TableIDToExcel, btnSearchStyle, CboSearchStyle, CboDataStyle;
        public int rnd;
        public string randomNumber;

        // Precomputed (server-side) "ParaName|Value^ParaName|Value..." string built from
        // GH.GridPara, matching the old sb_script GridPara loop. Kept as plain string
        // building in the .cs method since it's just static request parameters, not behavior.
        public string StaticFieldWithValue;
        public string JsonData;

        public List<RadioOptionSource> RadioOptions = new List<RadioOptionSource>();
        public List<CustomFieldSource> CustomFields = new List<CustomFieldSource>();
        public List<DateRangeSource> DateRanges = new List<DateRangeSource>();
    }

    public class RadioOptionSource
    {
        public string ID, OPTLabel, SQL, SelectValue, DisplayField, ShowAll, Value;
        public bool ISAutoComplete, ISAutoCbo, Multiple;
        public string IsUseConnection2, ConditionFieldVale, placeholder;
    }

    public class CustomFieldSource
    {
        public string ID, DBField;
    }

    public class DateRangeSource
    {
        public string FromDateID, ToDateID, DBField;
        public bool DateRangePicker, DateTimePickerWithRanges;
    }

    // ---- serialized-to-JS shape ------------------------------------------

    public class GridConfig
    {
        public string formId, tblId, serverRoot, loadGridFunctionName, overrideSearch;
        public string showGridTableId, searchButtonId, searchInputId;
        public string addButtonId, addButtonLabel, addFunctionName, addUrl;
        public bool allowAdd, allowEdit, allowDelete;
        public string dialogBoxWidth, dialogBoxHeight, modalWidth, modalHeight;
        public string onDialogCloseCallbackFunction;
        public bool drawPrintButton, showPrintOnTopTable, drawSendEmailButton;
        public string overridePrint;
        public string sql, gridProcedureName, gridDataEncrypted, modifyFieldEncrypted;
        public string tableNameEncrypted, primaryFieldEncrypted;
        public string userId, systemId, warehouseId, domainId;
        public bool isDrawKendoGrid;
        public string gridTable, kendoGridSetting, gridTable2, sql2Encrypted, linkFieldEncrypted;
        public bool showSearchBox;
        public string searchBoxWithColumnNameEncrypted, reportTitle, gridHeight, orderBy;
        public bool isUseHavingCondition, isHaveWhereCondition;
        public string editTitle;
        public int dataTableNumber;
        public bool isFixedTableHeader;
        public string tableClassName;
        public bool showPaging;
        public bool notShowWithNoRecord;
        public bool drawTotalHeaderTbl, drawTotalAllHeader, drawTotalAllFooter;
        public int drawTotalAllHeaderDataTableNumber, drawTotalAllFooterDataTableNumber;
        public bool gridUseConnection2;
        public int footerSummaryDataTable;
        public string linkSummaryField, linkSummaryColumn;
        public bool printHeader, printFooter, hideOnPrintTotalFooter;
        public string emailSubject;
        public int sendEmailType;
        public bool jqueryDataTable, usingJqueryDataTable, jqueryDataTableResponsive, showJqueryDataTablePrint;
        public string hiddenJqueryDataColumns;
        public int pageRecord, pageSize;
        public string gridEventClick, gridEventDblClick;
        public bool showTblTitle, isShowLoadingGrid, defaultSearch, showGridOnly, showSearchButton;
        public string caption, tableIdToExcel, btnSearchStyle, cboSearchStyle, cboDataStyle;
        public int rnd;
        public string randomNumber;
        public string staticFieldWithValue;

        public bool hasRadioSearch;
        public string radioName;
        public List<RadioOptionConfig> radioOptions = new List<RadioOptionConfig>();

        public List<CustomFieldConfig> customFields = new List<CustomFieldConfig>();
        public List<DateRangeConfig> dateRanges = new List<DateRangeConfig>();

        public string jsonData;
    }

    public class RadioOptionConfig
    {
        public string optLabel, id, sql, selectValue, displayField, showAll, value;
        public bool isAutoComplete, isAutoCbo, multiple;
        public string isUseConnection2, conditionFieldValue, placeholder;
    }

    public class CustomFieldConfig { public string id, dbField; }

    public class DateRangeConfig
    {
        public string fromDateId, toDateId, dbFieldEncrypted, betweenKeyEncrypted;
        public bool isRangePicker, dateTimePickerWithRanges;
    }
}