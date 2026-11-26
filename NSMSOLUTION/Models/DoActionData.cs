using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Utilities;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Data.SqlClient;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Web;
//using Utilities;

namespace NSM.Models
{
    public class DoActionData
    {

        public DoActionData()
        {
        }

        public string CheckExsitingDataField(string OldValue, string ConditionFiledName, string ConditonText, string CheckExsitingWithField, string FormID)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            Exception ex;
            string Value = ConditonText;
            string IsExsitsingData = "TRUE";
            string FieldName = EncryAndDecry.Decryp(ConditionFiledName);
            string TblName = common.getSessionName("TblName", FormID);
            string[] strArrays = new string[] { "Select ", FieldName, " From [", TblName, "] Where WarehouseID = " + common.getSessionName("WarehouseID") + " and LOWER(", FieldName, ") = @ConditonText" };
            string SQL = string.Concat(strArrays);
            if (CheckExsitingWithField != "")
            {
                SQL = string.Concat(SQL, " AND ", CheckExsitingWithField);
            }
            if (OldValue != "")
            {
                Value = OldValue;
                strArrays = new string[] { "Select ", FieldName, " From [", TblName, "] Where LOWER(", FieldName, ") <> @ConditonText" };
                SQL = string.Concat(strArrays);
                if (CheckExsitingWithField != "")
                {
                    SQL = string.Concat(SQL, " AND ", CheckExsitingWithField);
                }
            }
            string Result;
            try
            {
                dbHelper.InitiateConnection();
                try
                {
                    SqlCommand SQL_C = new SqlCommand(SQL, dbHelper.sqlConnection);
                    SQL_C.Parameters.Add(new SqlParameter("ConditonText", Value.ToLower()));
                    SqlDataReader SQL_D = SQL_C.ExecuteReader();
                    if (OldValue != "")
                    {
                        while (SQL_D.Read())
                        {
                            if (SQL_D[FieldName].ToString().ToLower() == ConditonText.ToLower())
                            {
                                IsExsitsingData = "FALSE";
                                break;
                            }
                            else
                            {
                                IsExsitsingData = "TRUE";
                            }
                        }
                    }
                    else
                    {
                        IsExsitsingData = !SQL_D.Read() ? "TRUE" : "FALSE";
                    }
                    Result = IsExsitsingData;
                    SQL_D.Close();
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
            return Result;
        }

        public SqlResult CheckField(string SQL, string Data, bool isStore)
        {
            DBHelper dbHelper = new DBHelper();
            SqlResult sqlResult = new SqlResult();
            try
            {
                try
                {
                    dbHelper.InitiateConnection();
                    SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                    SQL_C.CommandType = isStore ? CommandType.StoredProcedure : CommandType.Text;
                    SQL_C.CommandText = EncryAndDecry.Decryp(SQL);
                    string str = Data.ToString();
                    char[] chrArray = new char[] { '\u005E' };
                    string[] strArrays = str.Split(chrArray);
                    for (int i = 0; i < strArrays.Length; i++)
                    {
                        string dt = strArrays[i];
                        if (dt != "")
                        {
                            chrArray = new char[] { '|' };
                            string[] FNWV = dt.Split(chrArray);
                            SQL_C.Parameters.AddWithValue(string.Concat("@", FNWV[0].Trim().ToString()), FNWV[1].Trim().ToString());
                        }
                    }
                    int result = (int)SQL_C.ExecuteScalar();
                    if (result == 1)
                    {
                        sqlResult.Message = "Please check QTY Stock In. Your QTY is larger than QTY Stock In.";
                        sqlResult.Result = ResultType.Failed;
                    }
                    else if (result == 2)
                    {
                        sqlResult.Message = "";
                        sqlResult.Result = ResultType.Successful;
                    }
                    else
                    {
                        sqlResult.Message = "Please check QTY Stock In. QTY Stock In maybe contains zero value.";
                        sqlResult.Result = ResultType.Failed;
                    }
                }
                catch (Exception exception)
                {
                    sqlResult.Message = exception.Message;
                    sqlResult.Result = ResultType.Failed;
                }
            }
            finally
            {
                dbHelper.CloseConnection();
            }
            return sqlResult;
        }

        public SqlResult Delete(string SQL, Dictionary<string, string> Para = null)
        {
            DBHelper dbHelper = new DBHelper();
            SqlResult Result = new SqlResult();
            try
            {
                dbHelper.InitiateConnection();
                SqlTransaction SQL_T = dbHelper.sqlConnection.BeginTransaction();
                SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                SQL_C.CommandType = CommandType.Text;
                try
                {
                    if (Para != null && Para.Count > 0)
                    {
                        SQL_C.CommandType = CommandType.StoredProcedure;
                        SQL_C.CommandText = SQL;
                        foreach (KeyValuePair<string, string> item in Para)
                        {
                            SQL_C.Parameters.Add(new SqlParameter(item.Key, item.Value));
                        }
                    }
                    else
                    {
                        SQL_C.CommandText = SQL;
                    }
                    SQL_C.Transaction = SQL_T;
                    SQL_C.ExecuteNonQuery();
                    SQL_T.Commit();
                    dbHelper.CloseConnection();
                    Result.Result = ResultType.Successful;
                    Result.Message = "Data deleted successful.";
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    SQL_T.Rollback();
                    dbHelper.CloseConnection();
                    Result.Message = ex.Message;
                    Result.Result = ResultType.Failed;
                }
            }
            catch (Exception exception1)
            {
                Result.Message = exception1.Message;
                try
                {
                    dbHelper.CloseConnection();
                }
                catch
                {
                }
            }
            return Result;
        }

        public SqlResult DeteleData(string FieldName, string DataID, string DeleteUseProcedureName, string FormID)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            string[] fieldName;
            SqlResult Result = new SqlResult();
            string TblName = "";
            string LogText = "";
            if (DeleteUseProcedureName == "")
            {
                TblName = common.getSessionName("TblName", FormID);
                fieldName = new string[] { "Delete data from table ", TblName, " where ", FieldName, " = ", DataID };
                LogText = string.Concat(fieldName);
            }
            try
            {
                dbHelper.InitiateConnection();
                SqlTransaction SQL_T = dbHelper.sqlConnection.BeginTransaction();
                SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                SQL_C.CommandType = CommandType.Text;
                try
                {
                    string SQL = "";
                    if (DeleteUseProcedureName == "")
                    {
                        fieldName = new string[] { "Delete From ", TblName, " Where ", FieldName, " = @", FieldName };
                        SQL_C.CommandText = string.Concat(fieldName);
                    }
                    else
                    {
                        SQL = EncryAndDecry.Decryp(DeleteUseProcedureName);
                        SQL_C.CommandType = CommandType.StoredProcedure;
                        SQL_C.CommandText = SQL;
                        FieldName = string.Concat("@", FieldName);
                        SQL_C.Parameters.Add("@UserID", SqlDbType.Int).Value = HttpContext.Current.Session["UserID"].ToString();
                    }
                    SQL_C.Parameters.Add(new SqlParameter(FieldName, DataID));
                    SQL_C.Transaction = SQL_T;
                    SQL_C.ExecuteNonQuery();
                    if (DeleteUseProcedureName == "")
                    {
                        DataSet ds = this.LoadData(string.Concat("SELECT TableID FROM dbo.LogTableName WHERE TableOriginalName= '", TblName, "'"), false, null);
                        SQL_C.CommandType = CommandType.StoredProcedure;
                        SQL_C.CommandText = "SP_INSERT_LOG";
                        SQL_C.Parameters.Clear();
                        SQL_C.Parameters.Add("@LogTypeID", SqlDbType.VarChar).Value = 3;
                        SQL_C.Parameters.Add("@LogText", SqlDbType.VarChar).Value = LogText;
                        SQL_C.Parameters.Add("@LogParentID", SqlDbType.VarChar).Value = 0;
                        SQL_C.Parameters.Add("@TableID", SqlDbType.VarChar).Value = ds.Tables[0].Rows[0][0].ToString();
                        SQL_C.Parameters.Add("@UserID", SqlDbType.Int).Value = HttpContext.Current.Session["UserID"].ToString();
                        SQL_C.Transaction = SQL_T;
                        SQL_C.ExecuteNonQuery();
                    }
                    SQL_T.Commit();
                    dbHelper.CloseConnection();
                    Result.Result = ResultType.Successful;
                    Result.Message = "Delete data successful.";
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    Result.Result = ResultType.Failed;
                    Exception ex = exception;
                    SQL_T.Rollback();
                    dbHelper.CloseConnection();
                    Result.Message = ex.Message;
                }
            }
            catch (Exception exception2)
            {
                Result.Message = exception2.Message;
                try
                {
                    dbHelper.CloseConnection();
                }
                catch
                {
                }
            }
            return Result;
        }

