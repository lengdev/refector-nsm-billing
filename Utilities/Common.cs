using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
namespace Utilities
{
    public class Common
    {
        private string _ServerRoot;

        private static string[] formats;

        public string ServerRoot
        {
            get
            {
                return _ServerRoot;
            }
            set
            {
                _ServerRoot = GetServerRoot();
            }
        }
        public string HtmlDecode(string input)
        {
            return HttpUtility.HtmlDecode(HttpUtility.UrlDecode(input));
        }
        public Common()
        {
            ServerRoot = GetServerRoot();
            string[] strArrays = new string[] { "MM/dd/yyyy HH:mm:ss tt", "MM/dd/yyyy HH:mm:ss", "M/dd/yyyy H:mm:ss tt", "M/dd/yyyy H:mm:ss" };
            formats = strArrays;
        }
        private string _TableClassName = "table table-bordered table-head-fixed text-nowrap";
        public string TableClassName
        {
            get { return _TableClassName; }
            set { _TableClassName = value; }
        }
        public string FixedTableHeight = "class =\"divbody table-responsive p-0\"";
        private string _CboSearchStyle = "selectpicker";
        public string CboSearchStyle
        {
            get { return _CboSearchStyle; }
            set { _CboSearchStyle = value; }
        }
        private string _CboDataStyle = "btn-success";
        public string CboDataStyle
        {
            get { return _CboDataStyle; }
            set { _CboDataStyle = value; }
        }
        private string _btnSearchStyle = "btn btn-success";
        public string btnSearchStyle
        {
            set { _btnSearchStyle = value; }
            get { return _btnSearchStyle; }
        }

        public SqlResult CheckPermission(string PageCode)
        {
            SqlResult sqlResult = new SqlResult()
            {
                Result = ResultType.Failed,
                Message = "Sorry, you don't have permission for accessing this page. <br />Please contact administrator."
            };
            SqlResult result = sqlResult;
            string SQL = "SELECT p.PermissionPageCode FROM dbo.User_Permission up ";
            SQL = string.Concat(SQL, "INNER JOIN dbo.PermissionPage p ON p.PermissionPageID = up.PermissionID ");
            string str = SQL;
            Common common = new Common();
            string[] sessionName = new string[] { str, "WHERE up.UserID = ", common.getSessionName("UserID", ""), " AND p.PermissionPageCode = '", PageCode, "'" };
            SQL = string.Concat(sessionName);
            try
            {
                DBHelper dbHelper = new DBHelper();
                dbHelper.InitiateConnection();
                SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                SQL_C.CommandText = SQL;
                SQL_C.CommandType = CommandType.Text;
                if (common.ExecuteDataTable(SQL_C).Rows.Count > 0)
                {
                    result.Result = ResultType.Successful;
                }
                dbHelper.CloseConnection();
            }
            catch (Exception exception)
            {
                result.Message = exception.Message;
                result.Result = ResultType.Failed;
            }
            return result;
        }

        public void CheckSession()
        {
            string Path = HttpContext.Current.Request.ApplicationPath;
            if (Path.Replace("/", "") == "")
            {
                ServerRoot = GetServerRoot();
                Path = ServerRoot;
            }
            string CurrentPath = HttpContext.Current.Request.CurrentExecutionFilePath;
            HttpContext.Current.Session["CurrentPath"] = CurrentPath;
            if (!(HttpContext.Current.Session["ISLogin"] != null && !(HttpContext.Current.Session["ISLogin"].ToString() == "")))
            {
                HttpContext.Current.Response.Write(string.Concat("<script> window.location.href='", Path, "/Login'; </script>"));
                HttpContext.Current.Response.End();
            }
            else if (HttpContext.Current.Session["UserName"] == null || HttpContext.Current.Session["UserName"].ToString() == "")
            {
                HttpContext.Current.Response.Write(string.Concat("<script> window.location.href='", Path, "/Login'; </script>"));
                HttpContext.Current.Response.End();
            }
        }
        public bool CheckSessionOK(string OK)
        {
            bool isHaveSession = true;
            string Path = HttpContext.Current.Request.ApplicationPath;
            if (Path.Replace("/", "") == "")
            {
                ServerRoot = GetServerRoot();
                Path = ServerRoot;
            }
            string CurrentPath = HttpContext.Current.Request.CurrentExecutionFilePath;
            HttpContext.Current.Session["CurrentPath"] = CurrentPath;
            if (!(HttpContext.Current.Session["ISLogin"] != null && !(HttpContext.Current.Session["ISLogin"].ToString() == "")))
            {
                isHaveSession = false;
                HttpContext.Current.Response.Write(string.Concat("<script> window.location.href='", Path, "/Login'; </script>"));
                HttpContext.Current.Response.End();
            }
            else if (HttpContext.Current.Session["UserName"] == null || HttpContext.Current.Session["UserName"].ToString() == "")
            {
                isHaveSession = false;
                HttpContext.Current.Response.Write(string.Concat("<script> window.location.href='", Path, "/Login'; </script>"));
                HttpContext.Current.Response.End();
            }
            return isHaveSession;
        }
        public void clearSessionName()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.RemoveAll();
            HttpContext.Current.Session.Abandon();
        }

        public ArrayList ConvertDataTableToArray(DataTable dt)
        {
            ArrayList converted = new ArrayList(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                converted.Add(row);
            }
            return converted;
        }

        public string ConvertDataTableToXML(DataTable dt_data)
        {
            DataSet ds_Data = new DataSet();
            StringBuilder sbSQL = new StringBuilder();
            StringWriter swSQL = new StringWriter(sbSQL);
            ds_Data.Merge(dt_data, true, MissingSchemaAction.AddWithKey);
            foreach (DataColumn col in ds_Data.Tables[0].Columns)
            {
                col.ColumnMapping = MappingType.Attribute;
            }
            ds_Data.WriteXml(swSQL, XmlWriteMode.WriteSchema);
            return sbSQL.ToString();
        }

