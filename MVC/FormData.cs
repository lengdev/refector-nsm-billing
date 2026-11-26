using System.Collections.Generic;

namespace MVC
{
    public class SplitColumn
    {
        public string Col = "";
        public string ID = "";
        public string SQL = "";
    }
    public class FormData : HTMLControl
    {
        public bool NotRender = false;
        public bool ShowBtnExpans = false;
        public bool AutoHideCollapse = true;
        public string FormTitle = "";
        public string FormTitleID = "";
        public string OnClickRightLabelFunction = "";
        public string AddRightLabel = "";
        public HTMLControl[] htmlControl = null;
        public DBField[] dbField = null;
        public string SQLEdit = "";
        public List<SplitColumn> SplitColumns = new List<SplitColumn>();

        public FormData()
        {
        }
        public override string GetHtml()
        {
            return "";
        }
    }
}
