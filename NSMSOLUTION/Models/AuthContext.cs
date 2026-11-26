using Microsoft.AspNet.Identity.EntityFramework;
using Utilities;

namespace NSMSOLUTION.Models
{
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext() //: base("AuthContextNSM", throwIfV1Schema:false)
        {
            DBHelper dbHelper = new DBHelper();
            Database.Connection.ConnectionString = dbHelper.GetConnectionString();
        }
        //: base("AuthContextNSM") { Database.Connection.ConnectionString = ""; }
    }
}