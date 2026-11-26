using System.Configuration;
using System.Data.SqlClient;

namespace NSMSOLUTION.Features.DataAccess.Base
{
    public static class DbConnectionFactory
    {
        public static SqlConnection Create()
        {
            var connectionString =
                ConfigurationManager
                .ConnectionStrings["AuthContextNSM"]
                .ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}