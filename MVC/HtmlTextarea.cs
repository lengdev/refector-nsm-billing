using System.Text;
using System.Web;

namespace MVC
{
    public class HtmlTextarea : HTMLControl
    {
        public bool IsTextEditorSummernote = false;
        public string DeleteTDRowFunction = "";
        public bool IsReadOnly = false;
        public int Row = 3;
        public HtmlTextarea()
        {
            Type = "textarea";

        }

        public override string GetHtml()
        {
            string IsHide = "";
            string required = "";
            if (Hide)
            {
                IsHide = " style=\"display:none\"";
            }
            string[] labelAlign;
            string[] strArrays;
            base.ID = base.ID == null ? "" : base.ID;
            string valid = !base.ValidationField ? "" : " required=true";
            string ReadOnly = "";
            if (Required)
                required = "required";
            if (Label != "" && placeholder == "")
                placeholder = "Enter " + Label.Replace(":", "").ToLower();

            if (this.IsReadOnly)
            {
                ReadOnly = "readonly=\"readonly\"";
            }
            StringBuilder stb_result = new StringBuilder();
            if (!base.IsCustomForm)
            {
                if (Name == "") Name = ID;
                if (!UseTable) stb_result.Append("<tr>");
                stb_result.Append("<td align=\"" + base.LabelAlign + "\" valign=\"" + base.LabelValign + "\"><label for=\"" + ID + "\" class=\"col-label text-nowrap\">" + Label + "</label></td>");
                stb_result.Append("<td><textarea rows=" + Row + " " + (Required == true ? " " + required : "") + " " + ReadOnly + " placeholder=\"" + placeholder + "\" name=\"" + Name + "\" id=\"" + base.ID + "\" class=\"" + base.Class + "\" " + Style + ">");
                stb_result.Append("" + HttpUtility.HtmlDecode(Value) + "");
                stb_result.Append("</textarea>");
                stb_result.Append("</td>");
                if (!UseTable) stb_result.Append("</tr>");
            }
            else if (base.ShowUL)
            {
                stb_result.AppendLine("<ul class=\"Custom-Form\">");
                stb_result.AppendFormat("<li><label class=\"label text-nowrap\" for=\"{0}\">{1}</label></li>", base.ID, base.Label);
                StringBuilder stringBuilder2 = stb_result;
                strArrays = new string[] { "<li><textarea rows=" + Row + " ", ReadOnly, " placeholder=\"" + placeholder + "\" name=\"", null, null, null, null, null, null, null, null };
                labelAlign = strArrays;
                labelAlign[3] = base.Name ?? base.ID;
                labelAlign[4] = "\" id=\"";
                labelAlign[5] = base.ID;
                labelAlign[6] = "\" class=\"";
                labelAlign[7] = base.Class;
                labelAlign[8] = "\" >";
                labelAlign[9] = base.Value;
                labelAlign[10] = "</textarea></li>";
                stringBuilder2.Append(string.Concat(labelAlign));
                stb_result.AppendFormat("<li><label class=\"text-nowrap\" name=\"LCheckTextBox\" id=\"{0}\" for=\"{1}\"></label></li>", base.ID.Replace("Txt", ""), base.ID);
                stb_result.AppendLine("</ul>");
            }
            else
            {
                if (DivClass == "")
                    DivClass = "wrap-input2";
                if (Label != "")
                {
                    stb_result.AppendLine("<div " + IsHide + " class=\"col-sm-auto\">");
                    stb_result.AppendLine("<label for=\"" + ID + "\" class=\"col-form-label khmerFont1 text-nowrap\">" + Label + "</label></div>");
                }
                stb_result.AppendLine("<div " + IsHide + " class=\"" + DivClassCol + " align-text-bottom\">");
                stb_result.AppendLine("<div class=\"" + DivClass + " " + (Required == true ? "validate-input" : "") + (Required == true && Value == "" ? " alert-validate" : "") + "\" " + (Required == true ? "data-validate=\"" + (Label == "" ? ID : Label) + " is required\"" : "") + ">");
                stb_result.Append("<textarea rows=" + Row + " " + Style + " " + ReadOnly + " name=\"" + (Name == "" ? ID : Name) + "\" id=\"" + ID + "\" ");
                stb_result.Append("" + required + " placeholder=\"" + placeholder + "\" class=\"" + Class + "\" style=\"min-height: 80px; field-sizing: content;\">" + HttpUtility.HtmlDecode(Value) + "</textarea>");
                if (DeleteTDRowFunction != "")
                {
                    stb_result.AppendLine("<div class=\"input-delete\" title=\"Delete Row\" data-validate=\"Delete Service Plan Row\" onclick=\"" + DeleteTDRowFunction + "\"></div>");
                }
                //stb_result.AppendLine("<span class=\"focus-input2\"></span>");
                stb_result.AppendLine("</div></div>");
                //StringBuilder stringBuilder1 = stb_result;
                //strArrays = new string[] { "<textarea ", ReadOnly, valid, " name=\"", null, null, null, null, null, null, null, null };
                //labelAlign = strArrays;
                //labelAlign[4] = base.Name ?? base.ID;
                //labelAlign[5] = "\" id=\"";
                //labelAlign[6] = base.ID;
                //labelAlign[7] = "\" class=\"";
                //labelAlign[8] = base.Class;
                //labelAlign[9] = "\" >";
                //labelAlign[10] = base.Value;
                //labelAlign[11] = "</textarea>";
                //stringBuilder1.Append(string.Concat(labelAlign));
            }
            if (IsTextEditorSummernote)
            {
                stb_result.AppendLine("<script>");
                stb_result.AppendLine("$(function(){");
                stb_result.AppendLine("$(\"#" + ID + "\").summernote({height: 300,");
                stb_result.AppendLine("placeholder: " + (placeholder == "" ? "'Please entry your description'" : "'" + placeholder + "'") + ",");
                stb_result.AppendLine("toolbar: [");
                stb_result.AppendLine("['style', ['style']],");
                stb_result.AppendLine("['font', ['bold', 'italic', 'underline', 'clear']],");
                stb_result.AppendLine("['fontsize', ['fontsize']], ['color', ['color']],");
                stb_result.AppendLine("['para', ['ul', 'ol', 'paragraph']],['height', ['height']],");
                stb_result.AppendLine("['view', ['fullscreen','codeview']]");
                stb_result.AppendLine("]");
                stb_result.AppendLine("});");
                stb_result.AppendLine("});");
                stb_result.AppendLine("</script>");

            }

            return stb_result.ToString();
        }
    }
}