using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Utilities;

namespace MVC
{
    public class KendoComboBoxColumns
    {
        public string field = "";
        public string title = "";
        public string width = "";

    }

    public class HtmlComboBox : HTMLControl
    {
        public int KendoMultiColumnComboBoxHeight = 0;
        public Dictionary<string, string> DisplayKendoComboBoxColumn = null;
        public List<KendoComboBoxColumns> KendoComboBoxColumn = null;
        public bool IsUseSelect2 = true;
        public string AddNewFuntionName = "";
        public bool ISCallBackToCbo = false;
        public string GroupByField = "";
        public string SetChangeEventOnControlID = "";
        public bool Infinity = true;
        public string CallBackFunction = "";

        private string _Cbo_CallBackEvent = "";

        private string _CboClass = "Round";

        private string _SetSelectedValue = "";

        private bool _IsAutoComboBox = true;

        private bool _IsCallBack;

        private bool _IsUseConnection2 = false;

        private string _SQL2 = "";

        //private string _ConditionField = "";

        private string _ConditionValue = "";

        private string _ConditionFieldDate = "";

        private string _ConditionDateValue = "";

        public bool AppendParent
        {
            get;
            set;
        }

        public string Cbo_CallBackEvent
        {
            get
            {
                return this._Cbo_CallBackEvent;
            }
            set
            {
                this._Cbo_CallBackEvent = value;
            }
        }

        public string CboClass
        {
            get
            {
                return this._CboClass;
            }
            set
            {
                this._CboClass = value;
            }
        }

        public string CboValueComboCallBack = "";

        public string CheckBoxName
        {
            get;
            set;
        }

        public string ConditionDateValue
        {
            get
            {
                return this._ConditionDateValue;
            }
            set
            {
                this._ConditionDateValue = value;
            }
        }

        public string ConditionFieldDate
        {
            get
            {
                return this._ConditionFieldDate;
            }
            set
            {
                this._ConditionFieldDate = value;
            }
        }

        public string ConditionValue
        {
            get
            {
                return this._ConditionValue;
            }
            set
            {
                this._ConditionValue = value;
            }
        }
        public bool DefaultValueNull = false;
        public string DisplayOnFields = "";

        public string DisplayText = "";

        public string DisplayTextCallBack = "";

        public string EventChange = "";

        public string FieldCondition = "";

        public string IDCallBack = "";

        public bool IsAutoComboBox
        {
            get
            {
                return this._IsAutoComboBox;
            }
            set
            {
                this._IsAutoComboBox = value;
            }
        }

        public bool IsCallBack
        {
            get
            {
                return this._IsCallBack;
            }
            set
            {
                this._IsCallBack = value;
            }
        }



        public bool ISDrawCheckBoxCallBack
        {
            get;
            set;
        }

        public bool IsUseConnection2
        {
            get
            {
                return this._IsUseConnection2;
            }
            set
            {
                this._IsUseConnection2 = value;
            }
        }

        public string OrderBy
        {
            get;
            set;
        }

        public bool ReadOnly
        {
            get;
            set;
        }

        public bool ReadOnlyComboCallBack
        {
            get;
            set;
        }

        public string SelectField
        {
            get;
            set;
        }

        public string SetSelectedValue
        {
            get
            {
                return this._SetSelectedValue;
            }
            set
            {
                this._SetSelectedValue = value;
            }
        }

        public string SQL_CBO = "";

        public string SQL2
        {
            get
            {
                return this._SQL2;
            }
            set
            {
                this._SQL2 = value;
            }
        }

        public string ValueCallBack
        {
            get;
            set;
        }

        public HtmlComboBox()
        {
            Type = "Combobox";
        }