        public void Dispose()
        {
        }

        public string DrawCheckBox(string CheckBoxName, string SQL, string Value, string DisplayText)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            Exception ex;
            StringBuilder sb = new StringBuilder();
            try
            {
                dbHelper.InitiateConnection();
                try
                {
                    Value = EncryAndDecry.Decryp(Value);
                    DisplayText = EncryAndDecry.Decryp(DisplayText);
                    SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                    SQL_C.CommandType = CommandType.Text;
                    SQL_C.CommandText = SQL;
                    DataTable dt = common.ExecuteDataTable(SQL_C);
                    sb.AppendLine("<ul class=\"TimeCheckBox\" style=\"height:auto\">");
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        i++;
                        object[] checkBoxName = new object[] { "<li><label for=\"", CheckBoxName, i, "\">", dr[DisplayText], "</label>" };
                        sb.AppendLine(string.Concat(checkBoxName));
                        checkBoxName = new object[] { "<input id=\"", CheckBoxName, i, "\" type=\"checkbox\" value=\"", dr[Value], "\" name=\"Time\"></li>" };
                        sb.AppendLine(string.Concat(checkBoxName));
                    }
                    sb.AppendLine("</ul>");
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
            return sb.ToString();
        }

        ~DoActionData()
        {
        }

        public DataTable GetDataForEdit(string SQL)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            Exception ex;
            DataTable DT = new DataTable();
            try
            {
                dbHelper.InitiateConnection();
                try
                {
                    DT = common.ExecuteDataTable(new SqlCommand(SQL, dbHelper.sqlConnection));
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
            return DT;
        }

        public Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        protected Bitmap GetResizedImage(Bitmap source, int width, int height)
        {
            Bitmap thumb = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(thumb);
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.DrawImage(source, 0, 0, width, height);
            return thumb;
        }

        public Bitmap GetScaledPicture(Bitmap source, int maxWidth, int maxHeight)
        {
            int width;
            int height;
            Bitmap resizedImage;
            int num;
            float aspectRatio = source.Width / (float)source.Height;
            if (maxHeight <= 0 || maxWidth <= 0)
            {
                if (maxHeight != 0)
                {
                    goto Label1;
                }
                width = maxWidth;
                height = (int)(width / aspectRatio);
            }
            else
            {
                if (source.Width >= maxWidth)
                {
                    resizedImage = source;
                    return resizedImage;
                }
                if (aspectRatio > 1f)
                {
                    width = maxWidth;
                    height = (int)(width / aspectRatio);
                    if (height > maxHeight)
                    {
                        height = maxHeight;
                        width = (int)(height * aspectRatio);
                    }
                }
                else
                {
                    height = maxHeight;
                    width = (int)(height * aspectRatio);
                    if (width > maxWidth)
                    {
                        width = maxWidth;
                        height = (int)(width / aspectRatio);
                    }
                }
            }
            resizedImage = this.GetResizedImage(source, width, height);
            return resizedImage;
        Label1:
            num = maxWidth;// != 0 ? 0 : source.Height > maxHeight);
            if (num == 0)
            {
                resizedImage = source;
                return resizedImage;
            }
            else
            {
                height = maxHeight;
                width = (int)(height * aspectRatio);
                resizedImage = this.GetResizedImage(source, width, height);
                return resizedImage;
            }
        }

        public bool IsHavePermission(string UserID, string PageCode, string EditCode = "", string DeleteCode = "")
        {
            Common common = new Common();
            DoActionData doActionData = new DoActionData();
            bool HavePermission = false;
            if (common.CheckSessionOK("OK"))
            {
                if (UserID != "")
                {
                    string str = string.Concat("SELECT p.PermissionPageCode FROM dbo.User_Permission up ", "INNER JOIN dbo.PermissionPage p ON p.PermissionPageID = up.PermissionID ");
                    string[] userID = new string[] { str, "WHERE up.UserID = ", UserID, " AND p.PermissionPageCode = '", PageCode, "'" };
                    string SQL = string.Concat(userID);
                    if (doActionData.LoadData(SQL, false, null).Tables[0].Rows.Count > 0)
                    {
                        HavePermission = true;
                    }
                    else
                    {
                        str = string.Concat("SELECT p.PermissionPageCode FROM dbo.User_Permission up ", "INNER JOIN dbo.PermissionPage p ON p.PermissionPageID = up.PermissionID ");
                        userID = new string[] { str, "WHERE up.UserID = ", UserID, " AND (p.PermissionPageCode = '", EditCode, "' OR p.PermissionPageCode = '", DeleteCode, "')" };
                        SQL = string.Concat(userID);
                        if (doActionData.LoadData(SQL, false, null).Tables[0].Rows.Count > 0)
                        {
                            HavePermission = true;
                        }
                    }
                }
                if (!HavePermission)
                {
                    common.NoPermission();
                }
            }
            return HavePermission;
        }

        public bool IsHavePermissionHome(string UserID, string PageCode, string EditCode = "", string DeleteCode = "")
        {
            Common common = new Common();
            DoActionData doActionData = new DoActionData();
            //common.CheckSession();
            bool HavePermission = false;
            if (common.CheckSessionOK("OK"))
            {
                if (UserID != "")
                {
                    string str = string.Concat("SELECT TOP 1 p.PermissionPageCode FROM dbo.User_Permission up ", "INNER JOIN dbo.PermissionPage p ON p.PermissionPageID = up.PermissionID ");
                    string[] userID = new string[] { str, "WHERE up.UserID = ", UserID, " AND p.PermissionPageCode = '", PageCode, "'" };
                    string SQL = string.Concat(userID);
                    if (doActionData.LoadData(SQL, false, null).Tables[0].Rows.Count > 0)
                    {
                        HavePermission = true;
                    }
                    else
                    {
                        str = string.Concat("SELECT TOP 1 p.PermissionPageCode FROM dbo.User_Permission up ", "INNER JOIN dbo.PermissionPage p ON p.PermissionPageID = up.PermissionID ");
                        userID = new string[] { str, "WHERE up.UserID = ", UserID, " AND (p.PermissionPageCode = '", EditCode, "' OR p.PermissionPageCode = '", DeleteCode, "')" };
                        SQL = string.Concat(userID);
                        if (doActionData.LoadData(SQL, false, null).Tables[0].Rows.Count > 0)
                        {
                            HavePermission = true;
                        }
                    }
                }
            }
            return HavePermission;
        }

        private bool IsWordChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '\u005F';
        }