        public DataTable ConvertXMLToData(string str_xml)
        {
            DataTable dt_data = new DataTable();
            dt_data.ReadXml(new XmlTextReader(new StringReader(str_xml)));
            return dt_data;
        }


        public string CreatePageLinks(int CurrPage, int totalRecords, int pageSize, int totalPages, int TotalNoLink = 10, string LoadGridFunction = "", string FormID = "")
        {
            int min;
            int max;
            string str;
            object[] objArray;
            if (LoadGridFunction == "") LoadGridFunction = "LoadTableGrid";
            StringBuilder strPager = new StringBuilder();
            strPager.AppendLine("<table class=\"\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">");
            strPager.AppendLine("<tfoot><tr>");
            strPager.AppendFormat("<td>Page size:</td><td>&nbsp;</td>");
            strPager.AppendLine("<td><select class=\"form-control\" style=\"width:60px; height:30px; padding:0; margin: 0;\" name=\"cbopagesize\" id=\"cbopagesize" + FormID + "\">");
            strPager.AppendLine("<option value=\"10\">10</option>");
            strPager.AppendLine("<option value=\"30\">30</option>");
            strPager.AppendLine("<option value=\"50\">50</option>");
            strPager.AppendLine("<option value=\"100\">100</option>");
            strPager.AppendLine("<option value=\"0\">All</option>");
            strPager.AppendLine("</select>");
            strPager.AppendLine("</td><td>&nbsp;</td>");
            strPager.AppendFormat("<td>Page:</td><td><input class=\"form-control\" type=\"text\" name=\"txtpaging\" id=\"txtpaging\" style=\"color:black; height:30px; width:60px;\" value=\"{0}\" /></td><td> - {1} of {2} ", CurrPage, totalPages, totalRecords);
            strPager.AppendLine("&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>");
            if (totalRecords <= pageSize)
            {
                str = strPager.ToString();
            }
            else
            {
                strPager.AppendLine("<div>");
                strPager.AppendLine("<ul class=\"pagination m-0\">");
                //strPager.AppendLine("<ul id=\"icons\" class=\"ui-widget ui-helper-clearfix\">");
                int currentPage;
                if (!int.TryParse(CurrPage.ToString(), out currentPage))
                {
                    currentPage = 1;
                }
                if (currentPage > totalPages)
                {
                    currentPage = totalPages;
                }
                if (currentPage > 1)
                {
                    strPager.AppendLine("<li class=\"page-item\" title=\"First\" onclick=\"" + LoadGridFunction + "(1)\"><a href=\"javascript:void(0);\" class=\"page-link\">&laquo;</a></li>");
                    strPager.AppendLine(string.Concat("<li class=\"page-item\" title=\"Previous\" onclick=\"" + LoadGridFunction + "(", currentPage - 2 + 1, ")\"><a href=\"javascript:void(0);\" class=\"page-link\">&#8249;</a></li>"));
                }
                if (TotalNoLink >= totalPages)
                {
                    min = 1;
                    max = totalPages;
                }
                else
                {
                    max = currentPage - (TotalNoLink / 2) <= 0 ? TotalNoLink : currentPage + (TotalNoLink / 2) - ((TotalNoLink - 1) % 2);
                    if (max > totalPages)
                    {
                        max = totalPages;
                    }
                    min = max - TotalNoLink + 1 > 0 ? max - TotalNoLink + 1 : 1;
                }
                int n = min;
                while (n <= max)
                {
                    if (n > totalPages)
                    {
                        break;
                    }
                    else
                    {
                        if (n == currentPage)
                        {
                            objArray = new object[] { "<li class=\"page-item\" style=\"height:16px; line-height:18px; color:#F60\" title=\"", n, "\"><span class=\"page-link\" style=\"color:red\">", n, "</span></li>" };
                            strPager.AppendLine(string.Concat(objArray));
                        }
                        else
                        {
                            objArray = new object[] { "<li class=\"page-item\" title=\"", n, "\" onclick=\"" + LoadGridFunction + "(", n, ")\"><a class=\"page-link\" href=\"javascript:void(0);\">", n, "</a></li>" };
                            strPager.AppendLine(string.Concat(objArray));
                        }
                        n++;
                    }
                }
                if (currentPage < totalPages)
                {
                    strPager.AppendLine(string.Concat("<li class=\"page-item\" title=\"Next\" onclick=\"" + LoadGridFunction + "(", currentPage + 1, ")\"><a class=\"page-link\" href=\"javascript:void(0);\">&#8250;</a></li>"));
                    strPager.AppendLine(string.Concat("<li class=\"page-item\" title=\"Last\" onclick=\"" + LoadGridFunction + "(", totalPages, ")\"><a class=\"page-link\" href=\"javascript:void(0);\">&raquo;</a></li>"));
                }
                strPager.AppendLine("</url>");
                strPager.AppendLine("</td></tr></tfoot></table>");
                str = strPager.ToString();
            }
            return str;
        }

        public string ExcecuteString(string SQL)
        {
            DBHelper dbHelper = new DBHelper();
            Exception ex;
            string str_result = "";
            try
            {
                dbHelper.InitiateConnection();
                try
                {
                    SqlCommand command = dbHelper.sqlConnection.CreateCommand();
                    command.CommandText = SQL;
                    object obj = command.ExecuteScalar();
                    if (obj != null)
                    {
                        str_result = obj.ToString();
                    }
                }
                catch (Exception exception)
                {
                    ex = exception;
                    str_result = ex.Message;
                }
                dbHelper.CloseConnection();
            }
            catch (Exception exception1)
            {
                ex = exception1;
                try
                {
                    dbHelper.CloseConnection();
                }
                catch
                {
                }
                str_result = ex.Message;
            }
            return str_result;
        }

        public string ExcecuteString(string SQL, SqlConnection sqlConnection, SqlTransaction sql_tran)
        {
            string str_result = "";
            SqlCommand command = sqlConnection.CreateCommand();
            command.Transaction = sql_tran;
            command.CommandText = SQL;
            object obj = command.ExecuteScalar();
            if (obj != null)
            {
                str_result = obj.ToString();
            }
            return str_result;
        }

        public DataSet ExecuteDataSet(SqlCommand command)
        {
            SqlDataAdapter sqlAdapter = new SqlDataAdapter(command);
            DataSet ds_data = new DataSet();
            sqlAdapter.Fill(ds_data);
            return ds_data;
        }

        public DataTable ExecuteDataTable(SqlCommand command)
        {
            SqlDataAdapter sqlAdapter = new SqlDataAdapter(command);
            DataTable dt_data = new DataTable();
            sqlAdapter.Fill(dt_data);
            return dt_data;
        }

        public string GetHostName()
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            string IP = "";
            dbHelper.InitiateConnection();
            SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
            SQL_C.CommandText = "SELECT dbo.FN_GET_HOSTNAME()";
            SQL_C.CommandType = CommandType.Text;
            DataTable dt = common.ExecuteDataTable(SQL_C);
            if (dt.Rows.Count > 0)
            {
                IP = dt.Rows[0][0].ToString();
            }
            dbHelper.CloseConnection();
            return IP;
        }

