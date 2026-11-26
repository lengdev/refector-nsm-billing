using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using tik4net;
using Utilities;

namespace MVC
{
    public class Mikrotik
    {
        string HOST = "";
        string USER = "";
        string PASS = "";
        int PPORT = 8728;
        public int VERSION = 1; // OLD VERSION.
        private ITikConnection connection = null;
        private string Owner = "";
        public SqlResult OpenMKConnection(string ip = "", int port = 0, string username = "", string password = "")
        {
            ip = ip == "" ? HOST : ip;
            port = port == 0 ? PPORT : port;
            username = username == "" ? USER : username;
            password = password == "" ? PASS : password;
            SqlResult sqlResult = new SqlResult();
            try
            {
                if (connection == null)
                    connection = ConnectionFactory.OpenConnection(TikConnectionType.Api, ip, port, username, password);
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Connection to MK is successful.";
            }
            catch (System.Net.Sockets.SocketException s)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = s.Message;
            }
            catch (TikConnectionLoginException l)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = l.Message;
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public void CloseMKConnection()
        {
            if (connection == null)
                return;

            try
            {
                connection.Close();
            }
            catch (NotImplementedException)
            {
                // Tik4Net BUG / limitation:
                // Router returned !empty during close
                // Treat as SAFE and CLOSED
            }
            catch (TikCommandTrapException)
            {
                // Router already closed / invalid state
                // Log if you want
            }
            catch (ObjectDisposedException)
            {
                // Already disposed
            }
            catch (Exception ex)
            {
                // IMPORTANT: log unexpected errors
                Console.WriteLine("[MK CLOSE] Unexpected: " + ex);
                throw; // optional: rethrow if you want to fail fast
            }
            finally
            {
                try
                {
                    (connection as IDisposable)?.Dispose();
                }
                catch
                {
                    // Ignore dispose errors
                }

                connection = null;
            }
        }


