namespace MVC
{
    public class CustomGridField
    {
        private string _Header = "";

        private string _ID = "";

        private string _ClassName = "TR-Grid";

        private string _TextAlign = "Left";

        private bool _HiddenField = false;

        private string _Width = "";

        public string CheckField
        {
            get;
            set;
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

        public string DBField
        {
            get;
            set;
        }

        public string EventChange
        {
            get;
            set;
        }

        public string Header
        {
            get
            {
                return this._Header;
            }
            set
            {
                this._Header = value;
            }
        }

        public bool HiddenField
        {
            get
            {
                return this._HiddenField;
            }
            set
            {
                this._HiddenField = value;
            }
        }

        public MVC.HTMLControl HTMLControl
        {
            get;
            set;
        }

        public string ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                this._ID = value;
            }
        }

        public bool ISInteger
        {
            get;
            set;
        }

        public bool ISModifyField
        {
            get;
            set;
        }

        public bool ISNumeric
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string TextAlign
        {
            get
            {
                return this._TextAlign;
            }
            set
            {
                this._TextAlign = value;
            }
        }

        public string Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                this._Width = value;
            }
        }

        public CustomGridField()
        {
        }
    }
}