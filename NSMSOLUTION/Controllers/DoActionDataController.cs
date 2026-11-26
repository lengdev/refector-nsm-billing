using MVC;
using Newtonsoft.Json;
using NSM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TL;
using Utilities;

namespace NewInventory.Controllers
{
    [Authorize]
    public class DoActionDataController : Controller
    {
        static WTelegram.Client client = null;
        static string SessionPath = "";
        static string Config(string what)
        {
            switch (what)
            {
                case "session_pathname": return SessionPath;// "D:\\NSM PROJECT\\NSM_ISP_BILLING\\NSMSOLUTION\\Uploads\\TelegramSession\\0965565118.session";
                case "api_id": return "10369643";
                case "api_hash": return "98a5c16a746f18a7eda29bb08d428502";
                case "phone_number": return "+855965565118";
                case "verification_code": return "58676";
                //case "first_name": return "John";      // if sign-up is required
                //case "last_name": return "Doe";        // if sign-up is required
                case "password": return "Thea8989";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
        static string Configuration(string what)
        {
            switch (what)
            {
                case "session_pathname": return SessionPath;// "D:\\NSM PROJECT\\NSM_ISP_BILLING\\NSMSOLUTION\\Uploads\\TelegramSession\\0965565118.session";
                case "api_id": return "27294688";
                case "api_hash": return "708a468fb6811978a5ad88088a1a31ea";
                case "phone_number": return "+85561588777";
                //case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                //case "first_name": return "John";      // if sign-up is required
                //case "last_name": return "Doe";        // if sign-up is required
                //case "password": return "Acc@Team@NSM";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
        public DoActionDataController()
        {
        }

        [Authorize, ValidateInput(false)]
        public JsonResult Autocomplete(string term, string SQL, bool IsUseConnection2 = false, string SQL2 = "", string ValueControlNameID = "", string ConditionField = "", string CboValue = "", string DisplayOnFields = "", string HiddenFeild = "")
        {
            HTML html = new HTML();
            Collection<AutoCompleteDTO> collection = new Collection<AutoCompleteDTO>();
            collection = html.Autocomplete(term, SQL, IsUseConnection2, SQL2, ValueControlNameID, ConditionField, CboValue, DisplayOnFields, HiddenFeild);
            return base.Json(collection, JsonRequestBehavior.AllowGet);
        }
        public async Task<SqlResult> AddUserToTelegram(string CustomerID, string CustomerPhone, string FirstName, string LastName, string Memo = "", bool AutoCreate = true, int TelegramCustomerID = 0)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            try
            {
                SessionPath = html.httpContext.Server.MapPath("~/Uploads/TelegramSession");
                SessionPath = Path.Combine(SessionPath, "0965565118.session");
                using (client = new WTelegram.Client(Config))
                {
                    var userinfo = await client.LoginUserIfNeeded();
                    string output = Regex.Replace(CustomerPhone, @"\s+", "").Replace("+", string.Empty);
                    if (output == "." || output == "#") output = "";
                    string IsUsingTelegram = "No";
                    if (output != "")
                    {
                        CustomerPhone = output;
                        //var regex = new Regex(Regex.Escape("0"));
                        //CustomerPhone = regex.Replace(CustomerPhone, "855", 1, 1);
                        var strphone = CustomerPhone.Substring(0, 1).ToString();
                        if (strphone == "0")
                        {
                            strphone = "855" + CustomerPhone.Substring(1, CustomerPhone.Length - 1).ToString();
                            CustomerPhone = strphone;
                        }
                        if (html.IsValidPhoneNumber(CustomerPhone))
                        {
                            // Check if the phone number is connected to a Telegram account
                            var contacts = await client.Contacts_ImportContacts(new[] { new InputPhoneContact { phone = CustomerPhone, first_name = FirstName, last_name = LastName } });
                            if (contacts.users.Count > 0)
                            {
                                IsUsingTelegram = "Yes";
                                SessionPath = html.httpContext.Server.MapPath("~/Uploads/TelegramSession");
                                SessionPath = Path.Combine(SessionPath, "061588777.session");
                                using (client = new WTelegram.Client(Configuration))
                                {
                                    userinfo = await client.LoginUserIfNeeded();
                                    contacts = await client.Contacts_ImportContacts(new[] { new InputPhoneContact { phone = CustomerPhone, first_name = FirstName, last_name = LastName } });
                                }
                                ;
                            }
                            string sql = @"if(exists (select CustomerID from dbo.TblCustomerTelegram where CustomerID = @CustomerID))
begin
update dbo.TblCustomerTelegram set FirstName = @FirstName, LastName = @LastName where CustomerID = @CustomerID
end
else
begin
insert into TblCustomerTelegram (CustomerID, FirstName, LastName, CustomerPhone, IsUsingTelegram, Memo, CreateDate, CreateBy, UserID, SystemID, WarehouseID, DomainID, TelegramNotificationID)
values (@CustomerID, @FirstName, @LastName, @CustomerPhone, @IsUsingTelegram, @Memo, @CreateDate, @CreateBy, @UserID, @SystemID, @WarehouseID, @DomainID, (
select STUFF((SELECT ',' + convert(varchar, s.TelegramNotificationID) FROM dbo.TblTelegramNotificationSchedule s
where s.SystemID = @SystemID and s.WarehouseID = @WarehouseID and s.DomainID like '%'+@DomainID+'%'
FOR XML PATH('')), 1, 1, '')))
end";
                            Dictionary<string, string> para = new Dictionary<string, string>{
                                {"CustomerID",CustomerID.ToString() },
                                {"FirstName",FirstName },
                                {"LastName",LastName },
                                {"CustomerPhone",CustomerPhone },
                                {"IsUsingTelegram",IsUsingTelegram },
                                {"Memo",Memo },
                                {"CreateBy", html.UserFullName},
                                {"CreateDate", html.GetCurrentDate()},
                                {"UserID", html.UserID},
                                {"SystemID", html.SystemID},
                                {"WarehouseID", html.WarehouseID},
                                {"DomainID", html.DomainID},
                            };
                            if (!AutoCreate)
                            {
                                if (TelegramCustomerID == 0)
                                {
                                    sql = @"insert into TblCustomerTelegram (CustomerID, FirstName, LastName, CustomerPhone, IsUsingTelegram, Memo, CreateDate, CreateBy, UserID, SystemID, WarehouseID, DomainID, TelegramNotificationID)
                        	values (@CustomerID, @FirstName, @LastName, @CustomerPhone, @IsUsingTelegram, @Memo, @CreateDate, @CreateBy, @UserID, @SystemID, @WarehouseID, @DomainID, (
                        select STUFF((SELECT ',' + convert(varchar, s.TelegramNotificationID) FROM dbo.TblTelegramNotificationSchedule s
                        where s.SystemID = " + html.SystemID + " and s.WarehouseID = " + html.WarehouseID + " and s.DomainID like '%" + html.DomainID + @"%'
                        FOR XML PATH('')), 1, 1, '')))";
                                }
                                else
                                {
                                    sql = @"update TblCustomerTelegram set FirstName = @FirstName, LastName = @LastName, CustomerPhone = @CustomerPhone,
                        IsUsingTelegram = @IsUsingTelegram, Memo = @Memo, ModifyDate = @ModifyDate, ModifyBy = @ModifyBy, ModifyByUserID = @ModifyByUserID,
                        SystemID = @SystemID, WarehouseID = @WarehouseID, DomainID = @DomainID where TelegramCustomerID = @TelegramCustomerID";
                                    para.Add("ModifyDate", html.GetCurrentDate());
                                    para.Add("ModifyBy", html.UserFullName);
                                    para.Add("ModifyByUserID", html.UserID);
                                    para.Add("TelegramCustomerID", TelegramCustomerID.ToString());
                                }
                            }
                            sqlResult = html.SaveChangeData(sql, false, para);
                            sqlResult.Result = ResultType.Successful;
                        }
                    }
                }
                ;
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        [Authorize]
        public JsonResult CallBackHtmlData(string SQL, string Value, string Condition)
        {
            Common common = new Common();
            DBHelper dbHelper = new DBHelper();
            string Data = "";
            dbHelper.InitiateConnection();
            SQL = EncryAndDecry.Decryp(SQL);
            SqlCommand sqlCommand = new SqlCommand(SQL, dbHelper.sqlConnection)
            {
                CommandType = CommandType.Text,
                CommandText = SQL
            };
            SqlCommand SQL_C = sqlCommand;
            SQL_C.Parameters.AddWithValue("@SearchText", Value);
            if (Condition != "")
            {
                char[] chrArray = new char[] { '\u005E' };
                string[] strArrays = Condition.Split(chrArray);
                for (int i = 0; i < strArrays.Length; i++)
                {
                    string FV = strArrays[i];
                    if (FV != "")
                    {
                        chrArray = new char[] { '|' };
                        string[] FieldValue = FV.Split(chrArray);
                        SQL_C.Parameters.AddWithValue(string.Concat("@", FieldValue[0].ToString()), FieldValue[1].ToString());
                    }
                }
            }
            DataTable dt = common.ExecuteDataTable(SQL_C);
            foreach (DataColumn dc in dt.Columns)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Data = Data != "" ? string.Concat(Data, "|", dr[dc.ColumnName].ToString()) : dr[dc.ColumnName].ToString();
                }
            }
            dbHelper.CloseConnection();
            return base.Json(new { Data = Data });
        }
        [Authorize]
        public string CheckExsitingField(string OldValue, string ConditionFiledName, string ConditonText, string CheckExsitingWithField, string FormID, string ISUniqueFiledWithSystemID = "", string CheckExsitingWithControllName = "", string TblName = "", int SystemID = 0, int WarehouseID = 0)
        {
            HTML html = new HTML();
            return html.CheckExsitingDataField(OldValue, ConditionFiledName, ConditonText, CheckExsitingWithField, FormID, ISUniqueFiledWithSystemID, CheckExsitingWithControllName, TblName, SystemID, WarehouseID);
        }
        [Authorize]
        public JsonResult CheckField(string SQL, string Data, bool isStore)
        {
            SqlResult sqlResult = new SqlResult();
            DoActionData doActionData = new DoActionData();
            sqlResult = doActionData.CheckField(SQL, Data, isStore);
            JsonResult jsonResult = base.Json(new { Message = sqlResult.Message, Result = sqlResult.Result });
            return jsonResult;
        }
        [Authorize]
        public JsonResult DeleteData(string FieldName, string DataID, string DeleteUseProcedureName, string FormID, string TableName = "", string DeleteWithTableName = "", string DeletePrimaryTblName = "", string DeletePrimaryFiledName = "", string DeletePrimaryValue = "", string DeletePrimaryTblNameDependOnTblName = "")
        {
            HTML html = new HTML();
            SqlResult Result = new SqlResult();
            Result = html.DeteleData(FieldName, DataID, DeleteUseProcedureName, FormID, TableName, DeleteWithTableName, DeletePrimaryTblName, DeletePrimaryFiledName, DeletePrimaryValue, DeletePrimaryTblNameDependOnTblName);
            return Json(new { Message = Result.Message, Result = Result.Result });
        }
        [Authorize]
        public string DrawCheckBox(string CheckBoxName, string SQL, string Value, string DisplayText)
        {
            DoActionData doActionData = new DoActionData();
            return doActionData.DrawCheckBox(CheckBoxName, SQL, Value, DisplayText);
        }
        [Authorize]
        public string LoadCallBackData(string SQL, string Value, bool IsUseConnection2, string SQL2 = "", string SQLCallBack2 = "", Boolean Encry = false, string CallBackWithControllValueID = "")
        {
            Common common = new Common();
            DBHelper dbHelper = new DBHelper();
            SqlCommand SQL_C;
            Value = Value == "null" ? "" : Value;
            string Result = "";
            if (Encry == true)
                SQL = EncryAndDecry.Decryp(SQL);
            if (SQL != "")
            {
                SQL = SQL.Replace("Like", "=");
                try
                {
                    try
                    {
                        DataTable DT = new DataTable();
                        if (!IsUseConnection2 && SQLCallBack2 != "")
                        {
                            dbHelper.InitiateConnection2();
                            SQLCallBack2 = EncryAndDecry.Decryp(SQLCallBack2);
                            SQLCallBack2 = SQLCallBack2.Replace("Like", "=");
                            SqlCommand sqlCommand = new SqlCommand(SQLCallBack2, dbHelper.sqlConnection)
                            {
                                CommandType = CommandType.Text
                            };
                            SQL_C = sqlCommand;
                            SQL_C.Parameters.Clear();
                            SQL_C.Parameters.AddWithValue("@SearchText", Value.ToLower());
                            if (CallBackWithControllValueID != "")
                            {
                                SQL_C.Parameters.AddWithValue("@Search_Text2", CallBackWithControllValueID.ToLower());
                            }
                            DT = common.ExecuteDataTable(SQL_C);
                        }
                        if (DT.Rows.Count == 0)
                        {
                            dbHelper.InitiateConnection();
                            SqlCommand sqlCommand1 = new SqlCommand(SQL, dbHelper.sqlConnection)
                            {
                                CommandType = CommandType.Text,
                                CommandText = SQL
                            };
                            SQL_C = sqlCommand1;
                            SQL_C.Parameters.AddWithValue("@SearchText", Value.ToLower());
                            if (CallBackWithControllValueID != "")
                            {
                                SQL_C.Parameters.AddWithValue("@Search_Text2", CallBackWithControllValueID.ToLower());
                            }
                            DT = common.ExecuteDataTable(SQL_C);
                        }
                        string Rst = "";
                        int i = 0;
                        foreach (DataRow d in DT.Rows)
                        {
                            i = 0;
                            foreach (DataColumn dr in DT.Columns)
                            {
                                if (i == 0)
                                {
                                    if (Rst != "")
                                        Rst += "|" + d[i].ToString();
                                    else
                                        Rst = d[i].ToString();
                                }
                                else
                                {
                                    Rst += "|" + d[i].ToString();
                                }
                                //Rst = (Rst != "" ? string.Concat(Rst, "|", d[i].ToString()) : d[i].ToString());
                                i++;
                            }
                        }
                        dbHelper.CloseConnection();
                        Result = Rst;
                    }
                    catch (Exception exception)
                    {
                        Result = exception.Message;
                        dbHelper.CloseConnection();
                    }
                }
                catch (Exception exception1)
                {
                    Result = exception1.Message;
                    try
                    {
                        dbHelper.CloseConnection();
                    }
                    catch
                    {
                    }
                }
            }
            return Result;
        }
        [Authorize, ValidateInput(false)]
        public string LoadCBOData(string CboValue, string SQL, string Value, string DisplayText, bool ShowAll, string CallBackWithControllValueID = "", string ConditionField = "", string ConditionFieldSearch2 = "")
        {
            DoActionData doActionData = new DoActionData();
            string str = doActionData.LoadCBOData(CboValue, SQL, Value, DisplayText, ShowAll, CallBackWithControllValueID, ConditionField, ConditionFieldSearch2);
            return str;
        }
        [Authorize]
        public string LoadDataToCBO(string CboID, string SQL, string Value, string DisplayText, string Event, bool ShowAll, bool Encrypt = true, bool IsAppend = false, bool IsUseConnection2 = false, string ConditionFielVale = "", bool Multiple = false, bool IsCallBackToComboBox = false)
        {
            HTML html = new HTML();
            return html.LoadDataToCBO(CboID, SQL, Value, DisplayText, Event, ShowAll, false, Encrypt, IsAppend, IsUseConnection2, ConditionFielVale, Multiple, IsCallBackToComboBox);
        }
        [Authorize]
        public async Task<string> LoadTableGrid(int PageSize, int Page, string GData, string ModifyField, string FieldWithValue, string EditTitle, string EditURL, string DialogBoxWidth, int DialogBoxHeight, string FormID, bool DrawTotalAllHeader = false, bool DrawTotalAllFooter = false, int DrawTotalAllHeaderDataTableNumber = 0, int DrawTotalAllFooterDataTableNumber = 0, bool GridUseConnection2 = false, bool PrintHeader = false, int FooterSummaryDataTable = 0, string LinkSummaryField = "", string LinkSummaryColumn = "", bool PrintFooter = false, bool HideOnPrintTotalFooter = false, bool ISPrintOut = false, string DrawTAllHeader = null, string DrawTotalAllHeaderDTableNumber = null, string GridProcedureName = "", string LoadGridFunction = "", bool DrawTotalHeaderTbl = false, int dataTableNumber = 0, string TableClassName = "", string TblID = "", bool ShowPaging = true, bool NotShowWithNoRecord = false, string SQL = "", string GridHeight = "", string OrderBy = "", string TableName = "", string PrimaryField = "", bool AllowAdd = false, bool AllowEdit = false, bool AllowDelete = false, bool IsUseHavingCondition = false, string UserID = "", string SystemID = "", string WarehouseID = "", bool IsHaveWhereCondition = false, bool isfixedTableHeader = false, bool ShowPrintOnTopTable = true, string rnd = "", string Status = "", string Title = "", string ReportTitle = "", string OnDialogCloseCallbackFunction = "", bool ShowSearchBox = false, string SearchBoxWithColumnName = "", string SearchBoxData = "", string RDName = "", string DomainID = "", bool IsDrawKendoGrid = false, string GridTable = "", string kendoGridSetting = "", string GridTable2 = "", string SQL2 = "", string LinkField = "", string JsonData = "")
        {
            HTML html = new HTML();
            GridProperty gridProperty = new GridProperty
            {
                EditURL = EditURL,
                AllowAdd = AllowAdd,
                AllowEdit = AllowEdit,
                AllowDelete = AllowDelete,
                DialogBoxHeight = DialogBoxHeight.ToString(),
                DialogBoxWidth = DialogBoxWidth,
                OnDialogCloseCallbackFunction = OnDialogCloseCallbackFunction,
                ServerRoot = html.ServerRoot,
                TableName = TableName,
                FormID = FormID,
                LoadGridFunction = LoadGridFunction,
                TblID = TblID
            };
            if (Status != "")
                TableClassName = "table table-bordered table-head-fixed";
            return await html.DrawTableGrid(PageSize, Page, GData, ModifyField, FieldWithValue, EditTitle, EditURL, DialogBoxWidth, DialogBoxHeight, FormID, DrawTotalAllHeader, DrawTotalAllFooter, DrawTotalAllHeaderDataTableNumber, DrawTotalAllFooterDataTableNumber, GridUseConnection2, PrintHeader, FooterSummaryDataTable, LinkSummaryField, LinkSummaryColumn, PrintFooter, HideOnPrintTotalFooter, ISPrintOut, DrawTAllHeader, DrawTotalAllHeaderDTableNumber, GridProcedureName, LoadGridFunction, DrawTotalHeaderTbl, dataTableNumber, TableClassName == "" ? "table table-bordered table-head-fixed text-nowrap" : TableClassName, TblID, ShowPaging, NotShowWithNoRecord, SQL, GridHeight, OrderBy, TableName, PrimaryField, AllowAdd, AllowEdit, AllowDelete, IsUseHavingCondition, UserID, SystemID == "" ? html.SystemID : SystemID, WarehouseID == "" ? html.WarehouseID : WarehouseID, IsHaveWhereCondition, isfixedTableHeader, ShowPrintOnTopTable, rnd, Status, Title, ReportTitle, OnDialogCloseCallbackFunction, ShowSearchBox, SearchBoxWithColumnName, SearchBoxData, RDName, DomainID, IsDrawKendoGrid, GridTable, kendoGridSetting, gridProperty, GridTable2, SQL2, LinkField, JsonData);
        }
        [Authorize, ValidateInput(false)]
        public JsonResult SaveChange(string Data, string FormID, string TableName = "", string DestinationFile = "", string PrimaryField = "", string PrimaryFieldValue = "0", object[] objInsertTable = null, bool SaveChangeAndDeleteOldDataWhenDrawBtnSave = false, object[] UserLogData = null)
        {
            SqlResult result = new SqlResult();
            HTML html = new HTML();
            result = html.SaveChange(Data, FormID, TableName, DestinationFile, PrimaryField, PrimaryFieldValue, objInsertTable, SaveChangeAndDeleteOldDataWhenDrawBtnSave, UserLogData);
            return Json(new { Result = result.Result, Message = result.Message, Data = result.Value }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult SaveChangeCustomForm(string FormID, string Data, string SubData, int DBFields, string Discount = "")
        {
            Common common = new Common();
            DoActionData doActionData = new DoActionData();
            string FV;
            string[] FieldValue;
            int num;
            string[] strArrays;
            SqlResult sqlResult = new SqlResult();
            char[] chrArray = new char[] { '\u005E' };
            string[] Datas = Data.Split(chrArray);
            chrArray = new char[] { '\u005E' };
            string[] SubDatas = SubData.Split(chrArray);
            StringBuilder sb_Data = new StringBuilder();
            sb_Data.Append("<Data>");
            string[] strArrays1 = Datas;
            for (num = 0; num < strArrays1.Length; num++)
            {
                FV = strArrays1[num];
                chrArray = new char[] { '|' };
                FieldValue = FV.Split(chrArray);
                strArrays = new string[] { "<", EncryAndDecry.Decryp(FieldValue[0]), ">", FieldValue[1], "</", EncryAndDecry.Decryp(FieldValue[0]), ">" };
                sb_Data.Append(string.Concat(strArrays));
            }
            if (Discount != "")
            {
                sb_Data.Append(string.Concat("<Discount>", Discount, "</Discount>"));
            }
            sb_Data.Append("<SubDatas>");
            int i = 0;
            strArrays1 = SubDatas;
            for (num = 0; num < strArrays1.Length; num++)
            {
                FV = strArrays1[num];
                chrArray = new char[] { '|' };
                FieldValue = FV.Split(chrArray);
                strArrays = new string[] { "<", EncryAndDecry.Decryp(FieldValue[0]), ">", FieldValue[1], "</", EncryAndDecry.Decryp(FieldValue[0]), ">" };
                sb_Data.Append(string.Concat(strArrays));
                i++;
                if (DBFields == i)
                {
                    i = 0;
                    sb_Data.Append("</SubDatas><SubDatas>");
                }
            }
            sb_Data.Append("</SubDatas>");
            sb_Data.Append("</Data>");
            sqlResult = doActionData.SaveChange_CustomForm(FormID, sb_Data.ToString());
            JsonResult jsonResult = base.Json(new { PrimaryFieldValue = common.getSessionName("PrimaryFieldValue", FormID), Message = sqlResult.Message, Result = sqlResult.Result });
            return jsonResult;
        }

        public JsonResult checkISUniqueFiled(string OldValue, string NewValue, string TblName, string FieldName, string CheckExsitingWithControllName = "", string CheckExsitingWithField = "", string CustomCondition = "")
        {
            HTML html = new HTML();
            string SQL = "";
            SqlResult result = new SqlResult();
            TblName = EncryAndDecry.Decryp(HttpUtility.UrlDecode(TblName.Trim()));
            FieldName = EncryAndDecry.Decryp(HttpUtility.UrlDecode(FieldName.Trim()));
            OldValue = html.HtmlDecode(OldValue);
            NewValue = html.HtmlDecode(NewValue);
            if (OldValue.ToString().Trim() != NewValue.ToString().Trim() && NewValue.ToString().Trim() != "" && OldValue.ToString().Trim() != "null" && CheckExsitingWithControllName == "")
            {
                SQL = "select " + FieldName + " from dbo." + TblName + " ";
                SQL += "where lower(" + FieldName + ") = N'" + NewValue.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            }
            else if (CheckExsitingWithField != "" && CheckExsitingWithControllName != "")
            {
                SQL = "select " + FieldName + " from dbo." + TblName + " ";
                SQL += "where lower(" + FieldName + ") = N'" + NewValue.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
                SQL += " and " + CheckExsitingWithField + " = N'" + CheckExsitingWithControllName + "'";
            }
            if (CustomCondition != "")
            {
                SQL += " and " + CustomCondition;

            }
            result.Data = html.LoadData(SQL);
            result.Result = ResultType.Successful;
            if (result.Data.Tables.Count > 0)
            {
                if (result.Data.Tables[0].Rows.Count > 0)
                {
                    result.Message = NewValue + " is exsiting in database.";
                    result.Result = ResultType.Failed;
                }
            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public JsonResult SaveChangeFormV1(string[] Data, string[] DestinationFile = null, string SaveDataWithProcedureName = "", string[] InsertCustomTable = null, string[] UploadDragDropFile = null, string[] TblDestinationFile = null, object[] UserLogData = null, string[] UploadFile = null)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            //sqlResult = html.SaveChangeFormV1(Data, DestinationFile, SaveDataWithProcedureName, InsertCustomTable, UploadDragDropFile, TblDestinationFile, UserLogData, UploadFile);
            sqlResult = html.SaveChangeFormV2(Data, DestinationFile, SaveDataWithProcedureName, InsertCustomTable, UploadDragDropFile, TblDestinationFile, UserLogData, UploadFile);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value, IDENTITYValue = sqlResult.keepIDENTITYValue }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReturnSQLJson(string SQL, string FieldName = "", string Value = "", bool IsRender = false, string ID = "", string TypeaheadGroupByField = "", string DBField = "", string DisplayOnFields = "", string HiddenFeild = "", string CallBackFunction = "", string Name = "")
        {
            HTML html = new HTML();
            if (!IsRender)
            {
                SQL = EncryAndDecry.Decryp(SQL);
                string Data;
                if (FieldName == "")
                    Data = html.JsonData(SQL);
                else
                {
                    Dictionary<string, string> para = new Dictionary<string, string>
                    {
                        { FieldName, Value }
                    };
                    Data = html.JsonData(SQL, false, para);
                }
                return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<script>");
                SQL = EncryAndDecry.Decryp(SQL);

                //sb.AppendLine("function Reload_" + ID + "() {");
                //sb.AppendLine("var data = " + html.JsonData(SQL, false) + ";");
                //sb.AppendLine("PostData(\"DoActionData/ReturnSQLJson/\", {SQL:\"" + EncryAndDecry.Encryp(SQL) + "\"}, function(response) {");
                DataSet ds = new DataSet();
                if (FieldName == "")
                {
                    ds = html.LoadData(SQL);
                    sb.AppendLine("var data_" + ID + "=" + html.JsonData(SQL) + ";");
                }
                else
                {
                    Dictionary<string, string> para = new Dictionary<string, string>
                    {
                        { FieldName, Value }
                    };
                    ds = html.LoadData(SQL, false, para);
                    sb.AppendLine("var data_" + ID + "=" + html.JsonData(SQL, false, para) + ";");
                }

                sb.AppendLine("var Account_" + ID + " = data_" + ID + ";");
                sb.AppendLine("$(\"#" + ID + "\").typeahead(");
                sb.AppendLine("{");
                sb.AppendLine("hint: true,");
                sb.AppendLine("highlight: true,");
                sb.AppendLine("minLength: 0");
                sb.AppendLine("},");
                string GroupByField2 = "";
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string GroupByField1 = dr[TypeaheadGroupByField].ToString();
                    if (GroupByField1 != GroupByField2)
                    {
                        sb.AppendLine("{");
                        sb.AppendLine("limit: 9999,");
                        sb.AppendLine("display: 'Name',");
                        sb.AppendLine("source: function (query, cb) {");
                        sb.AppendLine("var matches_" + ID + " = [];");
                        sb.AppendLine("var Data_" + ID + " = Account_" + ID + ".filter(function (data) {");
                        sb.AppendLine("return data." + TypeaheadGroupByField + " === \"" + GroupByField1 + "\";");
                        sb.AppendLine("});");
                        sb.AppendLine("$.each(Data_" + ID + ", function (i, str) {");
                        sb.AppendLine("const value_" + ID + " = str." + DBField + ";");
                        sb.AppendLine("const globalRegex_" + ID + " = new RegExp(query, \"\\ig\");");
                        sb.AppendLine("if (globalRegex_" + ID + ".test(value_" + ID + ")) {");
                        sb.Append("matches_" + ID + ".push({ Name: str." + DBField + ", ID: str." + ds.Tables[0].Columns[0].ColumnName + " ");
                        if (DisplayOnFields != "")
                        {
                            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(DisplayOnFields);
                            foreach (var kv in dict)
                            {
                                string controlID = kv.Key;
                                string fname = kv.Value;
                                sb.Append(", " + kv.Value + ": str." + kv.Value);
                            }
                        }
                        sb.Append("});");
                        sb.AppendLine("}");
                        sb.AppendLine("});");
                        sb.AppendLine("cb(matches_" + ID + ");");
                        sb.AppendLine("},");
                        sb.AppendLine("templates: {");
                        sb.AppendLine("header: '<div class=\"border-bottom p-2 border-top typeahead-head\">" + GroupByField1 + "</div>'");
                        sb.AppendLine("}");
                        sb.AppendLine("},");
                    }
                    GroupByField2 = dr[TypeaheadGroupByField].ToString();
                }
                sb.AppendLine(").on(\"focus\", function (aEvent) { if (aEvent.isDefaultPrevented()) return; aEvent.preventDefault(); $(this).select(); }).on(\"click\", function (aEvent) { if (aEvent.isDefaultPrevented()) return; aEvent.preventDefault(); $(this).typeahead(\"open\"); }");
                sb.AppendLine(").on(\"typeahead:close\",function() { $(\"div[name=" + (ID == "" ? Name : ID) + "div_add_New_Item]\").remove(); }");
                sb.AppendLine(").on(\"typeahead:change\", function (aEvent, aSuggestion) {");
                sb.AppendLine("if (aEvent.isDefaultPrevented()) return;\r\naEvent.preventDefault();");
                sb.AppendLine("let data_" + ID + " = Account_" + ID + ".filter(name => name['" + DBField + "'] === aSuggestion);");
                if (HiddenFeild != "")
                {
                    sb.AppendLine("if(data_" + ID + "[0])");
                    sb.AppendLine("$(\"" + (ID != "" ? "#" + ID + "_" + HiddenFeild : "input[name=" + HiddenFeild + "]") + "\").val(data_" + ID + "[0]." + HiddenFeild + ");");
                    sb.AppendLine("else");
                    sb.AppendLine("$(\"" + (ID != "" ? "#" + ID + "_" + HiddenFeild : "input[name=" + HiddenFeild + "]") + "\").val('');");
                }
                if (DisplayOnFields != "")
                {
                    sb.AppendLine("if(data_" + ID + "[0]) {");
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(DisplayOnFields);
                    foreach (var kv in dict)
                    {
                        string controlID = kv.Key;
                        string fname = kv.Value;
                        sb.AppendLine("$(\"#" + controlID + "\").val(data_" + ID + "[0]." + fname + ");");
                    }
                    sb.AppendLine("}");
                }
                if (CallBackFunction != "")
                {
                    sb.AppendLine("setTimeout(function () {");
                    sb.AppendLine(CallBackFunction + "();");
                    sb.AppendLine("}, 100);");
                }
                sb.AppendLine("});");
                sb.AppendLine("</script>");
                //sb.AppendLine("});");
                //sb.AppendLine("}");
                return Json(new { Data = sb.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ContentToExcel(string Data)
        {
            Response.AddHeader("content-disposition", "attachment;filename=FileName.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.xls";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            //divExport.RenderControl(htmlWrite);
            Response.Write(stringWrite.ToString());
            Response.End();
            return View(Data);
        }
        public void ExportToExcel(string html, string fileName = "test")
        {
            html = html.Replace("&gt;", ">");
            html = html.Replace("&lt;", "<");
            HttpContext.Response.ClearContent();
            HttpContext.Response.AddHeader("content-disposition", "attachment;filename=" + fileName + "_" + DateTime.Now.ToString("M_dd_yyyy_H_M_s") + ".xls");
            HttpContext.Response.ContentType = "application/xls";
            HttpContext.Response.Write(html);
            HttpContext.Response.End();
        }
    }
}