        public string GetIP()
        {
            Common common = new Common();
            DBHelper dbHelper = new DBHelper();
            string IP = "";
            dbHelper.InitiateConnection();
            SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
            SQL_C.CommandText = "SELECT dbo.FN_GET_CURRIP()";
            SQL_C.CommandType = CommandType.Text;
            DataTable dt = common.ExecuteDataTable(SQL_C);
            if (dt.Rows.Count > 0)
            {
                IP = dt.Rows[0][0].ToString();
            }
            dbHelper.CloseConnection();
            return IP;
        }

        public string GetPublicIP()
        {
            string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            try
            {
                if (string.IsNullOrEmpty(visitorIPAddress))
                {
                    visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                if (string.IsNullOrEmpty(visitorIPAddress))
                {
                    visitorIPAddress = HttpContext.Current.Request.UserHostAddress;
                }
            }
            catch (Exception)
            {
            }
            return visitorIPAddress;
        }

        public string GetServerRoot()
        {
            string str_host_name = HttpContext.Current.Request.Url.Host;// request.Url.Host;
            string str_root = HttpContext.Current.Request.ApplicationPath;//request.ApplicationPath;
            string str_port = HttpContext.Current.Request.Url.Port.ToString();//request.Url.Port.ToString();
            str_root = str_root == "/" ? "" : str_root;
            string protocol = HttpContext.Current.Request.IsSecureConnection ? "https://" : "http://";
            string str_result;
            if (str_port == "80")
            {
                str_result = string.Concat(protocol, str_host_name, str_root);
            }
            else
            {
                string[] strArrays = new string[] { protocol, str_host_name, ":", str_port, str_root };
                str_result = string.Concat(strArrays);
            }
            return str_result;
        }

        public string getSessionName(string sessionName, string FormID = "")
        {
            string SName = FormID == "" ? sessionName : string.Concat(sessionName, "_", FormID);
            return HttpContext.Current.Session[SName] == null ? "" : HttpContext.Current.Session[SName].ToString();
        }

        public string LoadDataToCBO(string CboID, string SQL, string Value, string DisplayText, string CboEvent = "", bool ShowAll = false, bool ISstoreprocedure = false, bool Encrypt = true, bool IsAppend = false, bool IsUseConnection2 = false, string ConditionFielVale = "", bool Multiple = false, bool IsCallBackToComboBox = false, string Class = "", string Placeholder = "")
        {
            DBHelper dbHelper = new DBHelper();
            Exception ex;
            string str1;
            string[] cboID;
            string ISMultiple = "";
            if (Multiple)
            {
                ISMultiple = " Multiple";
            }
            StringBuilder str = new StringBuilder();
            if (SQL == "")
            {
                if (Class == "")
                    Class = "form-control";
                //cboID = new string[] { "<div id=\"Div_", CboID, "\"><select style=\"width:100%\"", ISMultiple, " id=\"", CboID, "\" name=\"", CboID, "\"  data-size=\"7\" ", CboEvent, "></select></div>" };
                cboID = new string[] { "<select class=\"" + Class + "\" style=\"width:100%\"", ISMultiple, " id=\"", CboID, "\" name=\"", CboID, "\" ", CboEvent, "></select>" };
                str.Append(string.Concat(cboID));
                str1 = str.ToString();
            }
            else
            {
                string Result;
                try
                {
                    if (!IsUseConnection2)
                    {
                        dbHelper.InitiateConnection();
                    }
                    else
                    {
                        dbHelper.InitiateConnection2();
                    }
                    try
                    {
                        if (CboEvent != "")
                        {
                            CboEvent = "onChange=" + CboEvent + "";
                        }

                        SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                        if (!Encrypt)
                        {
                            SQL = SQL.Replace("@SearchText", ConditionFielVale);
                            SQL_C.CommandText = SQL;
                        }
                        else
                        {
                            SQL = EncryAndDecry.Decryp(SQL);
                            if (SQL != "")
                            {
                                ConditionFielVale = ConditionFielVale == "All" ? "" : ConditionFielVale;
                                SQL = SQL.Replace("@SearchText", ConditionFielVale);
                                SQL_C.CommandText = SQL;
                            }
                        }
                        if (SQL != "")
                        {
                            SQL_C.CommandType = ISstoreprocedure ? CommandType.StoredProcedure : CommandType.Text;
                            SqlDataReader SQL_D = SQL_C.ExecuteReader();
                            if (!IsAppend)
                            {
                                cboID = new string[] { "<select class=\"" + Class + "\" style=\"width:100%\"", ISMultiple, " id=\"", CboID, "\" name=\"", CboID, "\" ", CboEvent, ">" };
                                str.Append(string.Concat(cboID));
                            }
                            if (ShowAll)
                            {
                                str.Append("<option value=\"All\">All</option>");
                            }
                            //if (Placeholder!="")
                            //{
                            //	str.Append("<option value=\"\"></option>");
                            //}
                            while (SQL_D.Read())
                            {
                                str.Append("<option value=\"" + HttpUtility.HtmlEncode(SQL_D[EncryAndDecry.Decryp(Value)]) + "\">" + SQL_D[EncryAndDecry.Decryp(DisplayText)] + "</option>");
                            }
                            SQL_D.Close();
                            if (!IsAppend)
                            {
                                str.Append("</select>");
                            }
                        }
                        else
                        {
                            str.Append("<select class=\"" + Class + "\" style=\"width:100%\"" + ISMultiple + " id=\"" + CboID + "\" name=\"" + CboID + "\" " + CboEvent + "></select>");
                        }
                        Result = str.ToString();
                        dbHelper.CloseConnection();
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                        dbHelper.CloseConnection();
                        Result = ex.Message;
                    }
                }
                catch (Exception exception1)
                {
                    ex = exception1;
                    try
                    {
                        dbHelper.CloseConnection();
                    }
                    catch
                    {
                    }
                    Result = ex.Message;
                }
                str1 = Result;
            }
            return str1;
        }

        public void NoPermission()
        {
            string Path = HttpContext.Current.Request.ApplicationPath;
            if (Path.Replace("/", "") == "")
            {
                ServerRoot = GetServerRoot();
                Path = ServerRoot;
            }
            HttpContext.Current.Response.Write(string.Concat("<script> window.location.href='", Path, "/Home/AccessDeniedPage'; </script>"));
        }

        public string Paging(int CurrPage, int totalRecords, int pageSize, int totalPages, string AjaxFunction = "", int TotalNoLink = 10)
        {
            int min;
            int max;
            string str;
            object[] ajaxFunction;
            if (totalRecords <= pageSize)
            {
                str = "";
            }
            else
            {
                StringBuilder strPager = new StringBuilder();
                strPager.AppendLine("<table align=\"center\" class=\"TableGrideFooter\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">");
                strPager.AppendLine("<tfoot><tr>");
                strPager.AppendFormat("<td>Page: <input id=\"txtpaging\" class=\"span1 form-control\" type=\"text\" value=\"{0}\"> - {1}  of {2} ", CurrPage, totalPages, totalRecords);
                strPager.AppendLine("&nbsp; </td><td>");
                strPager.AppendLine("   <ul id=\"icons\" class=\"ui-widget ui-helper-clearfix\">");
                int currentPage;
                if (!int.TryParse(CurrPage.ToString(), out currentPage))
                {
                    currentPage = 1;
                }
                if (currentPage > totalPages)
                {
                    currentPage = totalPages;
                }
                if (currentPage > 1)
                {
                    strPager.AppendLine(string.Concat("       <li class=\"ui-state-default ui-corner-all\" title=\"First\" onclick=\"", AjaxFunction, "(1)\"><span class=\"ui-icon ui-icon-seek-first\"></span></li>"));
                    ajaxFunction = new object[] { "       <li class=\"ui-state-default ui-corner-all\" title=\"Previous\" onclick=\"", AjaxFunction, "(", currentPage - 2 + 1, ")\"><span class=\"ui-icon ui-icon-seek-prev\"></span></li>" };
                    strPager.AppendLine(string.Concat(ajaxFunction));
                }
                if (TotalNoLink >= totalPages)
                {
                    min = 1;
                    max = totalPages;
                }
                else
                {
                    max = currentPage - (TotalNoLink / 2) <= 0 ? TotalNoLink : currentPage + (TotalNoLink / 2) - ((TotalNoLink - 1) % 2);
                    if (max > totalPages)
                    {
                        max = totalPages;
                    }
                    min = max - TotalNoLink + 1 > 0 ? max - TotalNoLink + 1 : 1;
                }
                int n = min;
                while (n <= max)
                {
                    if (n > totalPages)
                    {
                        break;
                    }
                    else
                    {
                        if (n == currentPage)
                        {
                            ajaxFunction = new object[] { "<li class=\"ui-state-default ui-corner-all\" style=\"height:16px; line-height:18px; color:#F60\" title=\"", n, "\"><span style=\"padding:8px\">", n, "</span></li>" };
                            strPager.AppendLine(string.Concat(ajaxFunction));
                        }
                        else
                        {
                            ajaxFunction = new object[] { "<li class=\"ui-state-default ui-corner-all\" style=\"height:16px; line-height:18px;\" title=\"", n, "\" onclick=\"", AjaxFunction, "(", n, ")\"><span style=\"padding:8px\">", n, "</span></li>" };
                            strPager.AppendLine(string.Concat(ajaxFunction));
                        }
                        n++;
                    }
                }
                if (currentPage < totalPages)
                {
                    ajaxFunction = new object[] { "       <li class=\"ui-state-default ui-corner-all\" title=\"Next\" onclick=\"", AjaxFunction, "(", currentPage + 1, ")\"><span class=\"ui-icon ui-icon-seek-next\"></span></li>" };
                    strPager.AppendLine(string.Concat(ajaxFunction));
                    ajaxFunction = new object[] { "       <li class=\"ui-state-default ui-corner-all\" title=\"Last\" onclick=\"", AjaxFunction, "(", totalPages, ")\"><span class=\"ui-icon ui-icon-seek-end\"></span></li>" };
                    strPager.AppendLine(string.Concat(ajaxFunction));
                }
                strPager.AppendLine("   </url>");
                strPager.AppendLine("</td></tr></tfoot></table>");
                strPager.AppendLine("<script>$(function(){ $(\"#txtpaging\").keypress(function(e) {");
                strPager.AppendLine("if(e.keyCode==13){");
                strPager.AppendLine("var Page= $(this).val();");
                strPager.AppendLine(string.Concat(AjaxFunction, "(Page);"));
                strPager.AppendLine("}");
                strPager.AppendLine("});});</script>");
                str = strPager.ToString();
            }
            return str;
        }

        public static DateTime ParseDate(string input)
        {
            DateTime dateTime = DateTime.ParseExact(input, Common.formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return dateTime;
        }

        public void setSessionName(string sessionName, string sessionValue)
        {
            HttpContext.Current.Session[sessionName] = sessionValue;
        }

        public void setSessionTimeOut(int TimeOut)
        {
            HttpContext.Current.Session.Timeout = TimeOut;
        }

        public string DataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(list);
        }

        public string ShowAtachmentFile(string Destination, string FileName, string CustomerID = "", string DelFunctionName = "Delete_CustomerFile")
        {
            Common common = new Common();
            StringBuilder sb = new StringBuilder();
            StringBuilder sbimg = new StringBuilder();
            StringBuilder sbfile = new StringBuilder();
            string[] filenames = FileName.Split('~');
            string sm = HttpContext.Current.Server.MapPath(Destination);
            sb.AppendLine("<div class=\"gallery-container\">");
            sb.AppendLine("<div class=\"tz-gallery\">");
            sb.AppendLine("<div class=\"container-fluid\">");
            sb.AppendLine("<div class=\"row\">");
            List<string> ImageExtensions = new List<string> { "JPG", "JPE", "BMP", "GIF", "PNG" };
            int k = 0, j = 0;
            foreach (var file in filenames)
            {
                string fname = sm + file;
                FileInfo fi = new FileInfo(fname);
                string ext = "";
                if (fi.Extension.Length > 1)
                    ext = fi.Extension.Substring(1).ToLower();
                Destination = Destination.Replace("~", "");
                fname = common.ServerRoot + Destination + file;
                if (ImageExtensions.Contains(ext.ToUpperInvariant()))
                {
                    k++;
                    if (k == 1)
                        sbimg.AppendLine("<div class=\"row\">");
                    sbimg.AppendLine("<div name=\"all_File\" class=\"span3\">");
                    sbimg.AppendLine("<input type=\"hidden\" name=\"file_Name\" value=\"" + file + "\">");
                    sbimg.AppendLine("<a class=\"lightbox\" href=\"" + fname + "\">");
                    sbimg.AppendLine("<img style=\"border:#A4A3A3 solid 1px\" src=\"" + fname + "\" class=\"span3\">");
                    sbimg.AppendLine("<br><div class=\"span3\">" + file + "</div>");
                    sbimg.AppendLine("</a>");
                    sbimg.AppendLine("<div class=\"span3\">");
                    sbimg.AppendLine("<button type=\"button\" class=\"btn btn-info\" onClick=\"openfile('" + fname + "')\">Download</button>");
                    sbimg.AppendLine("<button type=\"button\" name=\"Files\" class=\"btn btn-danger\" onClick=\"" + DelFunctionName + "(this, " + CustomerID + ");\">Delete</button>");
                    sbimg.AppendLine("<br><br></div>");
                    sbimg.AppendLine("</div>");
                    if (k == 3)
                    {
                        k = 0;
                        sbimg.AppendLine("</div");
                        sbimg.AppendLine("<div class=\"row\">");
                    }
                }
                else
                {
                    j++;
                    if (j == 1)
                        sbfile.AppendLine("<div class=\"row\">");
                    sbfile.AppendLine("<div name=\"all_File\" class=\"span3\">");
                    sbfile.AppendLine("<input type=\"hidden\" name=\"file_Name\" value=\"" + file + "\">");
                    sbfile.AppendLine("<a href=\"" + fname + "\" target=\"_blank\">");
                    sbfile.AppendLine("<div class=\"fi fi-" + ext + " fi-size-xl\">");
                    sbfile.AppendLine("<br><div class=\"fi-content\">." + ext + "</div></div>");
                    sbfile.AppendLine("<div class=\"span3\"><br>" + file + "</div></a>");
                    sbfile.AppendLine("<div class=\"span3\">");
                    sbfile.AppendLine("<button type=\"button\" class=\"btn btn-info\" onClick=\"openfile('" + fname + "')\">Download</button>");
                    sbfile.AppendLine("<button type=\"button\" name=\"Files\" class=\"btn btn-danger\" onClick=\"" + DelFunctionName + "(this, " + CustomerID + ");\">Delete</button>");
                    sbfile.AppendLine("<br><br></div>");
                    sbfile.AppendLine("</div>");
                    if (j == 3)
                    {
                        j = 0;
                        sbfile.AppendLine("</div");
                        sbfile.AppendLine("<div class=\"row\">");
                    }
                }
            }
            if (sbimg.ToString() != "")
            {
                if (k != 0)
                    sbimg.Append("</div>");
            }
            if (sbfile.ToString() != "")
            {
                if (j != 0)
                    sbfile.Append("</div>");
            }
            sb.AppendLine(sbimg.ToString());
            sb.AppendLine(sbfile.ToString());
            sb.AppendLine("</div></div>");
            sb.AppendLine("</div></div>");
            sb.AppendLine("<script>");
            sb.AppendLine("$(function() { ");
            sb.AppendLine("baguetteBox.run('.tz-gallery');");
            sb.AppendLine("});");
            sb.AppendLine("</script>");
            return sb.ToString();
        }
        public string ShowAtachmentFile2(string UploadUrl, string DeleteUrl, string IDInputFile, string Destination, DataTable FileName, string UploadID = "")
        {
            Common common = new Common();
            StringBuilder sb = new StringBuilder();
            StringBuilder sbimg = new StringBuilder();
            StringBuilder sbfile = new StringBuilder();
            StringBuilder sbfileList = new StringBuilder();
            StringBuilder sbfileListimg = new StringBuilder();
            //string[] filenames = FileName.Split('~');
            string sm = HttpContext.Current.Server.MapPath(Destination);
            string Extension = "";
            string URL = common.ServerRoot + "/" + UploadUrl + "?id=" + UploadID;
            string URLDelete = common.ServerRoot + "/" + DeleteUrl;
            List<string> ImageExtensions = new List<string> { "JPG", "JPE", "BMP", "GIF", "PNG" };
            Destination = Destination.Replace("~", "");
            string files = "";
            string fileList = "";
            int i = 0;
            string uploadID = "";
            //foreach (var file in filenames)
            if (FileName.Rows.Count > 0)
            {
                if (FileName.Columns.Count > 2)
                {
                    uploadID = FileName.Rows[0][2].ToString();
                }

                foreach (DataRow dr in FileName.Rows)
                {
                    i++;
                    string id = dr[0].ToString();
                    string file = dr[1].ToString();
                    string fname = sm + file;
                    if (File.Exists(fname))
                    {
                        FileInfo fileInfo = new FileInfo(fname);

                        if (fileInfo.Extension.Length > 1)
                            Extension = fileInfo.Extension.Substring(1).ToLower();
                        fname = common.ServerRoot + Destination + file;

                        if (ImageExtensions.Contains(Extension.ToUpperInvariant()))
                        {
                            if (fileList == "")
                            {
                                fileList = fname;
                                sbfileListimg.AppendLine("\"" + fname + "\"");
                            }
                            else
                            {
                                fileList = fname;
                                sbfileListimg.AppendLine(",\"" + fname + "\"");
                            }
                            if (files == "")
                            {
                                files = fname;
                                sbimg.Append("{ type: \"image\",");
                                sbimg.Append("size: " + fileInfo.Length + ",");
                                sbimg.Append("caption: \"" + file + "\",");
                                sbimg.Append("url:\"" + URLDelete + "\",");
                                sbimg.Append("downloadUrl:\"" + fname + "\",");
                                sbimg.Append("extra: {id: '" + uploadID + "'},");
                                sbimg.Append("key: '" + id + "' }");
                            }
                            else
                            {
                                files = fname;
                                sbimg.Append(",{ type: \"image\",");//
                                sbimg.Append("size: " + fileInfo.Length + ",");
                                sbimg.Append("caption: \"" + file + "\",");
                                sbimg.Append("url:\"" + URLDelete + "\",");
                                sbimg.Append("downloadUrl:\"" + fname + "\",");
                                sbimg.Append("extra: {id: '" + uploadID + "'},");
                                sbimg.Append("key: '" + id + "' }");
                            }
                        }
                        else
                        {
                            if (fileList == "")
                            {
                                fileList = fname;
                                sbfileList.AppendLine("\"" + fname + "\"");
                            }
                            else
                            {
                                fileList = fname;
                                sbfileList.AppendLine(",\"" + fname + "\"");
                            }
                            if (files == "")
                            {
                                files = fname;
                                if (Extension == "pdf")
                                    sbfile.Append("{ type: \"pdf\",");
                                else if (Extension == "doc" || Extension == "docx" || Extension == "xls" || Extension == "xlsx" || Extension == "ppt" || Extension == "pptx")
                                    sbfile.Append("{ type: \"office\",");
                                else
                                    sbfile.Append("{");
                                sbfile.Append("size: " + fileInfo.Length + ",");
                                sbfile.Append("caption: \"" + file + "\",");
                                sbfile.Append("url:\"" + URLDelete + "\",");
                                sbfile.Append("downloadUrl:\"" + fname + "\",");
                                sbfile.Append("extra: {id: '" + uploadID + "'},");
                                sbfile.Append("key: '" + id + "' }");
                            }
                            else
                            {
                                files = fname;
                                if (Extension == "pdf")
                                    sbfile.Append(",{ type: \"pdf\",");
                                else
                                    sbfile.Append(",{ ");
                                sbfile.Append("size: " + fileInfo.Length + ",");
                                sbfile.Append("caption: \"" + file + "\",");
                                sbfile.Append("url:\"" + URLDelete + "\",");
                                sbfile.Append("downloadUrl:\"" + fname + "\",");
                                sbfile.Append("extra: {id: '" + uploadID + "'},");
                                sbfile.Append("key: '" + id + "' }");
                            }
                        }
                    }
                }
            }
            sb.AppendLine("<div class=\"file-loading\">");
            sb.AppendLine("<input id=\"" + IDInputFile + "\" name=\"" + IDInputFile + "[]\" type=\"file\" multiple>");
            sb.AppendLine("</div>");
            sb.AppendLine("<script>");
            sb.AppendLine("$(function () {");
            sb.AppendLine("var krajeeGetCount = function(id) {");
            sb.AppendLine("var cnt = $('#' + id).fileinput('getFilesCount');");
            sb.AppendLine("return cnt === 0 ? 'You have no files remaining.' :");
            sb.AppendLine("'You have ' + cnt + ' file' + (cnt > 1 ? 's' : '') + ' remaining.';");
            sb.AppendLine("};");
            sb.AppendLine("var url = \"" + URL + "\"");
            sb.AppendLine("$(\"#" + IDInputFile + "\").fileinput({");
            sb.AppendLine("uploadUrl: url,");
            sb.AppendLine("validateInitialCount: true,");
            sb.AppendLine("uploadAsync: false,");
            sb.AppendLine("showRemove: false,");
            sb.AppendLine("showClose: false,");
            sb.AppendLine("theme: \"explorer\",");
            //sb.AppendLine("minFileCount: 1,");
            sb.AppendLine("overwriteInitial: false,");
            sb.AppendLine("browseClass: \"btn btn-success\",");
            sb.AppendLine("browseLabel: \"Pick Image\",");
            sb.AppendLine("browseIcon: '<i class=\"glyphicon glyphicon-picture\"></i>',");
            sb.AppendLine("removeClass: \"btn btn-danger\",");
            sb.AppendLine("removeLabel: \"Delete\",");
            sb.AppendLine("removeIcon: '<i class=\"glyphicon glyphicon-trash\"></i>',");
            sb.AppendLine("uploadClass: \"btn btn-info\",");
            sb.AppendLine("uploadLabel: \"Upload\",");
            sb.AppendLine("uploadIcon: '<i class=\"glyphicon glyphicon-upload\"></i>',");
            sb.AppendLine("initialPreview:[");
            sb.AppendLine(sbfileListimg.ToString());
            sb.AppendLine(sbfileList.ToString());
            sb.AppendLine("],");
            sb.AppendLine("initialPreviewAsData: true, // defaults markup");
            sb.AppendLine("initialPreviewFileType: 'any', // image is the default and can be overridden in config below");
            sb.AppendLine("initialPreviewConfig:");
            sb.AppendLine("[");
            sb.AppendLine(sbimg.ToString());
            sb.AppendLine(sbfile.ToString());
            sb.AppendLine("],");
            sb.AppendLine("fileActionSettings: {");
            sb.AppendLine("showDrag: false,");
            sb.AppendLine("},");
            sb.AppendLine("previewZoomSettings: {");
            sb.AppendLine("image: { width: 'auto', height: '500px' }");
            sb.AppendLine("},");
            sb.AppendLine("previewFileIconSettings:");
            sb.AppendLine("{ // configure your icon file extensions");
            sb.AppendLine("'doc': '<i class=\"fas fa-file-word text-primary\"></i>',");
            sb.AppendLine("'xls': '<i class=\"fas fa-file-excel text-success\"></i>',");
            sb.AppendLine("'ppt': '<i class=\"fas fa-file-powerpoint text-danger\"></i>',");
            sb.AppendLine("'pdf': '<i class=\"fas fa-file-pdf text-danger\"></i>',");
            sb.AppendLine("'zip': '<i class=\"fas fa-file-archive text-muted\"></i>',");
            sb.AppendLine("'htm': '<i class=\"fas fa-file-code text-info\"></i>',");
            sb.AppendLine("'txt': '<i class=\"fas fa-file-alt text-info\"></i>',");
            sb.AppendLine("'mov': '<i class=\"fas fa-file-video text-warning\"></i>',");
            sb.AppendLine("'mp3': '<i class=\"fas fa-file-audio text-warning\"></i>',");
            sb.AppendLine("// note for these file types below no extension determination logic");
            sb.AppendLine("// has been configured (the keys itself will be used as extensions)");
            sb.AppendLine("'jpg': '<i class=\"fas fa-file-image text-danger\"></i>',");
            sb.AppendLine("'gif': '<i class=\"fas fa-file-image text-muted\"></i>',");
            sb.AppendLine("'png': '<i class=\"fas fa-file-image text-primary\"></i>'");
            sb.AppendLine("},");
            sb.AppendLine("previewFileExtSettings:");
            sb.AppendLine("{ // configure the logic for determining icon file extensions");
            sb.AppendLine("'doc': function(ext) {");
            sb.AppendLine("return ext.match(/(doc|docx)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'xls': function(ext) {");
            sb.AppendLine("return ext.match(/(xls|xlsx)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'ppt': function(ext) {");
            sb.AppendLine("return ext.match(/ (ppt|pptx)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'zip': function(ext) {");
            sb.AppendLine("return ext.match(/(zip|rar|tar|gzip|gz|7z)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'htm': function(ext) {");
            sb.AppendLine("return ext.match(/(htm|html)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'txt': function(ext) {");
            sb.AppendLine("return ext.match(/(txt|ini|csv|java|php|js|css)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'mov': function(ext) {");
            sb.AppendLine("return ext.match(/(avi|mpg|mkv|mov|mp4|3gp|webm|wmv)$/i);");
            sb.AppendLine("},");
            sb.AppendLine("'mp3': function(ext) {");
            sb.AppendLine("return ext.match(/(mp3|wav)$/i);");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}).on('filesorted', function(e, params) {");
            sb.AppendLine("console.log('File sorted params', params);");
            sb.AppendLine("}).on('fileuploaded', function(e, params) {");
            sb.AppendLine("console.log('File uploaded params', params);");
            sb.AppendLine("}).on('filebeforedelete', function() {");
            sb.AppendLine("return new Promise(function(resolve, reject) {");
            sb.AppendLine("Swal.fire({");
            sb.AppendLine("title: 'Are you sure?',");
            sb.AppendLine("text: \"You won't be able to revert this!\",");
            sb.AppendLine("icon: 'warning',");
            sb.AppendLine("showCancelButton: true,");
            sb.AppendLine("confirmButtonColor: '#d33',");
            sb.AppendLine("cancelButtonColor: '#3085d6',");
            sb.AppendLine("confirmButtonText: 'Yes, delete it!',");
            sb.AppendLine("cancelButtonText: 'No, cancel!',");
            sb.AppendLine("reverseButtons: true");
            sb.AppendLine("}).then((result) => {");
            sb.AppendLine("if (result.value)");
            sb.AppendLine("{");
            sb.AppendLine("DialogSweetAlert('deleteted');");
            sb.AppendLine("resolve();");
            sb.AppendLine("}");
            sb.AppendLine("else if (result.dismiss === Swal.DismissReason.cancel)");
            sb.AppendLine("{");
            sb.AppendLine("Swal.fire({");
            sb.AppendLine("title: 'Cancelled',");
            sb.AppendLine("text: \"\",");
            sb.AppendLine("icon: 'error',");
            sb.AppendLine("showConfirmButton: false,");
            sb.AppendLine("allowOutsideClick: false,");
            sb.AppendLine("timerProgressBar: true,");
            sb.AppendLine("allowEscapeKey: false,");
            sb.AppendLine("heightAuto: false,");
            sb.AppendLine("timer: 700");
            sb.AppendLine("})");
            sb.AppendLine("}");
            sb.AppendLine("});");
            sb.AppendLine("});");
            sb.AppendLine("}).on('filedeleted', function() {");
            sb.AppendLine("});");
            sb.AppendLine("});");
            sb.AppendLine("</script>");
            return sb.ToString();
        }
        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        [Obsolete]
        public SqlResult Login(string UserName, string Password)
        {
            SqlResult sqlResult = new SqlResult();
            string SQL = "SELECT UserID, UserName, [Password], [Key] FROM dbo.[User] WHERE UserName='" + UserName + "'";
            DataSet ds = LoadData(SQL);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string Key = dt.Rows[0]["Key"].ToString();
                string PWD = dt.Rows[0]["Password"].ToString();
                if (EncryptionPassword.CreatePasswordHash(Password, Key) == PWD)
                {
                    SQL = "select top 1 WarehouseID from [dbo].[TblAccesswarehouse] where UserID = " + dt.Rows[0]["UserID"];
                    ds = LoadData(SQL);
                    HttpContext.Current.Session["WarehouseID"] = ds.Tables[0].Rows[0]["WarehouseID"];
                    HttpContext.Current.Session["ISLogin"] = true;
                    HttpContext.Current.Session["UserID"] = dt.Rows[0]["UserID"];
                    HttpContext.Current.Session["UserName"] = dt.Rows[0]["UserName"];
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Login Successful.";
                    ServerRoot = GetServerRoot();
                }
                else
                {
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = "Invalide user name or password.";
                }
            }
            else
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = "Invalide user name or password.";
            }
            return sqlResult;
        }
        public void Logout()
        {
            HttpContext.Current.Session["ISLogin"] = false;
            HttpContext.Current.Session["UserID"] = null;
            HttpContext.Current.Session["UserName"] = null;
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Current.Response.Cache.SetNoStore();
            HttpContext.Current.Session.RemoveAll();
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }
        public DataSet LoadData(string SQL, bool ISStoreProcedure = false, Dictionary<string, string> Para = null)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            Exception ex;
            DataSet DS = new DataSet();
            try
            {
                dbHelper.InitiateConnection();
                try
                {
                    SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                    if (ISStoreProcedure)
                    {
                        SQL_C.CommandType = CommandType.StoredProcedure;
                        if (Para != null)
                        {
                            foreach (KeyValuePair<string, string> item in Para)
                            {
                                SQL_C.Parameters.Add(new SqlParameter(item.Key, item.Value));
                            }
                        }
                    }
                    else
                    {
                        SQL_C.CommandType = CommandType.Text;
                    }
                    SQL_C.CommandText = SQL;
                    DS = common.ExecuteDataSet(SQL_C);
                    dbHelper.CloseConnection();
                }
                catch (Exception exception)
                {
                    ex = exception;
                    dbHelper.CloseConnection();
                }
            }
            catch (Exception)
            {
                try
                {
                    dbHelper.CloseConnection();
                }
                catch
                {
                }
            }
            return DS;
        }
        private string GetPaginatedSQL(int startRow, int numberOfRows, string sql, string orderingClause)
        {
            // Ordering clause is mandatory!
            if (String.IsNullOrEmpty(orderingClause))
                throw new ArgumentNullException("orderingClause");

            // numberOfRows here is checked of disable building paginated/limited query
            // in case is not greater than 0. In this case we simply return the
            // query with its ordering clause appended to it. 
            // If ordering is not spe
            if (numberOfRows <= 0)
            {
                return String.Format("{0} {1}", sql, orderingClause);
            }
            // Extract the SELECT from the beginning.
            String partialSQL = sql.Remove(0, "SELECT ".Length);

            // Build the limited query...
            return String.Format(
                "SELECT * FROM ( SELECT ROW_NUMBER() OVER ({0}) AS rn, {1} ) AS SUB WHERE rn > {2} AND rn <= {3}",
                orderingClause,
                partialSQL,
                startRow.ToString(),
                (startRow + numberOfRows).ToString()
            );
        }

        public DataSet GetSqlDataWithPagin(string SQL, int startRow, int numberOfRows)
        {
            DBHelper dbHelper = new DBHelper();
            dbHelper.InitiateConnection();
            SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            String limitedSQL = GetPaginatedSQL(startRow, numberOfRows, SQL, "ORDER BY CategoryID ASC");
            SQL_C.CommandText = limitedSQL;
            adapter.SelectCommand = SQL_C;
            adapter.Fill(ds);
            dbHelper.CloseConnection();
            return ds;
        }
    }
}