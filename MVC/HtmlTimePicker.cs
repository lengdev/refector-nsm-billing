using System.Text;

namespace MVC
{
    public class HtmlTimePicker : HTMLControl
    {
        public HtmlTimePicker()
        {
        }

        public override string GetHtml()
        {
            StringBuilder sb = new StringBuilder();
            string[] labelAlign = new string[] { "<tr><td align=\"", base.LabelAlign, "\" valign=\"", base.LabelValign, "\">", base.Label, "</td>" };
            sb.Append(string.Concat(labelAlign));
            StringBuilder stringBuilder = sb;
            object[] objArray = new object[] { "<td><input type=\"text\" name=\"", null, null, null, null, null, null, null, null, null, null };
            object[] name = objArray;
            name[1] = base.Name ?? base.ID;
            name[2] = "\" id=\"";
            name[3] = base.ID;
            name[4] = "\" value=\"";
            name[5] = base.Value;
            name[6] = "\" class=\"";
            name[7] = base.Class;
            name[8] = "\"  maxlength=\"";
            name[9] = base.MaxLength;
            name[10] = "\" /></td>";
            stringBuilder.AppendLine(string.Concat(name));
            labelAlign = new string[] { "<td width=\"22\"><label name=\"LCheckTextBox\" id=\"L", base.ID.Replace("Txt", ""), "\" for=\"", base.ID, "\"></label></td></tr>" };
            sb.Append(string.Concat(labelAlign));
            sb.Append(string.Concat("<script>$(function(){$(\"#", base.ID, "\").timeEntry({show24Hours:true});$(\".timeEntry_control\").hide();});"));
            if (base.Value != "")
            {
                labelAlign = new string[] { "$(\"#", base.ID, "\").timeEntry({defaultTime: '", base.Value, "'}); " };
                sb.Append(string.Concat(labelAlign));
            }
            sb.Append("</script>");
            return sb.ToString();
        }
    }
}