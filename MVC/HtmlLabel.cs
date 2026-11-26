using System.Text;

namespace MVC
{
    public class HtmlLabel : HTMLControl
    {
        public HtmlLabel()
        {
        }

        public override string GetHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Label);
            return sb.ToString();
        }
    }
}