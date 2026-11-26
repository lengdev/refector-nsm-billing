namespace MVC
{
    public class GridHandle
    {
        private string _SQL = "";

        public string SQL
        {
            get { return _SQL; }
            set { _SQL = value; }
        }
        private string _GridProcedureName = "";

        public string GridProcedureName
        {
            get { return _GridProcedureName; }
            set { _GridProcedureName = value; }
        }

        private GridField[] _GridTable = null;
        private GridField[] _GridTable2 = null;

        private MVC.GridPara[] _GridPara = null;

        private CustomGridField[] _CustomGridTable = null;

        public CustomGridField[] CustomGridTable
        {
            get
            {
                return this._CustomGridTable;
            }
            set
            {
                this._CustomGridTable = value;
            }
        }

        public MVC.GridPara[] GridPara
        {
            get
            {
                return this._GridPara;
            }
            set
            {
                this._GridPara = value;
            }
        }

        public GridField[] GridTable
        {
            get
            {
                return this._GridTable;
            }
            set
            {
                this._GridTable = value;
            }
        }

        public GridField[] GridTable2
        {
            get
            {
                return this._GridTable2;
            }
            set
            {
                this._GridTable2 = value;
            }
        }

        public GridHandle()
        {
        }
    }
}