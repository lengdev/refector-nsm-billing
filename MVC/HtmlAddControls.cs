using System.Text;
using Utilities;

namespace MVC
{
    public class HtmlAddControls : HTMLControl
    {
        /// <summary>Field used for grouping values.</summary>
        public string GroupByField = "";

        /// <summary>Indicates whether date range selection is enabled.</summary>
        public bool DateRange = false;

        /// <summary>Private flag for AutoComplete support.</summary>
        private bool _ISAutoComplete = false;

        /// <summary>Private SQL query for AutoComplete.</summary>
        private string _SQLAutoComplete = "";

        /// <summary>Value returned from combo box callback.</summary>
        public string CboCallbackValue = "";

        /// <summary>Indicates whether this is a combo box.</summary>
        public bool ComboBox = false;

        /// <summary>Text returned from combo box callback.</summary>
        public string CboCallbackDisplayText = "";

        /// <summary>Private combo box callback ID.</summary>
        private string _CboCallbackID = "CboSearchText";

        /// <summary>Private CSS class name for combo box.</summary>
        private string _CboClass = "Round";

        /// <summary>Private flag to show all items by default.</summary>
        private bool _ShowAll = true;

        /// <summary>Condition fields for filtering.</summary>
        public string ConditionFields { set; get; }

        /// <summary>Condition values for filtering.</summary>
        public string ConditionValue { set; get; }

        /// <summary>Condition field name when filtering by date.</summary>
        public string ConditionFieldDate { set; get; }

        /// <summary>Date value used for condition filtering.</summary>
        public string ConditionDateValue { set; get; }

        /// <summary>Order by clause for query results.</summary>
        public string OrderBy { set; get; }

        /// <summary>Callback control ID.</summary>
        public string IDCallBack { set; get; }

        /// <summary>Indicates whether callback is enabled.</summary>
        public bool IsCallBack { set; get; }

        /// <summary>Fields to display in the control.</summary>
        public string DisplayOnFields { set; get; }

        /// <summary>Value returned during callback.</summary>
        public string ValueCallBack { set; get; }

        /// <summary>Text displayed during callback.</summary>
        public string DisplayTextCallBack { set; get; }

        /// <summary>Event triggered by combo box callback.</summary>
        public string Cbo_CallBackEvent { set; get; }

        /// <summary>Value for combo box callback combo.</summary>
        public string CboValueComboCallBack { set; get; }

        /// <summary>Specifies if combo box callback is read-only.</summary>
        public bool ReadOnlyComboCallBack { set; get; }

        /// <summary>Secondary SQL query for callback or data loading.</summary>
        public string SQL2 { set; get; }

        /// <summary>Indicates whether text should be used instead of value.</summary>
        public bool UseText = false;

        /// <summary>Private flag for encryption.</summary>
        private bool _Encrypt = true;

        /// <summary>Indicates whether encryption is enabled.</summary>
        public bool Encrypt
        {
            get { return _Encrypt; }
            set { _Encrypt = value; }
        }

        /// <summary>Indicates whether callback is used for combo box.</summary>
        public bool IsCallBackToComboBox { get; set; }

        /// <summary>Combo box callback ID.</summary>
        public string CboCallbackID
        {
            get { return this._CboCallbackID; }
            set { this._CboCallbackID = value; }
        }

        /// <summary>CSS class name for combo box.</summary>
        public string CboClass
        {
            get { return this._CboClass; }
            set { this._CboClass = value; }
        }

        /// <summary>Text to display in the control.</summary>
        public string DisplayText { get; set; }

        /// <summary>Indicates whether AutoComplete is enabled.</summary>
        public bool ISAutoComplete
        {
            get { return this._ISAutoComplete; }
            set { this._ISAutoComplete = value; }
        }

        /// <summary>Specifies if combo box supports callback.</summary>
        public bool ISComboBoxCallback { get; set; }

        /// <summary>Indicates whether to use secondary connection.</summary>
        public bool IsUseConnection2 { get; set; }

        /// <summary>Indicates whether all items should be shown.</summary>
        public bool ShowAll
        {
            get { return this._ShowAll; }
            set { this._ShowAll = value; }
        }

        /// <summary>SQL query used for combo box data binding.</summary>
        public string SQL_CBO { get; set; }

