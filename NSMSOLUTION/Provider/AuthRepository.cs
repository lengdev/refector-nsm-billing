using Microsoft.AspNet.Identity.EntityFramework;
using MVC;
using MVC.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web;
using Utilities;

namespace NSMSOLUTION.Provider
{
    public class AuthRepository
    {
        //private AuthContext _ctx;

        //private UserManager<IdentityUser> _userManager;

        //public AuthRepository()
        //{
        //    _ctx = new AuthContext();
        //    _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));

        //}

        //public async Task<IdentityResult> RegisterUser(UserModel userModel)
        //{
        //    IdentityUser user = new IdentityUser
        //    {
        //        UserName = userModel.UserName
        //    };
        //    var result = await _userManager.CreateAsync(user, userModel.Password);

        //    return result;
        //}

        public Task<IdentityUser> FindUser(string userName, string password, string UserAgent, string WebBrowserName)
        {
            password = HttpUtility.UrlDecode(password);

            DBHelper dbHelper = new DBHelper();
            SqlCommand SQL_C = new SqlCommand();
            IdentityUser user = new IdentityUser();
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            dbHelper.InitiateConnection();
            try
            {
                SQL_C.Parameters.Add(new SqlParameter("@UserName", userName.ToLower()));
                string SQL = "select UserLoginID, PasswordSalt, UserRoleID from dbo.TblUserLogin ";
                SQL += "where lower(UserName)= @UserName and StatusID = 3";
                SQL_C.CommandText = SQL;
                SQL_C.CommandType = CommandType.Text;
                SQL_C.Connection = dbHelper.sqlConnection;
                SqlDataReader reader = SQL_C.ExecuteReader();
                if (reader.Read())
                {
                    user.Roles.Add(new IdentityUserRole() { RoleId = reader["UserRoleID"].ToString(), UserId = reader["UserLoginID"].ToString() });
                    user.Id = reader["UserLoginID"].ToString();
                    SQL = "select format(LockOutEnd, 'yyMMddhhmmss') LockOutEnd from dbo.TblUserLogin where ISLocked = 1 and UserLoginID = " + user.Id;
                    DataSet ds = new DataSet();
                    ds = html.LoadData(SQL);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        double LockOutEnd = double.Parse(ds.Tables[0].Rows[0]["LockOutEnd"].ToString());
                        double DateTime = double.Parse(easternTimeNow.ToString("yyMMddhhmmss"));
                        if (LockOutEnd < DateTime)
                        {
                            SQL = "update dbo.TblUserLogin set ISLocked = 0, AttemptFailedCount = 0, LockoutEnabled = 0, LockOutEnd='" + easternTimeNow.ToString() + "' where UserLoginID = " + user.Id;
                            html.SaveChangeData(SQL, false, false);
                        }
                    }
                    string PasswordSalt = reader["PasswordSalt"].ToString();
                    SQL_C.Parameters.Clear();
                    string passwordHash = PasswordHelper.HashUsingPbkdf2(password, Convert.FromBase64String(PasswordSalt));
                    SQL_C.Parameters.Add(new SqlParameter("@UserName", userName.ToLower()));
                    SQL_C.Parameters.Add(new SqlParameter("@Password", passwordHash));
                    SQL = "select UserLoginID, UserName, UserTypeID, Telephone, Email from dbo.TblUserLogin ";
                    SQL += "where StatusID = 3 and lower(UserName)= @UserName and Password = @Password";//and isnull(ISLocked, 0) = 0
                    reader.Close();
                    SQL_C.CommandText = SQL;
                    reader = SQL_C.ExecuteReader();
                    if (reader.Read())
                    {
                        user.UserName = reader["UserName"].ToString();
                        user.PasswordHash = passwordHash;
                        user.PhoneNumber = reader["Telephone"].ToString();
                        user.Email = reader["Email"].ToString();
                        user.EmailConfirmed = true;
                        user.PhoneNumberConfirmed = true;
                        user.TwoFactorEnabled = false;
                        user.LockoutEnabled = false;
                        reader.Close();
                        SQL = "insert into TblUserLog (UserLoginID, LockedInDate, IP, HostName, WebBrowserName, UserAgent, ISLocked, LockoutEnabled, Comment) ";
                        SQL += "Values (" + user.Id + ", '" + easternTimeNow.ToString() + "', '" + html.GetLocalIPAddress(out string HostName) + "', '" + HostName + "', '" + WebBrowserName + "', '" + UserAgent + "', 0, 0, 'Login Successful');";
                        SQL += "update dbo.TblUserLogin set IsLogged = 1, LoginTime='" + easternTimeNow.ToString() + "'";
                        SQL += ", IP = '" + html.GetLocalIPAddress(out HostName) + "', HostName='" + HostName + "', WebBrowserName = '" + WebBrowserName + "', UserAgent='" + UserAgent + "' where UserLoginID = " + user.Id;
                        html.SaveChangeData(SQL, false, false);
                    }
                    else
                    {
                        SQL = "insert into TblUserLog (UserLoginID, LockedInDate, IP, HostName, WebBrowserName, UserAgent, ISLocked, LockoutEnabled, Comment) ";
                        SQL += "Values (" + user.Id + ", '" + easternTimeNow.ToString() + "', '" + html.GetLocalIPAddress(out string HostName) + "', '" + HostName + "', '" + WebBrowserName + "', '" + UserAgent + "', 0, 0, 'Login Failed');";
                        SQL += "update dbo.TblUserLogin set AttemptFailedCount = (isnull(AttemptFailedCount, 0) + 1), WebBrowserName = '" + WebBrowserName + "', UserAgent = '" + UserAgent + "' where UserLoginID = " + user.Id;
                        SQL += "; select AttemptFailedCount from dbo.TblUserLogin where UserLoginID = " + user.Id;
                        sqlResult = html.SaveChangeData(SQL, false, true);
                        if (sqlResult.Value == "5")
                        {
                            DateTime dateTime = new DateTime();
                            dateTime = DateTime.Now.AddMinutes(5);
                            user.LockoutEnabled = true;
                            user.LockoutEndDateUtc = dateTime;
                            SQL = "insert into TblUserLog (UserLoginID, LockedInDate, IP, HostName, WebBrowserName, UserAgent, ISLocked, LockoutEnabled, LockOutEnd, Comment) ";
                            SQL += "Values (" + user.Id + ", '" + easternTimeNow.ToString() + "', '" + html.GetLocalIPAddress(out HostName) + "', '" + HostName + "', '" + WebBrowserName + "', '" + UserAgent + "', 1, 1, '" + dateTime + "', 'Login Failed');";
                            SQL += "update dbo.TblUserLogin set ISLocked = 1, LockoutEnabled = 1, LockOutEnd='" + dateTime + "', WebBrowserName = '" + WebBrowserName + "', UserAgent = '" + UserAgent + "' where UserLoginID = " + user.Id;
                            html.SaveChangeData(SQL, false, false);
                        }
                        else if (int.Parse(sqlResult.Value) > 5)
                        {
                            SQL = "select format(LockOutEnd, 'yyyy-MM-dd HH:mm:ss') LockOutEnd from dbo.TblUserLogin where UserLoginID = " + user.Id;
                            ds = html.LoadData(SQL);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime m = DateTime.Parse(ds.Tables[0].Rows[0]["LockOutEnd"].ToString());

                                user.LockoutEnabled = true;
                                user.LockoutEndDateUtc = m;
                            }

                        }
                    }
                }
                dbHelper.CloseConnection();
            }
            catch (Exception)
            {
                dbHelper.CloseConnection();
            }
            return Task.FromResult(user);
        }

        //public void Dispose()
        //{
        //    _ctx.Dispose();
        //    _userManager.Dispose();

        //}
    }

}