using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Utilities;

namespace MVC
{
    public class HtmlTextBox : HTMLControl
    {
        /// <summary>Text field name used for binding/display.</summary>
        public string DataTextField = "";

        /// <summary>Value field name used for binding.</summary>
        public string DataValueField = "";

        /// <summary>Indicates whether Kendo AutoComplete is enabled.</summary>
        public bool IsKendoAutoComplete = false;

        /// <summary>Field used to group items.</summary>
        public string GroupByField = "";

        /// <summary>Determines if display field is shown with AutoComplete.</summary>
        public bool IsDisplayOnFieldWithAutoComplete = true;

        /// <summary>Key/value mapping for replacing JSON keys.</summary>
        public Dictionary<string, string> ReplaceJsonKey = null;

        /// <summary>Indicates whether data should be loaded from an API.</summary>
        public bool IsGetDataFromAPI = false;

        /// <summary>Restrict date range picker by number of days.</summary>
        public int LimitDateRangePickerByDay = 0;

        /// <summary>Default to today's date.</summary>
        public bool ToDay = true;

        /// <summary>Whether a default date value should be set.</summary>
        public bool DefaultValueDate = true;

        /// <summary>Whether to allow description input.</summary>
        public bool InputDescription = false;

        /// <summary>Map URL for location fields.</summary>
        public string MapURL = "";

        /// <summary>Callback function for Typeahead.</summary>
        public string TypeaheadCallBackFunction = "";

        /// <summary>SQL query used for Typeahead callback.</summary>
        public string SQLTypeaheadCallBack = "";

        /// <summary>Comma-separated fields displayed in Typeahead.</summary>
        public string DisplayOnFieldsTypeahead = "";

        /// <summary>Latitude field for geolocation input.</summary>
        public string InputLatitude = "";

        /// <summary>Longitude field for geolocation input.</summary>
        public string InputLongitude = "";

        /// <summary>JavaScript function called on change event.</summary>
        public string OnChangeFunction = "";

        /// <summary>JavaScript function called on keyup event.</summary>
        public string OnKeyupFunction = "";

        /// <summary>JavaScript function called when add button is clicked.</summary>
        public string AddButtonFunction = "";

        /// <summary>Enables a time picker control.</summary>
        public bool TimePicker = false;

        /// <summary>Show default date value in date picker.</summary>
        public bool DefaultShowDateValue = true;

        /// <summary>Field name if database field is not set.</summary>
        public string FieldName = "";

        /// <summary>Whether the checkbox is checked by default.</summary>
        public bool DefaultCheckBoxChecked = false;

        /// <summary>Value for hidden field.</summary>
        public string HiddenFieldValue = "";

        /// <summary>Group by field used in Typeahead.</summary>
        public string TypeaheadGroupByField = "";

        /// <summary>SQL prefix for dynamic query building.</summary>
        public string SQLPreFix = "";

        /// <summary>JavaScript function to delete table row.</summary>
        public string DeleteTDRowFunction = "";

        /// <summary>Indicates whether to apply Typeahead on callback.</summary>
        public bool isApplyTypeaheadOnCallBack = false;

        /// <summary>Extra label displayed with the input.</summary>
        public string ExLabel = "";

        /// <summary>Flag for using secondary database connection (private).</summary>
        //private bool _IsUseConnection2 = false;

        /// <summary>Indicates whether callback should update combo box.</summary>
        public bool ISCallBackToCbo = false;

        /// <summary>Control ID that triggers change event.</summary>
        public string SetChangeEventOnControlID = "";

        /// <summary>JavaScript callback function name.</summary>
        public string CallBackFunction = "";

        /// <summary>Indicates whether value is a URL.</summary>
        public bool isURL = false;

        /// <summary>Field that this control must be equal to.</summary>
        public string EqualTo = "";

        /// <summary>ID or name of the control.</summary>
        public string ControlNameID = "";

        /// <summary>Fields to display with this control.</summary>
        public string DisplayOnFields = "";

        /// <summary>Event change script or handler.</summary>
        public string EventChange = "";

        /// <summary>Condition applied to this field.</summary>
        public string FieldCondition { get; set; }

        /// <summary>Enables AutoComplete.</summary>
        public bool ISAutoComplete = false;

        /// <summary>Whether callback is enabled.</summary>
        public bool ISCallBack { get; set; }

        /// <summary>Whether the control is read-only.</summary>
        public bool IsReadOnly { get; set; }

        /// <summary>Whether the control is read-only.</summary>
        public bool IsDisabled { get; set; }

        /// <summary>Whether to use secondary connection.</summary>
        public bool IsUseConnection2 = false;

        /// <summary>Field(s) selected from database.</summary>
        public string SelectField { get; set; }

        /// <summary>SQL query used for AutoComplete.</summary>
        public string SQL_AutoComplete = "";

        /// <summary>Secondary SQL query.</summary>
        public string SQL2 = "";

        /// <summary>SQL query for callback.</summary>
        public string SQLCallBack2 { get; set; }

        /// <summary>Constructor sets default control type and CSS.</summary>
        public HtmlTextBox()
        {
            Type = "text";
            DivClass = "";
        }

        public bool DivGroup { get; set; } = false;


