using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Utilities
{
    public class DBHelper
    {
        public SqlConnection sqlConnection = null;

        public DBHelper()
        {
        }
        //public void RunWithOpenSqlConnection(Action<SqlConnection> connectionCallBack)
        //{
        //    SqlConnection conn = null;
        //    try
        //    {
        //        string str_cnn = ConfigurationManager.ConnectionStrings["dbLactasoy"].ConnectionString;
        //        conn = new SqlConnection(str_cnn);
        //        connectionCallBack(conn);
        //    }
        //    catch (Exception)
        //    {
        //        //Log Error Here
        //    }
        //    finally
        //    {
        //        if (conn != null)
        //            conn.Dispose(); //will close the connection
        //    }
        //}

        public void CloseConnection()
        {
            if (sqlConnection != null)
            {
                if (sqlConnection.State != ConnectionState.Closed)
                    sqlConnection.Close();
            }
        }
        public string ConnectionString = ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString;//"Data Source=192.168.69.83;Initial Catalog=NSM_BILLING_DB;user id=sothea;password=Thea8989; Connection Timeout=900; Pooling=true; Max Pool Size=200; MultipleActiveResultSets=True";

        public string GetConnectionString() { return ConnectionString; }
        public string GetDMCTAXConnectionString() { return ConfigurationManager.ConnectionStrings["AuthContextNSMDMC"].ConnectionString; }
        public SqlResult InitiateConnection()
        {
            SqlResult result = new SqlResult();
            try
            {
                //if (sqlConnection == null)
                //{
                string str_cnn = GetConnectionString() == "" ? ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString : GetConnectionString();
                sqlConnection = new SqlConnection(str_cnn);
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
                sqlConnection.Open();
                result.Result = ResultType.Successful;
                result.Message = "Connection Successful.";
                //}
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }
            return result;
        }

        public void InitiateConnection2()
        {
            string str_cnn = ConfigurationManager.ConnectionStrings["dbcn2"].ConnectionString;
            sqlConnection = new SqlConnection(str_cnn);
            if (sqlConnection.State == ConnectionState.Open)
                sqlConnection.Close();
            sqlConnection.Open();
        }

        //public void ExecuteSqlDataSet(SqlCommand sqlCommand, Action<DataSet> readerCallBack)
        //{
        //    RunWithOpenSqlConnection(delegate(SqlConnection conn)
        //    {
        //        SqlDataAdapter sqlAdapter = null;
        //        DataSet ds_data = null;
        //        try
        //        {
        //            //cmd = new SqlCommand(sqlCommand, conn);
        //            //reader = cmd.();
        //            //readerCallBack(reader);
        //            sqlCommand.Connection = conn;
        //            sqlAdapter = new SqlDataAdapter(sqlCommand);
        //            ds_data = new DataSet();
        //            sqlAdapter.Fill(ds_data);
        //            readerCallBack(ds_data);
        //        }
        //        catch (Exception)
        //        {
        //            //Log Error Here
        //        }
        //        finally
        //        {
        //            if (sqlAdapter != null)
        //                sqlAdapter.Dispose();
        //            if (ds_data != null)
        //                ds_data.Dispose();
        //        }
        //    });
        //}


    }
}