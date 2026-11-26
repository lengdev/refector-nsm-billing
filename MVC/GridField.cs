using System.Collections.Generic;

namespace MVC
{
    public class GridField
    {
        public string SumGroupByFieldName = "";
        public string OnClickFunction = "";
        public string ExpandableTableFileNameIsNotNull = "";
        public string AvatarKendoGrid = "";
        public bool KendoGridAutoComplect = true;
        public bool DecodeEntities = false;
        public bool FilterableDate = false;
        public bool Filterable = true;
        public bool HideDeteleOnNotMatch = false;
        public bool HideEditOnActionNameNotMatch = false;
        public string ShowActionNameOnConditionValue = "";
        public string ShowActionNameOnConditionField = "";
        public string ActionName = "";
        public string ActionIcon = "";
        public string ActionClickFunction = "";
        public string Style = "";
        public bool isUrl = false;
        public bool isEmail = false;
        public string Format = "";
        public string FormatDateTime = "";
        public string EditFunction = "";
        public string DeleteFunction = "";
        public string DeleteWithTableName = "";
        public string DeletedCallBackFunction = "";
        public string DeletePrimaryTblName = "";
        public string DeletePrimaryFiledName = "";
        public string DeletePrimaryValue = "";
        public string DeletePrimaryTblNameDependOnTblName = "";
        public bool HideEdit = false;
        public bool HideDelete = false;
        public bool Editable = false;
        public string Type = "";
        public bool PrimaryKeyFieldName = false;


        public string ClassName = "";

        public int Colspan = 0;

        public bool CustomRow = false;

        public int DataTableNumber = 0;

        public string DeleteUseProcedureName = "";

        public string DisplayFields = "";

        public bool DisplayOneRow = false;

        public bool DisplayTextArea = false;

        public bool DisplayTextBox = false;
        public bool DisplayCheckBox = false;
        public bool DisplayBarcode = false;
        public bool ShowOnlyOneImg = false;

        public string FieldName = "";

        public string GroupLabel = "";

        public string Header = "";

        public bool HideOnPrint = false;

        public bool HideOnPrintTotalFooter = false;

        public string ImagePath = "";

        public bool IsBorderRight = false;

        public bool IsExternalLink = false;

        public bool IsGroupByField = false;

        public string OrderBy = "asc";

        public bool IsHiddenField = false;

        public bool IsModifyField = false;

        public bool ISTotal = false;

        public bool IsTotalFooter = false;

        public string Link = "";

        public string DataTitleOverlay = "";

        public string LinkField = "";

        public string LinkParameterField = "";

        public bool NewPage = false;

        public bool OnOneRow = false;

        public string PrintHeader = "";

        public string TextAlign = "";

        public string TotalGroupFieldName = "";

        public string TotalLabel = "";

        public string Width = "";

        public bool IsShowFormatUnitVariant = false;
        public string PrimaryID = "";

        public string DeleteWithTableNameEncrypted = "";
        public string RawBarcodeTemplate = "";

        public bool DisplayBadge { get; set; } = false;
        public string BadgePrefixField { get; set; } = "";
        public string BadgeSourceField { get; set; } = "";
        public Dictionary<string, string> BadgeClassMap { get; set; } = new Dictionary<string, string>();

        public GridField()
        {
        }
    }
}