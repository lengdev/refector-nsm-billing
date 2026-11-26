namespace MVC
{
    public abstract class HTMLControl
    {
        /// <summary>Field name used as a condition when rendering or binding.</summary>
        public string ConditionField = "";
        /// <summary>True if control is a Kendo Multi-Column ComboBox.</summary>
        public bool IsKendoMultiColumnComboBox = false;

        /// <summary>Allow adding new value if primary field is not zero.</summary>
        public bool AllowAddNewPrefixEventPrimaryValueIsNotZeo = false;

        /// <summary>Token value used for validation or API calls.</summary>
        public string Token = "";

        /// <summary>Enable file uploads using version 2 uploader.</summary>
        public bool UploadFilesV2 = false;

        /// <summary>Get value from textarea name attribute.</summary>
        public bool GetValueFromTextAreaName = false;

        /// <summary>Function name to call after value is added.</summary>
        public string AddCallBackFunction = "";

        /// <summary>Indicates whether value must be numeric.</summary>
        public bool ISNumeric = false;

        /// <summary>Numeric format (e.g. "0.00").</summary>
        public string Digit = "0.00";

        /// <summary>Predefined value to set in control.</summary>
        public string SetValue = "";

        /// <summary>True if control uses SummerNote editor.</summary>
        public bool ISSummerNote = false;

        /// <summary>True if control accepts only integer values.</summary>
        public bool ISInteger = false;

        /// <summary>Enable DateTime picker with ranges.</summary>
        public bool DateTimePickerWithRanges = false;

        /// <summary>Secondary condition field for search.</summary>
        public string ConditionFieldSearch2 = "";

        /// <summary>Callback value returned for display.</summary>
        public string CallBackDisplayValue = "";

        /// <summary>Callback text returned for display.</summary>
        public string CallBackDisplayText = "";

        /// <summary>Target control ID to set callback value.</summary>
        public string CallBackToControlID = "";

        /// <summary>Control ID whose value is used in callback.</summary>
        public string CallBackWithControlValueID = "";

        /// <summary>Target control ID for setting next value.</summary>
        public string SetValueToNextControlID = "";

        /// <summary>Control name for setting next value.</summary>
        public string SetValueToNextControl = "";

        /// <summary>Auto-generate barcode number if not provided.</summary>
        public bool AutoGenerateBarcodeNumberIfNotEntry = false;

        /// <summary>Show auto-generated barcode number in UI.</summary>
        public bool IsDisplayAutoGenerateBarcodeNumber = false;

        /// <summary>UPC digit format for auto-generated barcode (11 digits).</summary>
        public string AutoGenerateBarcodeNumberDigit = "00000000000";

        /// <summary>Require input if another field is not null.</summary>
        public bool RequiredInputIfOtherFieldIsNotNull = false;

        /// <summary>Enable drag-and-drop file uploads.</summary>
        public bool IsDragDropFile = false;

        /// <summary>Show drop zone for file upload.</summary>
        public bool ShowDropZoneEnabled = true;

        /// <summary>JS function called when a tag is added (tags input).</summary>
        public string onTagsInputAddedFunction = "";

        /// <summary>Marks control as cloned instance.</summary>
        public bool isCloned = false;

        /// <summary>Function to call for loading grid data.</summary>
        public string LoadGrid_Function = "";

        /// <summary>CSS class for column wrapper.</summary>
        public string DivClassCol = "col-sm";

        /// <summary>Field name to update with new value.</summary>
        public string UpdateValueFiledName = "";

        /// <summary>Database field name to update.</summary>
        public string UpdateFiledName = "";

        /// <summary>Table name for updating field values.</summary>
        public string UpdateValueFiledNameToTblName = "";

        /// <summary>Custom condition for updating (e.g. Field=Value).</summary>
        public string CustomCondition = "";

        /// <summary>Condition field used when updating values.</summary>
        public string UpdateValueFiledNameConditionFiledName = "";

        /// <summary>Field name for return value.</summary>
        public string ReturnFieldNameValue = "";

        /// <summary>Table name for return field value.</summary>
        public string ReturnFieldNameValueFromTblName = "";

        /// <summary>Keep data saving instructions for tables.</summary>
        public TableKeepDataSaving[] tableKeepDataSaving = null;

        /// <summary>HTML table row name.</summary>
        public string TRName = "";

        /// <summary>HTML table cell (TD) name.</summary>
        public string TDName = "";

        /// <summary>Text alignment (default "left").</summary>
        public string Align = "left";

        /// <summary>Enable single date picker.</summary>
        public bool SingleDatePicker = false;

        /// <summary>Position of single date picker (default "auto").</summary>
        public string SingleDatePickerPosition = "auto";

        /// <summary>Source control ID for setting identity.</summary>
        public string SETIDENTITYFromControlID = "";

        /// <summary>Database field name for identity.</summary>
        public string SETIDENTITYFieldName = "";

        /// <summary>Target table for identity values.</summary>
        public string SetIDENTITYToTblName = "";

        /// <summary>Field name from which identity is retrieved.</summary>
        public string GetIDENTITYFromFieldName = "";

        /// <summary>Function name for adding new items via typeahead.</summary>
        public string TypeAheadAddNewFunctionName = "";

        /// <summary>Primary field value (default "0").</summary>
        public string PrimaryFieldValue = "0";

        /// <summary>Control ID to copy same entry value.</summary>
        public string SameEntryToControlID = "";

        /// <summary>Database field name with prefix applied.</summary>
        public string SetPrefixToDbFieldName = "";

        /// <summary>Prefix value.</summary>
        public string Prefix = "";

        /// <summary>Module for updating prefix values.</summary>
        public string UpdatePrefixModule = "";

        /// <summary>SQL callback query.</summary>
        public string SQLCallBack = "";

        /// <summary>Hidden field value.</summary>
        public string HiddenField = "";

        /// <summary>Table name to insert value if not exists.</summary>
        public string AddValueToTblNameIfNotExistingData = "";

        /// <summary>Field name for inserting new value.</summary>
        public string AddValueToTblNameIfNotExistingDataFieldName = "";

        /// <summary>Primary database field name.</summary>
        public string PrimaryFieldName = "";

        /// <summary>Enable attachment file field.</summary>
        public bool ISAddAttachmentFile = false;

        /// <summary>Database field name for storing file name.</summary>
        public string DBFieldAttachmentFileName = "AttachmentFile";

        /// <summary>Enable setting location via Google Maps.</summary>
        public bool IsSetLocationWithGoogleMap = false;

        /// <summary>Prefix control name source.</summary>
        public string GetPrefixToControlName = "";

        /// <summary>Prefix target control.</summary>
        public string PrefixFor = "";

        /// <summary>Restrict input to numbers only.</summary>
        public bool IsAllowInputOnlyNumber = false;

        /// <summary>Enable floating labels for control.</summary>
        public bool ISFloatingLabel = false;

        /// <summary>Enable switch (toggle) style checkbox.</summary>
        public bool ISSwitchCheck = false;

        /// <summary>Enable typeahead search input.</summary>
        public bool isTypeahead = false;

        /// <summary>SQL query for checkbox group options.</summary>
        public string SqlCheckBoxGroup = "";

        /// <summary>Database table for checkbox group values.</summary>
        public string CheckBoxGroupTblName = "";

        /// <summary>HTML ID for checkbox group.</summary>
        public string CheckBoxGroupID = "CheckBoxGroup";

        /// <summary>CSS class for checkbox group.</summary>
        public string CheckBoxGroupClass = "CheckBoxGroup";

        /// <summary>Form ID where control belongs.</summary>
        public string FormID = "";

        /// <summary>Password validation regex pattern.</summary>
        public string PatternPassword = "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{6,}";

        /// <summary>Password validation tooltip/title.</summary>
        public string Title = "Password must contain at least 6 characters, including UPPER/lowercase and numbers.";

        /// <summary>Enable date range picker.</summary>
        public bool DateRangePicker = false;

        /// <summary>Render control using HTML table layout.</summary>
        public bool UseTable = false;

        /// <summary>Destination path for uploaded file.</summary>
        public string DestinationFile = "";

        /// <summary>Associated database table name.</summary>
        public string TableName = "";

        /// <summary>Input type (e.g., text, password, email).</summary>
        public string Type = "text";

        /// <summary>Column span if used in table layout.</summary>
        public int ColSpan = 0;

        /// <summary>Enable chosen.js combo box style.</summary>
        public bool ISCboChosen = false;

        /// <summary>Hide control in UI.</summary>
        public bool Hide = false;

        /// <summary>Allow multiple file uploads.</summary>
        public bool UploadMultiple = false;

        /// <summary>Index counter for internal use.</summary>
        protected int I = 0;

        /// <summary>Restrict uploads to image files only.</summary>
        public bool AcceptImageOnly = false;

        /// <summary>Database field name for duplicate check.</summary>
        public string CheckExistingWithField = "";

        /// <summary>Control name for duplicate check.</summary>
        public string CheckExistingWithControlName = "";

        /// <summary>Default value for duplicate check.</summary>
        public string CheckExistingWithDefaultValue = "";

        /// <summary>CSS class for input (default "form-control").</summary>
        public string Class = "form-control";

        /// <summary>Wrapper div class.</summary>
        public string DivClass = "";

        /// <summary>Placeholder text.</summary>
        public string placeholder = "";

        /// <summary>Mark control as required.</summary>
        public bool Required = false;

        /// <summary>Associated database field name.</summary>
        public string DBField = "";

        /// <summary>HTML ID of the control.</summary>
        public string ID = "";

        /// <summary>Enable auto-complete combo box.</summary>
        public bool ISAutoCbo = false;

        /// <summary>Render as checkbox.</summary>
        public bool ISCheckBox = false;

        /// <summary>Indicates if control is part of a custom form.</summary>
        public bool IsCustomForm = false;

        /// <summary>Render as datetime picker.</summary>
        public bool ISDateTime = false;

        /// <summary>Validate as email input.</summary>
        public bool ISEmail = false;

        /// <summary>Render as file input.</summary>
        public bool ISFile = false;

        /// <summary>Mark field as unique in DB.</summary>
        public bool ISUniqueField = false;

        /// <summary>Label text for control.</summary>
        public string Label = "";

        /// <summary>Horizontal alignment for label (default "right").</summary>
        public string LabelAlign = "right";

        /// <summary>Vertical alignment for label (default "top").</summary>
        public string LabelValign = "top";

        /// <summary>Maximum allowed input length.</summary>
        public int MaxLength = 999999;

        /// <summary>Validation message or hint.</summary>
        public string Message = "";

        /// <summary>Message if data already exists.</summary>
        public string MessageISExistingData = "";

        /// <summary>Minimum allowed input length.</summary>
        public int MinLength = 1;

        /// <summary>Allow selecting multiple values.</summary>
        public bool Multiple = false;

        /// <summary>Get value directly without binding.</summary>
        public bool GetDirectValue = false;

        /// <summary>Database field to get identity value by.</summary>
        public string GetIDENTITYByFiled = "";

        /// <summary>Name attribute of control.</summary>
        public string Name = "";

        /// <summary>Old/previous value for comparison.</summary>
        public string OldValue = "";

        /// <summary>Show list (UL) structure in UI.</summary>
        public bool ShowUL = false;

        /// <summary>SQL statement for populating data.</summary>
        public string SQL = "";

        /// <summary>Allow direct text input value.</summary>
        public bool UserTextValue = false;

        /// <summary>Enable field validation.</summary>
        public bool ValidationField = false;

        /// <summary>Default value of control.</summary>
        public string Value = "";

        /// <summary>Render as group of checkboxes.</summary>
        public bool ISCheckBoxGroup = false;

        /// <summary>Enable tags input style control.</summary>
        public bool isTagsInput = false;

        /// <summary>Inline CSS style string.</summary>
        public string Style = "";

        /// <summary>Selected value in combo box.</summary>
        public string CboValue = "";

        public HTMLControl()
        {
        }

        /// <summary>
        /// Generates the HTML markup for the control.
        /// </summary>
        public abstract string GetHtml();
    }
}
