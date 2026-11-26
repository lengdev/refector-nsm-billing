using System.Text;

namespace MVC
{
    public class RadioOptionBindDataToCbo : HTMLControl
    {
        public string ConditionFieldVale = "";
        public bool DefaultCheckBok = false;
        public string DisplayField = "";
        public bool ISAutoComplete = false;
        public bool IsUseConnection2 = false;
        public string OPTLabel = "";
        public string RadioValue = "";
        public string SelectValue = "";
        public bool ShowAll = true;

        public RadioOptionBindDataToCbo()
        {
        }

        public override string GetHtml()
        {
            StringBuilder sb = new StringBuilder();
            string DefaultCheck = "";
            if (DefaultCheckBok)
            {
                DefaultCheck = "checked=\"checked\"";
            }
            string str = Hide == true ? " style=\"display:none;\"" : "";
            sb.AppendLine("<td" + str + "><label class=\"col-form-label text-nowrap\">");
            sb.AppendLine("<input type=\"radio\" name=\"RD" + ID + "\" alt=\"" + OPTLabel + "\" value=\"" + RadioValue + "\" " + DefaultCheck + "/>&nbsp;&nbsp;" + OPTLabel);
            sb.AppendLine("</label></td>");
            return sb.ToString();
        }
    }
}