        public override string GetHtml()
        {
            Common common = new Common();
            string IsHide = "";
            ID = ID == null ? "" : ID;
            StringBuilder sb = new StringBuilder();
            string CboResult = "";
            if (Hide)
            {
                IsHide = " style=\"display:none\"";
                Type = "hidden";
            }
            if (IsCustomForm)
            {
                if (DivClass == "")
                    DivClass = "wrap-input2";
                if (Label != "")
                {
                    placeholder = !string.IsNullOrEmpty(placeholder) ? placeholder : "Enter " + Label.Replace(":", "").ToLower();
                    sb.AppendLine("<div " + IsHide + " class=\"col-sm-auto\">");
                    sb.AppendLine("<label for=\"" + ID + "\" class=\"col-form-label khmerFont1\">" + Label + "</label></div>");
                }
                sb.AppendLine("<div " + IsHide + " class=\"" + DivClassCol + " align-text-bottom\">");
                //sb.AppendLine("<div style=\"border:none;\" class=\"" + DivClass + " " + (Required == true ? "validate-input" : "") + (Required == true && Value == "" ? " alert-validate" : "") + "\" " + (Required == true ? "data-validate=\"" + Label + " is required\"" : "") + ">");
                sb.AppendLine("<div style=\"border:none !important;\" class=\"" + DivClass + " " + (Required == true ? "validate-input" : "") + (Required == true && Value == "" ? " alert-validate" : "") + "\" " + (Required == true ? "data-validate=\"" + Label + " is required\"" : "") + ">");
                //if (GroupByField == "")
                //{
                //	CboResult = common.LoadDataToCBO(ID, EncryAndDecry.Encryp(SQL_CBO), EncryAndDecry.Encryp(Value), EncryAndDecry.Encryp(DisplayText), "", false, false, true, false, false, "", Multiple, this.ISCallBackToCbo);
                //	sb.Append(CboResult);
                //}
                //else
                //{
                HTML html = new HTML();
                sb.Append("<span " + (ID == "" ? "name=\"span_" + Name + "\"" : "id=\"span_" + ID + "\"") + "></span>");
                //}

                sb.AppendLine("<span class=\"focus-input2\"></span>");
                sb.AppendLine("</div></div>");
                if (!IsKendoMultiColumnComboBox)
                {
                    if (SQL != "" || GroupByField != "")
                    {
                        if (SQL == "") SQL = SQL_CBO;
                        sb.AppendLine("<script>$(function(){");
                        sb.AppendLine("Reload_" + ID + "();");
                        sb.AppendLine("});");
                        sb.AppendLine("function Reload_" + ID + "(CallbackFunction) {");
                        sb.AppendLine("CallbackFunction = typeof CallbackFunction ==\"undefined\"?\"\":CallbackFunction;");
                        sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL) + "\"}, function(response) {");
                        sb.AppendLine("if ($(\"#" + ID + "\").hasClass(\"select2-hidden-accessible\")) {");
                        sb.AppendLine(" $(\"#" + ID + "\").select2(\"destroy\");}");
                        sb.AppendLine("$(\"#" + ID + "\").remove();");
                        sb.AppendLine("$(\"#span_" + ID + "\").html(`<select style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + ID + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\"></select>`);");
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
                        sb.AppendLine("allowClear: true,");
                        sb.AppendLine("maximumSelectionSize: 1,");
                        sb.AppendLine("multiple: true,");
                        sb.AppendLine("theme: \"bootstrap4\",");
                        sb.AppendLine("data: objAllData_" + ID + ",");
                        sb.AppendLine("dropdownParent: $('.modal-content'),");
                        sb.AppendLine("placeholder: `" + placeholder + "`,");
                        sb.AppendLine("escapeMarkup: function(m) {");
                        sb.AppendLine("return m;");
                        sb.AppendLine("}");
                        sb.AppendLine("}).on('select2:open', () => {");
                        sb.AppendLine("$(\"span.select2-dropdown--below\").css({ \"border-top\": \"1px solid #80bdff\", \"border-top-right-radius\": \"0.25rem\", \"border-top-left-radius\": \"0.25rem\" });");
                        if (AddNewFuntionName != "")
                        {
                            sb.AppendLine("$(\".select2-results:not(:has(a))\").append(`<a class=\"btn btn-block btn-default btn-flat text-primary\" href=\"javascript:void(0);\" onclick=\"$('#" + ID + "').select2('close');" + AddNewFuntionName + "\" style=\"padding: 6px;height: 20px;display: inline-table;\"><i class=\"fa fa-plus\"></i> Add New</a>`);");
                        }
                        sb.Append("})");
                        sb.Append(".on('select2:close', function (e) {");
                        sb.AppendLine("if ($(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").length > 1)");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").eq(0).remove();");
                        sb.AppendLine("$(\".select2-selection__choice__remove\").remove();");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                        sb.Append("})");
                        sb.Append(".on('select2:select', function (e) {");
                        sb.AppendLine("let data_" + ID + " = e.params.data;");
                        sb.AppendLine("let select_2_" + ID + " = $select2Elm_" + ID + ".data('select2'),");
                        sb.AppendLine("$select2Input_" + ID + " = $('.select2-input', select_2_" + ID + ".searchContainer),");
                        sb.AppendLine("$tagToRemove_" + ID + " = $('li', select_2_" + ID + ".selection).eq(0),");
                        sb.AppendLine("newValue_" + ID + " = $.trim(data_" + ID + ".id);");
                        sb.AppendLine("$select2Input_" + ID + ".val(data_" + ID + ".id);");
                        sb.AppendLine("$select2Input_" + ID + ".trigger('keyup');");
                        sb.AppendLine("$select2Elm_" + ID + ".select2(\"val\", [data_" + ID + ".id, data_" + ID + ".text]);");
                        sb.AppendLine("$tagToRemove_" + ID + ".remove();");
                        sb.AppendLine("if ($(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").length > 1)");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").eq(0).remove();");
                        sb.AppendLine("$(\".select2-selection__choice__remove\").remove();");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                        sb.Append("})");
                        sb.Append(".on('change', function(e) {");
                        sb.AppendLine("}).data('select2').clearSearch = function() {");
                        sb.AppendLine("};");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"*\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"ul.select2-selection__rendered\").css({ \"border-bottom\": \"1px solid #adadad\" });");
                        if (CboValue != "")
                        {
                            sb.AppendLine("$('#" + ID + "').val('" + CboValue + "')");
                            sb.AppendLine("setTimeout(function(){");
                            sb.AppendLine("$('#" + ID + "').val('" + CboValue + "').trigger('change.select2');");
                            sb.AppendLine("$(\".select2-selection__choice__remove\").remove();");
                            sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                            sb.AppendLine("}, 300);");
                        }
                        sb.AppendLine("if(CallbackFunction!=\"\") CallbackFunction;");
                        sb.AppendLine("});");
                        sb.AppendLine("};");
                        sb.AppendLine("</script>");
                    }
                    else if (SQL_CBO != "")
                    {
                        sb.AppendLine("<script>$(function(){");
                        sb.AppendLine("Reload_" + ID + "();");
                        sb.AppendLine("});");
                        sb.AppendLine("function Reload_" + ID + "(CallbackFunction) {");
                        sb.AppendLine("CallbackFunction = typeof CallbackFunction ==\"undefined\"?\"\":CallbackFunction;");
                        sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_CBO) + "\"}, function(response) {");
                        //sb.AppendLine("if ($(\"#" + ID + "\").hasClass(\"select2-hidden-accessible\")) {");
                        //sb.AppendLine(" $(\"#" + ID + "\").select2(\"destroy\");}");
                        sb.AppendLine("$(\"#" + ID + "\").remove();");
                        sb.AppendLine("$(\"#span_" + ID + "\").html(`<select style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + ID + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\"></select>`);");
                        sb.AppendLine("var Data_" + ID + " = JSON.parse(JSON.parse(response).Data);");
                        sb.AppendLine("var objData = [];");
                        sb.AppendLine("var objAllData = [];");
                        sb.AppendLine("$.each(Data_" + ID + ", function (k, val) {");
                        sb.AppendLine("var obj = {};");
                        sb.AppendLine("obj[\"id\"] = val." + Value + ";");
                        sb.AppendLine("obj[\"text\"] = val." + DisplayText + ";");
                        sb.AppendLine("objData.push(obj);");
                        sb.AppendLine("});");

                        sb.AppendLine("var $select2Elm = $('#" + ID + "');");
                        sb.AppendLine("$select2Elm.select2({");
                        if (!Multiple)
                            sb.AppendLine("maximumSelectionSize: 1,");
                        sb.AppendLine("allowClear: true,");
                        sb.AppendLine("multiple: true,");
                        sb.AppendLine("theme: \"bootstrap4\",");
                        sb.AppendLine("data: objData,");
                        sb.AppendLine("dropdownParent: $(\"#" + ID + "\").parent(),");
                        sb.AppendLine("placeholder: `" + placeholder + "`,");
                        sb.AppendLine("escapeMarkup: function(m) {");
                        sb.AppendLine("return m;");
                        sb.AppendLine("}");
                        sb.AppendLine("}).on('select2:open', (e) => { e.preventDefault();");
                        sb.AppendLine("$(\"span.select2-dropdown--below\").css({ \"border-top\": \"1px solid #80bdff\", \"border-top-right-radius\": \"0.25rem\", \"border-top-left-radius\": \"0.25rem\" });");
                        if (AddNewFuntionName != "")
                        {
                            sb.AppendLine("$(\".select2-results:not(:has(a))\").append(`<a class=\"btn btn-block btn-default btn-flat text-primary\" href=\"javascript:void(0);\" onclick=\"$('#" + ID + "').select2('close');" + AddNewFuntionName + "\" style=\"padding: 6px;height: 20px;display: inline-table;\"><i class=\"fa fa-plus\"></i> Add New</a>`);");
                        }
                        sb.Append("})");
                        sb.Append(".on('select2:select', function (e) {");
                        if (!Multiple)
                        {
                            sb.AppendLine("var data = e.params.data;");
                            sb.AppendLine("var select2 = $select2Elm.data('select2'),");
                            sb.AppendLine("$select2Input = $('.select2-input', select2.searchContainer),");
                            sb.AppendLine("$tagToRemove = $('li', select2.selection).eq(0),");
                            sb.AppendLine("newValue = $.trim(data.text);");
                            sb.AppendLine("$select2Input.val(newValue);");
                            sb.AppendLine("$select2Input.trigger('keyup');");
                            sb.AppendLine("$select2Elm.val(newValue);");
                            sb.AppendLine("$tagToRemove.remove();");
                            sb.AppendLine("if ($(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").length > 1)");
                            sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").eq(0).remove();");
                            sb.AppendLine("$(\"#" + ID + "\").select2('data', { id:data.id, text: data.text});");
                            sb.AppendLine("$(\"#" + ID + "\").val(data.id).trigger('change');");
                            sb.AppendLine("$(\".select2-selection__choice__remove\").remove();");
                            sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");

                            sb.AppendLine("let dataID = data.id;");
                            if (DisplayOnFields != "" && !IsCallBack && !ISCallBackToCbo)
                            {
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(DisplayOnFields);
                                sb.AppendLine("let dt = Data_" + ID + ".filter(function (currentdata) {");
                                sb.AppendLine("return currentdata." + Value + " == dataID;");
                                sb.AppendLine("})[0];");
                                foreach (var kv in dict)
                                {
                                    sb.AppendLine("$(\"#" + kv.Key + "\").val(dt." + kv.Value + ");");
                                }
                                sb.AppendLine("if ($(\"#" + ID + "\").val()==\"\") { ");
                                foreach (var kv in dict)
                                {
                                    sb.AppendLine("$(\"#" + kv.Key + "\").val('');");
                                }
                                sb.AppendLine("}");
                            }
                        }
                        sb.Append("})");
                        sb.Append(".on('change', function(e) {");
                        if (SQLCallBack != "" && CallBackToControlID != "")
                        {
                            //sb.AppendLine("$(\"#" + ID + "\").bind(\"change\",function() {");
                            sb.AppendLine("var CboValue = $(this).val();");
                            sb.AppendLine("LoadCallBackDataToCBO(\"" + CallBackToControlID + "\", `" + SQLCallBack + "`, \"" + CallBackDisplayValue + "\", \"" + CallBackDisplayText + "\", \"false\", CboValue, \"\", \"" + SetValueToNextControlID + "\", \"" + SetValueToNextControl + "\", \"" + CallBackWithControlValueID + "\");");
                            //sb.AppendLine("});");
                        }
                        else if (IsCallBack)
                        {
                            sb.AppendLine("LoadCallBackData(\"" + ID + "\", \"" + EncryAndDecry.Encryp(this.SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + SQL2 + "\",\"\",\"true\",\"\",\"\",\"\",\"" + CallBackFunction + "\",\"" + CallBackWithControlValueID + "\");");
                        }
                        sb.AppendLine("}).data('select2').clearSearch = function() {");
                        sb.AppendLine("return false;");
                        sb.AppendLine("};");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"*\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"ul.select2-selection__rendered\").css({ \"border-bottom\": \"1px solid #adadad\" });");
                        if (CboValue == "" && !Multiple)
                        {
                            sb.AppendLine("setTimeout(function(){");
                            sb.AppendLine("$('#" + ID + "').val('0').trigger('change.select2');");
                            sb.AppendLine("}, 300);");
                        }
                        else if (CboValue != "" && Multiple)
                        {
                            sb.AppendLine("$('#" + ID + "').val([" + CboValue + "])");
                            sb.AppendLine("setTimeout(function(){");
                            sb.AppendLine("$('#" + ID + "').val([" + CboValue + "]).trigger('change.select2');");
                            //sb.AppendLine("$(\".select2-selection__choice__remove\").remove();");
                            //sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                            sb.AppendLine("}, 300);");
                        }
                        else if (CboValue != "" && !Multiple)
                        {
                            sb.AppendLine("$('#" + ID + "').val('" + CboValue + "')");
                            sb.AppendLine("setTimeout(function(){");
                            sb.AppendLine("$('#" + ID + "').val('" + CboValue + "').trigger('change.select2');");
                            sb.AppendLine("}, 300);");
                        }
                        sb.AppendLine("if(CallbackFunction!=\"\") CallbackFunction();");
                        sb.AppendLine("})};");
                        sb.AppendLine("</script>");
                    }
                    else
                    {
                        sb.AppendLine("<script>$(function(){");
                        sb.AppendLine("$(\"#" + ID + "\").select2(\"destroy\");");
                        sb.AppendLine("$(\"#" + ID + "\").remove();");
                        sb.AppendLine("$(\"#span_" + ID + "\").html(`<select style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + ID + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\"></select>`);");

                        sb.AppendLine("var $select2Elm = $('#" + ID + "');");
                        sb.AppendLine("$select2Elm.select2({");
                        if (!Multiple)
                            sb.AppendLine("maximumSelectionSize: 1,");
                        sb.AppendLine("multiple: true,");
                        sb.AppendLine("allowClear: true,");
                        sb.AppendLine("theme: \"bootstrap4\",");
                        //sb.AppendLine("data: objData,");
                        sb.AppendLine("dropdownParent: $('.modal-content'),");
                        sb.AppendLine("placeholder: `" + placeholder + "`,");
                        sb.AppendLine("escapeMarkup: function(m) {");
                        sb.AppendLine("return m;");
                        sb.AppendLine("}");
                        sb.AppendLine("}).on('select2:open', () => {");
                        sb.AppendLine("$(\"span.select2-dropdown--below\").css({ \"border-top\": \"1px solid #80bdff\", \"border-top-right-radius\": \"0.25rem\", \"border-top-left-radius\": \"0.25rem\" });");
                        sb.Append("})");
                        sb.Append(".on('select2:select', function (e) {");
                        if (!Multiple)
                        {
                            sb.AppendLine("var data = e.params.data;");
                            sb.AppendLine("var select2 = $select2Elm.data('select2'),");
                            sb.AppendLine("$select2Input = $('.select2-input', select2.searchContainer),");
                            sb.AppendLine("$tagToRemove = $('li', select2.selection).eq(0),");
                            sb.AppendLine("newValue = $.trim(data.text);");
                            sb.AppendLine("$select2Input.val(newValue);");
                            sb.AppendLine("$select2Input.trigger('keyup');");
                            sb.AppendLine("$select2Elm.val(newValue);");
                            sb.AppendLine("$tagToRemove.remove();");
                            sb.AppendLine("if ($(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").length > 1)");
                            sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").eq(0).remove();");
                            sb.AppendLine("$(\"#" + ID + "\").select2('data', { id:data.id, text: data.text});");
                            sb.AppendLine("$(\"#" + ID + "\").val(data.id).trigger('change');");
                            sb.AppendLine("$(\".select2-selection__choice__remove\").remove();");
                            sb.AppendLine("$(\"#" + ID + "\").parent().find(\"li.select2-selection__choice\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                        }
                        sb.Append("})");
                        sb.Append(".on('change', function(e) {");
                        sb.AppendLine("}).data('select2').clearSearch = function() {");
                        sb.AppendLine("return false;");
                        sb.AppendLine("};");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"*\").css({ \"border\": \"none\", \"padding\": \"0\", \"margin\": \"0\" });");
                        sb.AppendLine("$(\"#" + ID + "\").parent().find(\"ul.select2-selection__rendered\").css({ \"border-bottom\": \"1px solid #adadad\" });");

                        sb.AppendLine("});");
                        sb.AppendLine("</script>");
                    }
                }
                else
                {
                    sb.AppendLine("<script>");
                    sb.AppendLine("$(function(){");
                    sb.AppendLine("setTimeout(function(){");
                    sb.AppendLine("Reload_" + (ID == "" ? Name : ID) + "();");
                    sb.AppendLine("}, 1000);");
                    sb.AppendLine("});");
                    sb.AppendLine("function Reload_" + (ID == "" ? Name : ID) + "(index) {");
                    sb.AppendLine("index = (typeof index == 'undefined' ? \"0\" : index);");
                    //sb.AppendLine("console.log(index);");
                    sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_CBO) + "\"}, function(response) {");
                    sb.AppendLine("var dataSource_" + (ID == "" ? Name : ID) + " = JSON.parse(JSON.parse(response).Data);");
                    sb.AppendLine("dataSource_" + (ID == "" ? Name : ID) + ".forEach(function(item) { for (var key in item) { if (typeof item[key] === 'string') { item[key] = $('<textarea/>').html(item[key]).text(); } } });");
                    if (ID == "")
                    {
                        sb.AppendLine("$(\"" + (ID == "" ? "span[name=span_" + Name + "]" : "#span_" + ID) + "\").eq(index).html('<input style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + (ID == "" ? Name : ID) + "\" data-validate=\"" + (!Required ? "" : Label.ToString() == "" ? ID == "" ? Name : ID : Label.Replace(":", "").ToString() + " is required.") + "\" />');");
                        //sb.AppendLine("setTimeout(function(){");
                        if (KendoComboBoxColumn != null)
                            sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").eq(index).kendoMultiColumnComboBox({");
                        else
                            sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").eq(index).kendoComboBox({");
                    }
                    else
                    {
                        sb.AppendLine("$(\"" + (ID == "" ? "span[name=span_" + Name + "]" : "#span_" + ID) + "\").html('<input style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + (ID == "" ? Name : ID) + "\" data-validate=\"" + (!Required ? "" : Label.ToString() == "" ? ID == "" ? Name : ID : Label.Replace(":", "").ToString() + " is required.") + "\" />');");
                        //sb.AppendLine("setTimeout(function(){");
                        if (KendoComboBoxColumn != null)
                            sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").kendoMultiColumnComboBox({");
                        else
                            sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").kendoComboBox({");
                    }
                    if (KendoMultiColumnComboBoxHeight != 0)
                        sb.AppendLine("height:" + KendoMultiColumnComboBoxHeight + ",");
                    sb.AppendLine("filter: \"contains\", autoBind: true, dataTextField: \"" + DisplayText + "\", dataValueField: \"" + Value + "\",");
                    sb.AppendLine("dataSource: {data: dataSource_" + (ID == "" ? Name : ID) + ",");
                    if (GroupByField != "")
                        sb.AppendLine("group: { field: \"" + GroupByField + "\" }");
                    sb.AppendLine("},");
                    if (CallBackFunction != "")
                    {
                        sb.AppendLine("change: function(e) {");
                        sb.AppendLine(CallBackFunction);
                        sb.AppendLine("},");
                    }
                    if (AddNewFuntionName != "")
                    {
                        sb.AppendLine("footerTemplate: `\r\n<div style=\"padding:8px;text-align:center;\">\r\n<button type=\"button\" class=\"k-button k-primary\" id=\"btnAddNewFooter\">\r\n+ Add New\r\n</button>\r\n</div>\r\n`,");
                        sb.AppendLine("open: function(e) {");
                        sb.AppendLine("var combo = this;");
                        sb.AppendLine("$(\"#btnAddNewFooter\").off(\"click\").on(\"click\", function (e) { e.preventDefault(); e.stopPropagation(); combo.close(); " + AddNewFuntionName + " });");
                        sb.AppendLine("},");
                    }
                    ;
                    sb.AppendLine("columns: [");
                    string filters = "";
                    if (KendoComboBoxColumn != null)
                    {
                        filters = "filters: [";
                        foreach (var item in KendoComboBoxColumn.ToList())
                        {
                            filters += "{field: \""+ item.field + "\", operator: \"contains\", value: filterValue },";
                            sb.AppendLine("{ field: \"" + item.field + "\", title: \"" + item.title + "\", width : \"" + item.width + "\" },");
                        }
                        filters += "]";
                    }
                    sb.AppendLine("],");
                    var str = @"