        public override string GetHtml()
        {
            HTML html = new HTML();
            object[] name;
            string[] labelAlign;
            object[] d;
            ID = ID == null ? "" : ID;
            string valid = !ValidationField ? "" : " required=true";
            StringBuilder sb = new StringBuilder();
            string IsHide = "";
            string ReadOnly = "";
            string required = "";
            //Value = (Value == null ? "" : Value.Replace("\"", string.Empty));
            Value = Value == null ? "" : Value.Replace("\"", "&quot;");//HttpUtility.HtmlEncode(Value));
            if (Required)
                required = "required";

            if (IsReadOnly && !IsDisabled)
            {
                ReadOnly = "readonly=\"readonly\" tabindex=\"-1\"";
            }
            if (IsDisabled && !IsReadOnly)
            {
                ReadOnly = "disabled=\"disabled\" tabindex=\"-1\"";
            }
            if (Hide)
            {
                IsHide = " style=\"display:none\"";
                Type = "hidden";
            }
            if (Label != "" && placeholder == "")
            {
                placeholder = ISNumeric ? "Enter " + Label.Replace(":", "").ToLower() + " (Number)" : "Enter " + Label.Replace(":", "").ToLower();
            }
            if (!IsCustomForm)
            {
                if (Name == "") Name = ID;
                string colspan = "";
                if (ColSpan != 0)
                {
                    colspan = " colspan=" + ColSpan.ToString();
                    ColSpan = 0;
                }
                if (!Hide)
                {
                    if (!UseTable) sb.Append("<tr " + IsHide + ">");
                    if (!ISFloatingLabel)
                    {
                        sb.Append("<td" + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label class=\"col-form-label text-nowrap\" for=\"" + ID + "\">" + Label + "</label></td>");
                        sb.Append("<td" + IsHide + "" + colspan + " valign=\"" + LabelValign + "\">");
                        sb.Append("<div class=\"" + (DivClass == "" ? "form-group position-relative" : DivClass) + "\">");
                    }
                    else
                    {
                        sb.Append("<td" + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"></td>");
                        sb.Append("<td" + IsHide + "" + colspan + " valign=\"" + LabelValign + "\">");
                        sb.Append("<div class=\"mb-3 floating-label" + (!Required ? "" : " validate-input alert-validate") + "\" data-validate=\"" + (!Required ? "" : Label.Replace(":", "").ToString()) + " is required.\">");
                    }
                }
                if (Type.ToLower() == "password")
                {
                    if (EqualTo != "")
                        EqualTo = " equalTo=\"#" + EqualTo + "\"";
                    sb.Append("<input onkeyup=\"" + OnKeyupFunction + "\" onchange=\"" + OnChangeFunction + "\" type=\"" + Type + "\" pattern=\"" + PatternPassword + "\"" + EqualTo + " titles=\"" + Title + "\" " + ReadOnly + " name=\"" + Name + "\" id=\"" + ID + "\" ");
                }
                else
                {
                    if (!isTagsInput)
                        sb.Append("<input onkeyup=\"" + OnKeyupFunction + "\" onchange=\"" + OnChangeFunction + "\" " + (DefaultCheckBoxChecked == true ? " checked=\"checked\" " : "") + " type=\"" + Type + "\" " + ReadOnly + " name=\"" + Name + "\" id=\"" + ID + "\" ");
                    else
                    {
                        if (Value != "")
                        {
                            string val = "";
                            string[] Value1 = Value.Split(new char[] { ',' });
                            foreach (string v in Value1)
                            {
                                if (val == "")
                                    val = "'" + v + "'";
                                else
                                    val += ",'" + v + "'";
                            }
                            Value = val;
                        }
                        sb.Append("<select id=\"" + ID + "\" class=\"" + Class + "\" multiple></select>");
                        sb.AppendLine("<script>");
                        sb.AppendLine("$(function(){ ");
                        if (Required)
                            sb.AppendLine("$(\"#" + ID + "\").attr(\"required\", \"true\");");
                        sb.AppendLine("$(\"#" + ID + "\").addClass(\"form-control select2bs4\");");
                        sb.AppendLine("$(\"#" + ID + "\").select2({");
                        sb.AppendLine("tags:true,");
                        //sb.AppendLine("allowClear: true,");
                        sb.AppendLine("theme: 'bootstrap',");
                        sb.AppendLine("dropdownCssClass : 'no-search',");
                        sb.AppendLine("width: '250px',");
                        sb.AppendLine("placeholder:\"" + placeholder + "\",");
                        sb.AppendLine("data:[" + Value + "]");
                        sb.AppendLine("});");
                        if (Value != "")
                        {
                            sb.AppendLine("$(\"#" + ID + "\").val([" + Value + "]).trigger('change');");
                        }
                        if (IsReadOnly)
                        {
                            sb.AppendLine("$(\"#" + ID + "\").prop('disabled', true);");
                            // or
                            // sb.AppendLine("$(\"#" + ID + "\").select2('enable', false);"); // old Select2 only
                        }
                        sb.AppendLine("});");
                        sb.AppendLine("</script>");
                    }
                }
                if (!isTagsInput)
                    sb.Append("value =\"" + Value + "\" " + required + " placeholder=\"" + placeholder + "\" class=\"" + Class + "\" />");
                if (ISFloatingLabel)
                    sb.Append("<label for=\"" + ID + "\">" + Label + "</label>");
                //sb.Append("<div class=\"invalid-feedback\">");
                //sb.Append("Please " + placeholder);
                //sb.Append("</div>");
                if (HiddenField != "")
                {
                    //sb.AppendLine("<input value=\"" + HidenFieldValue + "\" type=\"hidden\" id=\"" + (ID == "" ? "" : ID + "_" + HiddenFeild) + "\" name=\"" + (ID == "" ? HiddenFeild : ID + "_" + HiddenFeild) + "\" />");
                    string[] HFields = HiddenField.Split(',');
                    if (HiddenFieldValue == null)
                        HiddenFieldValue = "";
                    string[] HValue = HiddenFieldValue.Split(',');
                    for (int i = 0; i < HFields.Length; i++)
                    {
                        sb.AppendLine("<input value=\"" + HValue[i].Trim() + "\" type=\"hidden\" id=\"" + (ID != "" ? ID + "_" + HFields[i].Trim() : "") + "\" name=\"" + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + "\" />");
                    }
                }
                if (!Hide)
                {
                    if (ExLabel != "")
                    {
                        sb.Append("<label class=\"small dm-mono\">" + ExLabel + "</label>");

                    }
                    //sb.Append("</div></td>");
                    sb.Append("</div>");
                    //if (!UseTable) sb.Append("</tr>");
                }
            }
            else if (ShowUL)
            {
                sb.AppendFormat("<ul class=\"Custom-Form\" {0}>", IsHide);
                sb.AppendFormat("<li><label class=\"label\" for=\"{0}\">{1}</label></li>", base.ID, base.Label);
                StringBuilder stringBuilder2 = sb;
                d = new object[] { Type, ReadOnly, null, null, null, null, null };
                name = d;
                name[2] = base.Name ?? base.ID;
                name[3] = base.ID;
                name[4] = base.Value;
                name[5] = base.Class;
                name[6] = base.MaxLength;
                stringBuilder2.AppendFormat("<li><input  onkeyup=\"" + OnKeyupFunction + "\" onchange=\"" + OnChangeFunction + "\" type=\"{0}\" {1} name=\"{2}\" id=\"{3}\" value=\"{4}\" class=\"{5}\"  maxlength=\"{6}\" /></li>", name);
                sb.AppendFormat("<li><label name=\"LCheckTextBox\" id=\"{0}\" for=\"{1}\"></label></li>", base.ID.Replace("Txt", ""), base.ID);
                sb.AppendLine("</ul>");
            }
            else
            {
                if (DivClass == "")
                    DivClass = "wrap-input2";
                if (Label != "")
                {
                    if (DivGroup)
                    {
                        sb.AppendLine("<div class=\"div-parent d-flex flex-grow-1\">");
                    }
                    sb.AppendLine("<div " + IsHide + " class=\"col-sm-auto\">");
                    sb.AppendLine("<label for=\"" + ID + "\" class=\"col-form-label khmerFont1\">" + Label + "</label></div>");
                }
                sb.AppendLine("<div " + IsHide + " class=\"" + DivClassCol + " align-text-bottom\">");
                sb.AppendLine("<div class=\"" + DivClass + " " + (Required == true ? "validate-input alert-validate" : "") + (Required == true && Value == "" ? " alert-validate" : "") + "\" " + (Required == true ? "data-validate=\"" + Label + " is required\"" : "") + ">");
                sb.Append("<input  onkeyup=\"" + OnKeyupFunction + "\" onchange=\"" + OnChangeFunction + "\" " + (DefaultCheckBoxChecked == true ? " checked=\"checked\" " : "") + " type=\"" + Type + "\" " + ReadOnly + " name=\"" + (Name == "" ? ID : Name) + "\" id=\"" + ID + "\" ");
                sb.Append("value =\"" + Value + "\" " + required + " placeholder=\"" + placeholder + "\" class=\"" + Class + "\" />");

                if (HiddenField != "" && Prefix != "")
                {
                    sb.AppendLine("<input value=\"\" type=\"hidden\" id=\"" + ID + "_" + HiddenField + "\" name=\"" + ID + "_" + HiddenField + "\" />");
                    sb.AppendLine("<input value=\"" + HiddenFieldValue + "\" type=\"hidden\" id=\"" + Prefix + "\" name=\"" + Prefix + "\" />");
                }
                else if (Prefix != "")
                {
                    sb.AppendLine("<input value=\"" + HiddenFieldValue + "\" type=\"hidden\" id=\"" + Prefix + "\" name=\"" + Prefix + "\" />");
                }
                else if (!ISAutoComplete && HiddenField != "")
                {
                    //sb.AppendLine("<input value=\"" + HidenFieldValue + "\" type=\"hidden\" id=\"" + (ID == "" ? "" : ID + "_" + HiddenFeild) + "\" name=\"" + (ID == "" ? HiddenFeild : ID + "_" + HiddenFeild) + "\" />");
                    string[] HFields = HiddenField.Split(',');
                    if (HiddenFieldValue == null)
                        HiddenFieldValue = "";
                    string[] HValue = HiddenFieldValue.Split(',');

                    for (int i = 0; i < HFields.Length; i++)
                    {
                        if (Name != "")
                            sb.AppendLine("<input value=\"" + HValue[i].Trim() + "\" type=\"hidden\" id=\"" + (ID != "" ? ID + "_" + HFields[i].Trim() : "") + "\" name=\"" + (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + "\" />");
                        else
                            sb.AppendLine("<input value=\"" + HValue[i].Trim() + "\" type=\"hidden\" id=\"" + (ID != "" ? ID + "_" + HFields[i].Trim() : "") + "\" name=\"" + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + "\" />");
                    }
                }
                else if (ISAutoComplete && HiddenField != "")
                {
                    string[] HFields = HiddenField.Split(',');
                    if (HiddenFieldValue == null)
                        HiddenFieldValue = "";
                    string[] HValue = HiddenFieldValue.Split(',');

                    for (int i = 0; i < HFields.Length; i++)
                    {
                        sb.AppendLine("<input value=\"" + HValue[i].Trim() + "\" type=\"hidden\" id=\"" + (ID != "" ? HFields[i].Trim() : "") + "\" name=\"" + HFields[i].Trim() + "\" />");
                    }
                }
                if (DeleteTDRowFunction != "")
                {
                    sb.AppendLine("<div class=\"input-delete\" title=\"Delete Row\" data-validate=\"Delete Service Plan Row\" onclick=\"" + DeleteTDRowFunction + "\"></div>");
                }
                if (AddButtonFunction != "")
                {
                    sb.AppendLine("<div class=\"input-addnew\" title=\"Add New\" data-validate=\"Add New\" onclick=\"" + AddButtonFunction + "\"></div>");
                }
                sb.AppendLine("<span class=\"focus-input2\"></span>");
                if (InputDescription)
                {
                    //sb.AppendLine("<textarea placeholder=\"Description\" class=\"form-control\" id=\"" + (ID == "" ? "" : ID + "_Description") + "\" name=\"" + (ID == "" ? Name : ID) + "_Description" + "\"></textarea>");
                    sb.AppendLine("<textarea class=\"summernote form-control\" id=\"" + (ID == "" ? "" : ID + "_Description") + "\" name=\"" + (ID == "" ? Name : ID) + "_Description" + "\" style=\"field-sizing: content;\"></textarea>");
                    //sb.AppendLine("<script>");
                    //sb.AppendLine("$(function() {$('.summernote').summernote({toolbar: false, placeholder: 'Description',});});");
                    //sb.AppendLine("</script>");
                }
                //{
                //	sb.AppendLine("<input type=\"hidden\" id=\"" + ID + "_" + HiddenFeild + "\" name=\"" + ID + "_" + HiddenFeild + "\" />");
                //}
                //else
                //	sb.AppendLine("<input type=\"hidden\" id=\"" + HiddenFeild + "\" name=\"" + HiddenFeild + "\" />");
                sb.AppendLine("</div></div>");

            }
            if (ISNumeric)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function(){ $(\"input[name=" + (ID == "" ? Name : ID) + "]\").focus(function(){ $(this).select(); }); $(\"input[name=" + (ID == "" ? Name : ID) + "]\").autoNumeric(");
                if (ISInteger)
                {
                    sb.Append("{aSep: \"\",aPad: false, vMin: \"0\", vMax: \"999999999999999999999999999999999999999999999999999999999999999999999999999999999\"}");
                }
                else
                {
                    sb.Append("{vMin: \"-9999999999" + Digit.Replace("0", "9") + "\",vMax: \"999999999999999999999999999999999999999999999999999999" + Digit.Replace("0", "9") + "\"}");
                }
                sb.Append(");");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (!isTagsInput && (ISAutoComplete || IsKendoAutoComplete))
            {
                sb.AppendLine("<script>");
                sb.AppendLine("function CallSetAutoComplete_" + (ID == "" ? Name : ID) + "() {");
                if (CallBackFunction != "")
                {
                    d = new object[] { "SetAutoComplete(\"", ID == "" ? Name : ID, "\", \"", EncryAndDecry.Encryp(this.SQL_AutoComplete), "\", \"", HiddenField, "\", \"\" , \"", this.ControlNameID, "\", \"", this.FieldCondition, "\", \"", this.IsUseConnection2, "\", \"", SQL2 == "" ? "" : EncryAndDecry.Encryp(SQL2), "\",'" + CboValue + "',`" + CallBackFunction + "`, '" + (IsDisplayOnFieldWithAutoComplete == true ? DisplayOnFields : string.Empty) + "','" + HiddenField + "');" };
                    name = d;
                    sb.AppendLine(string.Concat(name));
                    if (ISCallBack || SQLCallBack != "" || isApplyTypeaheadOnCallBack)
                    {
                        if (SetChangeEventOnControlID != "" && Value == "")
                            sb.AppendLine("var CallBackFuncion_" + (ID == "" ? Name : ID) + " = function() { $(\"#" + SetChangeEventOnControlID + "\").change(); };");
                        else
                            sb.AppendLine("var CallBackFuncion_" + (ID == "" ? Name : ID) + " =\"\"");
                        sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + (HiddenField == "" ? Name : HiddenField) + "]" : "#" + (HiddenField == "" ? ID : HiddenField)) + "\").change(function(event){");
                        //sb.AppendLine("if (event.isDefaultPrevented()) return;\r\nevent.preventDefault();");
                        sb.AppendLine("if($(this).val()!=\"\")");
                        sb.AppendLine("LoadCallBackData(\"" + (ID == "" ? (HiddenField == "" ? Name : HiddenField) : (HiddenField == "" ? ID : HiddenField)) + "\", \"" + EncryAndDecry.Encryp(SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + IsUseConnection2 + "\", \"" + (SQL2 == "" ? "" : EncryAndDecry.Encryp(SQL2)) + "\", \"" + (SQLCallBack2 == "" ? "" : EncryAndDecry.Encryp(SQLCallBack2)) + "\",\"true\", \"0\", \"false\", \"\", CallBackFuncion_" + (ID == "" ? Name : ID) + ",'" + CallBackWithControlValueID + "', '" + isApplyTypeaheadOnCallBack + "','" + CallBackToControlID + "','" + FieldName + "','" + CallBackDisplayText + "',`" + TypeaheadCallBackFunction + "`, '" + EncryAndDecry.Encryp(SQLTypeaheadCallBack) + "', '" + DisplayOnFieldsTypeahead + "');");
                        sb.AppendLine("});");
                        sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").keypress(function(event) {");
                        sb.AppendLine("if (event.which == 13 ) {");
                        sb.AppendLine("if (event.isDefaultPrevented()) return;\r\nevent.preventDefault();");
                        sb.AppendLine("if($(this).val()!=\"\")");
                        sb.AppendLine("LoadCallBackData(\"" + (ID == "" ? (HiddenField == "" ? Name : HiddenField) : (HiddenField == "" ? ID : HiddenField)) + "\", \"" + EncryAndDecry.Encryp(SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + this.IsUseConnection2 + "\", \"" + (SQL2 == "" ? "" : EncryAndDecry.Encryp(SQL2)) + "\", \"" + (SQLCallBack2 == "" ? "" : EncryAndDecry.Encryp(SQLCallBack2)) + "\",\"true\", \"0\", \"false\", \"\", CallBackFuncion_" + (ID == "" ? Name : ID) + ",'" + CallBackWithControlValueID + "', '" + isApplyTypeaheadOnCallBack + "','" + CallBackToControlID + "','" + FieldName + "','" + CallBackDisplayText + "',`" + TypeaheadCallBackFunction + "`, '" + EncryAndDecry.Encryp(SQLTypeaheadCallBack) + "', '" + DisplayOnFieldsTypeahead + "');");
                        sb.AppendLine("}");
                        sb.AppendLine("});");
                    }
                }
                else
                {
                    if (ISCallBack || SQLCallBack != "" || isApplyTypeaheadOnCallBack)
                    {
                        StringBuilder sbfuntion = new StringBuilder();
                        if (SetChangeEventOnControlID != "" && Value == "")
                            sbfuntion.AppendLine("var CallBackFuncion_" + (ID == "" ? Name : ID) + " = function() { $(\"#" + SetChangeEventOnControlID + "\").change(); };");
                        else
                            sbfuntion.AppendLine("var CallBackFuncion_" + (ID == "" ? Name : ID) + " =\"\"");
                        sbfuntion.AppendLine("LoadCallBackData(\"" + (ID == "" ? (HiddenField == "" ? Name : HiddenField) : (HiddenField == "" ? ID : HiddenField)) + "\", \"" + EncryAndDecry.Encryp(SQLCallBack) + "\", \"" + DisplayOnFields + "\", \"" + this.IsUseConnection2 + "\", \"" + (SQL2 == "" ? "" : EncryAndDecry.Encryp(this.SQL2)) + "\", \"" + (SQLCallBack2 == "" ? "" : EncryAndDecry.Encryp(this.SQLCallBack2)) + "\",\"true\",  $('" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "').index(this), \"true\", \"\", CallBackFuncion_" + (ID == "" ? Name : ID) + ",'" + CallBackWithControlValueID + "', '" + isApplyTypeaheadOnCallBack + "','" + CallBackToControlID + "','" + FieldName + "','" + CallBackDisplayText + "','" + TypeaheadCallBackFunction + "', '" + EncryAndDecry.Encryp(SQLTypeaheadCallBack) + "', '" + DisplayOnFieldsTypeahead + "');");
                        CallBackFunction = sbfuntion.ToString();
                        if (AddCallBackFunction != "")
                            CallBackFunction += AddCallBackFunction;
                    }
                    if (!IsKendoAutoComplete)
                        sb.AppendLine("SetAutoComplete(\"" + (ID == "" ? Name : ID) + "\",\"" + EncryAndDecry.Encryp(this.SQL_AutoComplete) + "\", \"" + HiddenField + "\",\"\",\"" + this.ControlNameID + "\",\"" + this.FieldCondition + "\",\"" + this.IsUseConnection2 + "\",\"" + EncryAndDecry.Encryp(this.SQL2) + "\",'" + CboValue + "',`" + CallBackFunction + "`, '" + DisplayOnFields + "','" + HiddenField + "');");
                    else
                    {
                        sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL_AutoComplete) + "\"}, function(response) {");
                        sb.AppendLine("var Data = JSON.parse(JSON.parse(response).Data);");
                        sb.AppendLine("$(\"" + (ID == "" ? Name : "#" + ID) + "\").kendoAutoComplete({ dataTextField: \"" + DataTextField + "\", dataValueField:\"" + DataValueField + "\", height: 400, dataSource: { data: Data, ");
                        if (GroupByField != "")
                        {
                            sb.AppendLine("group: { field: \"" + GroupByField + "\" },");
                        }
                        sb.AppendLine("}});");
                        sb.AppendLine("});");
                    }
                }
                sb.AppendLine("}");
                sb.AppendLine("$(function() {");
                sb.AppendLine("CallSetAutoComplete_" + (ID == "" ? Name : ID) + "();");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (EventChange != "")
            {
                labelAlign = new string[] { "<script>$(function(){$(\"#", ID, "\").on('change', function(){ ", EventChange, " });});</script>" };
                sb.AppendLine(string.Concat(labelAlign));
            }
            if (ISCallBackToCbo)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function() {");
                //sb.AppendLine("$(\"#" + ID + "\").bind(\"propertychange change click keyup input paste\",function() {");

                if (Value != "")
                {
                    //sb.AppendLine("$(\"#" + ID + "\").change();");
                    sb.AppendLine("on" + ID + "Change();");
                }
                sb.AppendLine("$(\"#" + ID + "\").change(function(){ on" + ID + "Change(); });");
                sb.AppendLine("});");
                sb.AppendLine("function on" + ID + "Change() {");
                //sb.AppendLine("$(\"#" + ID + "\").change(function() {");
                sb.AppendLine("var CboValue = $(\"#" + ID + "\").val();");
                sb.AppendLine("if (CboValue!=\"\" && CboValue!=null)");
                if (CallBackFunction == "")
                    sb.AppendLine("LoadCallBackDataToCBO(\"" + CallBackToControlID + "\", `" + EncryAndDecry.Encryp(SQLCallBack) + "`, \"" + CallBackDisplayValue + "\", \"" + CallBackDisplayText + "\", \"false\", CboValue);");
                else
                    sb.AppendLine("LoadCallBackDataToCBO(\"" + CallBackToControlID + "\", `" + EncryAndDecry.Encryp(SQLCallBack) + "`, \"" + CallBackDisplayValue + "\", \"" + CallBackDisplayText + "\", \"false\", CboValue," + CallBackFunction + ");");
                //sb.AppendLine("});");
                sb.AppendLine("}");
                sb.AppendLine("</script>");
            }
            if (isTagsInput)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("var responsetagsInputData_" + (ID == "" ? Name.Trim() : ID.Trim()) + "=[];");
                sb.AppendLine("var cacheData = [],valueIdMap = {};");
                sb.AppendLine("$(function () { LoadtagsInput_" + ID + "(); });");
                sb.AppendLine("function LoadtagsInput_" + ID + "() {");
                sb.AppendLine("$('" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "').tagsInput({");
                sb.AppendLine($"placeholder:`{placeholder}`,");
                //sb.AppendLine("trimValue: true,");
                //sb.AppendLine("tagClass: 'badge bg-primary',");
                if (ISAutoComplete)
                {
                    sb.AppendLine(@"autocomplete: {
					source: function (request, response) {");
                    sb.AppendLine("var URL = ServerRoot + \"DoActionData/Autocomplete\";");
                    sb.AppendLine($"var SQL=`{(SQL_AutoComplete == "" ? "" : EncryAndDecry.Encryp(SQL_AutoComplete))}`;");
                    sb.AppendLine($"var HiddenFeild=`{HiddenField}`;");
                    //sb.AppendLine($"var HiddenFeild=`{HiddenField}`;");
                    sb.AppendLine($"var DisplayOnFields=`{DisplayOnFields}`;");
                    sb.AppendLine(@"$.ajax({
                    url: URL,
                    async: false,
                    data: {
                        term: request.term,
                        SQL: SQL,
                        IsUseConnection2: false,
                        ValueControlNameID: " + (ControlNameID != "" ? "$(\"#" + ControlNameID + "\").val()" : "''") + @",
                        ConditionField: """",
                        CboValue: " + (ControlNameID != "" ? "$(\"#" + ControlNameID + "\").val()" : "''") + @",
                        DisplayOnFields: DisplayOnFields,
                        HiddenFeild: HiddenFeild
                    },
                    dataType: ""json"",
                    dataFilter: function (data) { return data; },
                    converters: {
                        ""json jsond"": function (msg) {
                            return msg.hasOwnProperty('d') ? msg.d : msg;
                        }
                    },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('x-custom-header', JSON.stringify(headerPara));
                    },
                    headers: { ""Authorization"": ""Bearer "" + access_Token },
                    success: function (data) {
var dt=$.grep(data, function (item) {
                            return {
                                value: item.value,
                                id: item.id
                            }
                        });
responsetagsInputData_" + (ID == "" ? Name.Trim() : ID.Trim()) + @"=dt;

const list = (data || []).map(item => {
                            // 🔥 STORE permanently
                            valueIdMap[item.value] = item.id;
                            return {
                                value: item.value,
                                id: item.id
                            };
                        });

                        cacheData = list;

                       return response(dt);
                    },
                    error: function (response, callStatus, errorThrown) {
                        $('button').removeAttr('disabled');
                        DialogSweetAlert(""error"", response.status + "" "" + response.statusText);
                    }
                });");
                    sb.AppendLine("}},");
                }
                sb.AppendLine(@"onChange:function (event, ui) {
var ThisValue = $(this).val();

var mydt = responsetagsInputData_" + (ID == "" ? Name.Trim() : ID.Trim()) + @";
var index = $('" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + @"').index(this);
var Hidden = '" + HiddenField + @"';
var hiddenValue = [];
var hiddenValues = [];
const values = $(this).val()?.split(',') || [];

values.forEach(tag => {
        tag = $.trim(tag);
        const id = valueIdMap[tag];
        if (id) {
            hiddenValues.push(id); // ✅ ALLOW DUPLICATES
        }
    });
//console.log(hiddenValues.join(','));

$(""input[name="" + Hidden + ""]"").eq(index).val(hiddenValues).trigger('change');

" + OnChangeFunction + @"
}");

                sb.AppendLine("});");
                if (IsCustomForm)
                {
                    sb.AppendLine("$(\"div.bootstrap-tagsinput\").css({\"border\":\"none\", \"box-shadow\":\"none\"});");
                }
                if (OnKeyupFunction != "")
                {
                    sb.AppendLine("$('" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "').on('itemAdded', function(event) {");
                    sb.AppendLine("return " + OnKeyupFunction);
                    sb.AppendLine("});");
                }
                sb.AppendLine("};");
                sb.AppendLine(@"$('" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + @"').on('beforeItemAdd', function(event) {
   var tag = event.item;
   // Do some processing here
   if (!event.options || !event.options.preventPost) {
      //console.log(tag);
		//$.ajax('/ajax-url', ajaxData, function(response) {
         //if (response.failure) {
            // Remove the tag since there was a failure
            // ""preventPost"" here will stop this ajax call from running when the tag is removed
           // $('#tags-input').tagsinput('remove', tag, {preventPost: true});
         //}
      //});
   }
});");
                sb.AppendLine("</script>");
            }
            if (IsAllowInputOnlyNumber)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function(){");
                sb.AppendLine("$(\"" + (ID != "" ? "#" + ID : "input[name=" + Name + "]") + "\").focus().select();");
                sb.AppendLine("$(\"" + (ID != "" ? "#" + ID : "input[name=" + Name + "]") + "\").keypress(function(event) { allowInputOnlyNumber(event); });");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (IsSetLocationWithGoogleMap)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function(){");
                sb.AppendLine("$(\"#" + ID + "\").dblclick(function() { MarkerMap(\"" + ID + "\", '" + InputLatitude + "','" + InputLongitude + "','" + MapURL + "'); });");
                sb.Append("var autocompleteg = new google.maps.places.Autocomplete($(\"#" + ID + "\")[0], {});");
                sb.Append("google.maps.event.addListener(autocompleteg, 'place_changed', function () {");
                sb.Append("var place = autocompleteg.getPlace();");
                sb.Append("var lat = place.geometry.location.lat();");
                sb.Append("var lng = place.geometry.location.lng();");
                if (InputLatitude != "")
                    sb.Append("$(\"#" + InputLatitude + "\").val(lat);");
                if (InputLongitude != "")
                    sb.Append("$(\"#" + InputLongitude + "\").val(lng);");
                sb.Append("});");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (isTypeahead)
            {
                if (SQL != "")
                {
                    if (TypeaheadGroupByField != "")
                    {
                        DataSet ds = html.LoadData(SQL);
                        string GroupByField2 = "";
                        sb.AppendLine("<script>");
                        sb.AppendLine("$(function(){ Reload_" + (ID == "" ? Name : ID) + "(); ");
                        if (Value != "")
                        {
                            sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").typeahead('val','" + Value + "');");
                        }
                        sb.AppendLine("});");
                        if (TypeAheadAddNewFunctionName == "")
                        {
                            sb.AppendLine("function Reload_" + (ID == "" ? Name : ID) + "() {");
                            sb.AppendLine("var data_" + (ID == "" ? Name : ID) + " = " + html.JsonData(SQL, false) + ";");
                            sb.AppendLine("var Account_" + (ID == "" ? Name : ID) + " = data_" + (ID == "" ? Name : ID) + ";");
                            sb.AppendLine("$(\"" + (ID != "" ? "#" + ID : "input[name=" + Name + "]") + "\").typeahead(\"destroy\");");
                            sb.AppendLine("$(\"" + (ID != "" ? "#" + ID : "input[name=" + Name + "]") + "\").typeahead(");
                            sb.AppendLine("{");
                            sb.AppendLine("hint: true,");
                            sb.AppendLine("highlight: true,");
                            sb.AppendLine("minLength: 0");
                            sb.AppendLine("},");
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                string GroupByField1 = dr[TypeaheadGroupByField].ToString();
                                if (GroupByField1 != GroupByField2)
                                {
                                    sb.AppendLine("{");
                                    sb.AppendLine("limit: 9999,");
                                    sb.AppendLine("display: 'Name',");
                                    sb.AppendLine("source: function (query, cb) {");
                                    sb.AppendLine("let matches = [];");
                                    sb.AppendLine("let Data = Account_" + (ID == "" ? Name : ID) + ".filter(function (data) {");
                                    sb.AppendLine("return data." + TypeaheadGroupByField + " === \"" + GroupByField1 + "\";");
                                    sb.AppendLine("});");
                                    sb.AppendLine("$.each(Data, function (i, str) {");
                                    sb.AppendLine("const value = str." + DBField + ";");
                                    sb.AppendLine("const globalRegex = new RegExp(query, \"\\ig\");");
                                    sb.AppendLine("if (globalRegex.test(value)) {");
                                    sb.Append("matches.push({ Name: str." + DBField + ", ID: str." + ds.Tables[0].Columns[0].ColumnName + " ");
                                    if (DisplayOnFields != "")
                                    {
                                        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(DisplayOnFields);
                                        foreach (var kv in dict)
                                        {
                                            string controlID = kv.Key;
                                            string fname = kv.Value;
                                            sb.Append(", " + kv.Value + ": str." + kv.Value);
                                        }
                                    }
                                    sb.Append("});");
                                    sb.AppendLine("}");
                                    sb.AppendLine("});");
                                    sb.AppendLine("cb(matches);");
                                    sb.AppendLine("},");
                                    sb.AppendLine("templates: {");
                                    sb.AppendLine("header: '<div class=\"border-bottom p-2 border-top typeahead-head\">" + GroupByField1 + "</div>'");
                                    sb.AppendLine("}");
                                    sb.AppendLine("},");
                                }
                                GroupByField2 = dr[TypeaheadGroupByField].ToString();
                            }
                            //sb.AppendLine(").bind(\"typeahead:selected\", function (obj, data, name) {");
                            sb.AppendLine(").on(\"focus\", function (aEvent) {if (aEvent.isDefaultPrevented()) return; aEvent.preventDefault();$(this).select();}).on(\"click\", function (aEvent) { if (aEvent.isDefaultPrevented()) return; aEvent.preventDefault(); $(this).typeahead(\"open\");");
                            if (TypeAheadAddNewFunctionName != "")
                            {
                                sb.AppendLine("$(\"div[name=" + (ID == "" ? Name : ID) + "div_add_New_Item]\").remove();");
                                sb.AppendLine("$(\"div.tt-menu\").prepend(`<div name=\"" + (ID == "" ? Name : ID) + "div_add_New_Item\" id=\"" + (ID == "" ? Name : ID) + "div_add_New_Item\" onclick=\"" + TypeAheadAddNewFunctionName + "\" class=\"tt-suggestion tt-selectable\"><a href=\"javascript:void(0);\"><i class=\"fa fa-plus\"></i> Add New</a></div>`);");
                            }
                            sb.AppendLine("}).on(\"typeahead:active\", function (aEvent) {if (aEvent.isDefaultPrevented()) return; aEvent.preventDefault(); let originalVal=null; if ($(this).typeahead(\"val\") != \"\") originalVal = $(this).typeahead(\"val\");if (originalVal == null) { originalVal = $(this).val(); $(this).typeahead(\"val\", originalVal);}");
                            sb.AppendLine("}).on(\"typeahead:change\", function (aEvent, aSuggestion) {");
                            sb.AppendLine("if (aEvent.isDefaultPrevented()) return;\r\naEvent.preventDefault();");
                            sb.AppendLine("$(\"div[name=" + (ID == "" ? Name : ID) + "div_add_New_Item]\").remove();");
                            sb.AppendLine("let data = Account_" + (ID == "" ? Name : ID) + ".filter(name => name['" + (DBField == "" ? FieldName : DBField) + "'] === aSuggestion);");
                            //sb.AppendLine("console.log(data);");
                            sb.AppendLine("if(data[0])");
                            sb.AppendLine("$(\"" + (ID != "" ? "#" + ID + "_" + HiddenField : "input[name=" + HiddenField + "]") + "\").val(data[0]." + HiddenField + ");");
                            sb.AppendLine("else");
                            sb.AppendLine("$(\"" + (ID != "" ? "#" + ID + "_" + HiddenField : "input[name=" + HiddenField + "]") + "\").val('');");
                            if (DisplayOnFields != "")
                            {
                                sb.AppendLine("if(data[0]) {");
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(DisplayOnFields);
                                foreach (var kv in dict)
                                {
                                    string controlID = kv.Key;
                                    string fname = kv.Value;
                                    sb.AppendLine("$(\"#" + controlID + "\").val(data[0]." + fname + ");");
                                }
                                sb.AppendLine("}");
                                sb.AppendLine("else {");
                                foreach (var kv in dict)
                                {
                                    string controlID = kv.Key;
                                    string fname = kv.Value;
                                    sb.AppendLine("$(\"#" + controlID + "\").val('');");
                                }
                                sb.AppendLine("}");
                            }
                            if (CallBackFunction != "")
                            {
                                sb.AppendLine("setTimeout(function () {");
                                sb.AppendLine(CallBackFunction + "();");
                                sb.AppendLine("}, 100);");
                            }
                            sb.AppendLine("});");
                            sb.AppendLine("}");
                        }
                        else
                        {
                            sb.AppendLine("function Reload_" + (ID == "" ? Name : ID) + "(CallBackFunction) {");
                            sb.AppendLine("CallBackFunction = (typeof CallBackFunction == 'undefined') ? \"\" : CallBackFunction;");
                            sb.AppendLine("$(\"#" + ID + "\").typeahead(\"destroy\");");
                            sb.AppendLine("$(\"#TypeAheadGroup_" + ID + "\").remove();");
                            sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL) + "\", FieldName:\"" + FieldName + "\",Value:\"" + Value + "\",IsRender:\"True\",ID:\"" + ID + "\",TypeaheadGroupByField:\"" + TypeaheadGroupByField + "\",DBField:\"" + DBField + "\",DisplayOnFields:`" + DisplayOnFields + "`,HiddenFeild:\"" + HiddenField + "\",CallBackFunction:\"" + CallBackFunction + "\", Name:\"" + Name + "\"}, function(response) {");
                            sb.AppendLine("$(\"#" + ID + "\").parent().after(`<div id=\"TypeAheadGroup_" + ID + "\">`+JSON.parse(response).Data+`</div>`);");
                            sb.AppendLine("if (typeof CallBackFunction === 'function') {\r\nCallBackFunction();\r\n}");
                            sb.AppendLine("}, false);");
                            sb.AppendLine("}");
                        }
                        sb.AppendLine("</script>");
                    }
                    else
                    {
                        sb.AppendLine("<script>");
                        sb.AppendLine("$(function(){");
                        sb.AppendLine("let Data_" + (ID == "" ? Name : ID) + " = " + html.JsonData(SQL, false, null, false, "", IsGetDataFromAPI, Token, ReplaceJsonKey) + ";");
                        sb.AppendLine("if (Data_" + (ID == "" ? Name : ID) + ".length > 0) { ");
                        sb.AppendLine("let callbackfield=[]; var obj = {};");
                        if (HiddenField != "" && SQLCallBack.ToString() != "")
                        {
                            string[] HFields = HiddenField.Split(',');
                            if (HiddenFieldValue == null)
                                HiddenFieldValue = "";
                            string[] HValue = HiddenFieldValue.Split(',');
                            string stname = "";
                            for (int i = 0; i < HFields.Length; i++)
                            {
                                if (stname == "")
                                {
                                    stname = Name != ""
                                        ? (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'")
                                        : (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                                else
                                {
                                    if (Name != "")
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'");
                                    else
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                            }
                            sb.AppendLine("callbackfield.push({" + stname + "});");
                            sb.AppendLine("ApplyTypeahead(\"" + ID + "\", Data_" + (ID == "" ? Name : ID) + ",callbackfield[0], {Name: '" + (DBField == "" ? FieldName : DBField) + "', " + (ID == "" ? Name : ID) + "_" + HiddenField + ":'" + HiddenField + "'}, '" + (ID == "" ? Name : "") + "', {SQLCallBack:'" + EncryAndDecry.Encryp(SQLCallBack) + "', CallBackToControlID:'" + CallBackToControlID + "', FieldName:'" + HiddenField + "', Value:'" + (ID == "" ? Name : ID) + "_" + HiddenField + "',PrimaryValue:" + PrimaryFieldValue + ", CurrentValue:'" + Value + "'}, '" + TypeAheadAddNewFunctionName + "', '" + isApplyTypeaheadOnCallBack + "','" + CallBackFunction + "', '" + DisplayOnFields + "');");
                            if (Value != "")
                            {
                                sb.AppendLine("$(\"#" + ID + "\").val('');");
                                sb.AppendLine("$(\"#" + ID + "\").typeahead('val', '');");
                                sb.AppendLine("setTimeout(function(){");
                                sb.AppendLine("$(\"#" + ID + "\").typeahead('val','" + Value + "');");
                                sb.AppendLine("$(\"#" + ID + "\").typeahead('close');");
                                //sb.AppendLine("$(\"#" + ID + "\").trigger(\"change\")");
                                sb.AppendLine("}, 100);");
                                if (Prefix != "")
                                {
                                    sb.AppendLine("let d = Data_" + (ID == "" ? Name : ID) + ".filter(function (name) { return name." + DBField + " === '" + Value + "';})[0];");
                                    sb.AppendLine("$(\"#" + Prefix + "\").val(d." + Prefix + ");");
                                }

                            }
                        }
                        else if (HiddenField != "")
                        {
                            string[] HFields = HiddenField.Split(',');
                            if (HiddenFieldValue == null)
                                HiddenFieldValue = "";
                            string[] HValue = HiddenFieldValue.Split(',');
                            string stname = "";
                            for (int i = 0; i < HFields.Length; i++)
                            {
                                if (stname == "")
                                {
                                    stname = Name != ""
                                        ? (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'")
                                        : (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                                else
                                {
                                    if (Name != "")
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'");
                                    else
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                            }
                            //sb.AppendLine("callbackfield.push({" + (ID == "" ? Name : ID) + "_" + HiddenFeild + ":'" + HiddenFeild + "'});");
                            sb.AppendLine("callbackfield.push({" + stname + "});");
                            sb.AppendLine("ApplyTypeahead(\"" + ID + "\", Data_" + (ID == "" ? Name : ID) + ",callbackfield[0], {Name: '" + (DBField == "" ? FieldName : DBField) + "'," + stname + "},'" + (ID == "" ? Name : "") + "','','" + TypeAheadAddNewFunctionName + "','','" + CallBackFunction + "', '" + DisplayOnFields + "');");
                            if (Value != "")
                            {
                                sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").typeahead('val','" + Value + "');");
                            }
                        }
                        else
                        {
                            sb.AppendLine("ApplyTypeahead(\"" + ID + "\", Data_" + (ID == "" ? Name : ID) + ",\"\",{Name: '" + (DBField == "" ? FieldName : DBField) + "'},'" + (ID == "" ? Name : "") + "','','" + TypeAheadAddNewFunctionName + "','','" + CallBackFunction + "', '" + DisplayOnFields + "');");
                            if (Value != "")
                            {
                                sb.AppendLine("setTimeout(function(){");
                                sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").val('" + Value + "');");
                                sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").typeahead('val','" + Value + "');");
                                sb.AppendLine("}, 500);");
                            }
                        }
                        sb.AppendLine("}");
                        sb.AppendLine("});");

                        sb.AppendLine("function Reload_" + (ID == "" ? Name : ID) + "(CallBackFunction) {");
                        sb.AppendLine("CallBackFunction = (typeof CallBackFunction == 'undefined') ? \"\" : CallBackFunction;");
                        if (ID != "")
                            sb.AppendLine("$(\"#" + ID + "\").typeahead(\"destroy\");");
                        else
                            sb.AppendLine("$(\"input[name=" + Name + "]\").typeahead(\"destroy\");");
                        sb.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "]\").unbind();");

                        sb.AppendLine("if($(\"input[name=" + (ID == "" ? Name : ID) + "]\")){");
                        sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL) + "\"}, function(response) {");
                        sb.AppendLine("var Data_" + (ID == "" ? Name : ID) + " = JSON.parse(JSON.parse(response).Data);");
                        sb.AppendLine("if (Data_" + (ID == "" ? Name : ID) + ".length > 0) { ");
                        sb.AppendLine("var callbackfield=[]; var obj = {};");
                        if (HiddenField != "" && SQLCallBack.ToString() != "")
                        {
                            string[] HFields = HiddenField.Split(',');
                            if (HiddenFieldValue == null)
                                HiddenFieldValue = "";
                            string[] HValue = HiddenFieldValue.Split(',');
                            string stname = "";
                            for (int i = 0; i < HFields.Length; i++)
                            {
                                if (stname == "")
                                {
                                    stname = Name != ""
                                        ? (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'")
                                        : (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                                else
                                {
                                    if (Name != "")
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'");
                                    else
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                            }
                            sb.AppendLine("callbackfield.push({" + stname + "});");
                            sb.AppendLine("ApplyTypeahead(\"" + ID + "\", Data_" + (ID == "" ? Name : ID) + ",callbackfield[0], {Name: '" + (DBField == "" ? FieldName : DBField) + "', " + (ID == "" ? Name : ID) + "_" + HiddenField + ":'" + HiddenField + "'},'" + (ID == "" ? Name : "") + "', {SQLCallBack:'" + EncryAndDecry.Encryp(SQLCallBack) + "', CallBackToControlID:'" + CallBackToControlID + "', FieldName:'" + HiddenField + "', Value:'" + (ID == "" ? Name : ID) + "_" + HiddenField + "',PrimaryValue:" + PrimaryFieldValue + "}, '" + TypeAheadAddNewFunctionName + "','" + isApplyTypeaheadOnCallBack + "');");
                        }
                        else if (HiddenField != "")
                        {
                            //	sb.AppendLine("callbackfield.push({" + (ID == "" ? Name : ID) + "_" + HiddenFeild + ":'" + HiddenFeild + "'});");
                            string[] HFields = HiddenField.Split(',');
                            if (HiddenFieldValue == null)
                                HiddenFieldValue = "";
                            string[] HValue = HiddenFieldValue.Split(',');
                            string stname = "";
                            for (int i = 0; i < HFields.Length; i++)
                            {
                                if (stname == "")
                                {
                                    stname = Name != ""
                                        ? (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'")
                                        : (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                                else
                                {
                                    if (Name != "")
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HiddenField.Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HiddenField.Trim() + "'");
                                    else
                                        stname += "," + (ID == "" ? HFields[i].Trim() : ID + "_" + HFields[i].Trim()) + ":'" + (ID == "" ? HFields[i].Trim() + "'" : HFields[i].Trim() + "'");
                                }
                            }
                            //sb.AppendLine("callbackfield.push({" + (ID == "" ? Name : ID) + "_" + HiddenFeild + ":'" + HiddenFeild + "'});");
                            sb.AppendLine("callbackfield.push({" + stname + "});");
                            sb.AppendLine("ApplyTypeahead(\"" + ID + "\", Data_" + (ID == "" ? Name : ID) + ",callbackfield[0], {Name: '" + (DBField == "" ? FieldName : DBField) + "'," + stname + "},'" + (ID == "" ? Name : "") + "','','" + TypeAheadAddNewFunctionName + "','','" + CallBackFunction + "', '" + DisplayOnFields + "');");
                            if (Value != "")
                            {
                                sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").typeahead('val','" + Value + "');");
                            }
                        }
                        //else if (HiddenFeild != "")
                        //	sb.AppendLine("ApplyTypeahead(\"" + (ID == "" ? Name : ID) + "\", Data_" + (ID == "" ? Name : ID) + ",callbackfield[0], {Name: '" + (DBField == "" ? FieldName : DBField) + "', " + (ID == "" ? Name : ID) + "_" + HiddenFeild + ":'" + HiddenFeild + "'}, '" + (ID == "" ? Name : "") + "','','" + TypeAheadAddNewFuntionName + "','','" + CallBackFunction + "', '" + DisplayOnFields + "');");
                        else
                        {
                            sb.AppendLine("ApplyTypeahead(\"" + ID + "\", Data_" + (ID == "" ? Name : ID) + ",\"\",{Name: '" + (DBField == "" ? FieldName : DBField) + "'},'" + (ID == "" ? Name : "") + "','','" + TypeAheadAddNewFunctionName + "','','" + CallBackFunction + "', '" + DisplayOnFields + "');");
                            //if (Value != "")
                            //{
                            //    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").val('" + Value + "');");
                            //    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").typeahead('val','" + Value + "');");
                            //}
                        }
                        sb.AppendLine("}");
                        sb.AppendLine("if (typeof CallBackFunction === 'function') {\r\nCallBackFunction();\r\n}");
                        sb.AppendLine("}, false);}");
                        sb.AppendLine("}");
                        sb.AppendLine("</script>");
                    }
                }
            }
            if (PrefixFor != "")
            {
                SQL = "select PrefixID, Prefix from dbo.TblPrefix where PrefixFor = @PrefixFor and SystemID = @SystemID and WarehouseID = @WarehouseID";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    { "PrefixFor", PrefixFor },
                    { "SystemID", html.SystemID },
                    { "WarehouseID", html.WarehouseID }
                };

                DataSet ds = html.LoadData(SQL, false, para);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        SQL = "SP_GET_PREFIX";
                        para.Clear();
                        para.Add("PrefixID", dr["PrefixID"].ToString());
                        para.Add("Prefix", dr["Prefix"].ToString());
                        ds = html.LoadData(SQL, true, para);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                sb.AppendLine("<script>");
                                sb.AppendLine("$(function(){");
                                sb.AppendLine("$(\"#" + ID + "\").val('" + ds.Tables[0].Rows[0][0] + "')");
                                sb.AppendLine("});");
                                sb.AppendLine("</script>");
                            }
                        }
                    }
                }
            }
            if (SameEntryToControlID != "")
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(\"#" + ID + "\").keyup(function () {");
                sb.AppendLine("$(\"#" + SameEntryToControlID + "\").val($(this).val())});");
                sb.AppendLine("</script>");
            }
            if (SingleDatePicker)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function(){");
                sb.AppendLine((ID == "" ? Name : ID) + "_ApplyDatePicker();");
                sb.AppendLine("});");
                sb.AppendLine("function " + (ID == "" ? Name : ID) + "_ApplyDatePicker() {");
                sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").daterangepicker({");
                sb.AppendLine("drops: '" + SingleDatePickerPosition.ToLower() + "',");
                sb.AppendLine("showDropdowns: true, singleDatePicker: true, autoApply: true,");
                if (LimitDateRangePickerByDay > 0)
                {
                    sb.AppendLine("minDate:moment().format('DD/MM/YYYY'),");
                    sb.AppendLine("maxDate:moment().add(" + LimitDateRangePickerByDay + ", 'days'),");
                }
                if (!DefaultShowDateValue && Value == "")
                    sb.Append("autoUpdateInput: false,");
                if (TimePicker)
                {
                    sb.AppendLine("timePicker: true,");
                    sb.AppendLine("startDate: moment(),");
                }
                sb.AppendLine("parentEl: \"body\",");
                sb.AppendLine("locale: {");
                if (!TimePicker)
                    sb.AppendLine("format: 'DD/MM/YYYY'");
                else
                {
                    sb.AppendLine("cancelLabel: 'Clear',");
                    sb.AppendLine("format: 'DD/MM/YYYY hh:mm A'");
                }
                sb.AppendLine("},");

                sb.AppendLine("});");
                if (Value != "")
                    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").data('daterangepicker').setStartDate(moment(new Date('" + Value + "')).format('DD/MM/YYYY'));");
                else if (!DefaultShowDateValue)
                {
                    sb.AppendLine("if($(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").val()=='')");
                    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").val('');");
                }
                if (TimePicker)
                {
                    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").on('cancel.daterangepicker', function(ev, picker) { $(this).val(''); });");
                }
                if (!TimePicker)
                {
                    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").on('apply.daterangepicker', function(ev, picker) {");
                    sb.AppendLine("$(this).val(picker.startDate.format('DD/MM/YYYY'));");
                    sb.AppendLine("});");
                    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").on('cancel.daterangepicker', function(ev, picker) {");
                    sb.AppendLine("$(this).val('');");
                    sb.AppendLine("});");
                }
                sb.AppendLine("}");
                sb.AppendLine("</script>");
            }
            if (DateRangePicker)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function(){");
                if (ToDay)
                    sb.AppendLine("let start = moment(); let end = moment();");
                else
                    sb.AppendLine("let start = moment().startOf('month'); let end = moment();");
                sb.AppendLine("function cb(start, end, label) {");
                sb.AppendLine("$('#" + ID + "').val(start.format('DD/MM/YYYY') + ' - ' + end.format('DD/MM/YYYY'));");
                sb.AppendLine("}");
                sb.AppendLine("$('#" + ID + "').daterangepicker({");
                sb.AppendLine("alwaysShowCalendars: true,");
                sb.AppendLine("showDropdowns: true, showWeekNumbers: true, showISOWeekNumbers: true,");
                sb.AppendLine("autoApply: true,");
                sb.AppendLine("autoUpdateInput: false,");
                if (DefaultValueDate)
                {
                    sb.AppendLine("startDate: start,");
                    sb.AppendLine("endDate: end,");
                }
                sb.AppendLine("ranges: {");
                sb.AppendLine("'Today': [moment(), moment()],");
                sb.AppendLine("'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],");
                sb.AppendLine("'Last 7 Days': [moment().subtract(6, 'days'), moment()],");
                sb.AppendLine("'Last 30 Days': [moment().subtract(29, 'days'), moment()],");
                sb.AppendLine("'This Month': [moment().startOf('month'), moment().endOf('month')],");
                sb.AppendLine("'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]");
                sb.Append("}}, cb)");
                sb.Append(".on('apply.daterangepicker', function (ev, picker) {");
                sb.AppendLine("start = picker.startDate.format('DD/MM/YYYY');");
                sb.AppendLine("end = picker.endDate.format('DD/MM/YYYY');");
                sb.AppendLine("$('#" + ID + "').val(start + ' - ' + end);");
                sb.Append("if(picker.chosenLabel == 'Today') $('#" + ID + "').val(moment().format('DD/MM/YYYY') + ' - ' + moment().format('DD/MM/YYYY'));");
                if (OnChangeFunction != "")
                {
                    sb.Append(OnChangeFunction);
                }
                sb.Append("})");
                sb.Append(".on('cancel.daterangepicker', function(ev, picker) {$(this).val('');");
                sb.AppendLine("});");
                if (DefaultValueDate)
                    sb.AppendLine("cb(start, end);");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (SQLPreFix != "")
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function(){ Reload_Prefix_" + ID + "();});");
                sb.AppendLine("function Reload_Prefix_" + ID + "(CallBackFunction) {");
                sb.AppendLine("CallBackFunction = (typeof CallBackFunction == 'undefined') ? \"\" : CallBackFunction;");
                sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQLPreFix) + "\"}, function(response) {");
                sb.AppendLine("var Data_" + (ID == "" ? Name : ID) + " = JSON.parse(JSON.parse(response).Data);");
                sb.AppendLine("if (Data_" + (ID == "" ? Name : ID) + ".length > 0) { ");
                sb.AppendLine("Data_" + (ID == "" ? Name : ID) + " = JSON.parse(JSON.parse(response).Data)[0];");
                sb.AppendLine("Data_" + (ID == "" ? Name : ID) + " = Object.values(Data_" + (ID == "" ? Name : ID) + ");");
                sb.AppendLine("var DisplayOnFields=\"" + CallBackToControlID + "\";");
                sb.AppendLine("var DFiled = DisplayOnFields.split(\",\");");
                sb.AppendLine("for (i = 0; i < DFiled.length; i++) {");
                sb.AppendLine("$(\"#\"+DFiled[i].trim()).val(Data_" + (ID == "" ? Name : ID) + "[i]);");
                sb.AppendLine("}");
                sb.AppendLine("}");
                if (PrimaryFieldValue != "0" && !isCloned)
                {
                    if (Value != "")
                        sb.AppendLine("$(\"#" + ID + "\").val('" + Value + "');");
                }
                sb.AppendLine("if (typeof CallBackFunction === 'function') {\r\nCallBackFunction();\r\n}");
                sb.AppendLine("}, false);");
                sb.AppendLine("}");
                sb.AppendLine("</script>");
            }
            if (Required)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function() {");
                sb.AppendLine("$(\"#" + ID + "\").focus(function() {");
                sb.AppendLine("var this_Alert = $(this).parent();");
                sb.AppendLine("$(this_Alert).removeClass('alert-validate');");
                sb.AppendLine("});");
                //if (Value == "")
                //{
                //	sb.AppendLine("var thisAlert = $(\"#" + ID + "\").parent();");
                //	sb.AppendLine("$(thisAlert).addClass('alert-validate');");
                //}
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (Type.ToLower() == "checkbox")
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function() { ");
                if (PrimaryFieldValue == "0" && (Value == "" || Value == "0" || Value == "False" || Value.ToLower() == "false"))
                {
                    if (!DefaultCheckBoxChecked)
                        sb.AppendLine("$(\"#" + ID + "\").removeAttr(\"checked\");");
                }
                else if (PrimaryFieldValue != "0" && Value == "False")
                {
                    sb.AppendLine("$(\"#" + ID + "\").removeAttr(\"checked\");");
                }
                else
                {
                    sb.AppendLine("$(\"#" + ID + "\").attr(\"checked\" ,\"checked\");");
                }
                sb.AppendLine("$(\"#" + ID + "\").change(function() {");
                sb.AppendLine("if($(this).is(\":checked\"))");
                sb.AppendLine("$(\"#" + ID + "\").val(1);");
                sb.AppendLine("else");
                sb.AppendLine("$(\"#" + ID + "\").val(0);");
                sb.AppendLine("});");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (IsDisplayAutoGenerateBarcodeNumber)
            {
                if (Value == "")
                {
                    //				var num = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    //.AddMilliseconds((long)html.GetUnixEpochMethod(DateTime.Now)) // put your value here
                    //.ToLocalTime().ToString("g");
                    DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    string num = double.Parse((DateTime.Now - sTime).TotalMilliseconds.ToString()).ToString(AutoGenerateBarcodeNumberDigit);
                    num = num.Substring(0, AutoGenerateBarcodeNumberDigit.Length);
                    sb.AppendLine("<script>");
                    sb.AppendLine("$(function(){");
                    sb.AppendLine("$(\"" + (ID == "" ? "input[name=" + Name + "]" : "#" + ID) + "\").val(\"" + num + "\");");
                    sb.AppendLine("});");
                    sb.AppendLine("</script>");
                }
            }
            if (!IsCustomForm)
            {
                if (!Hide)
                {
                    sb.Append("</td>");
                    if (!UseTable) sb.Append("</tr>");
                }
            }

            if (Label != "" && DivGroup)
            {
                sb.AppendLine("</div>");
            }

            return sb.ToString();
        }
    }
}