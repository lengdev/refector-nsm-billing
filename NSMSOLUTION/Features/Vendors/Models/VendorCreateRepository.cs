
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NSMSOLUTION.Features.Vendors.Models;

namespace NSMSOLUTION.Features.Vendors.Repositories
{
    public sealed class VendorCreateScope
    {
        public string DomainID { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public int? UserID { get; set; }
        public int? CreateByUserID { get; set; }
        public string Username { get; set; }
    }

    public sealed class VendorCreateResult
    {
        public int VendorID { get; set; }
        public string VendorCode { get; set; }
        public int Seq { get; set; }
    }

    public class VendorCreateRepository
    {
        private readonly string _connectionString;

        public VendorCreateRepository()
        {
            var cs = ConfigurationManager.ConnectionStrings["AuthContextNSM"];

            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Connection string 'AuthContextNSM' was not found in Web.config.");
            }

            _connectionString = cs.ConnectionString;
        }

        public bool ExistsVendorName(string vendorName, VendorCreateScope scope)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
                return false;

            ValidateScope(scope);

            const string sql = @"
SELECT TOP (1) 1
FROM dbo.TblVendor
WHERE ISNULL(IsDeleted, 0) = 0
  AND LTRIM(RTRIM(VendorName)) = @VendorName
  AND DomainID = @DomainID
  AND (@SystemID IS NULL OR SystemID = @SystemID);";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@VendorName", SqlDbType.NVarChar, 4000)
                    .Value = vendorName.Trim();

                AddScope(cmd, scope);

                cn.Open();

