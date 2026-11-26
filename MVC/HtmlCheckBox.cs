using System.Text;

namespace MVC
{
    public class HtmlCheckBox : HTMLControl
    {
        private string _ChClass = "CheckBoxDefault";

        private bool _Checked;

        public string ChClass
        {
            get
            {
                return this._ChClass;
            }
            set
            {
                this._ChClass = value;
            }
        }

        public bool Checked
        {
            get
            {
                return this._Checked;
            }
            set
            {
                this._Checked = value;
            }
        }

        public HtmlCheckBox()
        {
        }

        public override string GetHtml()
        {
            StringBuilder sb = new StringBuilder();
            string CheckedBox = "";
            string IsHide = "";
            if (Checked)
            {
                CheckedBox = " checked=\"checked\"";
            }
            if (Hide)
            {
                IsHide = " style=\"display:none\"";
            }
            if (IsCustomForm)
            {
                sb.AppendLine("<ul class=\"Custom-Form\">");
                sb.AppendFormat("<li><label class=\"col-form-label\" for=\"{0}\">{1}</label></li>", base.ID, base.Label);
                StringBuilder stringBuilder1 = sb;
                object[] name = new object[] { base.Name ?? base.ID, base.ID, base.Value, base.Class };
                stringBuilder1.AppendFormat("<li><input type=\"checkbox\" name='{0}' id='{1}' value='{2}' /></li>", name);
                sb.AppendFormat("<li><label name=\"LCheckTextBox\" id=\"{0}\" for=\"{1}\"></label></li>", base.ID.Replace("Txt", ""), base.ID);
                sb.AppendLine("</ul>");
            }
            else
            {
                if (Name == "")
                    Name = ID;
                if (!UseTable) sb.Append("<tr " + IsHide + ">");
                if (!ISSwitchCheck)
                {
                    sb.AppendLine("<td" + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label class=\"col-form-label\">" + Label + "</label></td>");
                    sb.AppendLine("<td" + IsHide + " valign=\"" + LabelValign + "\"><p style=\"margin-top: 7px !important;\">");//<div class=\"icheck-success d-inline\">
                    sb.AppendLine("<input type=\"checkbox\" name=\"" + Name + "\" id=\"" + ID + "\" class=\"" + ChClass + "\" value=\"" + (Value == "" ? "False" : Value) + "\" " + CheckedBox + "/>");
                    sb.AppendLine("<label for=\"" + ID + "\"></label></p></td>");
                    sb.AppendLine("<script>");
                    sb.AppendLine("$(function() {");
                    sb.AppendLine("$(\"#" + ID + "\").click(function(){");
                    sb.AppendLine("if ($(\"#" + ID + "\").is(':checked')) {");
                    sb.AppendLine("$(\"#" + ID + "\").val('True')");
                    sb.AppendLine("}");
                    sb.AppendLine("else {");
                    sb.AppendLine("$(\"#" + ID + "\").val('False')");
                    sb.AppendLine("};");
                    sb.AppendLine("});");
                    if (Value != "")
                    {
                        if (Value == "True")
                            sb.AppendLine("$(\"#" + ID + "\").attr(\"checked\",\"checked\");");
                        else
                            sb.AppendLine("$(\"#" + ID + "\").removeAttr(\"checked\");");
                    }

                    sb.AppendLine("});");
                    sb.AppendLine("</script>");
                    //sb.AppendLine("<td" + IsHide + " width=\"22\"><label name=\"LCheckBox\" id=\"L" + ID.Replace("Ch", "") + "\" for=\"" + ID + "\"></label></td>");
                }
                else
                {
                    sb.AppendLine("<td" + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label for=\"" + ID + "\" class=\"col-form-label text-nowrap\">" + Label + "</label></td>");
                    sb.AppendLine("<td" + IsHide + " valign=\"" + LabelValign + "\">");
                    sb.AppendLine("<input type=\"checkbox\" name=\"" + ID + "\" id=\"" + ID + "\" value=\"False\" />");//data-bootstrap-switch data-off-color=\"danger\" value=\"false\" data-on-color=\"success\"
                    sb.AppendLine("<script>");
                    sb.AppendLine("$(function() {");
                    sb.AppendLine("$(\"#" + ID + "\").bootstrapSwitch('state', $(\"#" + ID + "\").prop('checked'));");
                    //sb.AppendLine("$(\"#" + ID + "\").bootstrapSwitch();");
                    sb.AppendLine("$(\"#" + ID + "\").on('switchChange.bootstrapSwitch',function(event, state){");
                    sb.AppendLine("if (state) {");
                    sb.AppendLine("$(\"#" + ID + "\").val('True')");
                    sb.AppendLine("}");
                    sb.AppendLine("else {");
                    sb.AppendLine("$(\"#" + ID + "\").val('False')");
                    sb.AppendLine("};");
                    sb.AppendLine("});");
                    sb.AppendLine("});");
                    sb.AppendLine("</script>");
                    sb.AppendLine("</td>");
                }
                if (!UseTable) sb.Append("</tr>");
            }
            return sb.ToString();
        }
    }
}