        public string LoadCBOData(string CboValue, string SQL, string Value, string DisplayText, bool ShowAll = true, string CallBackWithControllValueID = "", string ConditionField = "", string ConditionFieldSearch2 = "")
        {
            DBHelper dbHelper = new DBHelper();
            StringBuilder str = new StringBuilder();
            dbHelper.InitiateConnection();
            string Result;
            try
            {
                SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                SQL = EncryAndDecry.Decryp(SQL);
                //SQL = SQL.Replace("@SearchText", CboValue);
                if (CboValue.ToLower() == "all")
                    SQL = SQL.Replace("@SearchText", ConditionField);
                //SQL = SQL.Replace("@SearchText", ConditionField);
                else
                    SQL_C.Parameters.AddWithValue("@SearchText", CboValue.ToLower());
                if (CallBackWithControllValueID != "")
                {
                    if (CallBackWithControllValueID.ToLower() == "all")
                        SQL = SQL.Replace("@Search_Text2", ConditionFieldSearch2);
                    else
                        SQL_C.Parameters.AddWithValue("@Search_Text2", CallBackWithControllValueID.ToLower());
                }
                SQL_C.CommandText = SQL;
                SQL_C.CommandType = CommandType.Text;


                //SqlDataReader SQL_D = SQL_C.ExecuteReader();
                //while (SQL_D.Read())
                //{
                //	StringBuilder stringBuilder = str;
                //	object[] objArray = new object[] { "<option value=\"", null, null, null, null };
                //	object[] item = objArray;
                //	item[1] = SQL_D[EncryAndDecry.Decryp(Value) ?? ""].ToString().Trim();
                //	item[2] = "\">";
                //	item[3] = SQL_D[EncryAndDecry.Decryp(DisplayText).ToString().Trim() ?? ""];
                //	item[4] = "</option>";
                //	stringBuilder.Append(string.Concat(item));
                //}
                //SQL_D.Close();
                DataSet ds = new DataSet();
                SqlDataAdapter sda = new SqlDataAdapter(SQL_C);
                sda.Fill(ds);
                int i = 0;
                foreach (DataTable dt in ds.Tables)
                {
                    if (ShowAll)
                    {
                        str.Append("<option value=\"All\">All</option>");
                    }
                    string[] v = EncryAndDecry.Decryp(Value).Split(new char[] { ',' });
                    string[] d = EncryAndDecry.Decryp(DisplayText).Split(new char[] { ',' });
                    foreach (DataRow dr in dt.Rows)
                    {
                        str.Append("<option value=\"" + dr[v[i].Trim()].ToString() + "\">" + dr[d[i].Trim()].ToString() + "</option>");

                    }
                    if (ds.Tables.Count > 1)
                    {
                        str.Append("~");
                    }
                    i++;
                }

                Result = str.ToString();
                dbHelper.CloseConnection();
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                dbHelper.CloseConnection();
                Result = ex.Message;
            }
            return Result;
        }
        private string ReplaceCaseInsensitive(string input, string search, string replacement)
        {
            var regex = new Regex(search, RegexOptions.None);
            var newSentence = regex.Replace(input, replacement);
            return newSentence;
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
            catch
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

        //private static async Task<DataSet> fetchDataTable(SqlCommand sqlCommand)
        //{
        //    // Create a connection, open it and create a command on the connection
        //    SqlDataAdapter sqlAdapter = null;
        //    DataSet ds_data = null;
        //    try
        //    {
        //        string ConnectionString = ConfigurationManager.ConnectionStrings["dbLactasoy"].ConnectionString;
        //        using (SqlConnection connection = new SqlConnection(ConnectionString))
        //        {
        //            await connection.OpenAsync().ConfigureAwait(false);
        //            sqlCommand.Connection = connection;
        //            sqlAdapter = new SqlDataAdapter(sqlCommand);
        //            ds_data = new DataSet();
        //            sqlAdapter.Fill(ds_data);
        //            return ds_data;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //        return ds_data;
        //    }
        //}

        private static Task<DataSet> fetchDataTable(SqlCommand sqlCommand)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            DataSet ds = new DataSet();
            return Task<DataSet>.Factory.StartNew(() =>
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    sqlCommand.Connection = connection;
                    SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
                    ds = new DataSet();
                    sqlAdapter.Fill(ds);
                    connection.Close();
                }
                return ds;
            });
            //// Create a connection, open it and create a command on the connection
            //SqlDataAdapter sqlAdapter = null;
            //DataSet ds_data = null;
            //try
            //{
            //    string ConnectionString = ConfigurationManager.ConnectionStrings["dbLactasoy"].ConnectionString;
            //    using (SqlConnection connection = new SqlConnection(ConnectionString))
            //    {
            //        await connection.OpenAsync().ConfigureAwait(false);
            //        sqlCommand.Connection = connection;
            //        sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        ds_data = new DataSet();
            //        sqlAdapter.Fill(ds_data);
            //        return ds_data;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //    return ds_data;
            //}
        }

        public DataSet LoadDataGridTable(string FieldWithValue, int Page, int PageSize, string FormID, bool GridUseConnection2 = false, string GridProcedureName = "")
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            Exception ex;
            string ProcedureName = GridProcedureName == "" ? common.getSessionName("ProcedureName", FormID) : EncryAndDecry.Decryp(GridProcedureName);
            DataSet DS = new DataSet();
            string Result;
            try
            {
                //if (!GridUseConnection2)
                //{
                //    dbHelper.InitiateConnection();
                //}
                //else
                //{
                //    dbHelper.InitiateConnection2();
                //}
                try
                {
                    SqlCommand SQL_C = new SqlCommand();
                    string ConnectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    SqlConnection connection = new SqlConnection(ConnectionString);

                    SqlCommand cm = new SqlCommand(ProcedureName, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    connection.Open();

                    SQL_C.CommandType = CommandType.StoredProcedure;
                    SQL_C.CommandText = ProcedureName;
                    char[] chrArray = new char[] { '\u005E' };
                    string[] strArrays = FieldWithValue.Split(chrArray);

                    SqlCommandBuilder.DeriveParameters(cm);
                    string ParameterUserID = "";
                    string ParameterWarehouseID = "";

                    for (int i = 0; i < strArrays.Length; i++)
                    {
                        string FName = strArrays[i];
                        if (FName != "")
                        {
                            chrArray = new char[] { '|' };
                            string[] FNWV = FName.Split(chrArray);
                            string FieldName = EncryAndDecry.Decryp(FNWV[0]);
                            string Value = FNWV[1];
                            if (Value == "All")
                            {
                                Value = "";
                            }
                            if (FieldName == "UserID")
                            {
                                ParameterUserID = "@UserID";
                            }
                            if (FieldName == "WarehouseID")
                            {
                                ParameterWarehouseID = "@WarehouseID";
                            }
                            SQL_C.Parameters.Add(new SqlParameter(string.Concat("@", FieldName), Value));
                        }
                    }
                    foreach (SqlParameter pm in cm.Parameters)
                    {
                        if (pm.ParameterName == "@UserID" && ParameterUserID == "")
                        {
                            SQL_C.Parameters.Add(new SqlParameter("@UserID", HttpContext.Current.Session["UserID"].ToString()));
                        }
                        if (pm.ParameterName == "@WarehouseID" && ParameterWarehouseID == "")
                        {
                            SQL_C.Parameters.Add(new SqlParameter("@WarehouseID", HttpContext.Current.Session["WarehouseID"].ToString()));
                        }
                        if (pm.ParameterName == "@Page")
                        {
                            SQL_C.Parameters.Add(new SqlParameter("@Page", Page));
                        }
                        if (pm.ParameterName == "@RecsPerPage")
                        {
                            SQL_C.Parameters.Add(new SqlParameter("@RecsPerPage", PageSize));
                        }
                    }
                    connection.Close();

                    //SQL_C.CommandTimeout = 180;
                    //DS = Common.ExecuteDataSet(SQL_C);
                    //dbHelper.CloseConnection();
                    DS = fetchDataTable(SQL_C).Result;

                    //dbHelper.ExecuteSqlDataSet(SQL_C, delegate(DataSet datat)
                    //{
                    //  DS = datat;
                    //});
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
            return DS;
        }

        public string ReplaceFullWords(string s, string oldWord, string newWord)
        {
            string str;
            bool flag;
            if (s == null)
            {
                str = null;
            }
            else
            {
                int startIndex = 0;
                while (true)
                {
                    int position = s.IndexOf(oldWord, startIndex);
                    if (position == -1)
                    {
                        break;
                    }
                    int indexAfter = position + oldWord.Length;
                    flag = position != 0 && (this.IsWordChar(s[position - 1]) || (indexAfter != s.Length && this.IsWordChar(s[indexAfter])));
                    if (!flag)
                    {
                        s = string.Concat(s.Substring(0, position), newWord, s.Substring(indexAfter));
                        startIndex = position + newWord.Length;
                    }
                    else
                    {
                        startIndex = position + oldWord.Length;
                    }
                }
                str = s;
            }
            return str;
        }

        public string SaveChange(string Data, string FormID, string FilesName = "")
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            object AutoNumber;
            Exception ex;
            int i;
            string str;
            string[] strArrays;
            char[] chrArray = new char[] { '\u005E' };
            string[] DBDatas = Data.Split(chrArray);
            string DBFields = "";
            string DBDataForInsert = "";
            string Parameters = "";
            string LogText = string.Concat("Insert data to table ", common.getSessionName("TblName", FormID), " with values: ");
            string StatusProcessing;
            try
            {
                dbHelper.InitiateConnection();
                SqlTransaction SQL_T = dbHelper.sqlConnection.BeginTransaction();
                try
                {
                    string PrimaryFieldValue = common.getSessionName("PrimaryFieldValue", FormID);
                    string TblName = common.getSessionName("TblName", FormID);
                    string PrimaryField = common.getSessionName("PrimaryField", FormID);
                    if (PrimaryFieldValue != "0")
                    {
                        LogText = string.Concat("Update data to table ", TblName, " with values: ");
                        LogText = string.Concat(LogText, PrimaryField, " = ", PrimaryFieldValue);
                        DBDataForInsert = string.Concat(PrimaryField, "|", PrimaryFieldValue);
                        PrimaryField = string.Concat(PrimaryField, " = @", PrimaryField);
                    }
                    else
                    {
                        LogText = string.Concat(LogText, PrimaryField, " = AutoNumberID");
                    }
                    string[] strArrays1 = DBDatas;
                    for (i = 0; i < strArrays1.Length; i++)
                    {
                        string DBData = strArrays1[i];
                        chrArray = new char[] { '|' };
                        string[] DBFV = DBData.Split(chrArray);
                        if (DBFields == "")
                        {
                            if (PrimaryFieldValue == "0")
                            {
                                if (DBFV[2].ToString().Trim() == "File" && FilesName != "")
                                {
                                    str = LogText;
                                    DBFV[1] = FilesName;
                                    strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                    LogText = string.Concat(strArrays);
                                    DBFields = EncryAndDecry.Decryp(DBFV[0].ToString().Trim());
                                    Parameters = string.Concat("@", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                    DBDataForInsert = string.Concat(EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim());
                                }
                                else if (DBFV[2].ToString().Trim() != "File")
                                {
                                    str = LogText;
                                    strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                    LogText = string.Concat(strArrays);
                                    DBFields = EncryAndDecry.Decryp(DBFV[0].ToString().Trim());
                                    Parameters = string.Concat("@", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                    DBDataForInsert = string.Concat(EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim());
                                }
                            }
                            else
                            {
                                if (DBFV[2].ToString().Trim() == "File" && FilesName != "")
                                {
                                    str = LogText;
                                    DBFV[1] = FilesName;
                                    strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                    LogText = string.Concat(strArrays);
                                    DBFields = string.Concat(EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = @", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                    str = DBDataForInsert;
                                    strArrays = new string[] { str, "^", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                    DBDataForInsert = string.Concat(strArrays);
                                }
                                else if (DBFV[2].ToString().Trim() != "File")
                                {
                                    str = LogText;
                                    strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                    LogText = string.Concat(strArrays);
                                    DBFields = string.Concat(EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = @", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                    str = DBDataForInsert;
                                    strArrays = new string[] { str, "^", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                    DBDataForInsert = string.Concat(strArrays);
                                }
                            }
                        }
                        else if (PrimaryFieldValue == "0")
                        {
                            if (DBFV[2].ToString().Trim() == "File" && FilesName != "")
                            {
                                str = LogText;
                                DBFV[1] = FilesName;
                                strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                LogText = string.Concat(strArrays);
                                DBFields = string.Concat(DBFields, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                Parameters = string.Concat(Parameters, ", @", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                str = DBDataForInsert;
                                strArrays = new string[] { str, "^", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                DBDataForInsert = string.Concat(strArrays);
                            }
                            else if (DBFV[2].ToString().Trim() != "File")
                            {
                                str = LogText;
                                strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                LogText = string.Concat(strArrays);
                                DBFields = string.Concat(DBFields, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                Parameters = string.Concat(Parameters, ", @", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()));
                                str = DBDataForInsert;
                                strArrays = new string[] { str, "^", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                DBDataForInsert = string.Concat(strArrays);
                            }
                        }
                        else
                        {
                            if (DBFV[2].ToString().Trim() == "File" && FilesName != "")
                            {
                                str = LogText;
                                DBFV[1] = FilesName;
                                strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                LogText = string.Concat(strArrays);
                                str = DBFields;
                                strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = @", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()) };
                                DBFields = string.Concat(strArrays);
                                str = DBDataForInsert;
                                strArrays = new string[] { str, "^", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                DBDataForInsert = string.Concat(strArrays);
                            }
                            else if (DBFV[2].ToString().Trim() != "File")
                            {
                                str = LogText;
                                strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = ", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                LogText = string.Concat(strArrays);
                                str = DBFields;
                                strArrays = new string[] { str, ", ", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), " = @", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()) };
                                DBFields = string.Concat(strArrays);
                                str = DBDataForInsert;
                                strArrays = new string[] { str, "^", EncryAndDecry.Decryp(DBFV[0].ToString().Trim()), "|", DBFV[1].ToString().Trim() == "null" ? "0" : DBFV[1].ToString().Trim() };
                                DBDataForInsert = string.Concat(strArrays);
                            }
                        }
                    }
                    string SQL = "";
                    if (PrimaryFieldValue == "0")
                    {
                        strArrays = new string[] { "Insert Into [", TblName, "] (", DBFields, ") Values (", Parameters, "); SELECT @@IDENTITY" };
                        SQL = string.Concat(strArrays);
                        StatusProcessing = "Data inserted successful.";
                    }
                    else
                    {
                        strArrays = new string[] { "Update [", TblName, "] SET ", DBFields, " Where ", PrimaryField };
                        SQL = string.Concat(strArrays);
                        StatusProcessing = "Data updated successful.";
                    }
                    SqlCommand SQL_C = new SqlCommand(SQL, SQL_T.Connection);
                    chrArray = new char[] { '\u005E' };
                    DBDatas = DBDataForInsert.Split(chrArray);
                    strArrays1 = DBDatas;
                    for (i = 0; i < strArrays1.Length; i++)
                    {
                        string str1 = strArrays1[i].ToString();
                        chrArray = new char[] { '|' };
                        string[] FV = str1.Split(chrArray);
                        SQL_C.Parameters.Add(new SqlParameter(FV[0].Trim(), FV[1].Trim()));
                    }
                    SQL_C.Transaction = SQL_T;
                    if (PrimaryFieldValue == "0")
                    {
                        AutoNumber = SQL_C.ExecuteScalar();
                        LogText = LogText.Replace("AutoNumberID", AutoNumber.ToString());
                    }
                    else
                    {
                        AutoNumber = PrimaryFieldValue;
                        SQL_C.ExecuteNonQuery();
                    }
                    //DataSet ds = this.LoadData(string.Concat("SELECT TableID FROM dbo.LogTableName WHERE TableOriginalName= '", TblName, "'"), false, null);
                    //SQL_C.CommandType = CommandType.StoredProcedure;
                    //SQL_C.CommandText = "SP_INSERT_LOG";
                    //SQL_C.Parameters.Clear();
                    //SQL_C.Parameters.Add("@LogTypeID", SqlDbType.VarChar).Value = LogTypeID;
                    //SQL_C.Parameters.Add("@LogText", SqlDbType.VarChar).Value = LogText;
                    //SQL_C.Parameters.Add("@LogParentID", SqlDbType.VarChar).Value = 0;
                    //SQL_C.Parameters.Add("@TableID", SqlDbType.VarChar).Value = ds.Tables[0].Rows[0][0];
                    //SQL_C.Parameters.Add("@UserID", SqlDbType.VarChar).Value = common.getSessionName("UserID", "");
                    //SQL_C.Transaction = SQL_T;
                    //SQL_C.ExecuteNonQuery();
                    SQL_T.Commit();
                    dbHelper.CloseConnection();
                }
                catch (Exception exception)
                {
                    ex = exception;
                    SQL_T.Rollback();
                    dbHelper.CloseConnection();
                    StatusProcessing = ex.Message;
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
                StatusProcessing = ex.Message;
            }
            return StatusProcessing;
        }

        public SqlResult SaveChange_CustomForm(string FormID, string XML)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            SqlResult sqlResult = new SqlResult();
            try
            {
                try
                {
                    dbHelper.InitiateConnection();
                    string ProcedureName = common.getSessionName("ProcedureName", FormID);
                    SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                    SQL_C.CommandType = CommandType.StoredProcedure;
                    SQL_C.CommandText = ProcedureName;
                    SQL_C.Parameters.Add("@Data", SqlDbType.Xml).Value = XML;
                    SQL_C.Parameters.Add("@UserID", SqlDbType.Int).Value = common.getSessionName("UserID", "");
                    SQL_C.ExecuteNonQuery();
                    sqlResult.Message = "Data save change successful.";
                    sqlResult.Result = ResultType.Successful;
                }
                catch (Exception exception)
                {
                    sqlResult.Message = exception.Message;
                    sqlResult.Result = ResultType.Failed;
                }
            }
            finally
            {
                dbHelper.CloseConnection();
            }
            return sqlResult;
        }

        public string SaveChange_UseProccedure(string Data, string FormID)
        {
            DBHelper dbHelper = new DBHelper();
            Common common = new Common();
            Exception ex;
            char[] chrArray = new char[] { '\u005E' };
            string[] DBDatas = Data.Split(chrArray);
            string StatusProcessing;
            try
            {
                dbHelper.InitiateConnection();
                SqlTransaction SQL_T = dbHelper.sqlConnection.BeginTransaction();
                try
                {
                    string UseProcedureName = common.getSessionName("UseProcedureName", FormID);
                    string PrimaryField = common.getSessionName("PrimaryField", FormID);
                    string PrimaryFieldValue = common.getSessionName("PrimaryFieldValue", FormID);
                    SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                    SQL_C.CommandType = CommandType.StoredProcedure;
                    SQL_C.CommandText = UseProcedureName;
                    if (PrimaryFieldValue != "")
                    {
                        SQL_C.Parameters.Add(new SqlParameter(string.Concat("@", PrimaryField), PrimaryFieldValue));
                    }
                    string[] strArrays = DBDatas;
                    for (int i = 0; i < strArrays.Length; i++)
                    {
                        string str = strArrays[i].ToString();
                        chrArray = new char[] { '|' };
                        string[] FV = str.Split(chrArray);
                        if (EncryAndDecry.Decryp(FV[0].Trim()) != "")
                        {
                            SQL_C.Parameters.Add(new SqlParameter(EncryAndDecry.Decryp(FV[0].Trim()), FV[1].Trim()));
                        }
                    }
                    SQL_C.Parameters.Add(new SqlParameter("@UserID", HttpContext.Current.Session["UserID"].ToString()));
                    SQL_C.Transaction = SQL_T;
                    SQL_C.ExecuteNonQuery();
                    SQL_T.Commit();
                    dbHelper.CloseConnection();
                    StatusProcessing = PrimaryFieldValue != "0" ? "Data updated successful." : "Data inserted successful.";
                }
                catch (Exception exception)
                {
                    ex = exception;
                    SQL_T.Rollback();
                    dbHelper.CloseConnection();
                    StatusProcessing = ex.Message;
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
                StatusProcessing = ex.Message;
            }
            return StatusProcessing;
        }

        public SqlResult SaveChangeData(string SQL, bool ISStoreProcedure = false, Dictionary<string, string> Para = null, bool ExecuteScalar = false)
        {
            DBHelper dbHelper = new DBHelper();
            SqlResult Result = new SqlResult();
            try
            {
                dbHelper.InitiateConnection();
                SqlTransaction SQL_T = dbHelper.sqlConnection.BeginTransaction();
                SqlCommand SQL_C = dbHelper.sqlConnection.CreateCommand();
                SQL_C.CommandType = CommandType.Text;
                try
                {
                    if (Para != null && Para.Count > 0)
                    {
                        SQL_C.CommandType = CommandType.StoredProcedure;
                        SQL_C.CommandText = SQL;
                        foreach (KeyValuePair<string, string> item in Para)
                        {
                            SQL_C.Parameters.Add(new SqlParameter(item.Key, item.Value));
                        }
                        SQL_C.CommandTimeout = 600;
                    }
                    else
                    {
                        SQL_C.CommandText = SQL;
                    }
                    SQL_C.Transaction = SQL_T;
                    if (ExecuteScalar)
                    {
                        Result.Value = SQL_C.ExecuteScalar().ToString();
                    }
                    else
                    {
                        SQL_C.ExecuteNonQuery();
                    }
                    SQL_T.Commit();
                    dbHelper.CloseConnection();
                    Result.Result = ResultType.Successful;
                    Result.Message = "Data save change successful.";
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    SQL_T.Rollback();
                    dbHelper.CloseConnection();
                    Result.Message = ex.Message;
                    Result.Result = ResultType.Failed;
                }
            }
            catch (Exception exception1)
            {
                Result.Message = exception1.Message;
                try
                {
                    dbHelper.CloseConnection();
                }
                catch
                {
                }
            }
            return Result;
        }

        public SqlResult ValidationSubmitQTY(int ItemID, double QTY, int PurchaseOrderID = 0)
        {
            SqlResult result = new SqlResult
            {
                //Dictionary<string, string> strs = new Dictionary<string, string>()
                //{
                //	{ "ItemID", ItemID.ToString() },
                //	{ "QTY", QTY.ToString() },
                //	{ "PurchaseOrderID", PurchaseOrderID.ToString() }
                //};
                //DataSet ds = this.LoadData("SP_CHECK_QTYIN", true, strs);
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //	if (ds.Tables[0].Rows[0][0].ToString() == "1")
                //	{
                //		result.Result = ResultType.Failed;
                //	}
                //	else
                //	{
                Result = ResultType.Successful
            };
            //}
            //object[] qTY = new object[] { "Quantity ", QTY, " cannot bigger then Current Quantity: ", ds.Tables[0].Rows[0][1].ToString() };
            //result.Message = string.Concat(qTY);
            //}
            return result;
        }
    }
}