                return cmd.ExecuteScalar() != null;
            }
        }

        public VendorCreateResult Create(VendorModel m, VendorCreateScope scope)
        {
            if (m == null)
                throw new ArgumentNullException(nameof(m));

            ValidateScope(scope);

            if (string.IsNullOrWhiteSpace(m.VendorName))
                throw new ArgumentException("Vendor name is required.");

            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @NextSeq INT;
    DECLARE @NewCode NVARCHAR(100);
    DECLARE @NewID INT;

    -- Prevent duplicate vendor names within the same domain/system.
    IF EXISTS
    (
        SELECT 1
        FROM dbo.TblVendor WITH (UPDLOCK, HOLDLOCK)
        WHERE ISNULL(IsDeleted, 0) = 0
          AND LTRIM(RTRIM(VendorName)) = @VendorName
          AND DomainID = @DomainID
          AND (@SystemID IS NULL OR SystemID = @SystemID)
    )
    BEGIN
        THROW 50001, 'A vendor with this name already exists.', 1;
    END;

    -- Get the highest existing numeric vendor code.
    DECLARE @MaxCodeSeq INT;
    DECLARE @MaxStoredSeq INT;

    SELECT
        @MaxCodeSeq = MAX
        (
            CASE
                WHEN VendorCode LIKE 'VD-%'
                THEN TRY_CONVERT(INT, SUBSTRING(VendorCode, 4, 30))
                ELSE NULL
            END
        ),
        @MaxStoredSeq = MAX(Seq)
    FROM dbo.TblVendor WITH (UPDLOCK, HOLDLOCK)
    WHERE DomainID = @DomainID
      AND (@SystemID IS NULL OR SystemID = @SystemID);

    SET @NextSeq =
        CASE
            WHEN ISNULL(@MaxCodeSeq, 0) > ISNULL(@MaxStoredSeq, 0)
                THEN ISNULL(@MaxCodeSeq, 0) + 1
            ELSE ISNULL(@MaxStoredSeq, 0) + 1
        END;

    SET @NewCode = N'VD-' + CONVERT(NVARCHAR(20), @NextSeq);

    INSERT INTO dbo.TblVendor
    (
        VendorTypeID,
        VendorType,
        VendorCode,
        Seq,
        VendorName,
        StatusID,
        Gender,
        Phone,
        Address,
        Email,
        Website,
        ContactName,
        ContactPersonName,
        ContactPhoneNumber,
        Position,
        PaymentTerms,
        CompanyName,
        VATTIN,
        CompanyAddress,
        Remark,
        BankerName,
        SwiftCode,
        BankerAddress,
        AccountNo,
        AccountName,
        BankerRemark,
        RegisterDate,
        CreateBy,
        SystemID,
        WarehouseID,
        DomainID,
        UserID,
        CreateByUserID,
        CreateDate,
        IsDeleted
    )
    VALUES
    (
        @VendorTypeID,
        @VendorType,
        @NewCode,
        @NextSeq,
        @VendorName,
        @StatusID,
        @Gender,
        @Phone,
        @Address,
        @Email,
        @Website,
        @ContactName,
        @ContactPersonName,
        @ContactPhoneNumber,
        @Position,
        @PaymentTerms,
        @CompanyName,
        @VATTIN,
        @CompanyAddress,
        @Remark,
        @BankerName,
        @SwiftCode,
        @BankerAddress,
        @AccountNo,
        @AccountName,
        @BankerRemark,
        ISNULL(@RegisterDate, GETDATE()),
        @CreateBy,
        @SystemID,
        @WarehouseID,
        @DomainID,
        @UserID,
        @CreateByUserID,
        GETDATE(),
        0
    );

    SET @NewID = CONVERT(INT, SCOPE_IDENTITY());

    COMMIT TRANSACTION;

    SELECT
        @NewID AS VendorID,
        @NewCode AS VendorCode,
        @NextSeq AS Seq;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 60;

                AddScope(cmd, scope);

                cmd.Parameters.Add("@VendorTypeID", SqlDbType.Int).Value =
                    DbValue(m.VendorTypeID);

                AddText(cmd, "@VendorType", 200, m.VendorType);
                AddText(cmd, "@VendorName", 4000, m.VendorName);

                cmd.Parameters.Add("@StatusID", SqlDbType.Int).Value =
                    DbValue(3);

                //AddText(cmd, "@Status", 50, m.Status);
                AddText(cmd, "@Gender", 20, m.Gender);
                AddText(cmd, "@Phone", 100, m.Phone);
                AddText(cmd, "@Address", 1000, m.Address);
                AddText(cmd, "@Email", 100, m.Email);
                AddText(cmd, "@Website", 300, m.Website);
                AddText(cmd, "@ContactName", 200, m.ContactName);
                AddText(cmd, "@ContactPersonName", 200, m.ContactPersonName);
                AddText(cmd, "@ContactPhoneNumber", 100, m.ContactPhoneNumber);
                AddText(cmd, "@Position", 100, m.Position);
                AddText(cmd, "@PaymentTerms", 100, m.PaymentTerms);
                AddText(cmd, "@CompanyName", 300, m.CompanyName);
                AddText(cmd, "@VATTIN", 100, m.VATTIN);
                AddText(cmd, "@CompanyAddress", 1000, m.CompanyAddress);
                AddText(cmd, "@Remark", 2000, m.Remark);

                AddText(cmd, "@BankerName", 200, m.BankerName);
                AddText(cmd, "@SwiftCode", 100, m.SwiftCode);
                AddText(cmd, "@BankerAddress", 500, m.BankerAddress);
                AddText(cmd, "@AccountNo", 100, m.AccountNo);
                AddText(cmd, "@AccountName", 200, m.AccountName);
                AddText(cmd, "@BankerRemark", 1000, m.BankerRemark);

                cmd.Parameters.Add("@RegisterDate", SqlDbType.DateTime).Value =
                    m.RegisterDate.HasValue
                        ? (object)m.RegisterDate.Value
                        : DBNull.Value;

                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                    {
                        throw new InvalidOperationException(
                            "Vendor insert did not return an identity.");
                    }

                    return new VendorCreateResult
                    {
                        VendorID = Convert.ToInt32(rd["VendorID"]),
                        VendorCode = Convert.ToString(rd["VendorCode"]),
                        Seq = Convert.ToInt32(rd["Seq"])
                    };
                }
            }
        }

        private static void ValidateScope(VendorCreateScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            if (string.IsNullOrWhiteSpace(scope.DomainID))
                throw new ArgumentException("DomainID is required.");

            if (scope.DomainID.Trim().Length > 50)
                throw new ArgumentException("DomainID cannot exceed 50 characters.");
        }

        private static void AddScope(SqlCommand cmd, VendorCreateScope s)
        {
            AddText(cmd, "@DomainID", 50, s.DomainID);

            cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                DbValue(s.SystemID);

            cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                DbValue(s.WarehouseID);

            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value =
                DbValue(s.UserID);

            cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                DbValue(s.CreateByUserID);

            AddText(cmd, "@CreateBy", 200, s.Username);
        }

        private static object DbValue(object value)
        {
            return value ?? DBNull.Value;
        }

        private static void AddText(
            SqlCommand cmd,
            string name,
            int size,
            string value)
        {
            cmd.Parameters.Add(name, SqlDbType.NVarChar, size).Value =
                string.IsNullOrWhiteSpace(value)
                    ? (object)DBNull.Value
                    : value.Trim();
        }


        public bool Update(int vendorID, VendorModel m, VendorCreateScope scope)
        {
            if (vendorID <= 0)
                throw new ArgumentException("Invalid VendorID.");

            if (m == null)
                throw new ArgumentNullException(nameof(m));

            ValidateScope(scope);

            if (string.IsNullOrWhiteSpace(m.VendorName))
                throw new ArgumentException("Vendor name is required.");

            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Ensure the vendor exists within the current scope.
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblVendor WITH (UPDLOCK, HOLDLOCK)
        WHERE VendorID = @VendorID
          AND DomainID = @DomainID
          AND (@SystemID IS NULL OR SystemID = @SystemID)
          AND ISNULL(IsDeleted, 0) = 0
    )
    BEGIN
        THROW 50002, 'Vendor not found or access denied.', 1;
    END;

    -- Check duplicate vendor name, excluding current vendor.
    IF EXISTS
    (
        SELECT 1
        FROM dbo.TblVendor WITH (UPDLOCK, HOLDLOCK)
        WHERE ISNULL(IsDeleted, 0) = 0
          AND LTRIM(RTRIM(VendorName)) = @VendorName
          AND DomainID = @DomainID
          AND (@SystemID IS NULL OR SystemID = @SystemID)
          AND VendorID <> @VendorID
    )
    BEGIN
        THROW 50001, 'A vendor with this name already exists.', 1;
    END;

    UPDATE dbo.TblVendor
    SET
        VendorTypeID = @VendorTypeID,
        VendorType = @VendorType,
        VendorName = @VendorName,
        Gender = @Gender,
        Phone = @Phone,
        Address = @Address,
        Email = @Email,
        Website = @Website,
        ContactName = @ContactName,
        ContactPersonName = @ContactPersonName,
        ContactPhoneNumber = @ContactPhoneNumber,
        Position = @Position,
        PaymentTerms = @PaymentTerms,
        CompanyName = @CompanyName,
        VATTIN = @VATTIN,
        CompanyAddress = @CompanyAddress,
        Remark = @Remark,
        BankerName = @BankerName,
        SwiftCode = @SwiftCode,
        BankerAddress = @BankerAddress,
        AccountNo = @AccountNo,
        AccountName = @AccountName,
        BankerRemark = @BankerRemark,
        RegisterDate = @RegisterDate
    WHERE VendorID = @VendorID
      AND DomainID = @DomainID
      AND (@SystemID IS NULL OR SystemID = @SystemID)
      AND ISNULL(IsDeleted, 0) = 0;

    COMMIT TRANSACTION;

    SELECT 1 AS Success;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 60;

                AddScope(cmd, scope);

                cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = vendorID;

                cmd.Parameters.Add("@VendorTypeID", SqlDbType.Int).Value =
                    DbValue(m.VendorTypeID);

                AddText(cmd, "@VendorType", 200, m.VendorType);
                AddText(cmd, "@VendorName", 4000, m.VendorName);
                AddText(cmd, "@Gender", 20, m.Gender);
                AddText(cmd, "@Phone", 100, m.Phone);
                AddText(cmd, "@Address", 1000, m.Address);
                AddText(cmd, "@Email", 100, m.Email);
                AddText(cmd, "@Website", 300, m.Website);
                AddText(cmd, "@ContactName", 200, m.ContactName);
                AddText(cmd, "@ContactPersonName", 200, m.ContactPersonName);
                AddText(cmd, "@ContactPhoneNumber", 100, m.ContactPhoneNumber);
                AddText(cmd, "@Position", 100, m.Position);
                AddText(cmd, "@PaymentTerms", 100, m.PaymentTerms);
                AddText(cmd, "@CompanyName", 300, m.CompanyName);
                AddText(cmd, "@VATTIN", 100, m.VATTIN);
                AddText(cmd, "@CompanyAddress", 1000, m.CompanyAddress);
                AddText(cmd, "@Remark", 2000, m.Remark);

                AddText(cmd, "@BankerName", 200, m.BankerName);
                AddText(cmd, "@SwiftCode", 100, m.SwiftCode);
                AddText(cmd, "@BankerAddress", 500, m.BankerAddress);
                AddText(cmd, "@AccountNo", 100, m.AccountNo);
                AddText(cmd, "@AccountName", 200, m.AccountName);
                AddText(cmd, "@BankerRemark", 1000, m.BankerRemark);

                cmd.Parameters.Add("@RegisterDate", SqlDbType.DateTime).Value =
                    m.RegisterDate.HasValue
                        ? (object)m.RegisterDate.Value
                        : DBNull.Value;

                cn.Open();

                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

    }
}
