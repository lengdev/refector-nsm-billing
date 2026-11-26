using System;
using System.Text;

namespace MVC
{
    public class HtmlDateTimePicker : HTMLControl
    {
        public HtmlDateTimePicker()
        {
        }
        public bool IsReadOnly = false;
        public bool UseJavascript = true;
        public override string GetHtml()
        {
            string ReadOnly = "";
            string required = "";
            string Hidden = "";
            if (Hide)
                Hidden = " style=\"display:none;\"";
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            ISDateTime = true;
            StringBuilder stb_result = new StringBuilder();
            if (Required)
                required = "required";
            if (IsReadOnly)
            {
                ReadOnly = "readonly=\"readonly\" tabindex=\"-1\"";
            }
            if (IsCustomForm)
            {
                stb_result.AppendLine("<ul " + Hidden + " class=\"Custom-Form\">");
                stb_result.AppendFormat("<li><label class=\"label\" for=\"{0}\">{1}</label></li>", ID, Label);
                StringBuilder stringBuilder1 = stb_result;
                object[] name = new object[] { Name ?? ID, ID, Value, Class };
                stringBuilder1.AppendFormat("<li><input " + required + " " + ReadOnly + " type=\"text\" name='{0}' id='{1}' value='{2}' class='DateTimePicker {3}' /></li>", name);
                stb_result.AppendFormat("<li><label name=\"LCheckTextBox\" id=\"{0}\" for=\"{1}\"></label></li>", ID.Replace("Txt", ""), ID);
                stb_result.AppendLine("</ul>");
            }
            else
            {
                if (Name == "") Name = ID;
                if (!UseTable) stb_result.Append("<tr " + Hidden + ">");
                stb_result.Append("<td " + Hidden + " align=\"" + LabelAlign + "\" valign=\"top\"><label for=\"" + ID + "\" class=\"col-form-label\">" + Label + "</label></td>");
                stb_result.Append("<td " + Hidden + ">");
                if (!DateRangePicker)
                {
                    stb_result.Append("<div class=\"input-group\">");
                    stb_result.Append("<input " + required + " " + ReadOnly + " type=\"text\" name=\"" + Name + "\" id=\"" + ID + "\" value=\"" + Value + "\" class=\"" + Class + "\" data-target=\"#" + ID + "\" data-toggle=\"datetimepicker\" />");
                    //stb_result.Append("<div class=\"input-group-prepend\" data-target=\"#" + ID + "\" data-toggle=\"datetimepicker\">");
                    //stb_result.Append("<span class=\"input-group-text\">");
                    //stb_result.Append("<label for=\"" + ID + "\" class=\"far fa-calendar-alt\"></label>");
                    //stb_result.Append("</span></div>");
                    stb_result.Append("</div>");
                }
                else
                {
                    stb_result.Append("<div class=\"form-group\">");
                    stb_result.Append("<input " + required + " " + ReadOnly + " type=\"text\" name=\"" + Name + "\" id=\"" + ID + "\" value=\"" + Value + "\" class=\"" + Class + "\" data-target=\"#" + ID + "\" data-toggle=\"datetimepicker\" />");
                    stb_result.Append("</div>");
                }
                stb_result.Append("</td>");
                if (!UseTable) stb_result.Append("</tr>");
            }
            if (UseJavascript)
            {
                stb_result.Append("<script>");
                stb_result.AppendLine("$(document).ready(function() {");
                stb_result.AppendLine("$(\"#" + ID + "\").daterangepicker({");
                stb_result.AppendLine("singleDatePicker: true,");
                stb_result.AppendLine("locale:");
                stb_result.AppendLine("{ format: 'DD/MM/YYYY' } })");
                //if (Value == "" && Value != null)
                //{
                //	d = new string[] { "$(\"#", ID, "\").datepicker(\"setDate\", new Date(\"", easternTimeNow.ToString("yyyy/MM/dd"), "\"));" };
                //	labelAlign = d;
                //	stb_result.AppendLine(string.Concat(labelAlign));
                //}
                //else
                //{
                //	d = new string[] { "$(\"#", ID, "\").datepicker(\"setDate\", new Date(\"", Value, "\"));" };
                //	labelAlign = d;
                //	stb_result.AppendLine(string.Concat(labelAlign));
                //}
                stb_result.AppendLine("});");
                stb_result.AppendLine("</script>");
            }
            else
            {
                stb_result.AppendLine("<script>");
                stb_result.AppendLine("$(function() {");
                stb_result.AppendLine("$(\"#" + ID + "\").daterangepicker({");
                stb_result.AppendLine("singleDatePicker: true,");
                stb_result.AppendLine("locale:");
                stb_result.AppendLine("{ format: 'DD/MM/YYYY' } })");
                //stb_result.AppendLine("var dateNow = moment().format('YYYY/MM/DD');");
                //stb_result.AppendLine("$('#" + ID + "').datetimepicker({");
                //stb_result.AppendLine("Default: true,");
                //stb_result.AppendLine("defaultDate: dateNow,");
                //stb_result.AppendLine("format: 'DD/MM/YYYY',");
                //stb_result.AppendLine("maskInput: true");
                //stb_result.AppendLine("});");
                stb_result.AppendLine("})");
                //if (IsReadOnly)
                //{
                //	stb_result.Append(".on('show.daterangepicker', function (ev, picker) {");
                //	stb_result.AppendLine("picker.container.find(\".calendar-table\").hide();");
                //	stb_result.AppendLine("});");
                //}
                stb_result.AppendLine("</script>");
            }
            if (DateRangePicker)
            {
                stb_result.AppendLine("<script type=\"text/javascript\">");
                stb_result.AppendLine("$(function() { ");
                stb_result.AppendLine("$(\"#" + ID + "\").daterangepicker({");
                stb_result.AppendLine("autoApply: true,");
                stb_result.AppendLine("orientation: \"auto\",");
                stb_result.AppendLine("locale: {");
                stb_result.AppendLine("format:'DD/MM/YYYY'");
                stb_result.AppendLine("}");
                stb_result.AppendLine("});");

                stb_result.AppendLine("$(\"#" + ID + "\").on('apply.daterangepicker', function(ev, picker) {");
                stb_result.AppendLine("$(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));");
                stb_result.AppendLine("});");
                stb_result.AppendLine("})");
                //           if (IsReadOnly)
                //           {
                //               stb_result.Append(".on('show.daterangepicker', function (ev, picker) {");
                //               stb_result.AppendLine("picker.container.find(\".calendar-table\").hide();");
                //               stb_result.AppendLine("picker.container.find(\".drp-calendar\").hide();");
                //               stb_result.AppendLine("});");
                //           }
                stb_result.AppendLine("</script>");
            }
            return stb_result.ToString();
        }
    }
}