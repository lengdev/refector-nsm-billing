using System.Text;

namespace MVC
{
    public class HtmlCheckBoxCustoms : HTMLControl
    {
        private string _ClassName = "CheckBox";

        private string _GroupLabel = "";

        private bool _DisplayInRow = false;

        private bool _EndRow = true;

        private string _Checked = "";

        public string Checked
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

        public string ClassName
        {
            get
            {
                return this._ClassName;
            }
            set
            {
                this._ClassName = value;
            }
        }

        public bool DisplayInRow
        {
            get
            {
                return this._DisplayInRow;
            }
            set
            {
                this._DisplayInRow = value;
            }
        }

        public bool EndRow
        {
            get
            {
                return this._EndRow;
            }
            set
            {
                this._EndRow = value;
            }
        }

        public string GroupLabel
        {
            get
            {
                return this._GroupLabel;
            }
            set
            {
                this._GroupLabel = value;
            }
        }

        public HtmlCheckBoxCustoms()
        {
        }

        public override string GetHtml()
        {
            string[] labelAlign;
            string[] d;
            if (this.Checked.ToString() != "0")
            {
                this.Checked = "checked=\"checked\"";
            }
            StringBuilder sb = new StringBuilder();
            if (!this.DisplayInRow)
            {
                d = new string[] { "<tr><td align=\"", base.LabelAlign, "\" valign=\"", base.LabelValign, "\">", this.GroupLabel, "</td><td><ul class=\"", this.ClassName, "\">" };
                labelAlign = d;
                sb.Append(string.Concat(labelAlign));
            }
            StringBuilder stringBuilder = sb;
            d = new string[] { "<li><label for=\"", base.ID, "\">", base.Label, "</label><input type=\"checkbox\" name=\"", null, null, null, null, null, null, null, null };
            labelAlign = d;
            labelAlign[5] = base.Name ?? base.ID;
            labelAlign[6] = "\" id=\"";
            labelAlign[7] = base.ID;
            labelAlign[8] = "\" value=\"";
            labelAlign[9] = base.Value;
            labelAlign[10] = "\" ";
            labelAlign[11] = this.Checked;
            labelAlign[12] = "/></li>";
            stringBuilder.AppendLine(string.Concat(labelAlign));
            if (this.EndRow)
            {
                sb.Append("</ul></td></tr>");
            }
            return sb.ToString();
        }
    }
}