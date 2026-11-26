using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MVC
{
    public class HtmlMyTable : HTMLControl
    {
        /// <summary>Automatically calculates total for each row.</summary>
        public bool AutoTotalByRow = false;
        public bool ShowIncluceVAT = true;
        public bool ShowGrandTotal = true;
        public bool ShowRowTotal = true;

        /// <summary>Input name used for total calculation.</summary>
        public string TotalByInputName = "";

        /// <summary>Textarea name used for total calculation.</summary>
        public string TotalByTextAreaName = "";

        /// <summary>Input name for quantity total calculation.</summary>
        public string TotalByInputNameQTY = "";

        /// <summary>Input name for amount total calculation.</summary>
        public string TotalByInputNameAmount = "";

        /// <summary>Input name for discount total in USD calculation.</summary>
        public string TotalByInputNameDiscountUSD = "";

        /// <summary>Input name for tax in USD calculation.</summary>
        public string TotalByInputNameTAX = "";

        /// <summary>Input name for grand total in USD calculation.</summary>
        public string TotalByInputNameGrandTotal = "";

        /// <summary>Input name where the total will be displayed.</summary>
        public string TotalDisplayOnInputName = "";

        /// <summary>Allow editing VAT value in the table.</summary>
        public bool AllowEditVAT = false;

        /// <summary>Label text for customer notes.</summary>
        public string CustomerNotesLabel = "Customer Notes";

        /// <summary>Apply table header (TH) background styling.</summary>
        public bool ApplyTblTHBackground = true;

        /// <summary>Show remark section below table.</summary>
        public bool ShowRemark = false;

        /// <summary>Show bank information in the table.</summary>
        public bool ShowBankInfo = false;

        /// <summary>Show only total row without table details.</summary>
        public bool ShowTotalOnly = false;

        /// <summary>SQL ORDER BY clause or column name for table sorting.</summary>
        public string OrderBy = "";

        /// <summary>Show deposit column in the table.</summary>
        public bool ShowDeposit = true;

        /// <summary>Show balance column in the table.</summary>
        public bool ShowBalance = true;

        /// <summary>Enable responsive layout for the table.</summary>
        public bool TableResponsive = true;
        /// <summary>Enable responsive layout for the table with custom .</summary>
        public bool UserClassTableResponsive = false;

        /// <summary>Show footer row in the table.</summary>
        public bool ShowFooter = true;

        /// <summary>Show total row in the table.</summary>
        public bool ShowTotal = true;

        /// <summary>Show terms and conditions section in the table.</summary>
        public bool ShowTermsConditions = true;

        /// <summary>Show customer notes section in the table.</summary>
        public bool ShowCustomerNotes = true;

        /// <summary>Footer colspan for merged columns.</summary>
        public int FooterColspan = 2;

        /// <summary>Footer rowspan for merged rows.</summary>
        public int FooterRowspan = 8;

        /// <summary>Subtotal colspan for merged columns.</summary>
        public int SubTotalColspan = 5;

        /// <summary>Array of DBField objects linked to the table for dynamic data binding.</summary>
        public DBField[] dbField = null;

        /// <summary>Name of the HTML table.</summary>
        public string InputName = "";

        /// <summary>HTML table name attribute.</summary>
        public string HtmlTblName = "";

        /// <summary>HTML table ID attribute.</summary>
        public string HtmlTblID = "TblItemInfo";

        /// <summary>CSS class for the table element.</summary>
        public string ClassName = "table table-bordered text-nowrap khmerFont1 table-valign-middle";

        /// <summary>JavaScript callback function to execute after table operations.</summary>
        public string ScriptCallBackFunction = "";

        /// <summary>JavaScript callback function to get data for editing the table.</summary>
        public string ScriptCallBackGetDataTblEditFunction = "";

        /// <summary>List of table header rows.</summary>
        public List<TblTHead> tblTHead = null;

        /// <summary>List of table body rows.</summary>
        public List<TblTBody> tblTBody = null;

        public bool ISInputVAT = false;

        /// <summary>
        /// Constructor sets the HTML table name to table ID if not provided.
        /// </summary>
        public HtmlMyTable()
        {
            HtmlTblName = HtmlTblName == "" ? HtmlTblID : HtmlTblName;
        }

        public override string GetHtml()
        {
            HTML html = new HTML();
            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            StringBuilder script = new StringBuilder();
            StringBuilder sbTblScript2 = new StringBuilder();
            string _isTagsInput = "";
            string _onTagsInputAddedFunction = "";
            string sql = "";
            string Data = "";
            string AddCallBackFunction = "";
            string str_script = "";

            if (AutoTotalByRow)
            {
                str_script = @"function " + HtmlTblID + @"_TotalAll() {
var SubTotal = 0, GrandTotalTAX = 0;";
                if (TotalByInputName != "")
                    str_script += @"$(""#" + HtmlTblID + " input[name=" + TotalByInputName + @"]"").each(function (i) {";
                else if (TotalByTextAreaName != "")
                    str_script += @"$(""#" + HtmlTblID + " textarea[name=" + TotalByTextAreaName + @"]"").each(function (i) {";
                str_script += @"
var QTY = $(""#" + HtmlTblID + " input[name=" + TotalByInputNameQTY + @"]"").eq(i).val();
var UnitPrice = $(""#" + HtmlTblID + " input[name=" + TotalByInputNameAmount + @"]"").eq(i).val();            
QTY = (QTY == """" ? 0 : accounting.unformat(QTY));
UnitPrice = (UnitPrice == """" ? 0 : accounting.unformat(UnitPrice));
var Amount = (QTY * UnitPrice);";
                if (TotalByInputNameDiscountUSD != "")
                {
                    str_script += @"
var DisUSD = $(""#" + HtmlTblID + " input[name=" + TotalByInputNameDiscountUSD + @"]"").eq(i).val();
DisUSD = (DisUSD == """" ? 0 : accounting.unformat(DisUSD));
Amount = Amount - DisUSD;";
                }
                str_script += @"
SubTotal += Amount;
$(""#" + HtmlTblID + " input[name=" + TotalDisplayOnInputName + @"]"").eq(i).val(accounting.formatNumber(Amount, 2));";

                if (TotalByInputNameTAX != "")
                {
                    str_script += @"
var _VAT = $(""#" + HtmlTblID + $@" input[name={TotalByInputNameTAX}]"").eq(i).val();
_VAT = (_VAT == """" ? 0 : (accounting.unformat(_VAT)*Amount)/100);
GrandTotalTAX = accounting.formatNumber(Amount + _VAT, 2);
SubTotal += _VAT;
";
                }

                if (TotalByInputNameGrandTotal != "")
                {
                    str_script += @"
$(""#" + HtmlTblID + $@" input[name={TotalByInputNameGrandTotal}]"").eq(i).val(accounting.formatNumber(GrandTotalTAX, 2));";
                }

                str_script += @"});";

                str_script += @"
$(""#" + HtmlTblID + @" label[name=SubTotal]"").html(accounting.formatNumber(SubTotal, 2));
var Total = accounting.unformat(SubTotal);
var TxtDisUSD = $(""#" + HtmlTblID + @" input[name=TxtDisUSD]"").val();
TxtDisUSD = (TxtDisUSD == """" ? 0 : accounting.unformat(TxtDisUSD));
Total = (Total - TxtDisUSD);
$(""#" + HtmlTblID + @" label[name=Total]"").html(accounting.formatNumber(Total, 2));";
                if (AllowEditVAT)
                {
                    str_script += @"
if($('#" + HtmlTblID + @" #chIncluceVAT').is(':checked')) {
var VAT = ((accounting.unformat(Total) * 10) / 100);
$(""#" + HtmlTblID + @" label[name=VAT]"").html(accounting.formatNumber(VAT, 2));
Total += accounting.unformat(VAT);
//Total = accounting.unformat($(""#" + HtmlTblID + @" label[name=SubTotal]"").html()) + accounting.unformat($(""#" + HtmlTblID + @" label[name=VAT]"").html());
}
else {
$(""#" + HtmlTblID + @" label[name=VAT]"").html('0.00');
}";
                }
                str_script += @"
var GrandTotal = accounting.unformat(Total);
$(""#" + HtmlTblID + @" label[name=GrandTotal]"").html(accounting.formatNumber(GrandTotal, 2));
}";
                if (AllowEditVAT)
                {
                    str_script += @"$('#" + HtmlTblID + @" #chIncluceVAT').click(function() { " + HtmlTblID + @"_TotalAll(); });";
                }
                if (TotalByInputNameQTY != "")
                {
                    str_script += @"
function " + HtmlTblID + @"_CallTotalAll() {
	$(""#" + HtmlTblID + " input[name=" + TotalByInputNameQTY + @"], #" + HtmlTblID + " input[name=TxtDisUSD], #" + HtmlTblID + " input[name=" + TotalByInputNameAmount + "]" + (TotalByInputNameDiscountUSD == "" ? "" : ", #" + HtmlTblID + " input[name=" + TotalByInputNameDiscountUSD + "]") + (TotalByInputNameTAX == "" ? "" : ", #" + HtmlTblID + " input[name=" + TotalByInputNameTAX + "]") + @""").on(""keyup change"", function(e) {" + HtmlTblID + @"_TotalAll(); });
};";
                    ScriptCallBackFunction += "," + HtmlTblID + "_CallTotalAll()";
                }
            }
            script.AppendLine("<script>");
            script.AppendLine(str_script);
            script.AppendLine("objChangeHtmlTblID=\"" + HtmlTblID + "\";");
            script.AppendLine("var rowData_" + HtmlTblID + " = '';");
            script.AppendLine("var RowFields2_" + HtmlTblID + " = [];");
            script.AppendLine("$(function(){ setTimeout(function() {");
            if (TableResponsive)
            {
                if (UserClassTableResponsive)
                    sb.AppendLine("<div class=\"col-12\"><div class=\"tableresponsive\">");
                else
                    sb.AppendLine("<div class=\"col-12\"><div class=\"table-responsive\">");
            }
            else
                sb.AppendLine("<div class=\"col-12\"><div>");
            sb.AppendLine("<table class=\"" + ClassName + "\" id=\"" + HtmlTblID + "\" name=\"" + HtmlTblName + "\">");
            if (ApplyTblTHBackground)
                sb.AppendLine("<thead class=\"text-center modal-header-info\" style=\"color: white; background-color: #F89C39; text-align:center !important\">");
            else
                sb.AppendLine("<thead class=\"text-center\">");
            foreach (TblTHead th in tblTHead)
            {
                if (th.OpenTR != "")
                    sb.AppendLine(th.OpenTR);
                string Hide = "";
                if (th.Hide)
                    Hide = " style=\"display:none;\" ";
                sb.AppendLine("<th" + Hide + " class=\"" + th.Class + "\" " + (th.colspan != 0 ? "colspan=" + th.colspan.ToString() : "") + ">" + th.Label + "</th>");

                if (th.CloseTR != "")
                    sb.AppendLine(th.CloseTR);
            }
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            List<TableKeepDataSaving> tableKeepData = new List<TableKeepDataSaving>();
            if (tblTBody != null)
            {
                foreach (TblTBody tr in tblTBody)
                {
                    if (tr.html_Control != null)
                    {
                        if (tr.OpenTR != "")
                            sb1.Append(tr.OpenTR);
                        foreach (HTMLControl htmlControl in tr.html_Control)
                        {
                            if (AddCallBackFunction == "" && htmlControl.AddCallBackFunction != "")
                                AddCallBackFunction = htmlControl.AddCallBackFunction.Replace(",", ";");
                            else if (AddCallBackFunction != "" && htmlControl.AddCallBackFunction != "")
                                AddCallBackFunction += ";" + htmlControl.AddCallBackFunction.Replace(",", ";");
                            if (htmlControl.isTagsInput)
                            {
                                _isTagsInput = htmlControl.Name;
                                _onTagsInputAddedFunction = htmlControl.onTagsInputAddedFunction;
                            }
                            if (htmlControl.DBField != "")
                            {
                                string str = htmlControl.IsAllowInputOnlyNumber == true || htmlControl.ISNumeric == true ? "Unformat" : "";
                                if (htmlControl.ISDateTime || htmlControl.SingleDatePicker)
                                    str = "IsDate";
                                tableKeepData.Add(
                                    new TableKeepDataSaving()
                                    {
                                        InputName = InputName,
                                        FieldName = htmlControl.DBField,
                                        Value = htmlControl.Name,
                                        Required = htmlControl.Required == true ? "Required" : "",
                                        Unformat = str,
                                        TRName = TRName,
                                        TDName = TDName,
                                        TableName = TableName,
                                        RequiredInputIfOtherFieldIsNotNull = htmlControl.RequiredInputIfOtherFieldIsNotNull == true ? "RequiredInputIfOtherFieldIsNotNull" : "",
                                        AutoGenerateBarcodeNumberIfNotEntry = htmlControl.AutoGenerateBarcodeNumberIfNotEntry == true ? "AutoGenerateBarcodeNumberIfNotEntry" : "",
                                        AutoGenerateBarcodeNumberDigit = htmlControl.AutoGenerateBarcodeNumberDigit.Length.ToString(),
                                        DestinationFile = htmlControl.DestinationFile,
                                        HtmlTblName = HtmlTblID,
                                        ISInteger = htmlControl.ISInteger,
                                        ISSummerNote = htmlControl.ISSummerNote,
                                        Digit = htmlControl.Digit,
                                        GetValueFromTextAreaName = htmlControl.GetValueFromTextAreaName,
                                        Name = htmlControl.Name,
                                        ID = htmlControl.ID,
                                        PrimaryFieldName = htmlControl.PrimaryFieldName,
                                        PrimaryFieldValue = htmlControl.PrimaryFieldValue.ToString(),
                                        IsKendoMultiColumnComboBox = htmlControl.IsKendoMultiColumnComboBox
                                    });
                                if (htmlControl.HiddenField != "")
                                {
                                    //sb.AppendLine("<input value=\"" + HidenFieldValue + "\" type=\"hidden\" id=\"" + (ID == "" ? "" : ID + "_" + HiddenFeild) + "\" name=\"" + (ID == "" ? HiddenFeild : ID + "_" + HiddenFeild) + "\" />");
                                    string[] HFields = htmlControl.HiddenField.Split(',');
                                    for (int i = 0; i < HFields.Length; i++)
                                    {
                                        //script.AppendLine("RowFields2_" + HtmlTblID + ".push({FieldName: '" + HFields[i].Trim() + "',Value: '" + HFields[i].Trim() + "', Required: '" + (htmlControl.Required == true ? "Required" : "") + "',Format: '" + str + "', TableName: `" + TableName + "`,RequiredInputIfOtherFieldIsNotNull: '" + (htmlControl.RequiredInputIfOtherFieldIsNotNull == true ? "RequiredInputIfOtherFieldIsNotNull" : "") + "',AutoGenerateBarcodeNumberIfNotEntry: '" + (htmlControl.AutoGenerateBarcodeNumberIfNotEntry == true ? "AutoGenerateBarcodeNumberIfNotEntry" : "") + "',AutoGenerateBarcodeNumberDigit: '" + htmlControl.AutoGenerateBarcodeNumberDigit.Length.ToString() + "',InputName:'" + InputName + "',DestinationFile:'" + htmlControl.DestinationFile + "',ISInteger:'" + htmlControl.ISInteger + "',SetValue:" + (htmlControl.SetValue == "" ? "''" : htmlControl.SetValue) + ", ISSummerNote:'" + htmlControl.ISSummerNote + "', Digit:'" + htmlControl.Digit + "'});");
                                        script.AppendLine("RowFields2_" + HtmlTblID + ".push({FieldName: '" + HFields[i].Trim() + "',Value: '" + HFields[i].Trim() + "', Required: '" + (htmlControl.Required == true ? "Required" : "") + "'});");
                                    }
                                }
                                script.AppendLine("RowFields2_" + HtmlTblID + ".push({FieldName: '" + htmlControl.DBField + "',Value: '" + htmlControl.Name + "', Required: '" + (htmlControl.Required == true ? "Required" : "") + "',Format: '" + str + "', TableName: `" + TableName + "`,RequiredInputIfOtherFieldIsNotNull: '" + (htmlControl.RequiredInputIfOtherFieldIsNotNull == true ? "RequiredInputIfOtherFieldIsNotNull" : "") + "',AutoGenerateBarcodeNumberIfNotEntry: '" + (htmlControl.AutoGenerateBarcodeNumberIfNotEntry == true ? "AutoGenerateBarcodeNumberIfNotEntry" : "") + "',AutoGenerateBarcodeNumberDigit: '" + htmlControl.AutoGenerateBarcodeNumberDigit.Length.ToString() + "',InputName:'" + InputName + "',DestinationFile:'" + htmlControl.DestinationFile + "',ISInteger:'" + htmlControl.ISInteger + "',SetValue:" + (htmlControl.SetValue == "" ? "''" : htmlControl.SetValue) + ", ISSummerNote:'" + htmlControl.ISSummerNote + "', Digit:'" + htmlControl.Digit + "', GetValueFromTextareaName:'" + htmlControl.GetValueFromTextAreaName + "', IsKendoMultiColumnComboBox:'" + htmlControl.IsKendoMultiColumnComboBox + "'});");
                                htmlControl.Required = false;
                            }
                            string Hide = "";
                            if (htmlControl.Hide)
                                Hide = " style=\"display:none;\" ";
                            sb1.Append("<td" + Hide + " name=\"" + htmlControl.TDName + "\" align=\"" + htmlControl.Align + "\" class=\"" + tr.Class + "\" " + (tr.colspan != 0 ? "colspan=" + tr.colspan.ToString() : "") + ">" + htmlControl.GetHtml() + "</td>");
                        }
                        if (tr.TDDelete != "")
                            sb1.Append("<td align=\"" + tr.TDAlign + "\" class=\"" + tr.Class + "\" " + (tr.colspan != 0 ? "colspan=" + tr.colspan.ToString() : "") + ">" + tr.TDDelete + "</td>");
                    }
                    if (tr.CloseTR != "")
                        sb1.Append(tr.CloseTR);
                }
            }

            if (dbField != null)
            {
                foreach (DBField db in dbField)
                {
                    string str = db.IsNumber == true ? "Unformat" : "";
                    if (db.IsDate)
                        str = "IsDate";
                    tableKeepData.Add(
                        new TableKeepDataSaving()
                        {
                            InputName = InputName,
                            FieldName = db.DBFieldName,
                            Value = db.Value,
                            Unformat = str,
                            TRName = TRName,
                            TDName = TDName,
                            TableName = db.TblName,
                            RequiredInputIfOtherFieldIsNotNull = db.RequiredInputIfOtherFieldIsNotNull == true ? "RequiredInputIfOtherFieldIsNotNull" : "",
                            AutoGenerateBarcodeNumberIfNotEntry = db.AutoGenerateBarcodeNumberIfNotEntry == true ? "AutoGenerateBarcodeNumberIfNotEntry" : "",
                            AutoGenerateBarcodeNumberDigit = AutoGenerateBarcodeNumberDigit.Length.ToString(),
                            DestinationFile = db.DestinationFile,
                            HtmlTblName = HtmlTblID,
                            ISInteger = db.ISInteger,
                            SetValue = db.SetValue,
                            ISSummerNote = db.ISSummerNote,
                            Digit = db.Digit,
                            GetValueFromTextAreaName = db.GetValueFromTextAreaName,
                            Name = db.Name,
                            ID = db.ID,
                            PrimaryFieldName = db.PrimaryFieldName,
                            PrimaryFieldValue = db.PrimaryFieldValue.ToString(),
                            IsKendoMultiColumnComboBox = db.IsKendoMultiColumnComboBox
                        });
                    script.AppendLine("RowFields2_" + HtmlTblID + ".push({FieldName: '" + db.DBFieldName + "',Value: '" + db.Value + "', Required: '',Format: '" + str + "', TableName: `" + db.TblName + "`,RequiredInputIfOtherFieldIsNotNull: '" + (db.RequiredInputIfOtherFieldIsNotNull == true ? "RequiredInputIfOtherFieldIsNotNull" : "") + "',AutoGenerateBarcodeNumberIfNotEntry: '" + (db.AutoGenerateBarcodeNumberIfNotEntry == true ? "AutoGenerateBarcodeNumberIfNotEntry" : "") + "',AutoGenerateBarcodeNumberDigit: '" + db.AutoGenerateBarcodeNumberDigit.Length.ToString() + "',InputName:'" + InputName + "',DestinationFile:'" + db.DestinationFile + "',ISInteger:'" + db.ISInteger + "',SetValue:" + (db.SetValue == "" ? "''" : db.SetValue) + ",ISSummerNote:'" + db.ISSummerNote + "', Digit:'" + db.Digit + "', GetValueFromTextareaName:'" + db.GetValueFromTextAreaName + "', IsKendoMultiColumnComboBox:'" + db.IsKendoMultiColumnComboBox + "'});");
                }
            }
            tableKeepDataSaving = tableKeepData.ToArray();
            StringBuilder apScript = new StringBuilder();
            StringBuilder aptd = new StringBuilder();
            StringReader strReader = new StringReader(sb1.ToString());
            bool isHaveScript = false;

            string line;
            while ((line = strReader.ReadLine()) != null)
            {
                if (line == "</script>")
                    isHaveScript = false;
                if (isHaveScript)
                {
                    apScript.AppendLine(line);
                }
                if (line == "<script>")
                    isHaveScript = true;
                if (!isHaveScript && line != "<script>" && line != "</script>")
                {
                    aptd.AppendLine(line);
                }
            }

            script.AppendLine("rowData_" + HtmlTblID + " +=`" + aptd.ToString() + "`;");
            if (PrimaryFieldValue != "0")
            {
                if (SQL == "")
                {
                    string[] TableNames = TableName.Split(new char[] { ',' });
                    TableName = TableNames[0].ToString().Trim();
                    if (TableName != "")
                    {
                        sql = "select * from " + TableName + " where " + GetIDENTITYByFiled + " = " + PrimaryFieldValue;
                        if (CustomCondition != "")
                            sql += " and " + CustomCondition;
                        if (OrderBy != "")
                            sql += " order by " + OrderBy;
                    }
                }
                else sql = SQL;
                if (sql != "")
                    Data = html.JsonData(sql);
                //AddTableRow(inputName, TblID, TRName, TblRow, TDNoName, THIS, callback, SQL, SQLCallBack, DisplayOnFiled, callbackOnAddRow, isTagsInput)
                script.AppendLine("AddTableRow(\"" + InputName + "\", \"" + HtmlTblID + "\", \"" + TRName + "\", rowData_" + HtmlTblID + ", \"" + TDName + "\", this,`" + ScriptCallBackFunction + "`,'', '', " + ScriptCallBackGetDataTblEditFunction + "(`" + Data + "`),'','" + _isTagsInput + "','" + _onTagsInputAddedFunction + "',RowFields2_" + HtmlTblID + ");");
                if (ScriptCallBackGetDataTblEditFunction == "")
                {
                    //ScriptCallBackFunction += "," + HtmlTblID + @"_TotalAll()";
                    if (sql != "")
                    {
                        script.AppendLine("var TblData=" + Data + ";");
                        //script.AppendLine("setTimeout(function() { ");
                        //SetValueTableRow(Data, inputName, TblID, TRName, TblRow, TDNoName, THIS, callback, SQL, SQLCallBack, DisplayOnFiled, callbackOnAddRow, isTagsInput, onTagsInputAddedFunction, RowFields)14
                        script.AppendLine("SetValueTableRow(TblData,'" + InputName + "','" + HtmlTblID + "','" + TRName + "',rowData_" + HtmlTblID + ", '" + TDName + "',this,`" + ScriptCallBackFunction + "`,'','','','','" + _isTagsInput + "','" + _onTagsInputAddedFunction + "',RowFields2_" + HtmlTblID + ");");
                        script.AppendLine("setTimeout(function() { ");
                        script.AppendLine("if (typeof " + HtmlTblID + "_TotalAll === \"function\") { " + HtmlTblID + "_TotalAll(); }");
                        script.AppendLine("}, 2000);");
                        //script.AppendLine("}, 500);");
                    }
                    else
                        script.AppendLine("AddTableRow(\"" + InputName + "\", \"" + HtmlTblID + "\", \"" + TRName + "\", rowData_" + HtmlTblID + ", \"" + TDName + "\", this,`" + ScriptCallBackFunction + "`,'', '', '', '','" + _isTagsInput + "','" + _onTagsInputAddedFunction + "',RowFields2_" + HtmlTblID + ");");
                }
            }
            else
                script.AppendLine("AddTableRow(\"" + InputName + "\", \"" + HtmlTblID + "\", \"" + TRName + "\", rowData_" + HtmlTblID + ", \"" + TDName + "\", this,`" + ScriptCallBackFunction + "`,'', '', '', '','" + _isTagsInput + "','" + _onTagsInputAddedFunction + "',RowFields2_" + HtmlTblID + ");");
            if (AddCallBackFunction != "" && PrimaryFieldValue != "0")
                script.AppendLine(AddCallBackFunction);
            script.AppendLine("},900);");
            script.AppendLine("});");
            //script.AppendLine(apScript.ToString());
            script.AppendLine("</script>");
            sb.AppendLine("</tbody>");
            if (ShowFooter)
            {
                sb.AppendLine("<tfoot>");
                //sb.AppendLine("<div class=\"row\"><div class=\"col-auto\">");
                //sb.AppendLine("<div class=\"icheck-material-cyan icheck-inline\">");
                //sb.AppendLine("<input type=\"checkbox\" name=\"IsIncludedVAT\" id=\"ExcludedVAT\" value=\"false\">");
                //sb.AppendLine("<label for=\"ExcludedVAT\">Excluded VAT</label></div></div>");
                //sb.AppendLine("<div class=\"col-auto\"><div class=\"icheck-material-cyan icheck-inline\">");
                //sb.AppendLine("<input type=\"checkbox\" name=\"IsIncludedVAT\" id=\"IncludedVAT\" value=\"true\">");
                //sb.AppendLine("<label for=\"IncludedVAT\">Included VAT</label></div></div>");
                //sb.AppendLine("<div class=\"col-auto\">");
                //sb.AppendLine("<div class=\"form-group pt-1\">");
                //sb.AppendLine("<div class=\"custom-control custom-switch\">");
                //sb.AppendLine("<input type=\"checkbox\" checked=\"\" class=\"custom-control-input\" id=\"isNextInvoice\">");
                //sb.AppendLine("<label class=\"custom-control-label\" for=\"isNextInvoice\">Is Next Invoice</label>");
                //sb.AppendLine("</div></div></div></div>");
                if (ShowCustomerNotes || ShowTermsConditions || ShowRemark || ShowBankInfo)
                {
                    sb.AppendLine("<tr><td colspan=\"" + FooterColspan + "\" rowspan=\"" + FooterRowspan + "\" class=\"p-2\" valign=\"top\">");
                    if (ShowCustomerNotes)
                    {
                        sb.AppendLine("<div class=\"row\"><div class=\"col-12\"><div class=\"mb-3 floating-label\">");
                        sb.AppendLine("<textarea rows=\"5\" placeholder=\"" + CustomerNotesLabel + "\" class=\"text-nowrap-pre text-sm p-2 border border-1 col-12\" style=\"min-height: 150px; resize: none; field-sizing: content;\" id=\"Customer_Notes\" name=\"Customer_Notes\"></textarea>");//Customer Notes Will not be displayed on this bill
                        sb.AppendLine("<label for=\"Customer_Notes\">" + CustomerNotesLabel + "</label></div></div></div>");
                    }
                    if (ShowTermsConditions)
                    {
                        sb.AppendLine("<div class=\"row\"><div class=\"col-12\"><div class=\"mb-3 floating-label\">");
                        sb.AppendLine("<textarea rows=\"5\" placeholder=\"Enter the terms and conditions of your business to be displayed in your transaction\" class=\"text-nowrap-pre text-sm p-2 border border-1 col-12\" style=\"min-height: 150px; resize: none; field-sizing: content;\" id=\"TermsConditions\" name=\"TermsConditions\"></textarea>");
                        sb.AppendLine("<label for=\"TermsConditions\">Terms & Conditions</label></div></div></div>");
                    }
                    if (ShowRemark)
                    {
                        sb.AppendLine("<div class=\"row\"><div class=\"col-12\"><div class=\"mb-3 floating-label\">");
                        sb.AppendLine("<textarea rows=\"5\" placeholder=\"Remark\" class=\"text-nowrap-pre text-sm p-2 border border-1 col-12\" style=\"min-height: 150px; resize: none; field-sizing: content;\" id=\"RemarkInfo\" name=\"RemarkInfo\"></textarea>");//Customer Notes Will not be displayed on this bill
                        sb.AppendLine("<label for=\"Remark\">Remark</label></div></div></div>");
                    }
                    if (ShowBankInfo)
                    {
                        sb.AppendLine("<div class=\"row\"><div class=\"col-12\"><div class=\"mb-3 floating-label\">");
                        sb.AppendLine("<textarea rows=\"5\" placeholder=\"Bank Information\" class=\"text-nowrap-pre text-sm p-2 border border-1 col-12\" style=\"min-height: 150px; resize: none; field-sizing: content;\" id=\"BankInfo\" name=\"BankInfo\"></textarea>");//Customer Notes Will not be displayed on this bill
                        sb.AppendLine("<label for=\"BankInfo\">Bank Information</label></div></div></div>");
                    }
                    sb.AppendLine("</td></tr>");
                }
                if (ShowTotal)
                {
                    sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                    sb.AppendLine("<label class=\"col-form-label\">Sub Total:</label>");
                    sb.AppendLine("</td><td align=\"center\">");
                    sb.AppendLine("<label class=\"col-form-label\" name=\"SubTotal\">0.00</label>");
                    sb.AppendLine("</td></tr>");
                    sb.AppendLine("<tr style=\"display:none\"><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                    sb.AppendLine("<label class=\"col-form-label khmerFont1\">");
                    sb.AppendLine("បញ្ចុះតម្លៃ / Discount %:</label>");
                    sb.AppendLine("</td><td align=\"center\"><div class=\"wrap-input3\">");
                    sb.AppendLine("<input type=\"text\" class=\"input2 text-center text-bold\" value=\"0.00\" name=\"TxtDisP\">");
                    sb.AppendLine("<span class=\"focus-input2\"></span></div></td></tr>");
                    sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                    sb.AppendLine("<label class=\"col-form-label khmerFont1\">");
                    sb.AppendLine("បញ្ចុះតម្លៃ / Discount $:</label></td>");
                    sb.AppendLine("<td align=\"center\"><div class=\"wrap-input3\">");
                    sb.AppendLine("<input type=\"text\" class=\"input2 text-center text-bold\" value=\"0.00\" id=\"TxtDisUSD\" name=\"TxtDisUSD\" placeholder=\"Discount $\">");
                    sb.AppendLine("<span class=\"focus-input2\"></span></div>");
                    sb.AppendLine("<script>");
                    sb.AppendLine("$('#" + HtmlTblID + " #TxtDisUSD').autoNumeric({vMin: \"-9999999999" + Digit.Replace("0", "9") + "\",vMax: \"999999999999999999999999999999999999999999999999999999" + Digit.Replace("0", "9") + "\"});");
                    sb.AppendLine("</script>");
                    sb.AppendLine("</td></tr>");
                    if (ShowRowTotal)
                    {
                        sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                        sb.AppendLine("<label class=\"col-form-label khmerFont1\">សរុប / Total:</label>");
                        sb.AppendLine("</td><td align=\"center\">");
                        sb.AppendLine("<label class=\"col-form-label\" name=\"Total\">0.00</label>");
                        sb.AppendLine("</td></tr>");
                    }
                    if (ShowIncluceVAT)
                    {
                        sb.AppendLine("<tr name=\"trVAT\"><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                        sb.AppendLine("<label class=\"col-form-label khmerFont1\">");
                        if (AllowEditVAT)
                        {
                            sb.AppendLine("<input type=\"checkbox\" name=\"chIncluceVAT\" id=\"chIncluceVAT\" value=\"false\" />");
                            //sb.AppendLine("<script>");
                            //sb.AppendLine("function "+ HtmlTblID+ "_chIncluceVAT() {");
                            //sb.AppendLine("if($('#"+ HtmlTblID + " #chIncluceVAT').is(':checked')) {");
                            //sb.AppendLine("$('#" + HtmlTblID + " label[name=VAT]').html('<div class=\"wrap-input3\"><input placeholder=\"VAT 10%\" type=\"text\" class=\"input2 text-center\" value=\"0.00\" id=\"TxtVAT\" name=\"TxtVAT\" /><span class=\"focus-input2\"></span></div>');$('#" + HtmlTblID + " #TxtVAT').focus().select();");
                            //sb.AppendLine("$('#" + HtmlTblID + " #TxtVAT').autoNumeric({vMin: \"-9999999999" + Digit.Replace("0", "9") + "\",vMax: \"999999999999999999999999999999999999999999999999999999" + Digit.Replace("0", "9") + "\"});");
                            //sb.AppendLine("}");
                            //sb.AppendLine("else {");
                            //sb.AppendLine("$('#" + HtmlTblID + " label[name=VAT]').html('0.00');");
                            //sb.AppendLine("}");
                            //sb.AppendLine("}");
                            //sb.AppendLine("</script>");
                        }
                        sb.AppendLine("ពន្ធបន្ថែម / VAT 10%:");
                        sb.AppendLine("</label>");
                        sb.AppendLine("</td><td align=\"center\">");
                        if (ISInputVAT)
                        {
                            sb.AppendLine("<div class=\"wrap-input3\">");
                            sb.AppendLine("<input type=\"text\" class=\"input2 text-center text-bold\" value=\"0.00\" id=\"VAT\" name=\"VAT\" placeholder=\"Enter VAT\">");
                            sb.AppendLine("<span class=\"focus-input2\"></span></div>");
                        }
                        else
                        {
                            sb.AppendLine("<label class=\"col-form-label\" name=\"VAT\">0.00</label>");
                        }
                        sb.AppendLine("</td></tr>");
                    }
                    if (ShowGrandTotal)
                    {
                        sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                        sb.AppendLine("<label class=\"col-form-label khmerFont1\">សរុបចុងក្រោយ / Grand Total:</label>");
                        sb.AppendLine("</td><td align=\"center\">");
                        sb.AppendLine("<label class=\"col-form-label\" name=\"GrandTotal\">0.00</label>");
                        sb.AppendLine("</td></tr>");
                    }
                    if (ShowDeposit)
                    {
                        sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                        sb.AppendLine("<label class=\"col-form-label khmerFont1\">");
                        sb.AppendLine("លុយកក់ / Deposit:</label>");
                        sb.AppendLine("</td><td align=\"center\"><div class=\"wrap-input3\">");
                        sb.AppendLine("<input type=\"text\" class=\"input2 text-center text-bold\" value=\"0.00\" name=\"TxtDeposit\">");
                        sb.AppendLine("<span class=\"focus-input2\"></span></div></td></tr>");
                    }
                    if (ShowBalance)
                    {
                        sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                        sb.AppendLine("<label class=\"col-form-label khmerFont1\">");
                        sb.AppendLine("នៅសល់ / Balance:</label>");
                        sb.AppendLine("</td><td align=\"center\"><div class=\"wrap-input3\">");
                        sb.AppendLine("<input readonly type=\"text\" class=\"input2 text-center text-bold\" value=\"0.00\" name=\"TxtBalance\">");
                        sb.AppendLine("<span class=\"focus-input2\"></span></div></td></tr>");
                    }
                }
                else if (ShowTotalOnly)
                {
                    sb.AppendLine("<tr><td colspan=\"" + SubTotalColspan + "\" align=\"right\">");
                    sb.AppendLine("<label class=\"col-form-label khmerFont1\">សរុប / Total:</label>");
                    sb.AppendLine("</td><td align=\"center\">");
                    sb.AppendLine("<label class=\"col-form-label\" name=\"SubTotal\">0.00</label>");
                    sb.AppendLine("</td></tr>");
                }
                //sb.AppendLine("<tr name=\"TRTotalAS\"><td colspan=\"5\" align=\"right\">");
                //sb.AppendLine("<label class=\"col-form-label khmerFont1\" name=\"LBLTotalAS\">សរុបជា / Total​ As:</label>");
                //sb.AppendLine("</td><td align=\"center\">");
                //sb.AppendLine("<label class=\"col-form-label\" name=\"TotalAS\">0.00</label>");
                //sb.AppendLine("</td></tr>");
                sb.AppendLine("</tfoot>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<script>" + apScript.ToString() + "</script>");
            //script.AppendLine(str_script);
            sb.AppendLine(script.ToString());
            return sb.ToString();
        }
    }
    public class TblTHead
    {
        public string OpenTR = "";
        public string CloseTR = "";
        public string Class = "";
        public string Label = "";
        public int colspan = 0;
        public bool Hide = false;

    }
    public class TblTBody
    {
        public HTMLControl[] html_Control = null;
        public string OpenTR = "";
        public string CloseTR = "";
        public string Class = "";
        public int colspan = 0;
        public string TDDelete = "";
        public string TDAlign = "";
    }
}