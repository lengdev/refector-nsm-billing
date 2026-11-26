using NSMSOLUTION.Features.DataAccess.Base;
using NSMSOLUTION.Features.Vendors.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace NSMSOLUTION.Features.Vendors.VendorCenter
{
    public class VendorRepository
    {
        // =========================================
        // GET ALL
        // =========================================

        public List<VendorModel> GetAll(
            string domainID,
            string systemID,
            string warehouseID,
            string searchText = null)
        {
            var result = new List<VendorModel>();

            const string sql = @"
                SELECT
                    VendorID, VendorTypeID, VendorType,
                    VendorCode, VendorName,
                    StatusID, Status, Gender,
                    Address, CityID, City,
                    StateRegionID, StateRegion,
                    ZIPPostalCodeID, ZIPPostalCode,
                    CountryID, Country,
                    Phone, Email, Website,
                    ContactName, ContactPersonName,
                    ContactPhoneNumber, Position,
                    PaymentTerms, NumberofDays,
                    VATTIN, CompanyName, CompanyAddress,
                    BankerName, BankerAddress,
                    SwiftCode, AccountNo, AccountName,
                    BankerRemark, Remark,
                    Photo, AttachmentFile,
                    VendorPicture, VendorProfilePicture,
                    RegisterDate, Seq,
                    SystemID, WarehouseID, DomainID,
                    UserID, RefID, RefType,
                    CreateByUserID, CreateBy, CreateDate,
                    IsDeleted
                FROM dbo.TblVendor
                WHERE DomainID = @DomainID
                  AND IsDeleted = 0
                  AND (@SystemID IS NULL OR SystemID = @SystemID)
                  AND (@WarehouseID IS NULL OR WarehouseID = @WarehouseID)
                  AND
                  (
                      @SearchText IS NULL
                      OR VendorName LIKE @SearchPattern
                      OR VendorCode LIKE @SearchPattern
                      OR Phone LIKE @SearchPattern
                      OR Email LIKE @SearchPattern
                  )
                ORDER BY VendorID DESC;";

            using (var con = DbConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, con))
            {
                AddScopeParameters(
                    cmd, domainID, systemID, warehouseID);

                cmd.Parameters.Add(
                    "@SearchText", SqlDbType.NVarChar, 4000)
                    .Value = (object)searchText ?? DBNull.Value;

                cmd.Parameters.Add(
                    "@SearchPattern", SqlDbType.NVarChar, 4002)
                    .Value = string.IsNullOrWhiteSpace(searchText)
                        ? (object)DBNull.Value
                        : "%" + searchText.Trim() + "%";

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Map(reader));
                    }
                }
            }

            return result;
        }

        // =========================================
        // GET BY ID
        // =========================================

        public VendorModel GetById(
            int vendorID,
            string domainID,
            string systemID,
            string warehouseID)
        {
            const string sql = @"
                SELECT *
                FROM dbo.TblVendor
                WHERE VendorID = @VendorID
                  AND DomainID = @DomainID
                  AND IsDeleted = 0
                  AND (@SystemID IS NULL OR SystemID = @SystemID)
                  AND (@WarehouseID IS NULL OR WarehouseID = @WarehouseID);";

            using (var con = DbConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@VendorID",
                    SqlDbType.Int).Value = vendorID;

                AddScopeParameters(
                    cmd, domainID, systemID, warehouseID);

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        // =========================================
        // CHECK DUPLICATE VENDOR CODE
        // =========================================

        public bool IsVendorCodeExists(
            string vendorCode,
            int vendorID,
            string domainID,
            string systemID,
            string warehouseID)
        {
            if (string.IsNullOrWhiteSpace(vendorCode))
                return false;

            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.TblVendor
                WHERE VendorCode = @VendorCode
                  AND DomainID = @DomainID
                  AND IsDeleted = 0
                  AND VendorID <> @VendorID
                  AND (@SystemID IS NULL OR SystemID = @SystemID)
                  AND (@WarehouseID IS NULL OR WarehouseID = @WarehouseID);";

            using (var con = DbConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@VendorCode",
                    SqlDbType.NVarChar, 100).Value =
                    vendorCode.Trim();

                cmd.Parameters.Add("@VendorID",
                    SqlDbType.Int).Value = vendorID;

                AddScopeParameters(
                    cmd, domainID, systemID, warehouseID);

                con.Open();

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // =========================================
        // INSERT
        // =========================================

        public int Insert(
            VendorModel model,
            string domainID,
            string systemID,
            string warehouseID,
            string userID,
            string username)
        {
            const string sql = @"
                INSERT INTO dbo.TblVendor
                (
                    VendorTypeID,
                    VendorType,
                    VendorCode,
                    VendorName,
                    StatusID,
                    Status,
                    Address,
                    Phone,
                    Email,
                    Website,
                    ContactName,
                    ContactPersonName,
                    ContactPhoneNumber,
                    Position,
                    PaymentTerms,
                    NumberofDays,
                    VATTIN,
                    CompanyName,
                    CompanyAddress,
                    BankerName,
                    BankerAddress,
                    SwiftCode,
                    AccountNo,
                    AccountName,
                    BankerRemark,
                    Remark,
                    RegisterDate,
                    SystemID,
                    WarehouseID,
                    DomainID,
                    UserID,
                    CreateByUserID,
                    CreateBy,
                    CreateDate
                )
                VALUES
                (
                    @VendorTypeID,
                    @VendorType,
                    @VendorCode,
                    @VendorName,
                    @StatusID,
                    @Status,
                    @Address,
                    @Phone,
                    @Email,
                    @Website,
                    @ContactName,
                    @ContactPersonName,
                    @ContactPhoneNumber,
                    @Position,
                    @PaymentTerms,
                    @NumberofDays,
                    @VATTIN,
                    @CompanyName,
                    @CompanyAddress,
                    @BankerName,
                    @BankerAddress,
                    @SwiftCode,
                    @AccountNo,
                    @AccountName,
                    @BankerRemark,
                    @Remark,
                    @RegisterDate,
                    @SystemID,
                    @WarehouseID,
                    @DomainID,
                    @UserID,
                    @CreateByUserID,
                    @CreateBy,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, con))
            {
                AddVendorParameters(cmd, model);

                AddScopeParameters(
                    cmd, domainID, systemID, warehouseID);

                cmd.Parameters.Add("@UserID",
                    SqlDbType.Int).Value = userID;

                cmd.Parameters.Add("@CreateByUserID",
                    SqlDbType.Int).Value =
                    (object)userID ?? DBNull.Value;

                cmd.Parameters.Add("@CreateBy",
                    SqlDbType.NVarChar, 100).Value =
                    (object)username ?? DBNull.Value;

                con.Open();

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // =========================================
        // UPDATE
        // =========================================

        public bool Update(
            VendorModel model,
            string domainID,
            string systemID,
            string warehouseID)
        {
            const string sql = @"
                UPDATE dbo.TblVendor
                SET
                    VendorTypeID = @VendorTypeID,
                    VendorType = @VendorType,
                    VendorCode = @VendorCode,
                    VendorName = @VendorName,
                    StatusID = @StatusID,
                    Status = @Status,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    Website = @Website,
                    ContactName = @ContactName,
                    ContactPersonName = @ContactPersonName,
                    ContactPhoneNumber = @ContactPhoneNumber,
                    Position = @Position,
                    PaymentTerms = @PaymentTerms,
                    NumberofDays = @NumberofDays,
                    VATTIN = @VATTIN,
                    CompanyName = @CompanyName,
                    CompanyAddress = @CompanyAddress,
                    BankerName = @BankerName,
                    BankerAddress = @BankerAddress,
                    SwiftCode = @SwiftCode,
                    AccountNo = @AccountNo,
                    AccountName = @AccountName,
                    BankerRemark = @BankerRemark,
                    Remark = @Remark,
                    RegisterDate = @RegisterDate
                WHERE VendorID = @VendorID
                  AND DomainID = @DomainID
                  AND IsDeleted = 0
                  AND (@SystemID IS NULL OR SystemID = @SystemID)
                  AND (@WarehouseID IS NULL OR WarehouseID = @WarehouseID);";

            using (var con = DbConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, con))
            {
                AddVendorParameters(cmd, model);

                AddScopeParameters(
                    cmd, domainID, systemID, warehouseID);

                cmd.Parameters.Add("@VendorID",
                    SqlDbType.Int).Value = model.VendorID;

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================
        // SOFT DELETE
        // =========================================

        public bool Delete(
            int vendorID,
            string domainID,
            string systemID,
            string warehouseID)
        {
            const string sql = @"
                UPDATE dbo.TblVendor
                SET IsDeleted = 1
                WHERE VendorID = @VendorID
                  AND DomainID = @DomainID
                  AND IsDeleted = 0
                  AND (@SystemID IS NULL OR SystemID = @SystemID)
                  AND (@WarehouseID IS NULL OR WarehouseID = @WarehouseID)
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.TblAccount_Transaction T
                      WHERE T.VendorID = @VendorID
                        AND T.DomainID = @DomainID
                  );";

            using (var con = DbConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@VendorID",
                    SqlDbType.Int).Value = vendorID;

                AddScopeParameters(
                    cmd, domainID, systemID, warehouseID);

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================
        // COMMON PARAMETERS
        // =========================================

        private static void AddScopeParameters(
            SqlCommand cmd,
            string domainID,
            string systemID,
            string warehouseID)
        {
            cmd.Parameters.Add("@DomainID",
                SqlDbType.NVarChar, 100).Value = domainID;

            cmd.Parameters.Add("@SystemID",
                SqlDbType.Int, 100).Value = systemID;

            cmd.Parameters.Add("@WarehouseID",
                SqlDbType.Int, 100).Value = warehouseID;
        }

        private static void AddVendorParameters(
            SqlCommand cmd,
            VendorModel m)
        {
            AddInt(cmd, "@VendorTypeID", m.VendorTypeID);
            AddString(cmd, "@VendorType", m.VendorType, 100);
            AddString(cmd, "@VendorCode", m.VendorCode, 100);
            AddString(cmd, "@VendorName", m.VendorName, -1);
            AddInt(cmd, "@StatusID", m.StatusID);
            AddString(cmd, "@Status", m.Status, 100);
            AddString(cmd, "@Address", m.Address, -1);
            AddString(cmd, "@Phone", m.Phone, -1);
            AddString(cmd, "@Email", m.Email, 100);
            AddString(cmd, "@Website", m.Website, 100);
            AddString(cmd, "@ContactName", m.ContactName, -1);
            AddString(cmd, "@ContactPersonName", m.ContactPersonName, -1);
            AddString(cmd, "@ContactPhoneNumber", m.ContactPhoneNumber, -1);
            AddString(cmd, "@Position", m.Position, 100);
            AddString(cmd, "@PaymentTerms", m.PaymentTerms, 100);
            AddString(cmd, "@NumberofDays", m.NumberofDays, 100);
            AddString(cmd, "@VATTIN", m.VATTIN, 100);
            AddString(cmd, "@CompanyName", m.CompanyName, -1);
            AddString(cmd, "@CompanyAddress", m.CompanyAddress, -1);
            AddString(cmd, "@BankerName", m.BankerName, -1);
            AddString(cmd, "@BankerAddress", m.BankerAddress, -1);
            AddString(cmd, "@SwiftCode", m.SwiftCode, -1);
            AddString(cmd, "@AccountNo", m.AccountNo, -1);
            AddString(cmd, "@AccountName", m.AccountName, -1);
            AddString(cmd, "@BankerRemark", m.BankerRemark, -1);
            AddString(cmd, "@Remark", m.Remark, -1);

            cmd.Parameters.Add("@RegisterDate",
                SqlDbType.DateTime).Value =
                (object)m.RegisterDate ?? DBNull.Value;
        }

        private static void AddString(
            SqlCommand cmd, string name,
            string value, int size)
        {
            cmd.Parameters.Add(name,
                SqlDbType.NVarChar, size).Value =
                string.IsNullOrWhiteSpace(value)
                    ? (object)DBNull.Value
                    : value.Trim();
        }

        private static void AddInt(
            SqlCommand cmd, string name, int? value)
        {
            cmd.Parameters.Add(name,
                SqlDbType.Int).Value =
                (object)value ?? DBNull.Value;
        }

        // =========================================
        // DATA MAPPING
        // =========================================

        private static VendorModel Map(SqlDataReader r)
        {
            return new VendorModel
            {
                VendorID = Convert.ToInt32(r["VendorID"]),
                VendorTypeID = GetInt(r, "VendorTypeID"),
                VendorType = GetString(r, "VendorType"),
                VendorCode = GetString(r, "VendorCode"),
                VendorName = GetString(r, "VendorName"),
                StatusID = GetInt(r, "StatusID"),
                Status = GetString(r, "Status"),
                Gender = GetString(r, "Gender"),
                Address = GetString(r, "Address"),
                CityID = GetInt(r, "CityID"),
                City = GetString(r, "City"),
                StateRegionID = GetInt(r, "StateRegionID"),
                StateRegion = GetString(r, "StateRegion"),
                ZIPPostalCodeID = GetInt(r, "ZIPPostalCodeID"),
                ZIPPostalCode = GetString(r, "ZIPPostalCode"),
                CountryID = GetInt(r, "CountryID"),
                Country = GetString(r, "Country"),
                Phone = GetString(r, "Phone"),
                Email = GetString(r, "Email"),
                Website = GetString(r, "Website"),
                ContactName = GetString(r, "ContactName"),
                ContactPersonName = GetString(r, "ContactPersonName"),
                ContactPhoneNumber = GetString(r, "ContactPhoneNumber"),
                Position = GetString(r, "Position"),
                PaymentTerms = GetString(r, "PaymentTerms"),
                NumberofDays = GetString(r, "NumberofDays"),
                VATTIN = GetString(r, "VATTIN"),
                CompanyName = GetString(r, "CompanyName"),
                CompanyAddress = GetString(r, "CompanyAddress"),
                BankerName = GetString(r, "BankerName"),
                BankerAddress = GetString(r, "BankerAddress"),
                SwiftCode = GetString(r, "SwiftCode"),
                AccountNo = GetString(r, "AccountNo"),
                AccountName = GetString(r, "AccountName"),
                BankerRemark = GetString(r, "BankerRemark"),
                Remark = GetString(r, "Remark"),
                Photo = GetString(r, "Photo"),
                AttachmentFile = GetString(r, "AttachmentFile"),
                VendorPicture = GetString(r, "VendorPicture"),
                VendorProfilePicture = GetString(r, "VendorProfilePicture"),
                RegisterDate = GetDate(r, "RegisterDate"),
                Seq = GetInt(r, "Seq"),
                SystemID = GetInt(r, "SystemID"),
                WarehouseID = GetInt(r, "WarehouseID"),
                DomainID = GetString(r, "DomainID"),
                UserID = GetInt(r, "UserID"),
                RefID = GetInt(r, "RefID"),
                RefType = GetString(r, "RefType"),
                CreateByUserID = GetInt(r, "CreateByUserID"),
                CreateBy = GetString(r, "CreateBy"),
                CreateDate = GetDate(r, "CreateDate"),
                IsDeleted = r["IsDeleted"] != DBNull.Value
                    && Convert.ToBoolean(r["IsDeleted"])
            };
        }

        private static string GetString(
            SqlDataReader r, string name)
        {
            return r[name] == DBNull.Value
                ? null : Convert.ToString(r[name]);
        }

        private static int? GetInt(
            SqlDataReader r, string name)
        {
            return r[name] == DBNull.Value
                ? (int?)null : Convert.ToInt32(r[name]);
        }

        private static DateTime? GetDate(
            SqlDataReader r, string name)
        {
            return r[name] == DBNull.Value
                ? (DateTime?)null : Convert.ToDateTime(r[name]);
        }
    }
}