filtering: function (e) {
    var combo = this;

    var filterValue = e.filter
        ? e.filter.value
        : """";

    // Prevent Kendo from applying its default single-column filter.
    e.preventDefault();

    // If the search box is empty, show all accounts again.
    if (!filterValue || filterValue.toString().trim() === """") {
        combo.dataSource.filter({});
        return;
    }

    filterValue = filterValue.toString().trim();

    // Filter by account code OR account name.
    combo.dataSource.filter({
        logic: ""or"",
        " + filters + @"
    });
}";
                    sb.AppendLine(str);
                    sb.AppendLine("});");

                    if (CboValue != "")
                    {
                        if (ID == "")
                        {
                            if (KendoComboBoxColumn != null)
                                sb.AppendLine("var multicolumncombobox = $(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").eq(index).data(\"kendoMultiColumnComboBox\");\r\nmulticolumncombobox.value(\"" + CboValue + "\");\r\nmulticolumncombobox.trigger(\"change\");");
                            else
                                sb.AppendLine("var multicolumncombobox = $(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").eq(index).data(\"kendoComboBox\");\r\nmulticolumncombobox.value(\"" + CboValue + "\");\r\nmulticolumncombobox.trigger(\"change\");");
                        }
                        else
                        {
                            if (KendoComboBoxColumn != null)
                            {
                                sb.AppendLine("var multicolumncombobox = $(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").data(\"kendoMultiColumnComboBox\");");

                                if (ConditionField == "")
                                {
                                    sb.AppendLine("multicolumncombobox.value(\"" + CboValue + "\");\r\nmulticolumncombobox.trigger(\"change\");");
                                }
                                else
                                {
                                    sb.AppendLine("var " + ID + "_Item = dataSource_" + ID + ".find(function(item) { return item.Type === \"" + ConditionField + "\" && item.ID === " + CboValue + "; });");
                                    sb.AppendLine(@"if (" + ID + @"_Item) {
var itemIndex = dataSource_" + ID + ".indexOf(" + ID + @"_Item);
multicolumncombobox.select(itemIndex);
//multicolumncombobox.value(" + ID + @"_Item.ID);
}");
                                }

                            }
                            else
                                sb.AppendLine("var multicolumncombobox = $(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").data(\"kendoComboBox\");\r\nmulticolumncombobox.value(\"" + CboValue + "\");\r\nmulticolumncombobox.trigger(\"change\");");
                        }
                        //sb.AppendLine("$('#" + ID + "').val('" + CboValue + "')");
                        //sb.AppendLine("setTimeout(function(){");
                        //sb.AppendLine("$('#" + ID + "').val('" + CboValue + "').trigger('change');");
                        //sb.AppendLine("}, 300);");
                    }
                    //sb.AppendLine("var comboBox = $(\"" + (ID == "" ? "input[name=" + Name+"]" : "#"+ID) + "\").data(\"kendoMultiColumnComboBox\"); comboBox.list.width(\"auto\");");
                    if (ID == "")
                        sb.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "_input]\").eq(index).click(function(e) { e.preventDefault(); $(this).parent().find(\"button.k-input-button\").click(); });");
                    else
                        sb.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "_input]\").click(function(e) { e.preventDefault(); $(this).parent().find(\"button.k-input-button\").click(); });");

                    //
                    if (ReadOnly)
                    {
                        if (KendoComboBoxColumn != null)
                        {
                            sb.AppendLine(
                                "var multicolumncomboboxs = $(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").eq(index).data(\"kendoMultiColumnComboBox\");\r\n" +
                                (ReadOnly ? "multicolumncomboboxs.readonly(true);\r\n" : "")
                            );
                        }
                        else
                        {
                            sb.AppendLine(
                                "var comboboxs = $(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").eq(index).data(\"kendoComboBox\");\r\n" +
                                (ReadOnly ? "comboboxs.readonly(true);\r\n" : "")
                            );
                        }
                    }
                    //sb.AppendLine("}, 1500);");
                    sb.AppendLine("});");
                    sb.AppendLine("};");
                    sb.AppendLine("</script>");
                }
            }
            else
            {
                if (!UseTable) sb.Append("<tr>");
                if (!ISFloatingLabel)
                {
                    sb.Append("<td " + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label class=\"col-form-label text-nowrap\" for=\"" + ID + "\">" + Label + "</label></td>");
                    sb.Append("<td " + IsHide + " valign=\"" + LabelValign + "\">");
                    if (!IsKendoMultiColumnComboBox)
                    {
                        CboResult = common.LoadDataToCBO(ID, EncryAndDecry.Encryp(SQL_CBO), EncryAndDecry.Encryp(Value), EncryAndDecry.Encryp(DisplayText), "", false, false, true, false, false, "", Multiple, ISCallBackToCbo, Class, placeholder);
                        sb.Append("<span id=\"span_" + ID + "\">" + CboResult + "</span></td>");
                    }
                    else
                    {
                        sb.Append("<select id=\"" + ID + "\"></select></td>");
                    }
                }
                else
                {
                    sb.Append("<td " + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"></td>");
                    sb.Append("<td " + IsHide + " valign=\"" + LabelValign + "\">");
                    if (IsUseSelect2 && GroupByField == "")
                        CboResult = common.LoadDataToCBO(ID, EncryAndDecry.Encryp(SQL_CBO), EncryAndDecry.Encryp(Value), EncryAndDecry.Encryp(DisplayText), "", false, false, true, false, false, "", Multiple, ISCallBackToCbo, Class, placeholder);
                    else if (!IsUseSelect2)
                        CboResult = common.LoadDataToCBO(ID, EncryAndDecry.Encryp(SQL_CBO), EncryAndDecry.Encryp(Value), EncryAndDecry.Encryp(DisplayText), "", false, false, true, false, false, "", Multiple, ISCallBackToCbo, Class, placeholder);
                    sb.Append("<span id=\"span_" + ID + "\"><div class=\"mb-3 floating-label\">" + CboResult + "<label for=\"" + ID + "\">" + Label + "</label></div></span>");
                    sb.Append("</td>");
                }
                if (!UseTable) sb.Append("</tr>");

                sb.AppendLine("<script>$(function(){");
                if (Required)
                    sb.AppendLine("$(\"#" + ID + "\").attr(\"required\", \"true\");");
                if (IsUseSelect2)
                {
                    if (!IsKendoMultiColumnComboBox)
                    {
                        if (GroupByField != "")
                        {
                            sb.AppendLine("Reload_" + ID + "();");
                            sb.AppendLine("});");
                            sb.AppendLine("function Reload_" + ID + "() {");
                            sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_CBO) + "\"}, function(response) {");
                            sb.AppendLine("if ($(\"#" + ID + "\").hasClass(\"select2-hidden-accessible\")) {");
                            sb.AppendLine(" $(\"#" + ID + "\").select2(\"destroy\");}");
                            sb.AppendLine("$(\"#" + ID + "\").remove();");
                            sb.AppendLine("$(\"#span_" + ID + "\").html(`<select style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + ID + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\"></select>`);");
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
                            string[] strval = Value.Split(',');
                            if (strval.Length > 1)
                            {
                                Value = strval[1];
                                sb.AppendLine("obj_" + ID + "[\"parentId\"] = val." + strval[0] + ";");
                                sb.AppendLine("obj_" + ID + "[\"parentField\"] = '" + EncryAndDecry.Encryp(strval[0]) + "';");
                            }
                            else
                            {
                                sb.AppendLine("obj_" + ID + "[\"parentField\"] = '';");
                                sb.AppendLine("obj_" + ID + "[\"parentId\"] = 0;");
                            }

                            sb.AppendLine("obj_" + ID + "[\"id\"] = val." + Value + ";");
                            sb.AppendLine("obj_" + ID + "[\"text\"] = val." + DisplayText + ";");
                            sb.AppendLine("objData_" + ID + ".push(obj_" + ID + ");");
                            sb.AppendLine("});");
                            sb.AppendLine("objAllData_" + ID + ".push({");
                            if (strval.Length > 1)
                            {
                                sb.AppendLine("'parentField': '" + EncryAndDecry.Encryp(strval[0]) + "',");
                                sb.AppendLine("'parentId': value." + strval[0] + ",");
                            }
                            else
                            {
                                sb.AppendLine("'parentField': '',");
                                sb.AppendLine("'parentId': 0,");
                            }
                            sb.AppendLine("'text': group1_" + ID + ",");
                            sb.AppendLine("'children': objData_" + ID + "");
                            sb.AppendLine("});");
                            sb.AppendLine("}");
                            sb.AppendLine("objData_" + ID + " = [];");
                            sb.AppendLine("group2_" + ID + " = value." + GroupByField + ";");
                            sb.AppendLine("});");

                            sb.AppendLine("let $select2Elm_" + ID + " = $('#" + ID + "');");
                            sb.AppendLine("$select2Elm_" + ID + ".select2({");
                            sb.AppendLine("allowClear: true,");
                            sb.AppendLine("maximumSelectionSize: 1,");
                            sb.AppendLine("theme: \"bootstrap4\",");
                            sb.AppendLine("data: objAllData_" + ID + ",");
                            sb.AppendLine("dropdownParent: $('.modal-content'),");
                            sb.AppendLine("placeholder: `" + placeholder + "`,");
                            sb.AppendLine("escapeMarkup: function(m) {");
                            sb.AppendLine("return m;");
                            sb.AppendLine("}");
                            sb.AppendLine("}).on('select2:open', () => {");
                            if (AddNewFuntionName != "")
                            {
                                sb.AppendLine("$(\".select2-results:not(:has(a))\").append(`<a class=\"btn btn-block btn-default btn-flat text-primary\" href=\"javascript:void(0);\" onclick=\"$('#" + ID + "').select2('close');" + AddNewFuntionName + "\" style=\"padding: 6px;height: 20px;display: inline-table;\"><i class=\"fa fa-plus\"></i> Add New</a>`);");
                            }
                            sb.Append("})");
                            sb.Append(".on('select2:close', function (e) {");
                            sb.Append("})");
                            sb.Append(".on('select2:select', function (e) {");
                            if (EventChange != "")
                            {
                                sb.AppendLine(EventChange);

                            }
                            sb.Append("})");
                            sb.Append(".on('change', function(e) {");
                            //if (EventChange != "")
                            //{
                            //	sb.AppendLine(EventChange);

                            //}
                            sb.AppendLine("}).data('select2').clearSearch = function() {");
                            sb.AppendLine("};");
                            if (CboValue != "")
                            {
                                sb.AppendLine("$('#" + ID + "').val('" + CboValue + "')");
                                sb.AppendLine("setTimeout(function(){");
                                sb.AppendLine("$('#" + ID + "').val('" + CboValue + "').trigger('change.select2');");
                                sb.AppendLine("}, 300);");
                            }
                            if (Required)
                                sb.AppendLine("$(\"#" + ID + "\").attr(\"required\", \"true\");");
                            sb.AppendLine("});");
                            sb.AppendLine("};");
                        }
                        else
                        {
                            sb.AppendLine("Reload_" + ID + "();");
                            sb.AppendLine("});");
                            sb.AppendLine("function Reload_" + ID + "() {");
                            sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_CBO) + "\"}, function(response) {");
                            //sb.AppendLine(" $(\"#" + ID + "\").select2(\"destroy\");");
                            sb.AppendLine("$(\"#" + ID + "\").remove();");
                            sb.AppendLine("$(\"#span_" + ID + "\").html(`<select style=\"width:100%\" data-placeholder=\"" + placeholder + "\" id=\"" + ID + "\" name=\"" + ID + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\"></select>`);");
                            sb.AppendLine("var Data_" + ID + " = JSON.parse(JSON.parse(response).Data);");
                            sb.AppendLine("var objData = [];");
                            if (placeholder != "")
                                sb.AppendLine("objData.push({id:\"\", text:\"" + placeholder + "\"});");
                            sb.AppendLine("$.each(Data_" + ID + ", function (k, val) {");
                            sb.AppendLine("var obj = {};");
                            sb.AppendLine("obj[\"id\"] = val." + Value + ";");
                            sb.AppendLine("obj[\"text\"] = val." + DisplayText + ";");
                            sb.AppendLine("objData.push(obj);");
                            sb.AppendLine("});");

                            sb.AppendLine("var $select2Elm = $('#" + ID + "');");
                            sb.AppendLine("$select2Elm.select2({");
                            if (Multiple)
                                sb.AppendLine("multiple: true,");
                            sb.AppendLine("allowClear: true,");
                            sb.AppendLine("theme: \"bootstrap4\",");
                            sb.AppendLine("data: objData,");
                            //sb.AppendLine("dropdownParent: $('.modal-content'),");
                            sb.AppendLine("placeholder: `" + placeholder + "`,");
                            sb.AppendLine("escapeMarkup: function(m) {");
                            sb.AppendLine("return m;");
                            sb.AppendLine("}");
                            sb.AppendLine("})");
                            sb.Append(".on('change', function(e) {");
                            if (EventChange != "")
                            {
                                sb.AppendLine(EventChange);

                            }
                            else if (SQLCallBack != "" && CallBackToControlID != "")
                            {
                                //sb.AppendLine("$(\"#" + ID + "\").bind(\"change\",function() {");
                                sb.AppendLine("var CboValue = $(this).val();");
                                sb.AppendLine("LoadCallBackDataToCBO(\"" + CallBackToControlID + "\", `" + SQLCallBack + "`, \"" + CallBackDisplayValue + "\", \"" + CallBackDisplayText + "\", \"false\", CboValue, \"\", \"" + SetValueToNextControlID + "\", \"" + SetValueToNextControl + "\", \"" + CallBackWithControlValueID + "\");");
                                //sb.AppendLine("});");
                            }
                            else if (IsCallBack)
                            {
                                sb.AppendLine("LoadCallBackData(\"" + ID + "\", \"" + EncryAndDecry.Encryp(this.SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + SQL2 + "\",\"\",\"true\",\"\",\"\",\"\",\"" + CallBackFunction + "\",\"" + CallBackWithControlValueID + "\");");
                            }
                            sb.AppendLine("})");

                            if (CboValue != "")
                            {
                                sb.AppendLine("var selected = \"" + CboValue + "\";");
                                sb.AppendLine("var selectedArray = selected.split(',');");

                                //sb.AppendLine("$('#" + ID + "').val(selectedArray)");
                                //sb.AppendLine("$('#" + ID + "').val('" + CboValue + "')");
                                sb.AppendLine("setTimeout(function(){");
                                sb.AppendLine("$('#" + ID + "').val(selectedArray).trigger('change.select2');");
                                if (EventChange != "")
                                {
                                    EventChange = EventChange.Replace("this", "$('#" + ID + "')");
                                    sb.AppendLine(EventChange);

                                }
                                sb.AppendLine("}, 300);");
                            }
                            sb.AppendLine("})};");
                        }
                    }
                    else
                    {
                        sb.AppendLine("Reload_" + ID + "();");
                        sb.AppendLine("});");
                        sb.AppendLine("function Reload_" + ID + "() {");
                        sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_CBO) + "\"}, function(response) {");
                        sb.AppendLine("var dataSource_" + ID + " = JSON.parse(JSON.parse(response).Data);");
                        if (KendoComboBoxColumn != null)
                            sb.AppendLine("$(\"#" + ID + "\").kendoMultiColumnComboBox({");
                        else
                            sb.AppendLine("$(\"#" + ID + "\").kendoComboBox({");
                        if (KendoMultiColumnComboBoxHeight != 0)
                            sb.AppendLine("height:" + KendoMultiColumnComboBoxHeight + ",");
                        sb.AppendLine("filter: \"contains\", dataTextField: \"" + DisplayText + "\", dataValueField: \"" + Value + "\",");
                        sb.AppendLine("dataSource: {data: dataSource_" + ID + ",");
                        if (GroupByField != "")
                            sb.AppendLine("group: { field: \"" + GroupByField + "\" }");
                        sb.AppendLine("},");
                        sb.AppendLine("columns: [");
                        if (KendoComboBoxColumn != null)
                        {
                            foreach (var item in KendoComboBoxColumn.ToList())
                            {
                                sb.AppendLine("{ field: \"" + item.field + "\", title: \"" + item.title + "\", width : \"" + item.width + "\" },");
                            }
                        }
                        sb.AppendLine("]});");

                        if (CboValue != "")
                        {
                            if (KendoComboBoxColumn != null)
                                sb.AppendLine("var multicolumncombobox = $(\"#" + ID + "\").data(\"kendoMultiColumnComboBox\");\r\nmulticolumncombobox.value(\"" + CboValue + "\");\r\nmulticolumncombobox.trigger(\"change\");");
                            else
                                sb.AppendLine("var multicolumncombobox = $(\"#" + ID + "\").data(\"kendoComboBox\");\r\nmulticolumncombobox.value(\"" + CboValue + "\");\r\nmulticolumncombobox.trigger(\"change\");");
                            //sb.AppendLine("$('#" + ID + "').val('" + CboValue + "')");
                            //sb.AppendLine("setTimeout(function(){");
                            //sb.AppendLine("$('#" + ID + "').val('" + CboValue + "').trigger('change');");
                            //sb.AppendLine("}, 300);");
                        }
                        if (KendoComboBoxColumn != null)
                            sb.AppendLine("var comboBox = $(\"#" + ID + "\").data(\"kendoMultiColumnComboBox\"); comboBox.list.width(\"auto\");");
                        else
                            sb.AppendLine("var comboBox = $(\"#" + ID + "\").data(\"kendoComboBox\"); comboBox.list.width(\"auto\");");
                        sb.AppendLine("$(\"input[aria-controls=" + ID + "_listbox]\").click(function(e) { e.preventDefault(); $(this).parent().find(\"button.k-input-button\").click(); });");

                        sb.AppendLine("});");
                        sb.AppendLine("};");
                    }
                }
                else
                    sb.AppendLine("});");
                sb.AppendLine("</script>");
            }

            if (ISCallBackToCbo)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function() {");
                if (SetChangeEventOnControlID != "")
                    sb.AppendLine("var CallBackFuncion = function() { $(\"#" + SetChangeEventOnControlID + "\").change(); };");
                else if (CboValueComboCallBack != "")
                    sb.AppendLine("var CallBackFuncion = function() { $(\"#" + CallBackToControlID + "\").val('" + CboValueComboCallBack + "').trigger('change'); };");
                else
                    sb.AppendLine("var CallBackFuncion =\"\"");
                sb.AppendLine("$(\"#" + ID + "\").bind(\"change\",function() {");
                sb.AppendLine("var CboValue = $(this).val();");
                sb.AppendLine("LoadCallBackDataToCBO(\"" + CallBackToControlID + "\", `" + SQLCallBack + "`, \"" + CallBackDisplayValue + "\", \"" + CallBackDisplayText + "\", \"false\", CboValue, CallBackFuncion, \"" + SetValueToNextControlID + "\", \"" + SetValueToNextControl + "\", \"" + CallBackWithControlValueID + "\");");
                sb.AppendLine("});");
                CboValue = CboValue == null ? "" : CboValue.ToString();
                if (CboValue != "")
                {
                    sb.AppendLine("setTimeout(function(){");
                    sb.AppendLine("$(\"#" + ID + "\").val('" + CboValue + "');");
                    sb.AppendLine("$(\"#" + ID + "\").change();");
                    sb.AppendLine("}, 1200);");
                }
                sb.AppendLine("});</script>");
            }
            if (!IsCustomForm && IsCallBack)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function() {");
                if (ConditionField != "" && ConditionValue != "")
                {
                    string Condition = " AND " + ConditionField + " = N'" + "\" + $(\"#" + ConditionValue + "\").val() + \"" + "'";
                    SQLCallBack += Condition;
                    sb.AppendLine("$(\"#" + ID + "\").change(function(){LoadCallBackData(\"" + ID + "\", \"" + SQLCallBack + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + SQL2 + "\", \"\", \"false\", \"\",\"\",\"\"," + CallBackFunction + ",\"" + CallBackWithControlValueID + "\");});");
                }
                else
                {
                    if (CallBackFunction != "")
                        sb.AppendLine("$(\"#" + ID + "\").change(function(){LoadCallBackData(\"" + ID + "\", \"" + EncryAndDecry.Encryp(this.SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + SQL2 + "\",\"\",\"true\",\"\",\"\",\"\"," + CallBackFunction + ",\"" + CallBackWithControlValueID + "\");});");
                    else
                        sb.AppendLine("$(\"#" + ID + "\").change(function(){LoadCallBackData(\"" + ID + "\", \"" + EncryAndDecry.Encryp(this.SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + SQL2 + "\",\"\",\"true\",\"\",\"\",\"\",\"" + CallBackFunction + "\",\"" + CallBackWithControlValueID + "\");});");
                }
                sb.AppendLine("$(\"#" + ID + "\").change();");
                sb.AppendLine("});</script>");
            }
            return sb.ToString();
        }
    }
}