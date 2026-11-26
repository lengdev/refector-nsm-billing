using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace MVC
{
    public class CheckBoxValue_Lable
    {
        public CheckBoxValue_Lable()
        {

        }
        public string Value = "";
        public string Label = "";
        public string DBField = "";
        public string GroupByCheckBoxVale = "";
        public string SubGroupByCheckBoxVale = "";
        public string ID = "";
        public string Name = "";
        public bool ISCheckBox = true;

    }
    public class HtmlCheckBoxGroup : HTMLControl
    {
        public CheckBoxValue_Lable[] CheckBoxGroup
        {
            set; get;
        }
        public HtmlCheckBoxGroup()
        {
            ISCheckBoxGroup = true;
            Type = "CheckBoxGroup";
        }
        public bool SetDefaultCheck = false;
        public string GetValueFromDBField = "";
        public string StyleCheckBoxGroupTitle = "card-secondary";
        public string CheckBoxGroupTitle = "Custom Elements";
        public string Colspan = "2";
        public bool ISTab = false;
        public string SqlTab = "";
        public string LinkTabField = "";
        public string TabGroupBoxID = "GroupCheckBoxTab";
        public string TabTitle = "";
        public override string GetHtml()
        {
            string IsHide = "";
            StringBuilder sb = new StringBuilder();
            DataSet ds = new DataSet();
            DataSet dsgcbg = new DataSet();
            HTML html = new HTML();
            if (Hide)
            {
                IsHide = " style=\"display:none\"";
                Type = "hidden";
            }

            if (!Hide)
            {
                if (!UseTable) sb.Append("<tr" + IsHide + ">");
                sb.Append("<td" + IsHide + " colspan=\"" + Colspan + "\">");
            }
            if (ISTab)
            {
                ds = html.LoadData(SqlTab);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //string r = html.GetUnixEpochMethod(DateTime.Now).ToString("0000");
                    sb.AppendLine("<div id=\"DivID" + TabGroupBoxID + "\">");
                    sb.AppendLine("<div class=\"card card-secondary card-tabs\">");
                    sb.AppendLine("<div class=\"card-header p-0 pt-1\">");
                    sb.AppendLine("<ul class=\"nav nav-tabs\" id=\"" + TabGroupBoxID + "\" role=\"tablist\">");
                    StringBuilder sbTap = new StringBuilder();
                    DataSet dsCh = html.LoadData(SqlCheckBoxGroup);
                    int i = 0;
                    if (CheckBoxGroupTblName != "")
                    {
                        CheckBoxGroupTblName = EncryAndDecry.Decryp(CheckBoxGroupTblName);
                        dsgcbg = html.LoadData(CheckBoxGroupTblName);
                    }
                    foreach (DataRow drdata in ds.Tables[0].Rows)
                    {
                        string setActiveTab = "";
                        if (i == 0)
                        {
                            setActiveTab = " active";
                            sb.AppendLine("<li class=\"pt-2 px-3\"><span class=\"card-title\">" + TabTitle + "</span></li>");
                        }
                        sb.AppendLine("<li class=\"nav-item\">");
                        sb.AppendLine("<a class=\"nav-link" + setActiveTab + "\" id=\"" + Regex.Replace(drdata[1].ToString(), @"\s", "") + "-tab\" data-toggle=\"pill\" data-target=\"#ID-" + drdata[0].ToString() + "-tabs\" href=\"javascript:void(0);\" role=\"tab\" aria-controls=\"ID-" + drdata[0].ToString() + "-tabs\" aria-selected=\"true\">" + drdata[1].ToString() + "</a>");
                        sb.AppendLine("</li>");
                        DataRow[] drch = dsCh.Tables[0].Select(dsCh.Tables[0].Columns[LinkTabField].ColumnName + "=" + drdata[LinkTabField].ToString());
                        string isGroupbyCheckBoxValue = "", isGroupbyCheckBoxValue2 = "";
                        string subGroupbyCheckBoxValue = "", subGroupbyCheckBoxValue2 = "";
                        string r = html.GetUnixEpochMethod(DateTime.Now).ToString("0000");
                        string GroupFiledAndValue = "";
                        setActiveTab = "";
                        if (i == 0)
                            setActiveTab = " active show";
                        i++;
                        sbTap.AppendLine("<div class=\"tab-pane fade" + setActiveTab + "\" id=\"ID-" + drdata[0].ToString() + "-tabs\" role=\"tabpanel\" aria-labelledby=\"" + Regex.Replace(drdata[1].ToString(), @"\s", "") + "-tab\">");
                        sbTap.AppendLine("<div class=\"row\">");
                        sbTap.AppendLine("<div class=\"col-12\">");
                        sbTap.AppendLine("<div id=\"" + CheckBoxGroupID + "\" class=\"form-group\">");
                        foreach (DataRow dr in drch)
                        {
                            int j = 0;
                            j++;
                            foreach (CheckBoxValue_Lable chl in CheckBoxGroup)
                            {
                                r = html.GetUnixEpochMethod(DateTime.Now).ToString("0000");
                                if (chl.ID != "")
                                {
                                    chl.Name = chl.ID;
                                    chl.ID = chl.ID + "_" + r.ToString();
                                }
                                else
                                {
                                    chl.ID = chl.Name + "_" + r.ToString();
                                }
                                string attrChecked = "";
                                if (SetDefaultCheck)
                                    attrChecked = " checked=\"checked\"";
                                if (CheckBoxGroupTblName != "")
                                {
                                    if (chl.DBField != "")
                                    {
                                        attrChecked = "";
                                        DataRow[] dataRow = dsgcbg.Tables[0].Select(chl.DBField + " = " + dr[chl.Value]);
                                        if (dataRow.Length > 0)
                                            attrChecked = " checked=\"checked\"";
                                    }
                                }

                                if (chl.GroupByCheckBoxVale != "")
                                {
                                    isGroupbyCheckBoxValue = dr[chl.GroupByCheckBoxVale].ToString();
                                    if (isGroupbyCheckBoxValue != isGroupbyCheckBoxValue2)
                                    {
                                        if (isGroupbyCheckBoxValue2 != "")
                                        {
                                            //if (chl.SubGroupByCheckBoxVale != "")
                                            sbTap.AppendLine("</div>");
                                            sbTap.AppendLine("</" + CheckBoxGroupClass + ">");
                                        }
                                        //if (chl.SubGroupByCheckBoxVale != "")
                                        //{
                                        //    sbTap.AppendLine("<div class=\"card p-2\">");
                                        //}
                                        sbTap.AppendLine("<" + CheckBoxGroupClass + ">");
                                        if (chl.SubGroupByCheckBoxVale == "")
                                        {
                                            sbTap.AppendLine("<div class=\"\">");
                                            sbTap.AppendLine("<div class=\"custom-control custom-checkbox nav\">");//sb.AppendLine("<div class=\"form-check nav nav-pills\">");
                                            sbTap.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + EncryAndDecry.Encryp(chl.DBField) + "," + dr[chl.Value] + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', 'checked'); } else { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', '');} \">");//form-check-input
                                            sbTap.AppendLine("<label for=\"" + chl.ID + "\" class=\"custom-control-label text-info\">" + dr[chl.Label] + "</label>");
                                            sbTap.AppendLine("</div>");
                                        }
                                        else
                                        {
                                            sbTap.AppendLine("<div class=\"card p-2\">");
                                            sbTap.AppendLine("<div class=\"custom-control custom-checkbox nav\">");//sb.AppendLine("<div class=\"form-check nav nav-pills\">");
                                            sbTap.AppendLine("<label for=\"" + chl.ID + "\" class=\"text-red\"><i class=\"fa-solid fa-hand-back-point-right\"></i> " + dr[chl.Label] + "</label>");
                                            sbTap.AppendLine("</div>");
                                        }
                                        isGroupbyCheckBoxValue2 = isGroupbyCheckBoxValue;
                                        GroupFiledAndValue = EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "~";
                                    }
                                }
                                else if (chl.SubGroupByCheckBoxVale != "")
                                {
                                    subGroupbyCheckBoxValue = dr[chl.SubGroupByCheckBoxVale].ToString();
                                    if (subGroupbyCheckBoxValue != subGroupbyCheckBoxValue2)
                                    {
                                        //if (subGroupbyCheckBoxValue2 != "")
                                        //  sbTap.AppendLine("</" + CheckBoxGroupClass + ">");
                                        //sbTap.AppendLine("<" + CheckBoxGroupClass + ">");
                                        sbTap.AppendLine("<div class=\"custom-control custom-checkbox nav\">");//sb.AppendLine("<div class=\"form-check nav nav-pills\">");
                                                                                                               //sbTap.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + EncryAndDecry.Encryp(chl.DBField) + "," + dr[chl.Value] + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', 'checked'); } else { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', '');} \">");//form-check-input
                                        sbTap.AppendLine("<label for=\"" + chl.ID + "\" class=\"text-info\">" + dr[chl.Label] + "</label>");
                                        sbTap.AppendLine("</div>");
                                        subGroupbyCheckBoxValue2 = subGroupbyCheckBoxValue;
                                        GroupFiledAndValue = EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "~";
                                    }
                                }
                                else
                                {
                                    //if (SetDefaultCheck)
                                    //    attrChecked = " checked=\"checked\"";
                                    sbTap.AppendLine("<div class=\"custom-control custom-checkbox\">");
                                    sbTap.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " SubCheckBoxGroup custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + GroupFiledAndValue + EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', 'checked'); } else { var result = $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + ":not(:eq(0))').is (':checked'); if (!result) $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', ''); }\">");
                                    sbTap.AppendLine("<label for=\"" + chl.ID + "\" class=\"custom-control-label\">" + dr[chl.Label] + "</label>");//form-check-label

                                    if (attrChecked != "" && j == 1)
                                    {
                                        sbTap.AppendLine("<script>");
                                        sbTap.AppendLine("$(function() { ");
                                        sbTap.AppendLine("if ($(\"#" + chl.ID + "\").is(':checked')) { $(\"#" + chl.ID + "\").closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', 'checked'); }");
                                        sbTap.AppendLine("});");
                                        sbTap.AppendLine("</script>");
                                    }
                                    sbTap.AppendLine("</div>");
                                }
                            }
                            //foreach (CheckBoxValue_Lable chl in CheckBoxGroup)
                            //{
                            //    r = html.GetUnixEpochMethod(DateTime.Now).ToString("0000");
                            //    if (chl.ID != "")
                            //    {
                            //        chl.Name = chl.ID;
                            //        chl.ID = chl.ID + "_" + r.ToString();
                            //    }
                            //    else
                            //    {
                            //        chl.ID = CheckBoxGroupID + "_" + r.ToString();
                            //    }
                            //    string attrChecked = "";
                            //    if (SetDefaultCheck)
                            //        attrChecked = " checked=\"checked\"";
                            //    if (CheckBoxGroupTblName != "")
                            //    {
                            //        if (chl.DBField != "")
                            //        {
                            //            attrChecked = "";
                            //            DataRow[] dataRow = dsgcbg.Tables[0].Select(chl.DBField + " = " + dr[chl.Value]);
                            //            if (dataRow.Length > 0)
                            //                attrChecked = " checked=\"checked\"";
                            //        }
                            //    }
                            //    if (chl.GroupByCheckBoxVale != "")
                            //    {
                            //        isGroupbyCheckBoxValue = dr[chl.GroupByCheckBoxVale].ToString();
                            //        if (isGroupbyCheckBoxValue != isGroupbyCheckBoxValue2)
                            //        {
                            //            if (isGroupbyCheckBoxValue2 != "")
                            //                sbTap.AppendLine("</" + CheckBoxGroupClass + ">");
                            //            sbTap.AppendLine("<" + CheckBoxGroupClass + ">");
                            //            sbTap.AppendLine("<div class=\"custom-control custom-checkbox nav\">");//sb.AppendLine("<div class=\"form-check nav nav-pills\">");
                            //            sbTap.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + EncryAndDecry.Encryp(chl.DBField) + "," + dr[chl.Value] + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', 'checked'); } else { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', '');} \">");//form-check-input
                            //            sbTap.AppendLine("<label for=\"" + chl.ID + "\" class=\"custom-control-label text-info\">" + dr[chl.Label] + "</label>");
                            //            sbTap.AppendLine("</div>");
                            //            isGroupbyCheckBoxValue2 = isGroupbyCheckBoxValue;
                            //            GroupFiledAndValue = EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "~";
                            //        }
                            //    }
                            //    else
                            //    {
                            //        sbTap.AppendLine("<div class=\"custom-control custom-checkbox\">");
                            //        sbTap.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " SubCheckBoxGroup custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + GroupFiledAndValue + EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', 'checked'); } else { var result = $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + ":not(:eq(0))').is (':checked'); if (!result) $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', ''); }\">");
                            //        sbTap.AppendLine("<label for=\"" + chl.ID + "\" class=\"custom-control-label\">" + dr[chl.Label] + "</label>");//form-check-label

                            //        if (attrChecked != "" && j == 1)
                            //        {
                            //            sbTap.AppendLine("<script>");
                            //            sbTap.AppendLine("$(function() { ");
                            //            sbTap.AppendLine("if ($(\"#" + chl.ID + "\").is(':checked')) { $(\"#" + chl.ID + "\").closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', 'checked'); }");
                            //            sbTap.AppendLine("});");
                            //            sbTap.AppendLine("</script>");
                            //        }
                            //        sbTap.AppendLine("</div>");
                            //    }
                            //}
                        }
                        if (isGroupbyCheckBoxValue2 != "")
                            sbTap.AppendLine("</div></" + CheckBoxGroupClass + ">");
                        sbTap.AppendLine("</div></div></div></div>");
                    }
                    sb.AppendLine("</ul></div>");
                    sb.AppendLine("<div class=\"card-body\">");
                    sb.AppendLine("<div class=\"tab-content\" id=\"" + TabGroupBoxID + "-tabContent\">");
                    sb.AppendLine(sbTap.ToString());
                    sb.AppendLine("</div></div></div>");
                    //sb.AppendLine("</div></div>");
                    sb.AppendLine("</div>");
                }
            }
            else
            {
                string r = html.GetUnixEpochMethod(DateTime.Now).ToString("0000");
                CheckBoxGroupID = CheckBoxGroupID + CheckBoxGroupTitle.Trim().Replace(" ", "") + "_" + r;
                sb.AppendLine("<div id=\"DivID" + CheckBoxGroupID + "\">");
                sb.AppendLine("<div class=\"card " + StyleCheckBoxGroupTitle + "\">");
                sb.AppendLine("<div class=\"card-header\">");
                sb.AppendLine("<span class=\"card-title\">" + CheckBoxGroupTitle + "</span>");
                sb.AppendLine("</div>");
                sb.AppendLine("<div class=\"card-body\">");
                sb.AppendLine("<div class=\"container-fluid\">");
                sb.AppendLine("<div class=\"col-12\">");
                sb.AppendLine("<div id=\"" + CheckBoxGroupID + "\" class=\"form-group\">");
                ds = html.LoadData(SqlCheckBoxGroup);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string isGroupbyCheckBoxValue2 = "";
                    string subGroupbyCheckBoxValue2 = "";
                    string GroupFiledAndValue = "";
                    if (CheckBoxGroupTblName != "")
                    {
                        CheckBoxGroupTblName = EncryAndDecry.Decryp(CheckBoxGroupTblName);
                        dsgcbg = html.LoadData(CheckBoxGroupTblName);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        foreach (CheckBoxValue_Lable chl in CheckBoxGroup)
                        {
                            r = html.GetUnixEpochMethod(DateTime.Now).ToString("0000");
                            if (chl.ID != "")
                            {
                                chl.Name = chl.ID;
                                chl.ID = chl.ID + "_" + r.ToString();
                            }
                            else
                            {
                                chl.ID = chl.Name + "_" + r.ToString();
                            }
                            string attrChecked = "";
                            if (SetDefaultCheck)
                                attrChecked = " checked=\"checked\"";
                            if (CheckBoxGroupTblName != "")
                            {
                                if (chl.DBField != "")
                                {
                                    attrChecked = "";
                                    DataRow[] dataRow = dsgcbg.Tables[0].Select(chl.DBField + " = " + dr[chl.Value]);
                                    if (dataRow.Length > 0)
                                        attrChecked = " checked=\"checked\"";
                                }
                            }

                            if (chl.GroupByCheckBoxVale != "")
                            {
                                string isGroupbyCheckBoxValue = dr[chl.GroupByCheckBoxVale].ToString();
                                if (isGroupbyCheckBoxValue != isGroupbyCheckBoxValue2)
                                {
                                    if (chl.SubGroupByCheckBoxVale == "")
                                    {
                                        if (isGroupbyCheckBoxValue2 != "")
                                            sb.AppendLine("</" + CheckBoxGroupClass + ">");
                                        sb.AppendLine("<" + CheckBoxGroupClass + ">");
                                        sb.AppendLine("<div class=\"custom-control custom-checkbox nav\">");//sb.AppendLine("<div class=\"form-check nav nav-pills\">");                                    
                                        sb.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + EncryAndDecry.Encryp(chl.DBField) + "," + dr[chl.Value] + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', 'checked'); } else { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', '');} \">");//form-check-input
                                        sb.AppendLine("<label for=\"" + chl.ID + "\" class=\"custom-control-label text-info\">" + dr[chl.Label] + "</label>");
                                    }
                                    else
                                    {
                                        if (isGroupbyCheckBoxValue2 != "")
                                            sb.AppendLine("</div>");
                                        sb.AppendLine("<div class=\"p-2 card\">");
                                        sb.AppendLine("<div class=\"custom-control custom-checkbox nav\">");
                                        sb.AppendLine("<label for=\"" + chl.ID + "\" class=\"text-red\"><i class=\"fa-solid fa-hand-back-point-right\"></i> " + dr[chl.Label] + "</label>");

                                    }

                                    sb.AppendLine("</div>");
                                    isGroupbyCheckBoxValue2 = isGroupbyCheckBoxValue;
                                    if (chl.SubGroupByCheckBoxVale == "")
                                        GroupFiledAndValue = EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "~";
                                }
                            }
                            else if (chl.SubGroupByCheckBoxVale != "")
                            {
                                string subGroupbyCheckBoxValue = dr[chl.SubGroupByCheckBoxVale].ToString();
                                if (subGroupbyCheckBoxValue != subGroupbyCheckBoxValue2)
                                {
                                    if (subGroupbyCheckBoxValue2 != "")
                                        sb.AppendLine("</" + CheckBoxGroupClass + ">");
                                    sb.AppendLine("<" + CheckBoxGroupClass + ">");
                                    sb.AppendLine("<div class=\"custom-control custom-checkbox nav\">");//sb.AppendLine("<div class=\"form-check nav nav-pills\">");
                                    sb.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + EncryAndDecry.Encryp(chl.DBField) + "," + dr[chl.Value] + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', 'checked'); } else { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').prop('checked', '');} \">");//form-check-input
                                    sb.AppendLine("<label for=\"" + chl.ID + "\" class=\"text-info\">" + dr[chl.Label] + "</label>");
                                    sb.AppendLine("</div>");
                                    subGroupbyCheckBoxValue2 = subGroupbyCheckBoxValue;
                                    GroupFiledAndValue = EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "~";
                                }
                            }
                            else
                            {
                                sb.AppendLine("<div class=\"custom-control custom-checkbox\">");
                                sb.AppendLine("<input" + attrChecked + " class=\"" + CheckBoxGroupClass + " SubCheckBoxGroup custom-control-input\" type=\"checkbox\" id=\"" + chl.ID + "\" name=\"" + chl.Name + "\" value=\"" + GroupFiledAndValue + EncryAndDecry.Encryp(chl.DBField) + "|" + dr[chl.Value] + "|" + Type + "|" + EncryAndDecry.Encryp(TableName) + "|" + EncryAndDecry.Encryp(GetIDENTITYByFiled) + "|" + EncryAndDecry.Encryp(GetValueFromDBField) + "\" onclick=\"if ($(this).is(':checked')) { $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', 'checked'); } else { var result = $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + ":not(:eq(0))').is (':checked'); if (!result) $(this).closest('" + CheckBoxGroupClass + "').find('input." + CheckBoxGroupClass + "').eq(0).prop('checked', ''); }\">");
                                sb.AppendLine("<label for=\"" + chl.ID + "\" class=\"custom-control-label\">" + dr[chl.Label] + "</label>");//form-check-label
                                sb.AppendLine("</div>");
                            }
                        }
                    }
                    if (isGroupbyCheckBoxValue2 != "")
                        sb.AppendLine("</" + CheckBoxGroupClass + ">");
                }
                sb.AppendLine("</div></div></div></div></div>");
                sb.AppendLine("</div>");
            }
            if (Required)
            {
                sb.AppendLine("<script>");
                sb.AppendLine("$(function() {");
                sb.AppendLine("checkBoxesGroup(\"" + CheckBoxGroupID + "\", \"" + CheckBoxGroupClass + "\");");
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            if (!Hide)
            {
                sb.Append("</td>");
                sb.Append("</tr>");
            }
            return sb.ToString();
        }
    }
}