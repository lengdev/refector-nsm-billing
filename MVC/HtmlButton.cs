using System.Text;

namespace MVC
{
    public class HtmlButton : HTMLControl
    {
        public string ButtonID = "";
        public string ButtonType = "button";

        public string ClassName = "btn btn-success";

        public bool Disabled = false;

        public string Event = "";

        public string ImageIcon = "fa fa-plus";

        public string TextAlign = "";

        public string TextValue = "";
        public HtmlButton()
        {
        }

        public override string GetHtml()
        {
            StringBuilder sb = new StringBuilder();
            object[] buttonType = new object[] { ButtonType, ButtonID, Event, ClassName, ImageIcon, TextValue };
            if (!IsCustomForm)
                sb.AppendFormat("<button type=\"{0}\" name=\"{1}\" id=\"{1}\" {2} class=\"{3}\">{5}</button>", buttonType);
            else
            {
                sb.AppendLine("<div class=\"" + DivClassCol + " align-text-bottom\">");
                sb.AppendLine("<div class=\"" + DivClass + "\">");
                sb.AppendFormat("<button type=\"{0}\" name=\"{1}\" id=\"{1}\" {2} class=\"{3}\"><i class=\"{4}\"></i> {5}</button>", buttonType);
                sb.AppendLine("</div></div>");
            }
            return sb.ToString();
        }
    }
}