        public void SetConnection(int RouterID)
        {
            HTML html = new HTML();
            string sql = "select RouterID, IPAddress, [Port], UserName, [Password], Owner, MikroTikVersion, MikroTikVersionID from dbo.TblRouter where RouterID = @RouterID";
            Dictionary<string, string> para = new Dictionary<string, string>{
                { "RouterID", RouterID.ToString() }
            };
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                HOST = dr["IPAddress"].ToString();
                PPORT = int.Parse(dr["Port"].ToString());
                USER = dr["UserName"].ToString();
                PASS = dr["Password"].ToString();
                Owner = dr["Owner"].ToString();
                VERSION = int.Parse(dr["MikroTikVersionID"].ToString());
            }
        }
        public SqlResult UpdatePPPoEUser(string OldPPPoEUserName, string PPPoEUserName, string PPPoEPassword)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    bool isAlreadyUser = false;
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                    {
                        try
                        {
                            row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                        }
                        catch (NotImplementedException)
                        {
                            // No matching limitation → RouterOS returned !empty
                            row = null;
                        }
                    }
                    if (row != null)
                    {
                        if (OldPPPoEUserName != PPPoEUserName)
                        {
                            foreach (var data in row)
                            {
                                isAlreadyUser = true;
                            }
                        }
                    }
                    if (!isAlreadyUser)
                    {
                        if (VERSION == 1)
                            row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", OldPPPoEUserName).ExecuteList();
                        else if (VERSION == 2)
                            row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", OldPPPoEUserName).ExecuteList();
                        foreach (var data in row)
                        {
                            string Userid = data.Words[".id"];
                            if (VERSION == 1)
                            {
                                if (PPPoEPassword.ToString().Contains("*"))
                                    connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "username", PPPoEUserName).ExecuteNonQuery();
                                else
                                    connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "username", PPPoEUserName, "password", PPPoEPassword).ExecuteNonQuery();
                            }
                            else if (VERSION == 2)
                            {
                                if (PPPoEPassword.ToString().Contains("*"))
                                    connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "name", PPPoEUserName).ExecuteNonQuery();
                                else
                                    connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "name", PPPoEUserName, "password", PPPoEPassword).ExecuteNonQuery();
                            }
                        }
                        sqlResult.Result = ResultType.Successful;
                        sqlResult.Message = "Update PPPoE User Successful.";
                    }
                    else
                    {
                        sqlResult.Result = ResultType.Failed;
                        sqlResult.Message = "PPPoE User Name: " + PPPoEUserName + " is already used. Please choose other one.";
                    }
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CheckIsExisitingPPPoEUserName(string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                    foreach (var data in row)
                    {
                        sqlResult.Value = data.Words[".id"];
                    }
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CreateAndUpdateUser(string ConnectionID, string PPPoEUserName, string PPPoEPassword, string PPPoEProfileName, string FirstName, string LastName, string Phone, string Email, string Location, string Comment, int ActivateCustomerID = 0, string IPPOOL = "", string PPPoEIPAddress = "")
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            //double Validity = 0;
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    //string sql = "select ProfileName, Duration, LimitationName, LimitationDoubleSpeedName, FromTime1, FromTime2, ToTime1, ToTime2, ToTime3, ToTime4 from dbo.TblRediusProfile where ProfileName = N'" + PPPoEProfileName + "';";
                    //sql += "select bi.Duration, bi.StartDate, bi.EndDate, bi.UnitPrice, ";
                    //sql += "(pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' +  bi.UM) PackageName ";
                    //sql += "from dbo.TblCustomerBillingInvoice bi ";
                    //sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
                    //sql += "where ConnectionID='" + ConnectionID + "'";
                    //DataSet ds = html.LoadData(sql);
                    //if (ds.Tables.Count > 0)
                    //{
                    // if (ds.Tables[0].Rows.Count > 0)
                    //{
                    //string ProfileName = ds.Tables[0].Rows[0]["ProfileName"].ToString();
                    //string Duration1 = ds.Tables[0].Rows[0]["Duration"].ToString();
                    //string LimitationName = ds.Tables[0].Rows[0]["LimitationName"].ToString();
                    //string LimitationDoubleSpeedName = ds.Tables[0].Rows[0]["LimitationDoubleSpeedName"].ToString();
                    //string FromTime1 = ds.Tables[0].Rows[0]["FromTime1"].ToString();
                    //string FromTime2 = ds.Tables[0].Rows[0]["FromTime2"].ToString();
                    //string ToTime1 = ds.Tables[0].Rows[0]["ToTime1"].ToString();
                    //string ToTime2 = ds.Tables[0].Rows[0]["ToTime2"].ToString();
                    //string ToTime3 = ds.Tables[0].Rows[0]["ToTime3"].ToString();
                    //string ToTime4 = ds.Tables[0].Rows[0]["ToTime4"].ToString();
                    //string Duration2 = ds.Tables[1].Rows[0]["Duration"].ToString();
                    //DateTime StartDate = DateTime.Parse(ds.Tables[1].Rows[0]["StartDate"].ToString());
                    //DateTime EndDate = DateTime.Parse(ds.Tables[1].Rows[0]["EndDate"].ToString());
                    //string Price = ds.Tables[1].Rows[0]["UnitPrice"].ToString();
                    //string PackageName = ds.Tables[1].Rows[0]["PackageName"].ToString();
                    //Validity = (EndDate.Date - StartDate.Date).TotalDays;//.Days;
                    //if (Duration1 != Duration2 || PackageName != ProfileName)
                    //{
                    //    //ProfileName = PackageName;
                    //    //PPPoEProfileName = ProfileName;
                    //    //sqlResult = CreateProfileName(ProfileName, Validity.ToString() + "d", Price);
                    //    //if (sqlResult.Value != "")
                    //    //{
                    //    //    if (LimitationDoubleSpeedName != "")
                    //    //    {
                    //    //        ApplyProfileLimitation(ProfileName, LimitationName, FromTime1, FromTime2);
                    //    //        ApplyProfileLimitation(ProfileName, LimitationDoubleSpeedName, ToTime1, ToTime2);
                    //    //        ApplyProfileLimitation(ProfileName, LimitationDoubleSpeedName, ToTime3, ToTime4);
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        ApplyProfileLimitation(ProfileName, LimitationName, ToTime3, ToTime4);
                    //    //    }
                    //    //}
                    //    sqlResult.Value = ProfileName;
                    //}
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        try
                        {
                            row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                        }
                        catch (NotImplementedException)
                        {
                            // No matching limitation → RouterOS returned !empty
                            row = null;
                        }
                    if (row != null)
                    {
                        if (row.Count() == 0)
                        {
                            if (VERSION == 1)
                            {
                                if (PPPoEIPAddress != "")
                                {
                                    connection.CreateCommandAndParameters("/tool/user-manager/user/add", "customer", Owner, "username", PPPoEUserName, "password", PPPoEPassword, "comment", Comment, "email", Email, "phone", Phone, "location", Location, "first-name", FirstName, "last-name", LastName, "shared-users", "1", "ip-address", PPPoEIPAddress).ExecuteNonQuery();
                                }
                                else
                                    connection.CreateCommandAndParameters("/tool/user-manager/user/add", "customer", Owner, "username", PPPoEUserName, "password", PPPoEPassword, "comment", Comment, "email", Email, "phone", Phone, "location", Location, "first-name", FirstName, "last-name", LastName, "shared-users", "1").ExecuteNonQuery();
                                connection.CreateCommandAndParameters("/tool/user-manager/user/create-and-activate-profile", "customer", Owner, "profile", PPPoEProfileName, "numbers", PPPoEUserName).ExecuteNonQuery();
                            }
                            else if (VERSION == 2)
                            {
                                try
                                {
                                    //connection.CreateCommandAndParameters("/user-manager/user/add", "name", PPPoEUserName, "password", PPPoEPassword, "comment", Comment, "attributes", "Framed-Pool:pool20M").ExecuteNonQuery();
                                    connection.CreateCommandAndParameters("/user-manager/user/add", "name", PPPoEUserName, "password", PPPoEPassword, "comment", Comment, "group", IPPOOL, "shared-users", "1").ExecuteNonQuery();
                                    connection.CreateCommandAndParameters("/user-manager/user-profile/add", "profile", PPPoEProfileName, "user", PPPoEUserName).ExecuteNonQuery();
                                }
                                catch (NotImplementedException)
                                {
                                    // No matching limitation → RouterOS returned !empty
                                    row = null;
                                }
                            }
                        }
                        else
                        {
                            foreach (var data in row)
                            {
                                string Userid = data.Words[".id"];
                                if (VERSION == 1)
                                {
                                    if (PPPoEPassword.Contains("*"))
                                        connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "username", PPPoEUserName, "comment", Comment, "email", Email, "phone", Phone, "location", Location, "first-name", FirstName, "last-name", LastName).ExecuteNonQuery();
                                    else
                                        connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "username", PPPoEUserName, "password", PPPoEPassword, "comment", Comment, "email", Email, "phone", Phone, "location", Location, "first-name", FirstName, "last-name", LastName).ExecuteNonQuery();
                                    connection.CreateCommandAndParameters("/tool/user-manager/user/create-and-activate-profile", "customer", Owner, "profile", PPPoEProfileName, "numbers", PPPoEUserName).ExecuteNonQuery();
                                }
                                else if (VERSION == 2)
                                {
                                    if (PPPoEPassword.Contains("*"))
                                        connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "name", PPPoEUserName, "comment", Comment).ExecuteNonQuery();
                                    else
                                        connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "name", PPPoEUserName, "password", PPPoEPassword, "comment", Comment).ExecuteNonQuery();
                                    connection.CreateCommandAndParameters("/user-manager/user-profile/add", "profile", PPPoEProfileName, "user", PPPoEUserName).ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Save Change User Successful.";
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        try
                        {
                            row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                            sqlResult.DataRowRouter = row;
                        }
                        catch (NotImplementedException)
                        {
                            // No matching limitation → RouterOS returned !empty
                            row = null;
                        }

                    //    }
                    //    else
                    //    {
                    //        sqlResult.Result = ResultType.Failed;
                    //        sqlResult.Message = "Profile Name: " + PPPoEProfileName + " does not exisiting in database. Please contact administrator.";
                    //    }
                    //}
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult AppendProfileToUser(string PPPoEUserName, string PPPoEProfileName, string Comment = "")
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                    {
                        connection.CreateCommandAndParameters("/tool/user-manager/user/create-and-activate-profile", "customer", Owner, "profile", PPPoEProfileName, "numbers", PPPoEUserName).ExecuteNonQuery();
                    }
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/user-profile/add", "profile", PPPoEProfileName, "user", PPPoEUserName).ExecuteNonQuery();

                    if (Comment != "")
                    {
                        IEnumerable<ITikReSentence> row = null;
                        if (VERSION == 1)
                            row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                        else if (VERSION == 2)
                            row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                        foreach (var data in row)
                        {
                            string Userid = data.Words[".id"];
                            if (VERSION == 1)
                            {
                                connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "comment", Comment).ExecuteNonQuery();
                            }
                            else if (VERSION == 2)
                                connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "comment", Comment).ExecuteNonQuery();
                        }
                    }
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Apply PPPoE Profile to User Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult DownSpeedUser3M(string PPPoEUserName, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    string ProfileName = "Expired-Alert-3day-3M";
                    string LimitationName = "Alert-3day-3M";
                    sqlResult = CreateProfileName(ProfileName, "3d", "0");
                    if (sqlResult.Value != "")
                    {
                        sqlResult = CreateLimitationName(LimitationName, "10", "10", "CR03-PPPoE-Pool10M", "M");//pppoe-pool
                        sqlResult = ApplyProfileLimitation(ProfileName, LimitationName, "0s", "23h59m59s");
                    }
                    if (VERSION == 1)
                    {
                        connection.CreateCommandAndParameters("/tool/user-manager/user/clear-profiles", "numbers", PPPoEUserName).ExecuteNonQuery();
                        connection.CreateCommandAndParameters("/tool/user-manager/user/create-and-activate-profile", "customer", Owner, "profile", ProfileName, "numbers", PPPoEUserName).ExecuteNonQuery();
                    }
                    else if (VERSION == 2)
                    {
                        connection.CreateCommandAndParameters("/user-manager/user-profile/clear-profiles", "user", PPPoEUserName).ExecuteNonQuery();
                        connection.CreateCommandAndParameters("/user-manager/user-profile/add", "profile", ProfileName, "user", PPPoEUserName).ExecuteNonQuery();
                    }
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                    foreach (var data in row)
                    {
                        string Userid = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "comment", Comment).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "comment", Comment).ExecuteNonQuery();
                    }

                    sqlResult.Message = "Down Speed User TO 3M Successful.";
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult ClearProfileName(string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/user/clear-profiles", "numbers", PPPoEUserName).ExecuteNonQuery();
                    else if (VERSION == 2)
                    {
                        IEnumerable<ITikReSentence> row = null;
                        try
                        {
                            row = connection.CreateCommandAndParameters("/user-manager/user-profile/print", "user", PPPoEUserName).ExecuteList();
                            if (row.Count() > 0)
                            {
                                string Userid = row.ToList()[0].Words[".id"];
                                connection.CreateCommandAndParameters("/user-manager/user-profile/remove", ".id", Userid).ExecuteNonQuery();
                            }
                        }
                        catch (NotImplementedException)
                        {
                            // No matching limitation → RouterOS returned !empty
                            row = null;
                        }
                    }
                    sqlResult.Message = "Clear Profile PPPoE User Successful.";
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult LockUser(string PPPoEUserName, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                    foreach (var data in row)
                    {
                        string Userid = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "username", PPPoEUserName, "disabled", "yes", "comment", Comment).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "name", PPPoEUserName, "disabled", "yes", "comment", Comment).ExecuteNonQuery();
                    }
                    sqlResult.Message = "Locked User Successful.";
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }

        public SqlResult DisablePPPoE(string ID, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", ID, "customer", Owner, "disabled", "yes", "comment", Comment).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/user/set", ".id", ID, "disabled", "yes", "comment", Comment).ExecuteNonQuery();
                    sqlResult.Message = "Disabled User Successful.";
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult EnablePPPoE(string ID, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", ID, "customer", Owner, "disabled", "no", "comment", Comment).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/user/set", ".id", ID, "disabled", "no", "comment", Comment).ExecuteNonQuery();
                    sqlResult.Message = "Enabled User Successful.";
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult SetPPPoEUserComment(string PPPoEUserName, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                    foreach (var data in row)
                    {
                        string Userid = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", Userid, "customer", Owner, "comment", Comment).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/user/set", ".id", Userid, "comment", Comment).ExecuteNonQuery();
                    }
                    sqlResult.Message = "Set Comment To User Successful.";
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult SetVlanComment(string VlanID, string Comment)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    connection.CreateCommandAndParameters("/interface/vlan/set", ".id", VlanID, "comment", Comment).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Comment to Vlan Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult RenameVlan(string VlanID, string NewName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    connection.CreateCommandAndParameters("/interface/vlan/set", ".id", VlanID, "name", NewName).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Rename Vlan Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult ReActivateUser(string PPPoEUserName, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = CheckIsExisitingPPPoEUserName(PPPoEUserName);
                if (sqlResult.Result == ResultType.Successful)
                {
                    string PPPoEUserNameID = sqlResult.Value;
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/user/set", ".id", PPPoEUserNameID, "customer", Owner, "username", PPPoEUserName, "disabled", "no", "comment", Comment).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/user/set", ".id", PPPoEUserNameID, "name", PPPoEUserName, "disabled", "no", "comment", Comment).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult RemoveUser(string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                    foreach (var data in row)
                    {
                        string Userid = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/user/remove", ".id", Userid).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/user/remove", ".id", Userid).ExecuteNonQuery();
                    }
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult RemoveUserByID(string PPPoEUserID)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/user/remove", ".id", PPPoEUserID).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/user/remove", ".id", PPPoEUserID).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CheckIsExisitingLimitationName(string LimitationName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    ITikReSentence id = null;
                    if (VERSION == 1)
                        id = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print", "name", LimitationName).ExecuteSingleRow();
                    else if (VERSION == 2)
                        id = connection.CreateCommandAndParameters("/user-manager/profile/limitation/print", "name", LimitationName).ExecuteSingleRow();
                    sqlResult.Value = id.Words[".id"];
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult UpdateLimitationName(string LimitationName, string Download, string Upload, string IPPOOL, string Prefix)
        {
            int Bytes = 1048576;
            Download = (int.Parse(Download) * Bytes).ToString();
            Upload = (int.Parse(Upload) * Bytes).ToString();
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print", "name", LimitationName).ExecuteList();
                    foreach (var data in row)
                    {
                        string LimitationID = data.Words[".id"];
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/set", ".id", LimitationID, "owner", Owner, "rate-limit-rx", Download, "rate-limit-tx", Upload, "rate-limit-min-tx", Download, "rate-limit-min-rx", Upload, "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
                    }
                    if (row.Count() == 0)
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/add", "owner", Owner, "rate-limit-rx", Download, "rate-limit-tx", Upload, "rate-limit-min-tx", Download, "rate-limit-min-rx", Upload, "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Update Limitation Name Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult UpdateLimitationName(string LimitationName, int minrx, int mintx, int rx, int tx, string IPPOOL)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print", "name", LimitationName).ExecuteList();
                    else if (VERSION == 2)
                    {
                        row = connection.CreateCommandAndParameters("/user-manager/limitation/print", "name", LimitationName).ExecuteList();
                    }
                    foreach (var data in row)
                    {
                        string LimitationID = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/set", ".id", LimitationID, "owner", Owner, "rate-limit-rx", rx.ToString(), "rate-limit-tx", tx.ToString(), "rate-limit-min-tx", mintx.ToString(), "rate-limit-min-rx", minrx.ToString(), "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/limitation/set", ".id", LimitationID, "rate-limit-rx", rx.ToString(), "rate-limit-tx", tx.ToString(), "rate-limit-min-tx", mintx.ToString(), "rate-limit-min-rx", minrx.ToString(), "name", LimitationName).ExecuteNonQuery();
                    }
                    //if (row.Count() == 0)
                    //connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/add", "owner", Owner, "rate-limit-rx", rx.ToString(), "rate-limit-tx", tx.ToString(), "rate-limit-min-tx", mintx.ToString(), "rate-limit-min-rx", minrx.ToString(), "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Update Limitation Name Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CheckIsExisitingProfileName(string ProfileName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/profile/print", "name", ProfileName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/profile/print", "name", ProfileName).ExecuteList();
                    foreach (var data in row)
                    {
                        sqlResult.Value = data.Words[".id"];
                    }
                    if (sqlResult.Value == "")
                    {
                        sqlResult.Result = ResultType.Failed;
                        sqlResult.Message = "Activate Fail for this PPPoE account due to Profile Name: " + ProfileName + " doesn't existing in Radius Profile. Please contact administrator.";
                    }
                    else
                        sqlResult.Result = ResultType.Successful;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult UpdateProfileName(string ProfileName, string Validity, string Price)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/tool/user-manager/profile/print", "name", ProfileName).ExecuteList();
                    foreach (var data in row)
                    {
                        string ProfileID = data.Words[".id"];
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/set", ".id", ProfileID, "owner", Owner, "name", ProfileName, "validity", Validity, "starts-at", "logon", "price", Price).ExecuteNonQuery();
                    }
                    if (row.Count() == 0)
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/add", "owner", Owner, "name", ProfileName, "validity", Validity, "starts-at", "logon", "price", Price).ExecuteNonQuery();
                    sqlResult.Message = "Update Profile Name Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult RemoveProfileLimitation(string ProfileName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = VERSION == 1
                        ? connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/print", "profile", ProfileName).ExecuteList()
                        : connection.CreateCommandAndParameters("/user-manager/profile-limitation/print", "profile", ProfileName).ExecuteList();
                    foreach (var data in row)
                    {
                        string ProfileLimitationID = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/remove", ".id", ProfileLimitationID).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/profile-limitation/remove", ".id", ProfileLimitationID).ExecuteNonQuery();
                    }
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Remove Profile Limitation Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CheckIsNotExisitingLimitationNameCreate(string LimitationName, string Download, string Upload, string IPPOOL, string Prefix)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print", "name", LimitationName).ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/profile-limitation/print", "name", LimitationName).ExecuteList();
                    foreach (var data in row)
                    {
                        string LimitationNameID = data.Words[".id"];
                        if (VERSION == 1)
                            connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/set", ".id", LimitationNameID, "owner", Owner, "rate-limit-rx", Download + Prefix, "rate-limit-tx", Upload + Prefix, "rate-limit-min-tx", Download + Prefix, "rate-limit-min-rx", Upload + Prefix, "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
                        else if (VERSION == 2)
                            connection.CreateCommandAndParameters("/user-manager/profile-limitation/set", ".id", LimitationNameID, "name", LimitationName, "from-time", "").ExecuteNonQuery();
                    }
                    if (row.Count() == 0)
                    {
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/add", "owner", Owner, "rate-limit-rx", Download + Prefix, "rate-limit-tx", Upload + Prefix, "rate-limit-min-tx", Download + Prefix, "rate-limit-min-rx", Upload + Prefix, "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
                    }
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Save Change Limitation Name Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult ApplyProfileLimitation(string ProfileName, string LimitationName, string FromTime, string ToTime)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/print", "limitation", LimitationName).ExecuteList();
                    else if (VERSION == 2)
                    {
                        try
                        {
                            row = connection.CreateCommandAndParameters("/user-manager/profile-limitation/print", "limitation", LimitationName).ExecuteList();
                        }
                        catch (NotImplementedException)
                        {
                            // No matching limitation → RouterOS returned !empty
                            row = null;
                        }
                    }
                    if (row != null)
                    {
                        if (row.Count() == 0)
                        {
                            if (VERSION == 1)
                                connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/add", "limitation", LimitationName, "profile", ProfileName, "weekdays", "sunday,monday,tuesday,wednesday,thursday,friday,saturday", "from-time", FromTime, "till-time", ToTime).ExecuteNonQuery();
                            else if (VERSION == 2)
                            {
                                try
                                {
                                    connection.CreateCommandAndParameters("/user-manager/profile-limitation/add", "limitation", LimitationName, "profile", ProfileName, "weekdays", "sunday,monday,tuesday,wednesday,thursday,friday,saturday", "from-time", FromTime, "till-time", ToTime, "comment", "Created By: " + html.UserFullName + ", " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt")).ExecuteNonQuery();
                                }
                                catch (NotImplementedException)
                                {
                                    // No matching limitation → RouterOS returned !empty
                                    row = null;
                                }
                            }
                            sqlResult.Message = "Apply Profile Limitation Successfully.";
                        }
                        else
                            sqlResult.Message = "Profile Limitation Applied.";
                    }
                    else
                        sqlResult.Message = "Profile Limitation Applied.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CreateLimitationName(
            string limitationName,
            string download,
            string upload,
            string ipPool,
            string prefix)
        {
            SqlResult result = new SqlResult();
            HTML html = new HTML();

            try
            {
                // Open MikroTik connection
                result = OpenMKConnection();
                if (result.Result != ResultType.Successful)
                    return result;

                try
                {
                    // Create limitation directly (NO print)
                    if (VERSION == 1)
                    {
                        connection.CreateCommandAndParameters(
                            "/tool/user-manager/profile/limitation/add",
                            "owner", Owner,
                            "name", limitationName,
                            "rate-limit-rx", download + prefix,
                            "rate-limit-tx", upload + prefix,
                            "rate-limit-min-rx", upload + prefix,
                            "rate-limit-min-tx", download + prefix,
                            "ip-pool", ipPool
                        ).ExecuteNonQuery();
                    }
                    else
                    {
                        connection.CreateCommandAndParameters(
                            "/user-manager/limitation/add",
                            "name", limitationName,
                            "rate-limit-rx", download + prefix,
                            "rate-limit-tx", upload + prefix,
                            "rate-limit-min-rx", upload + prefix,
                            "rate-limit-min-tx", download + prefix,
                            "comment",
                            "Created By: " + html.UserFullName + ", " +
                            DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt")
                        ).ExecuteNonQuery();
                    }

                    result.Result = ResultType.Successful;
                    result.Message = "Create Limitation Name successful.";
                }
                catch (TikCommandTrapException ex)
                {
                    // Duplicate → treat as already exists
                    if (ex.Message != null &&
                        ex.Message.ToLower().Contains("already have such name"))
                    {
                        result.Result = ResultType.Successful;
                        result.Message = "Limitation Name already exists.";
                    }
                    else
                    {
                        throw; // real MikroTik error
                    }
                }
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }

            return result;
        }



        //public SqlResult CreateLimitationName(string LimitationName, string Download, string Upload, string IPPOOL, string Prefix)
        //{
        //	SqlResult sqlResult = new SqlResult();
        //	HTML html = new HTML();
        //	try
        //	{
        //		sqlResult = OpenMKConnection();
        //		if (sqlResult.Result == ResultType.Successful)
        //		{
        //			//IEnumerable<ITikReSentence> row2 = connection.CreateCommandAndParameters("/user-manager/user/print").ExecuteList();
        //			//IEnumerable<ITikReSentence> row2 = connection.CreateCommandAndParameters("/tool/user-manager/customer/print").ExecuteList();
        //			//tool user-manager customer set admin password=adminpassword
        //			//connection.CreateCommandAndParameters("/tool/user-manager/customer/set", "admin", "password", "NSM@Admin").ExecuteNonQuery();
        //			IEnumerable<ITikReSentence> row = null;
        //			if (VERSION == 1)
        //				row = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print", "name", LimitationName).ExecuteList();
        //			else if (VERSION == 2)
        //				row = connection.CreateCommandAndParameters("/user-manager/limitation/print", "name", LimitationName).ExecuteList();
        //			if (row.Count() == 0)
        //			{
        //				if (VERSION == 1)
        //					connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/add", "owner", Owner, "rate-limit-rx", Download + Prefix, "rate-limit-tx", Upload + Prefix, "rate-limit-min-tx", Download + Prefix, "rate-limit-min-rx", Upload + Prefix, "ip-pool", IPPOOL, "name", LimitationName).ExecuteNonQuery();
        //				else if (VERSION == 2)
        //					connection.CreateCommandAndParameters("/user-manager/limitation/add", "rate-limit-rx", Download + Prefix, "rate-limit-tx", Upload + Prefix, "rate-limit-min-tx", Download + Prefix, "rate-limit-min-rx", Upload + Prefix, "name", LimitationName, "comment", "Created By: " + html.UserFullName + ", " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt")).ExecuteNonQuery();
        //				sqlResult.Message = "Create Limitation Name Successful.";
        //			}
        //			else
        //			{
        //				sqlResult.Message = "Limitation Name Existing.";
        //			}
        //			sqlResult.Result = ResultType.Successful;
        //		}
        //	}
        //	catch (TikCommandTrapException ex)
        //	{
        //		sqlResult.Result = ResultType.Failed;
        //		sqlResult.Message = ex.Message;
        //	}
        //	return sqlResult;
        //}
        public SqlResult CreateProfileName(
            string profileName,
            string validity,
            string price)
        {
            SqlResult result = new SqlResult();
            HTML html = new HTML();

            try
            {
                // Open MikroTik connection
                result = OpenMKConnection();
                if (result.Result != ResultType.Successful)
                    return result;

                try
                {
                    // Try create directly
                    if (VERSION == 1)
                    {
                        connection.CreateCommandAndParameters(
                            "/tool/user-manager/profile/add",
                            "owner", Owner,
                            "name", profileName,
                            "validity", validity,
                            "starts-at", "logon",
                            "price", price
                        ).ExecuteNonQuery();
                    }
                    else
                    {
                        connection.CreateCommandAndParameters(
                            "/user-manager/profile/add",
                            "name", profileName,
                            "name-for-users", profileName,
                            "validity", validity,
                            "starts-when", "first-auth",
                            "price", price,
                            "comment",
                            "Created By: " + html.UserFullName + ", " +
                            DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt")
                        ).ExecuteNonQuery();
                    }

                    result.Result = ResultType.Successful;
                    result.Message = "Create Profile Name successful.";
                }
                catch (TikCommandTrapException ex)
                {
                    // Duplicate name → treat as exists
                    if (ex.Message != null &&
                        ex.Message.ToLower().Contains("already have such name"))
                    {
                        result.Result = ResultType.Successful;
                        result.Message = "Profile Name already exists.";
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }

            return result;
        }


        public SqlResult RemoveProfileName(string ProfileName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    ITikReSentence id = connection.CreateCommandAndParameters("/tool/user-manager/profile/print", "name", ProfileName).ExecuteSingleRow();
                    string ProfileID = id.Words[".id"];
                    connection.CreateCommandAndParameters("/tool/user-manager/profile/remove", ".id", ProfileID).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Remove Profile Name Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult RemoveUserProfile(string ProfileID, string ProfileName, string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 2)
                    {
                        //IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/user-manager/user-profile/print").ExecuteList();
                        connection.CreateCommandAndParameters("/user-manager/user-profile/remove", ".id", ProfileID).ExecuteNonQuery();
                    }
                    else if (VERSION == 1)
                    {
                        connection.CreateCommandAndParameters("/tool/user-manager/user/clear-profiles", "numbers", PPPoEUserName).ExecuteNonQuery();
                    }

                    //string ProfileID = id.Words[".id"];
                    //connection.CreateCommandAndParameters("/tool/user-manager/profile/remove", ".id", ProfileID).ExecuteNonQuery();
                    //sqlResult.Result = ResultType.Successful;
                    //sqlResult.Message = "Remove Profile Name Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult DeleteProfileName(string ProfileID)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/remove", ".id", ProfileID).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/profile/remove", ".id", ProfileID).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Delete Profile Name Successfully.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult RemoveLimitationName(string LimitationName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    ITikReSentence id = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print", "name", LimitationName).ExecuteSingleRow();
                    string LimitationID = id.Words[".id"];
                    connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/remove", ".id", LimitationID).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Remove Limitation Name Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult DeleteLimitation(string LimitationID)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/remove", ".id", LimitationID).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/limitation/remove", ".id", LimitationID).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Remove Limitation Name Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult DeleteProfileLimitation(string LimitationID)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/remove", ".id", LimitationID).ExecuteNonQuery();
                    else if (VERSION == 2)
                        connection.CreateCommandAndParameters("/user-manager/limitation/remove", ".id", LimitationID).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Remove Limitation Name Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }

        public SqlResult GetVLanInfo()
        {
            SqlResult sqlResult = new SqlResult();
            //List<VlanName> vlanName = new List<VlanName>();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/interface/vlan/print").ExecuteList();
                    // List<KeyValuePair<string, string>> VlanList = new List<KeyValuePair<string, string>>();

                    DataTable table = new DataTable();
                    table.Columns.Add("ID");
                    table.Columns.Add("Name");
                    DataRow dr = null;// table.NewRow();
                    foreach (var data in row)
                    {
                        dr = table.NewRow();
                        dr["ID"] = data.Words[".id"];
                        dr["Name"] = data.Words["name"];
                        table.Rows.Add(dr);
                        //vlanName.Add(new VlanName()
                        //{
                        //    ID = data.Words[".id"],
                        //    Name = data.Words["name"]
                        //});
                        //VlanList.Add(new KeyValuePair<string, string>(data.Words[".id"], data.Words["name"]));
                    }
                    //DataTable dt = ToDataTable(vlanName);
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Load Vlan List Successful.";
                    sqlResult.DataTable = table;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }

        public SqlResult GetVlanAddressInfo(string VlanName)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row;

                    try
                    {
                        row = connection.CreateCommandAndParameters("/ip/address/print", "interface", VlanName).ExecuteList();
                    }
                    catch (NotImplementedException)
                    {
                        // MikroTik returned '!empty' — no addresses found on this interface
                        row = Enumerable.Empty<ITikReSentence>();
                    }

                    Dictionary<string, string> list = new Dictionary<string, string>();
                    foreach (var data in row)
                    {
                        list.Add(data.Words["address"], data.Words["network"]);
                    }

                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Load Vlan Address Successful.";
                    sqlResult.DictionaryList = list;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }

        public SqlResult EnableVlanID(string VlanID, string Comment)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    connection.CreateCommandAndParameters("/interface/vlan/set", ".id", VlanID, "disabled", "no", "comment", Comment).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Enable Vlan Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult GetVlanIDStatus(string VlanID)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    //IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/interface/vlan/print").ExecuteList();
                    IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/interface/vlan/print", ".id", VlanID).ExecuteList();
                    //row = connection.CreateCommandAndParameters("/interface/vlan/print").ExecuteList();
                    sqlResult.MikrotikExecuteList = row;
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Enable Vlan Successful.";
                    if (row.Count() > 0)
                        sqlResult.Value = row.ToList()[0].Words["disabled"];
                }
                CloseMKConnection();
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult DisableVlanID(string VlanID, string Comment)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    connection.CreateCommandAndParameters("/interface/vlan/set", ".id", VlanID, "disabled", "yes", "comment", Comment).ExecuteNonQuery();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Disable Vlan Successful.";
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public class VlanName
        {
            public string ID { get; set; }
            public string Name { get; set; }
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public SqlResult ShowUserLog(string PPPoEUserName = "nsm")
        {
            SqlResult sqlResult = new SqlResult();
            SetConnection(6);
            sqlResult = OpenMKConnection();
            //IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/tool/user-manager/log/print").ExecuteList();
            //IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/interface/ppp-client/info", TikCommandParameterFormat.NameValue, "number", "nsm").ExecuteList();
            //IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/user/active/listen").ExecuteList();
            IEnumerable<ITikReSentence> row = connection.CreateCommandAndParameters("/tool/user-manager/session/print", "active", "yes").ExecuteList();
            Dictionary<string, string> para = new Dictionary<string, string>();
            foreach (ITikReSentence sentence in row)
            {
                string user = sentence.Words["user"];

                var s = sentence.Words.ToList().Where(v => v.Key == "user" && v.Value == "nsm");

                DataTable dt = ToDataTable(sentence.Words.ToList());
                //var sd = dt.Select("user='nsm'");
                //str = dt.Select("user = 'nsm'");
                //foreach (DataRow o in dt.Select("user='nsm'"))
                //{

                //}
                //HTML html = new HTML();
                //str = html.DrawHtmlTable(dt);
            }
            return sqlResult;
        }
        public string GetPPPeEUserOnline(bool ShowDetail = false, bool ShowHistory = false, string PPPoEUser_Name = "")
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @" select distinct ac.RouterID, r.RouterName from dbo.TblActivateCustomer ac
                            inner join dbo.TblRouter r on r.RouterID = ac.RouterID
                            where ac.PPPoEUserName = case when '" + PPPoEUser_Name + "' = '' then ac.PPPoEUserName else '" + PPPoEUser_Name + "' end and ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID;
            DataSet ds = html.LoadData(sql);
            int coutUserOnline = 0;
            int i = 0;
            JObject jobj = new JObject();
            JObject Alljobj = new JObject();
            List<Result> results = new List<Result>();
            IEnumerable<ITikReSentence> row = new List<ITikReSentence>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        SetConnection(int.Parse(dr["RouterID"].ToString()));
                        sqlResult = OpenMKConnection();
                        if (sqlResult.Result == ResultType.Successful)
                        {
                            try
                            {
                                if (!ShowHistory)
                                {
                                    if (VERSION == 1)
                                        row = connection.CreateCommandAndParameters("/tool/user-manager/session/print", "active", "yes").ExecuteList();
                                    else if (VERSION == 2)
                                        row = connection.CreateCommandAndParameters("/user-manager/session/print", "active", "yes").ExecuteList();
                                }
                                if (ShowHistory)
                                {
                                    if (VERSION == 1)
                                        row = connection.CreateCommandAndParameters("/tool/user-manager/session/print", "user", PPPoEUser_Name).ExecuteList();
                                    else if (VERSION == 2)
                                        row = connection.CreateCommandAndParameters("/user-manager/session/print", "user", PPPoEUser_Name).ExecuteList();
                                }
                                CloseMKConnection();
                            }
                            catch (TikCommandException ex)
                            {
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = ex.Message;
                                CloseMKConnection();
                            }
                        }
                        List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>();
                        if (row.Count() > 0)
                        {
                            coutUserOnline += row.Count();
                            if (ShowDetail)
                            {
                                foreach (ITikReSentence sentence in row)
                                {
                                    i++;
                                    string PPPoEUserName = sentence.Words["user"];
                                    string sqlpppoe = @"select top 1 ac.CustomerID, ac.[SID], 
                                ac.PPPoEProfileName, ac.ActivateDate, ac.ActivateBy, 
                                DATEDIFF(D, getdate(), ac.EndDate) RemainDay, c.CustomerName, ac.PPPoEUserName
                                from dbo.TblActivateCustomer ac
                                inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
                                where ac.PPPoEUserName = @PPPoEUserName";
                                    Dictionary<string, string> para = new Dictionary<string, string>
                                    {
                                        { "PPPoEUserName", PPPoEUserName }
                                    };
                                    DataSet dset = html.LoadData(sqlpppoe, false, para);

                                    if (dset.Tables.Count > 0)
                                    {
                                        if (dset.Tables[0].Rows.Count > 0)
                                        {
                                            DataRow drow = dset.Tables[0].Rows[0];
                                            string CustomerID = drow["CustomerID"].ToString();
                                            string CustomerName = drow["CustomerName"].ToString();
                                            string SID = drow["SID"].ToString();
                                            string PPPoEProfileName = drow["PPPoEProfileName"].ToString();
                                            string ActivateDate = drow["ActivateDate"].ToString();
                                            string ActivateBy = drow["ActivateBy"].ToString();
                                            string RemainDay = drow["RemainDay"].ToString();
                                            string str = JsonConvert.SerializeObject(sentence.Words.Append(new KeyValuePair<string, string>("RouterName", dr["RouterName"].ToString())).
                                                Append(new KeyValuePair<string, string>("RouterID", dr["RouterID"].ToString())).
                                                Append(new KeyValuePair<string, string>("CustomerID", CustomerID)).
                                                Append(new KeyValuePair<string, string>("CustomerName", CustomerName)).
                                                Append(new KeyValuePair<string, string>("SID", SID)).
                                                Append(new KeyValuePair<string, string>("PPPoEProfileName", PPPoEProfileName)).
                                                Append(new KeyValuePair<string, string>("ActivateDate", ActivateDate)).
                                                Append(new KeyValuePair<string, string>("ActivateBy", ActivateBy)).
                                                Append(new KeyValuePair<string, string>("RemainDay", RemainDay)).ToList());
                                            //kvpList.Add(new KeyValuePair<string, string>("Data" + i, str));
                                            Alljobj.Add(new JProperty("Data" + i, str));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            string json = JsonConvert.SerializeObject(new
            {
                results = new List<Result>()
                {
                    new Result { CoutUserOnline = coutUserOnline,
                    UserOnline = Alljobj, Data = row.ToList()
                    }
                }
            });
            return json;
        }
        public class Result
        {
            public int CoutUserOnline = 0;
            public JObject UserOnline = new JObject();
            public dynamic Data = new JObject();
        }
        public SqlResult RemovePPPeEUserSession(int RouterID, string ID)
        {
            SqlResult sqlResult = new SqlResult();
            SetConnection(RouterID);
            sqlResult = OpenMKConnection();
            connection.CreateCommandAndParameters("/tool/user-manager/session/close-session", ".id", ID).ExecuteNonQuery();
            sqlResult.Message = "Cut Session Successful.";
            return sqlResult;
        }
        public SqlResult GetDataRowFromRouterPPPoE(string RouterID)
        {
            SetConnection(int.Parse(RouterID.ToString()));
            SqlResult Result = new SqlResult();
            Result = OpenMKConnection();
            try
            {
                if (Result.Result == ResultType.Successful)
                {
                    //var row = connection.CreateCommandAndParameters("/tool/user-manager/log/print").ExecuteList();
                    if (VERSION == 1)
                        Result.DataRowRouter = connection.CreateCommandAndParameters("/tool/user-manager/user/print").ExecuteList();
                    else if (VERSION == 2)
                        try
                        {
                            Result.DataRowRouter = connection.CreateCommandAndParameters("/user-manager/user/print").ExecuteList();
                        }
                        catch (NotImplementedException)
                        {
                            // No matching limitation → RouterOS returned !empty
                        }
                    CloseMKConnection();
                }
            }
            catch (TikCommandTrapException ex)
            {
                CloseMKConnection();
                Result.Result = ResultType.Failed;
                Result.Message = ex.Message;
                Result.DataRowRouter = null;
            }
            return Result;
        }

        public SqlResult GetPPPoEUserStatus(int RouterID, string PPPoEUserName)
        {
            SetConnection(RouterID);
            SqlResult Result = new SqlResult();
            Result = OpenMKConnection();
            try
            {
                if (Result.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                        Result.DataRowRouter = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", PPPoEUserName).ExecuteList();
                    else if (VERSION == 2)
                        try
                        {
                            Result.DataRowRouter = connection.CreateCommandAndParameters("/user-manager/user/print", "name", PPPoEUserName).ExecuteList();
                        }
                        catch (NotImplementedException)
                        {

                        }
                    CloseMKConnection();
                }
            }
            catch (TikCommandTrapException ex)
            {
                CloseMKConnection();
                Result.Result = ResultType.Failed;
                Result.Message = ex.Message;
                Result.DataRowRouter = null;
            }
            return Result;
        }
        public SqlResult GetDataRowFromRouterVlan(string RouterID)
        {
            SetConnection(int.Parse(RouterID.ToString()));
            SqlResult Result = new SqlResult();
            Result = OpenMKConnection();
            try
            {
                if (Result.Result == ResultType.Successful)
                {
                    Result.DataRowRouter = connection.CreateCommandAndParameters("/interface/vlan/print").ExecuteList();
                    //connection.CreateCommandAndParameters("/interface/vlan/set", ".id", VlanID, "disabled", "no", "comment", Comment).ExecuteNonQuery();
                    CloseMKConnection();
                }
            }
            catch (TikCommandTrapException ex)
            {
                CloseMKConnection();
                Result.Result = ResultType.Failed;
                Result.Message = ex.Message;
            }
            return Result;
        }
        public SqlResult ListsUserPPPoE(string RouterID)
        {
            return GetDataRowFromRouterPPPoE(RouterID);
        }
        public SqlResult ListsUserPPPoE(string C_ID, string Status, string RouterID)
        {
            SqlResult sqlResult = new SqlResult();
            SqlResult Result = new SqlResult();
            HTML html = new HTML();
            DataSet ds2 = new DataSet();
            try
            {
                string sql = @"select RouterID, RouterName from dbo.TblRouter where 
convert(varchar, RouterID) = case when @RouterID = 'All' then convert(varchar, RouterID) else @RouterID end
and SystemID = @SystemID and WarehouseID = @WarehouseID";
                Dictionary<string, string> para = new Dictionary<string, string> {
                    { "RouterID",RouterID},
                    { "SystemID",html.SystemID},
                    { "WarehouseID",html.WarehouseID},
                };
                DataSet ds = new DataSet();
                ds = html.LoadData(sql, false, para);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    RouterID = dr["RouterID"].ToString();
                    Result = GetDataRowFromRouterPPPoE(RouterID);
                    if (Result.Result == ResultType.Successful)
                    {
                        IEnumerable<ITikReSentence> row = Result.DataRowRouter;
                        string pppoeuserid = "";
                        if (row != null)
                        {
                            if (row.Count() > 0)
                            {
                                DataTable dt = new DataTable
                                {
                                    TableName = dr["RouterName"].ToString()
                                };
                                //dt.Columns.Add("No");
                                dt.Columns.Add("CID");
                                //dt.Columns.Add("Phone");
                                dt.Columns.Add("User Name");
                                //dt.Columns.Add("Password");
                                //dt.Columns.Add("Last Seen");
                                dt.Columns.Add("Disabled");
                                //dt.Columns.Add("SysStatus");
                                dt.Columns.Add("Sart - End");
                                //dt.Columns.Add("Remain");
                                dt.Columns.Add("Actual Profile");
                                //dt.Columns.Add("Package Name");
                                dt.Columns.Add("Comment");
                                dt.Columns.Add("OrderBy");

                                int i = 0;

                                var rows = Status == "All" || Status == "Expired" ? row : row.Where(v => v.Words["disabled"].ToString() == Status).ToList();
                                if (C_ID != "" && Status != "All" && Status != "Expired")
                                    rows = row.Where(v => v.Words["username"].Contains(C_ID) && v.Words["disabled"].ToString() == Status).ToList();
                                else if ((C_ID != "" && Status == "All") || Status == "Expired")
                                    rows = row.Where(v => v.Words["username"].Contains(C_ID)).ToList();

                                foreach (ITikReSentence sentence in rows)
                                {
                                    i++;
                                    sql = @"select top 1 '<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">'+ c.CustomerCode + '</a>' CustomerCode,
case when bi.invTmp = 1 then bi.PackageName else replace(bi.ServiceName, s.ServiceName + ' - ', '') end PackageName,
c.CustomerContactMobileNumber Phone, c.CustomerCode, format(bi.StartDate,'dd-MM-yy')StartDate, format(bi.EndDate,'dd-MM-yy')EndDate,
(case when DATEDIFF(d, getdate(), bi.EndDate) >=0 then case when DATEDIFF(d, getdate(), bi.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), bi.EndDate)) + ' days'
when DATEDIFF(d, getdate(), bi.StartDate) > 0 then convert(varchar, DATEDIFF(d, bi.StartDate, bi.EndDate)) + ' days'
end else '<span class=""text-danger"">Expired</span>' end) Remain,
convert(varchar, DATEDIFF(d, bi.StartDate, bi.EndDate)) +' days' [Day],
convert(varchar, DATEDIFF(m, bi.StartDate, bi.EndDate)) +' Months' [Month],
st.[Status] SysStatus
from dbo.TblCustomer c 
left join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID-- and isnull(bi.IsRenew,0) = 0
 and bi.StatusID not in (12,13,14) and bi.[Type]='Official Invoice'
inner join dbo.TblServices s on s.ServiceID = bi.ServiceID
inner join dbo.TblStatus st on st.StatusID = bi.ConnectivityStatus
where c.CustomerCode = @CustomerCode or isnull(ac.PPPoEUserName, '') = @username order by bi.BillingInvoiceID desc
--where ac.[SID] = @username";
                                    pppoeuserid = (sentence.Words.ToList().Where(v => v.Key == ".id").ToList().Count > 0) ? sentence.Words[".id"] : "";

                                    string actualprofile = (sentence.Words.ToList().Where(v => v.Key == "actual-profile").ToList().Count > 0) ? sentence.Words["actual-profile"] : "";
                                    string username = VERSION == 1 ? sentence.Words["username"] : sentence.Words["name"];
                                    string password = (sentence.Words.ToList().Where(v => v.Key == "password").ToList().Count > 0) ? sentence.Words["password"] : "";
                                    string Phone = (sentence.Words.ToList().Where(v => v.Key == "phone").ToList().Count > 0) ? sentence.Words["phone"] : "";
                                    string active = VERSION == 1 ? sentence.Words["active"] : sentence.Words["disabled"];
                                    string Disabled = sentence.Words["disabled"];
                                    string comment = (sentence.Words.ToList().Where(v => v.Key == "comment").ToList().Count > 0) ? sentence.Words["comment"] : "";
                                    string lastseen = (sentence.Words.ToList().Where(v => v.Key == "last-seen").ToList().Count > 0) ? sentence.Words["last-seen"] : "";
                                    string SysStatus = "";
                                    para = new Dictionary<string, string> {
                                    {"CustomerCode",username },
                                    {"username",username }
                                };
                                    ds = new DataSet();
                                    string CID = "", packagename = "", StartDate = "", EndDate = "", Remain = "", OrderBy = "1", TDay = "", Month = "";
                                    ds = html.LoadData(sql, false, para);
                                    if (ds.Tables.Count > 0)
                                    {
                                        if (ds.Tables[0].Rows.Count > 0)
                                        {
                                            CID = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
                                            packagename = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                            StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                            EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                            Remain = ds.Tables[0].Rows[0]["Remain"].ToString();
                                            TDay = ds.Tables[0].Rows[0]["Day"].ToString();
                                            Month = ds.Tables[0].Rows[0]["Month"].ToString();
                                            Phone = Phone == "" ? ds.Tables[0].Rows[0]["Phone"].ToString() : Phone;
                                            SysStatus = ds.Tables[0].Rows[0]["SysStatus"].ToString();
                                        }
                                    }
                                    if (Disabled == "false" && Remain == "")
                                        OrderBy = "0";
                                    else if (Disabled == "false" && Remain == "<span class=\"text-danger\">Expired</span>")
                                        OrderBy = "1";
                                    else if (Disabled == "false" && Remain != "")
                                        OrderBy = "2";
                                    else if (Disabled == "true" && Remain != "" && Remain != "<span class=\"text-danger\">Expired</span>")
                                        OrderBy = "3";
                                    else if (Disabled == "true" && Remain == "<span class=\"text-danger\">Expired</span>")
                                        OrderBy = "4";
                                    else if (Disabled == "true" && Remain != "")
                                        OrderBy = "5";
                                    else if (Disabled == "true" && Remain == "")
                                        OrderBy = "6";

                                    DataRow datarow = dt.NewRow();
                                    datarow["CID"] = CID + "<br><button type=\"button\" name=\"delPPPoEUser\" onclick=\"delPPPoEUser(" + RouterID + ",'" + pppoeuserid + "');\" class=\"btn btn-danger\">Delete</button>";// + "<br/>" + Phone;
                                                                                                                                                                                                                          //datarow["Phone"] = Phone;
                                    datarow["User Name"] = username + "<br>" + Phone + "<br>PWD:" + password + "<br>Last Seen: " + lastseen;
                                    //datarow["Password"] = password;
                                    //datarow["Last Seen"] = lastseen;
                                    datarow["Disabled"] = (Disabled == "true" ? "<span class=\"text-danger\"><b>Disabled</b></span>" : "<b class=\"text-primary\">Active</b>") + "<br>Sys: " + SysStatus;
                                    //datarow["SysStatus"] = SysStatus;
                                    datarow["Sart - End"] = (StartDate != "" ? StartDate + " - " + EndDate + "<br>" + TDay + ", " + Month : "") + "<br>" + Remain;
                                    //datarow["Remain"] = Remain;
                                    datarow["Actual Profile"] = actualprofile + "<br>" + packagename;
                                    //datarow["Package Name"] = packagename;
                                    datarow["Comment"] = comment;
                                    datarow["OrderBy"] = OrderBy;

                                    dt.Rows.Add(datarow);
                                }
                                ds2.Tables.Add(dt);
                            }
                        }
                    }
                    else
                    {
                        sqlResult.Message = Result.Message;
                    }
                }
                sqlResult.Data = ds2;
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult ListsUserVlan(string C_ID, string Status, string RouterID)
        {
            SqlResult sqlResult = new SqlResult();
            SqlResult Result = new SqlResult();
            HTML html = new HTML();
            DataSet ds2 = new DataSet();
            try
            {
                string sql = @"select RouterID, RouterName from dbo.TblRouter where 
convert(varchar, RouterID) = case when @RouterID = 'All' then convert(varchar, RouterID) else @RouterID end
and SystemID = @SystemID and WarehouseID = @WarehouseID";
                Dictionary<string, string> para = new Dictionary<string, string> {
                    { "RouterID",RouterID},
                    { "SystemID",html.SystemID},
                    { "WarehouseID",html.WarehouseID},
                };
                DataSet ds = new DataSet();
                ds = html.LoadData(sql, false, para);
                int j = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    RouterID = dr["RouterID"].ToString();
                    Result = GetDataRowFromRouterVlan(RouterID);
                    if (Result.Result == ResultType.Successful)
                    {
                        IEnumerable<ITikReSentence> row = Result.DataRowRouter;
                        if (row.Count() > 0)
                        {
                            DataTable dt = new DataTable
                            {
                                TableName = dr["RouterName"].ToString()
                            };
                            //dt.Columns.Add("No");
                            dt.Columns.Add("CID");
                            //dt.Columns.Add("Phone");
                            //dt.Columns.Add("VlanID");
                            dt.Columns.Add("Vlan Name");
                            //dt.Columns.Add("Mac Address");
                            dt.Columns.Add("Disabled");
                            dt.Columns.Add("SysStatus");
                            dt.Columns.Add("Sart - End");
                            //dt.Columns.Add("End Date");
                            //dt.Columns.Add("Remain");
                            dt.Columns.Add("Package Name");
                            //dt.Columns.Add("Comment");
                            dt.Columns.Add("OrderBy");
                            //dt.Columns.Add("Interface");
                            //dt.Columns.Add("MTU");
                            int i = 0;
                            //var s = sentence.Words.ToList().Where(v => v.Key == "user" && v.Value == "nsm");

                            //var rows = row;// = (C_ID == "" ? row : row.Where(v => v.Words["name"].Contains(C_ID)).ToList());
                            var rows = Status == "All" || Status == "Expired" ? row : row.Where(v => v.Words["disabled"].ToString() == Status).ToList();
                            if (C_ID != "" && Status != "All" && Status != "Expired")
                                rows = row.Where(v => v.Words["name"].Contains(C_ID) && v.Words["disabled"].ToString() == Status).ToList();
                            else if ((C_ID != "" && Status == "All") || Status == "Expired")
                                rows = row.Where(v => v.Words["name"].Contains(C_ID)).ToList();

                            foreach (ITikReSentence sentence in rows)
                            {
                                string VlanName = (sentence.Words.ToList().Where(v => v.Key == "name").ToList().Count > 0) ? sentence.Words["name"] : "";
                                Regex regex = new Regex(@"\(([^()]+)\)*");
                                string vname = "";
                                foreach (Match match in regex.Matches(VlanName))
                                {
                                    vname = match.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                                }

                                var s = sentence.Words.ToList().Where(v => v.Key == "user" && v.Value == "nsm");
                                i++;
                                sql = @"select top 1 '<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">'+ c.CustomerCode + '</a>' CustomerCode,
case when bi.invTmp = 1 then bi.PackageName else replace(bi.ServiceName, s.ServiceName + ' - ', '') end PackageName,
c.CustomerContactMobileNumber Phone, c.CustomerCode, format(bi.StartDate,'dd-MM-yy')StartDate, format(bi.EndDate,'dd-MM-yy')EndDate,
(case when DATEDIFF(d, getdate(), bi.EndDate) >=0 then case when DATEDIFF(d, getdate(), bi.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), bi.EndDate)) + ' days'
when DATEDIFF(d, getdate(), bi.StartDate) > 0 then 
case when isnull(ac.ActivateCustomerID, 0) <> 0 then
convert(varchar, DATEDIFF(d, ac.StartDate, ac.EndDate)) 
else
convert(varchar, DATEDIFF(d, bi.StartDate, bi.EndDate)) 
end
+ ' days'
end else '<span class=""text-danger"">Expired</span>' end) Remain,
case when isnull(ac.ActivateCustomerID, 0) <> 0 then
convert(varchar, DATEDIFF(d, ac.StartDate, ac.EndDate)) +' days'
else
convert(varchar, DATEDIFF(d, bi.StartDate, bi.EndDate)) +' days'
end [Day],
case when isnull(ac.ActivateCustomerID, 0) <> 0 then
convert(varchar, DATEDIFF(m, ac.StartDate, ac.EndDate)) 
else
convert(varchar, DATEDIFF(m, bi.StartDate, bi.EndDate)) 
end
+' Months' [Month],
st.[Status] SysStatus
from dbo.TblCustomer c 
left join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID -- and isnull(bi.IsRenew,0) = 0
 and bi.StatusID not in (12,13,14) and bi.[Type]='Official Invoice'
inner join dbo.TblServices s on s.ServiceID = bi.ServiceID
inner join dbo.TblStatus st on st.StatusID = bi.ConnectivityStatus
where c.CustomerCode = @CustomerCode or isnull(ac.VLanID, '') = '" + sentence.Words[".id"] + "' order by format(bi.EndDate,'yyyyMMdd') desc";


                                string MacAddress = (sentence.Words.ToList().Where(v => v.Key == "mac-address").ToList().Count > 0) ? sentence.Words["mac-address"] : "";
                                string Disabled = sentence.Words["disabled"];
                                string Interface = (sentence.Words.ToList().Where(v => v.Key == "interface").ToList().Count > 0) ? sentence.Words["interface"] : "";
                                string MTU = sentence.Words["mtu"];
                                string VlanID = sentence.Words["vlan-id"];
                                string SysStatus = "";
                                string Comment = (sentence.Words.ToList().Where(v => v.Key == "comment").ToList().Count > 0) ? sentence.Words["comment"] : "";
                                para = new Dictionary<string, string> {
                                    {"CustomerCode",vname.Replace("CID-",string.Empty) }
                                };
                                ds = new DataSet();
                                string CID = "", packagename = "", Phone = "", StartDate = "", EndDate = "", Remain = "", OrderBy = "1", TDay = "", Month = "";
                                ds = html.LoadData(sql, false, para);
                                if (ds.Tables.Count > 0)
                                {
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        CID = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
                                        packagename = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                        Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                                        StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                        EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                        Remain = ds.Tables[0].Rows[0]["Remain"].ToString();
                                        TDay = ds.Tables[0].Rows[0]["Day"].ToString();
                                        Month = ds.Tables[0].Rows[0]["Month"].ToString();
                                        SysStatus = ds.Tables[0].Rows[0]["SysStatus"].ToString();
                                        sql = "select ac.VLanID, count(ac.VLanID) CountVLanID,\r\nSTUFF((select ','+cc.CustomerCode from dbo.TblCustomer cc \r\ninner join dbo.TblActivateCustomer acc on acc.CustomerID = cc.CustomerID \r\nwhere acc.VLanID = ac.VLanID FOR XML PATH('')), 1, 1, '') CustomerCode\r\nfrom dbo.TblActivateCustomer ac\r\ninner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID\r\ngroup by ac.VLanID\r\nhaving ac.VLanID ='" + sentence.Words[".id"] + "' and count(ac.VLanID) > 1";
                                        ds = html.LoadData(sql);
                                        if (ds.Tables[0].Rows.Count > 0)
                                        {
                                            CID += "<br><label style=\"max-width: 200px; white-space:break-spaces\">(" + ds.Tables[0].Rows[0]["CustomerCode"].ToString() + ") (" + ds.Tables[0].Rows[0]["CountVLanID"].ToString() + ")</label>";

                                        }
                                    }
                                }
                                if (Disabled == "false" && Remain == "")
                                    OrderBy = "0";
                                else if (Disabled == "false" && Remain == "<span class=\"text-danger\">Expired</span>")
                                    OrderBy = "1";
                                else if (Disabled == "false" && Remain != "")
                                    OrderBy = "2";
                                else if (Disabled == "true" && Remain != "" && Remain != "<span class=\"text-danger\">Expired</span>")
                                    OrderBy = "3";
                                else if (Disabled == "true" && Remain == "<span class=\"text-danger\">Expired</span>")
                                    OrderBy = "4";
                                else if (Disabled == "true" && Remain != "")
                                    OrderBy = "5";
                                else if (Disabled == "true" && Remain == "")
                                    OrderBy = "6";
                                DataRow datarow = dt.NewRow();
                                datarow["CID"] = CID;// + "<br/>" + Phone;
                                                     //datarow["Phone"] = Phone;
                                                     //datarow["VlanID"] = VlanID;
                                datarow["Vlan Name"] = Comment != "" ? VlanName + "<br><label style=\"max-width: 350px; white-space:break-spaces\">" + Comment + "</label>" : VlanName;
                                //datarow["Mac Address"] = MacAddress;
                                datarow["Disabled"] = Disabled == "true" ? "<span class=\"text-danger\"><b>Disabled</b></span>" : "<b class=\"text-primary\">Active</b>";
                                datarow["SysStatus"] = SysStatus;
                                datarow["Sart - End"] = (StartDate != "" ? StartDate + " - " + EndDate + "<br>" + TDay + ", " + Month : "") + "<br><span class=\"text-red\">" + Remain + "</span>";
                                //datarow["End Date"] = EndDate;
                                //datarow["Remain"] = Remain;
                                datarow["Package Name"] = packagename;
                                //datarow["Comment"] = Comment;
                                datarow["OrderBy"] = OrderBy;
                                //datarow["Interface"] = Interface;
                                //datarow["MTU"] = MTU;
                                dt.Rows.Add(datarow);
                            }
                            ds2.Tables.Add(dt);
                            //ds2.Tables[j].DefaultView.Sort = "Remain";
                            j++;
                        }
                    }
                }
                sqlResult.Data = ds2;
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Data = ds2;
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }

        public SqlResult GetProfileName(string ip, int port, string username, string password, bool getrawData = false, int RouterID = 0)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                if (RouterID == 0)
                    sqlResult = OpenMKConnection(ip, port, username, password);
                else
                {
                    SetConnection(RouterID);
                    sqlResult = OpenMKConnection();
                }
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                    {
                        row = connection.CreateCommandAndParameters("/tool/user-manager/profile/print").ExecuteList();
                        sqlResult.Parameters.ProfileLimitation = connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/print").ExecuteList();
                    }
                    else if (VERSION == 2)
                    {
                        //Monitor
                        //row = connection.CreateCommandAndParameters("/interface/print", "type", "pppoe-in", "disabled", "false").ExecuteList();
                        var userGroups = connection.CreateCommandAndParameters("/user-manager/user/group/print").ExecuteList();
                        //row = connection.CreateCommandAndParameters("/user-manager/profile/print").ExecuteList();
                        //
                        try
                        {
                            row = connection
                                .CreateCommandAndParameters("/user-manager/profile/print")
                                .ExecuteList();
                        }
                        catch (NotImplementedException)
                        {
                            // UM v2 returns !empty (!done only) → safe to ignore
                            row = null;
                        }
                        sqlResult.Parameters.ProfileLimitation = connection.CreateCommandAndParameters("/user-manager/profile-limitation/print").ExecuteList();
                        sqlResult.Parameters.UserGroups = userGroups;
                    }
                    if (row != null)
                    {
                        if (row.Count() > 0)
                        {
                            if (!getrawData)
                            {
                                string cbo = "<select id=\"CboProfileName\" name=\"CboProfileName\">";
                                foreach (var data in row)
                                {
                                    cbo += "<option data-id=\"" + data.Words[".id"] + "\" value=\"" + data.Words["name"] + "\">" + data.Words["name"] + "</option>";
                                }
                                cbo += "</select>";
                                sqlResult.Value = cbo;
                            }
                            else
                                sqlResult.Value = JsonConvert.SerializeObject(row.ToList());
                        }
                        else
                            sqlResult.Message = "No Profile Name.";
                    }
                    else
                        sqlResult.Message = "No Profile Name.";
                    sqlResult.Result = ResultType.Successful;
                    CloseMKConnection();
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
                CloseMKConnection();
            }
            return sqlResult;
        }
        public SqlResult GetLimitation(string ip, int port, string username, string password)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection(ip, port, username, password);
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    if (VERSION == 1)
                        row = connection.CreateCommandAndParameters("/tool/user-manager/profile/limitation/print").ExecuteList();
                    else if (VERSION == 2)
                        row = connection.CreateCommandAndParameters("/user-manager/limitation/print").ExecuteList();
                    if (row.Count() > 0)
                        sqlResult.Value = JsonConvert.SerializeObject(row.ToList());
                    else
                        sqlResult.Message = "No Profile Name.";
                    sqlResult.Result = ResultType.Successful;
                    CloseMKConnection();
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
                CloseMKConnection();
            }
            return sqlResult;
        }
        public SqlResult ShowPPPoEUserUsingPackage(string PackageID, string PackageName)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                IEnumerable<ITikReSentence> row = null;
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (VERSION == 1)
                    {
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "actual-profile", PackageName).ExecuteList();
                    }
                    else if (VERSION == 2)
                    {
                        row = connection.CreateCommandAndParameters("/user-manager/user-profile/print", "profile", PackageName).ExecuteList();
                    }
                    sqlResult.DataRowRouter = row;
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        public SqlResult CreatePPPoEUser(string ip, int port, string username, string password, string pppoeusername, string pppoeuserpassword, string packagename)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection(ip, port, username, password);
                if (sqlResult.Result == ResultType.Successful)
                {
                    connection.CreateCommandAndParameters("/tool/user-manager/user/add", "customer", Owner, "username", pppoeusername, "password", pppoeuserpassword, "comment", "Created from system.").ExecuteNonQuery();
                    connection.CreateCommandAndParameters("/tool/user-manager/user/create-and-activate-profile", "customer", Owner, "profile", packagename, "numbers", pppoeusername).ExecuteNonQuery();
                }
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }

        public SqlResult GetUserProfileName(string username)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                sqlResult = OpenMKConnection();
                if (sqlResult.Result == ResultType.Successful)
                {
                    IEnumerable<ITikReSentence> row = null;
                    string actualprofile = "";

                    if (VERSION == 1)
                    {
                        row = connection.CreateCommandAndParameters("/tool/user-manager/user/print", "username", username).ExecuteList();
                        if (row.Count() > 0)
                        {
                            if (row.ToList()[0].Words.ContainsKey("actual-profile"))
                            {
                                actualprofile = row.ToList()[0].Words["actual-profile"].ToString();
                            }
                            row = connection.CreateCommandAndParameters("/tool/user-manager/profile/print", "name", actualprofile).ExecuteList();
                            sqlResult.Parameters.UserProfileInfo = JsonConvert.SerializeObject(row.ToList());
                            row = connection.CreateCommandAndParameters("/tool/user-manager/profile/profile-limitation/print", "profile", actualprofile).ExecuteList();
                            sqlResult.Parameters.UserProfileLimitationInfo = JsonConvert.SerializeObject(row.ToList());
                        }
                    }
                    else if (VERSION == 2)
                    {
                        row = connection.CreateCommandAndParameters("/user-manager/user-profile/print", "user", username).ExecuteList();
                        if (row.Count() > 0)
                        {
                            sqlResult.Parameters.UserProfileInfo = JsonConvert.SerializeObject(row.ToList());
                            string profile = row.ToList()[0].Words["profile"].ToString();
                            row = connection.CreateCommandAndParameters("/user-manager/profile-limitation/print", "profile", profile).ExecuteList();
                            sqlResult.Parameters.UserProfileLimitationInfo = JsonConvert.SerializeObject(row.ToList());
                        }
                        //row = connection.CreateCommandAndParameters("/log/print", "topics", "pppoe,ppp,info").ExecuteList();
                    }
                }
                else
                    sqlResult.Message = "No Profile Name.";
                sqlResult.Result = ResultType.Successful;
                CloseMKConnection();
            }
            catch (TikCommandTrapException ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
                CloseMKConnection();
            }
            return sqlResult;
        }

    }
}