        /// <summary>SQL query used for AutoComplete functionality.</summary>
        public string SQLAutoComplete
        {
            get { return this._SQLAutoComplete; }
            set { this._SQLAutoComplete = value; }
        }

        /// <summary>Constructor initializes HtmlAddControls with default values.</summary>
        public HtmlAddControls()
        {
        }

        public override string GetHtml()
        {
            Common common = new Common();
            string DateValue;
            ID = ID == null ? "" : ID;
            if (Name == "")
                Name = ID;
            string display = "";
            if (Hide)
            {
                display = " style=\"display:none\"";
            }
            StringBuilder sb = new StringBuilder();
            string HidenFieldValue = "";
            if (ISAutoComplete)
            {
                sb.Append("<tr" + display + "><td" + (LabelAlign != "" ? " align =\"" + LabelAlign + "\"" : "") + " valign=\"" + LabelValign + "\"><label class=\"col-form-label text-nowrap\" for=\"" + ID + "\">" + Label + "</label></td>");
                sb.Append("<td><input type=\"text\" name=\"" + Name + "\" id=\"" + ID + "\" value=\"" + Value + "\" class=\"" + Class + "\" maxlength=\"" + MaxLength + "\" placeholder=\"" + placeholder + "\"/>");
                if (HiddenField != "")
                {
                    string[] HFields = HiddenField.Split(',');
                    if (HidenFieldValue == null)
                        HidenFieldValue = "";
                    //string[] HValue = HidenFieldValue.Split(',');
                    for (int i = 0; i < HFields.Length; i++)
                    {
                        sb.AppendLine("<input value=\"0\" type=\"hidden\" id=\"" + (ID != "" ? ID + "_" + HFields[i].Trim() : "") + "\" name=\"" + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + "\" />");
                    }
                }
                sb.Append("</td></tr>");
                sb.Append("<script>$(function(){");
                sb.Append("SetAutoComplete(\"" + ID + "\", '" + EncryAndDecry.Encryp(SQLAutoComplete) + "',\"" + (ID != "" ? ID + "_" + HiddenField.Trim() : "") + "\", \"\", \"\", \"\", \"\", \"\", \"\");");
                sb.Append("});</script>");
            }
            else if (DateRange)
            {
                sb.Append("<tr" + display + "><td align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label class=\"col-form-label text-nowrap\" for=\"" + ID + "\">" + Label + "</label></td>");
                sb.Append("<td><input type=\"text\" name=\"" + Name + "\" id=\"" + ID + "\" value=\"" + Value + "\" class=\"" + Class + "\" maxlength=\"" + MaxLength + "\" placeholder=\"" + placeholder + "\"/>");
                sb.Append("<input type=\"hidden\" name=\"FromDate\" id=\"FromDate\" />");
                sb.Append("<input type=\"hidden\" name=\"ToDate\" id=\"ToDate\" />");
                sb.Append("</td></tr>");
                sb.Append("<script>$(function(){");
                sb.Append("$(\"#" + ID + "\").daterangepicker({");
                sb.Append("locale: { format: 'DD/MM/YYYY' }, autoApply:true ");
                sb.Append("}).on('apply.daterangepicker', function (ev, picker) {");
                sb.Append(" $(\"#FromDate\").val(picker.startDate.format('YYYY-MM-DD'));");
                sb.Append(" $(\"#ToDate\").val(picker.endDate.format('YYYY-MM-DD'));");
                sb.Append("});});</script>");
            }
            else if (UseText)
            {
                sb.Append("<tr" + display + "><td align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label for=\"" + ID + "\" class=\"col-form-label text-nowrap\">" + Label + "</label></td>");
                sb.Append("<td><input type=\"text\" name=\"" + Name + "\" id=\"" + ID + "\" value=\"" + Value + "\" class=\"" + Class + "\" maxlength=\"" + MaxLength + "\" placeholder=\"" + placeholder + "\"/></td></tr>");
            }
            else if (ComboBox)
            {

                if (GroupByField != "")
                {
                    sb.AppendLine("<tr" + display + "><td align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label for=\"" + ID + "\" class=\"col-form-label text-nowrap\">" + Label + "</label></td>");
                    sb.AppendLine("<td width=\"230\"><span id=\"span_" + ID + "\"></span></td></tr>");
                    sb.AppendLine("<script>$(function(){");
                    sb.AppendLine("Reload_" + ID + "();");
                    sb.AppendLine("});");
                    sb.AppendLine("function Reload_" + ID + "() {");
                    sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_CBO) + "\"}, function(response) {");
                    sb.AppendLine("if ($(\"#" + ID + "\").hasClass(\"select2-hidden-accessible\")) {");
                    sb.AppendLine(" $(\"#" + ID + "\").select2(\"destroy\");}");
                    sb.AppendLine("$(\"#" + ID + "\").remove();");
                    sb.AppendLine("$(\"#span_" + ID + "\").html(`<select style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + ID + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\"><option value=\"All\">All</option></select>`);");
                    sb.AppendLine("let Data_" + ID + " = JSON.parse(JSON.parse(response).Data);");
                    sb.AppendLine("let group1_" + ID + " = \"\"; group2_" + ID + " = \"\";");
                    sb.AppendLine("let objData_" + ID + " = [];");
                    sb.AppendLine("let objAllData_" + ID + " = [];");
                    sb.AppendLine("$.each(Data_" + ID + ", function (key, value) {");
                    sb.AppendLine("group1_" + ID + " = value." + GroupByField + ";");
                    sb.AppendLine("if (group1_" + ID + " != group2_" + ID + ") {");
                    sb.AppendLine("objData_" + ID + " = [];");
                    sb.AppendLine("let Data" + ID + " = Data_" + ID + ".filter(function (data) {");
                    sb.AppendLine("return data['" + GroupByField + "'] === group1_" + ID + ";");
                    sb.AppendLine("});");
                    sb.AppendLine("$.each(Data" + ID + ", function (k, val) {");
                    sb.AppendLine("let obj_" + ID + " = {};");
                    sb.AppendLine("obj_" + ID + "[\"id\"] = val." + Value + ";");
                    sb.AppendLine("obj_" + ID + "[\"text\"] = val." + DisplayText + ";");
                    sb.AppendLine("objData_" + ID + ".push(obj_" + ID + ");");
                    sb.AppendLine("});");
                    sb.AppendLine("objAllData_" + ID + ".push({");
                    sb.AppendLine("'text': group1_" + ID + ",");
                    sb.AppendLine("'children': objData_" + ID + "");
                    sb.AppendLine("});");
                    sb.AppendLine("}");
                    sb.AppendLine("objData_" + ID + " = [];");
                    sb.AppendLine("group2_" + ID + " = value." + GroupByField + ";");
                    sb.AppendLine("});");

                    sb.AppendLine("let $select2Elm_" + ID + " = $('#" + ID + "');");
                    sb.AppendLine("$select2Elm_" + ID + ".select2({");
                    //sb.AppendLine("allowClear: true,");
                    sb.AppendLine("maximumSelectionSize: 1,");
                    //sb.AppendLine("multiple: true,");
                    sb.AppendLine("theme: \"bootstrap4\",");
                    sb.AppendLine("data: objAllData_" + ID + ",");
                    sb.AppendLine("dropdownParent: $(\"#" + ID + "\").parent(),");
                    sb.AppendLine("placeholder: `" + placeholder + "`,");
                    sb.AppendLine("escapeMarkup: function(m) {");
                    sb.AppendLine("return m;");
                    sb.AppendLine("}");
                    sb.AppendLine("}).on('select2:open', () => {");
                    sb.Append("})");
                    sb.Append(".on('select2:close', function (e) {");
                    sb.Append("})");
                    sb.Append(".on('select2:select', function (e) {");
                    sb.Append("})");
                    sb.Append(".on('change', function(e) {");
                    sb.AppendLine("if (typeof " + LoadGrid_Function + " === 'function')");
                    sb.AppendLine(LoadGrid_Function + "(1);");
                    sb.AppendLine("}).data('select2').clearSearch = function() {");
                    sb.AppendLine("};");
                    sb.AppendLine("});");
                    sb.AppendLine("};");
                    sb.AppendLine("</script>");
                }
                else
                {
                    string CboResult = common.LoadDataToCBO(ID, EncryAndDecry.Encryp(SQL_CBO), EncryAndDecry.Encryp(Value), EncryAndDecry.Encryp(DisplayText), "", ShowAll, false, true, false, IsUseConnection2, "", Multiple, IsCallBackToComboBox);
                    sb.AppendLine("<tr" + display + "><td align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label for=\"" + ID + "\" class=\"col-form-label text-nowrap\">" + Label + "</label></td>");
                    sb.AppendLine("<td width=\"230\">" + CboResult + "</td></tr>");
                    sb.AppendLine("<script>$(function(){");
                    sb.AppendLine("$(\"#" + ID + "\").addClass(\"form-control select2\");");
                    sb.AppendLine("$(\"#" + ID + "\").select2({");
                    sb.AppendLine("dropdownParent:$(\"#" + ID + "\").parent(),");
                    sb.AppendLine("theme: 'bootstrap4',");
                    sb.AppendLine("placeholder: \"" + placeholder + "\"");
                    //sb.AppendLine(",minimumResultsForSearch: Infinity");
                    sb.AppendLine("});");
                    sb.AppendLine("$(\"#" + ID + "\").change(function() {");
                    sb.AppendLine("if (typeof " + LoadGrid_Function + " === 'function')");
                    sb.AppendLine(LoadGrid_Function + "(1);");
                    sb.AppendLine("});");
                    if (CboValue != "")
                    {
                        sb.AppendLine("$(\"#" + ID + "\").val(\"" + CboValue + "\").trigger(\"change\");");
                    }
                    sb.AppendLine("});");
                    sb.AppendLine("</script>");
                }
            }
            else
            {
                string CboResult = common.LoadDataToCBO(base.ID, EncryAndDecry.Encryp(this.SQL_CBO), EncryAndDecry.Encryp(base.Value), EncryAndDecry.Encryp(this.DisplayText), "", this.ShowAll, false, true, false, this.IsUseConnection2, "", false, IsCallBackToComboBox);
                sb.AppendLine("<tr" + display + "><td align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label class=\"col-form-label text-nowrap\" for=\"" + ID + "\">" + Label + "</label></td>");
                sb.AppendLine("<td width=\"230\">" + CboResult + "</td></tr>");
                sb.AppendLine("<script>$(function(){");
                if (CboValue != "")
                {
                    sb.AppendLine("$(\"#" + ID + "\").val(\"" + CboValue + "\")");
                }
                if (ISAutoCbo)
                {
                    sb.AppendLine(string.Concat("$(\"#", base.ID, "\").combobox();"));
                }
                else
                {
                    sb.AppendLine("$(\"#" + ID + "\").select2({");
                    //sb.AppendLine("minimumResultsForSearch: Infinity,");
                    sb.AppendLine("theme: 'bootstrap4',");
                    sb.AppendLine("});");
                }
                if (IsCallBack)
                {
                    sb.AppendLine(" $(\"#" + ID + "\").change(function(){LoadCallBackData(\"" + ID + "\", \"" + EncryAndDecry.Encryp(this.SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + SQL2 + "\");});");
                }
                else if (ISComboBoxCallback)
                {
                    sb.AppendLine("$(\"#" + ID + "\").change(function() {");
                    sb.AppendLine("setTimeout(function(){");
                    if (IsCallBackToComboBox)
                        sb.AppendLine("SearchBy_Change()");
                    sb.AppendLine("LoadTableGrid(1);");
                    sb.AppendLine("}, 300);");
                    sb.AppendLine("});");
                }
                else if (IsCallBackToComboBox)
                {
                    string Condition;
                    string ConditionDate;
                    //if (ConditionField == null) ConditionField = "";

                    if (this.ConditionFields == "" && this.ConditionValue != "")
                    {
                        Condition = string.Concat(" AND ", this.ConditionFields, " = ");
                        if (this.ConditionFieldDate == "" && this.ConditionDateValue != "")
                        {
                            ConditionDate = " AND CONVERT(VARCHAR, " + ConditionFieldDate + ", 105) = ";
                            DateValue = "$(\"#" + ConditionDateValue + "\").val()";
                            sb.AppendLine("var SQL = \"" + SQLCallBack + "\"+ $(\"#" + ID + "\").val() + \" " + Condition + "\" + $(\"#" + ConditionValue + "\").val() + \" " + ConditionDate + "'\" + " + DateValue + " + \"' " + OrderBy + "\";");
                        }
                        else
                        {
                            sb.AppendLine("var SQL = \"" + SQLCallBack + "\"+ $(\"#" + ID + "\").val() + \" " + Condition + "\" + $(\"#" + ConditionValue + "\").val() + \" " + OrderBy + "\";");
                        }
                    }
                    //else
                    //{
                    //	sb.AppendLine("SQL = \"" + SQLCallBack + "\"+ $(\"#" + ID + "\").val() + \" " + OrderBy + "\";");
                    //}
                    sb.AppendLine(string.Concat("$(\"#", base.ID, "\").change(function(){"));
                    if (this.ConditionFields == "" && this.ConditionValue != "")
                    {
                        Condition = string.Concat(" AND ", this.ConditionFields, " = ");
                        if (this.ConditionFieldDate == "" && this.ConditionDateValue != "")
                        {
                            ConditionDate = string.Concat(" AND CONVERT(VARCHAR, ", this.ConditionFieldDate, ", 105) = ");
                            DateValue = string.Concat("$(\"#", this.ConditionDateValue, "\").val()");
                            sb.AppendLine("var SQL = \"" + SQLCallBack + "\"+ $(\"#" + ID + "\").val() + \" " + Condition + "\" + $(\"#" + this.ConditionValue + "\").val() + \" " + ConditionDate + "'\" + " + DateValue + " + \"' " + this.OrderBy + "\";");
                        }
                        else
                        {
                            sb.AppendLine("var SQL = \"" + SQLCallBack + "\"+ $(\"#" + ID + "\").val() + \" " + Condition + "\" + $(\"#" + ConditionValue + "\").val() + \" " + OrderBy + "\";");
                        }
                    }
                    else
                    {
                        //labelAlign = new string[] { "SQL = \"",  SQLCallBack, "\"+ $(\"#", base.ID, "\").val() + \" ", this.OrderBy, "\";" };                        
                        sb.AppendLine("var SQL=\"" + EncryAndDecry.Encryp(SQLCallBack) + "\"");
                    }
                    sb.AppendLine("var CboValue = $(this).val();");
                    sb.AppendLine("var CallBackFuncion =\"\"");
                    sb.AppendLine("LoadCallBackDataToCBO(\"" + CallBackToControlID + "\", SQL, \"" + CallBackDisplayValue + "\", \"" + CallBackDisplayText + "\", \"" + ShowAll + "\", CboValue, CallBackFuncion, \"" + SetValueToNextControlID + "\", \"" + SetValueToNextControl + "\", \"" + CallBackWithControlValueID + "\",\"" + ConditionFields + "\", \"" + ConditionFieldSearch2 + "\");");

                    //DIV, CboID, SQLStatement, Value, DisplayText, Event, ShowAll, SetSelectedValue, Encrypt, ReadOnly, IsAppend, IsUseConnection2, ConditionFielVale, ISCbo, ISMultiple, ISChosenStyle, IsCallBackToComboBox, ISComboboxCallback
                    //sb.AppendLine("LoadDataToComboBox(\"Div_" + IDCallBack + "\", \"" + IDCallBack + "\", SQL, \"" + EncryAndDecry.Encryp(this.CboCallbackValue) + "\", \"" + EncryAndDecry.Encryp(this.CboCallbackDisplayText) + "\", \"" + Cbo_CallBackEvent + "\", \"" + ShowAll + "\", \"" + CboValueComboCallBack + "\", \"" + Encrypt + "\", \"" + ReadOnlyComboCallBack + "\", false, false, \"" + ConditionField + "\", \"" + ISAutoCbo.ToString().ToLower() + "\", false, \"" + ISCboChoosen.ToString().ToLower() + "\", \"" + IsCallBackToComboBox.ToString().ToLower() + "\", \"" + ISComboboxCallback.ToString().ToLower() + "\");});");
                    sb.AppendLine("});");
                    //if (IsCallBackToComboBox)
                    //{
                    //    sb.AppendLine(string.Concat("$(\"#", IDCallBack, "\").change(function() {"));
                    //    sb.AppendLine("setTimeout(function(){");
                    //    sb.AppendLine("SearchBy_Change()");
                    //    sb.AppendLine("LoadTableGrid(1);");
                    //    sb.AppendLine("}, 300);");
                    //    sb.AppendLine("});");
                    //}
                    //sb.AppendLine(string.Concat(d));
                }
                sb.AppendLine("});</script>");
            }
            return sb.ToString();
        }
    }
}