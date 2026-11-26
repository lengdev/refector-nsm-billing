namespace MVC
{
    public class GridPara
    {
        private string _ParaName = "";

        private string _Value = "";

        public string ParaName
        {
            get
            {
                return this._ParaName;
            }
            set
            {
                this._ParaName = value;
            }
        }

        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
            }
        }

        public GridPara()
        {
        }
    }
}