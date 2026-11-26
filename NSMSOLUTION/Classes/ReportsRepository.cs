using NSMSOLUTION.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NSMSOLUTION.Classes
{
    public class ReportsRepository
    {
        private readonly string _connString;
        public ReportsRepository()
        {
            _connString = ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString;
        }

        public List<TrialBalanceItem> GetHierarchyTrialBalance(int systemId, int warehouseId, string domainId)
        {
            var list = new List<TrialBalanceItem>();

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand("sp_HierarchyTrialBalance", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SystemID", systemId);
                cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                cmd.Parameters.AddWithValue("@DomainID", domainId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TrialBalanceItem
                        {
                            AccountNameIndented = reader["AccountNameIndented"].ToString(),
                            AccountNo = reader["AccountNo"].ToString(),
                            AccountCategory = reader["AccountCategory"].ToString(),
                            DebitBalance = reader["DebitBalance"] != DBNull.Value ? (decimal)reader["DebitBalance"] : 0,
                            CreditBalance = reader["CreditBalance"] != DBNull.Value ? (decimal)reader["CreditBalance"] : 0,
                            NormalSide = reader["NormalSide"].ToString(),
                            AncestorLevel = reader["AncestorLevel"] != DBNull.Value ? (int)reader["AncestorLevel"] : 0
                        });
                    }
                }
            }

            return list;
        }
    }
}