using NSMSOLUTION.Features.Vendors.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NSMSOLUTION.Features.Vendors.Services
{
    public class VendorService : IVendorService
    {
        private readonly string _connectionString;

        public VendorService()
        {
            var cs = ConfigurationManager.ConnectionStrings["AuthContextNSM"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new ConfigurationErrorsException(
                    "Connection string 'DefaultConnection' was not found in Web.config.");
            _connectionString = cs.ConnectionString;
        }
        public bool ExistsVendorName(string vendorName, VendorCreateScope scope)
        {
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
                cmd.Parameters.Add("@VendorName", SqlDbType.NVarChar, 200).Value = vendorName.Trim();
                AddScope(cmd, scope);
                cn.Open();
                return cmd.ExecuteScalar() != null;
            }
        }
        private static void AddScope(SqlCommand cmd, VendorCreateScope s)
        {
            cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50).Value = s.DomainID;
            cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = s.SystemID;
            cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = s.WarehouseID;
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = s.UserID;
            cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =  s.CreateByUserID;
            AddText(cmd, "@CreateBy", 100, s.CreateBy);
        }
        public VendorCreateResult Create(
            VendorCreateViewModel m,
            VendorCreateScope scope)
        {
            if (m == null)
                throw new ArgumentNullException(nameof(m));

            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            if (string.IsNullOrWhiteSpace(scope.DomainID))
                throw new InvalidOperationException(
                    "DomainID is required.");

            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @NextSeq INT;
    DECLARE @NewCode NVARCHAR(50);
    DECLARE @NewID INT;

    SELECT @NextSeq =
        ISNULL(MAX(
            CASE
                WHEN VendorCode LIKE 'VD-%'
                 AND TRY_CONVERT(
                     INT,
                     SUBSTRING(VendorCode, 4, 30)
                 ) IS NOT NULL
                THEN TRY_CONVERT(
                    INT,
                    SUBSTRING(VendorCode, 4, 30)
                )
                ELSE TRY_CONVERT(INT, Seq)
            END
        ), 0) + 1
    FROM dbo.TblVendor WITH (UPDLOCK, HOLDLOCK)
    WHERE DomainID = @DomainID
      AND (
            @SystemID IS NULL
            OR SystemID = @SystemID
          );

    SET @NewCode =
        'VD-' + RIGHT(
            '000000' + CONVERT(VARCHAR(20), @NextSeq),
            6
        );

    INSERT INTO dbo.TblVendor
    (
        VendorTypeID,
        VendorType,
        VendorCode,
        Seq,
        VendorName,

        Status,
        Gender,
        Address,
        CityID,
        City,
        StateRegionID,
        StateRegion,
        ZIPPostalCodeID,
        ZIPPostalCode,
        CountryID,
        Country,

        Phone,
        Email,
        Website,

        RegisterDate,
        CreateBy,
        Remark,

        BankerName,
        BankerAddress,
        SwiftCode,
        AccountNo,
        AccountName,
        BankerRemark,

        ContactName,
        ContactPersonName,
        Position,
        ContactPhoneNumber,

        PaymentTerms,
        NumberofDays,

        VATTIN,
        VATTINPrefix,
        CompanyAddress,

        CompanyName,

        SystemID,
        WarehouseID,
        DomainID,

        UserID,
        CreateByUserID,
        CreateDate,
        IsDeleted,
        Currency,

        HouseNo,
        Street,
        Commune,
        District,

        LocalVendorName,
        LocalHouseNo,
        LocalStreet,
        LocalCommune,
        LocalDistrict,
        LocalCity,

        Photo,
        AttachmentFile,
        VendorPicture,
        VendorProfilePicture
    )
    VALUES
    (
        @VendorTypeID,
        @VendorType,
        @NewCode,
        @NextSeq,
        @VendorName,

        @Status,
        @Gender,
        @Address,
        @CityID,
        @City,
        @StateRegionID,
        @StateRegion,
        @ZIPPostalCodeID,
        @ZIPPostalCode,
        @CountryID,
        @Country,

        @Phone,
        @Email,
        @Website,

        @RegisterDate,
        @CreateBy,
        @Remark,

        @BankerName,
        @BankerAddress,
        @SwiftCode,
        @AccountNo,
        @AccountName,
        @BankerRemark,

        @ContactName,
        @ContactPersonName,
        @Position,
        @ContactPhoneNumber,

        @PaymentTerms,
        @NumberofDays,

        @VATTIN,
        @VATTINPrefix,
        @CompanyAddress,

        @CompanyName,

        @SystemID,
        @WarehouseID,
        @DomainID,

        @UserID,
        @CreateByUserID,
        GETDATE(),
        0,
        @Currency,

        @HouseNo,
        @Street,
        @Commune,
        @District,

        @LocalVendorName,
        @LocalHouseNo,
        @LocalStreet,
        @LocalCommune,
        @LocalDistrict,
        @LocalCity,

        @Photo,
        @AttachmentFile,
        @VendorPicture,
        @VendorProfilePicture
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
END CATCH;
";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 60;

                AddInt(cmd, "@VendorTypeID", m.VendorTypeID);

                AddText(cmd, "@VendorType", 100, m.VendorType);
                AddText(cmd, "@VendorName", 200, m.VendorName);

                AddText(cmd, "@Status", 50, m.Status);
                AddText(cmd, "@Gender", 20, m.Gender);
                AddText(cmd, "@Address", 500, m.Address);

                AddInt(cmd, "@CityID", m.CityID);
                AddText(cmd, "@City", 150, m.City);
                AddInt(cmd, "@StateRegionID", m.StateRegionID);
                AddText(cmd, "@StateRegion", 150, m.StateRegion);
                AddInt(cmd, "@ZIPPostalCodeID", m.ZIPPostalCodeID);
                AddText(cmd, "@ZIPPostalCode", 50, m.ZIPPostalCode);
                AddInt(cmd, "@CountryID", m.CountryID);
                AddText(cmd, "@Country", 150, m.Country);

                AddText(cmd, "@Phone", 50, m.Phone);
                AddText(cmd, "@Email", 150, m.Email);
                AddText(cmd, "@Website", 250, m.Website);

                cmd.Parameters.Add("@RegisterDate", SqlDbType.DateTime)
                    .Value = m.RegisterDate == DateTime.MinValue
                        ? (object)DateTime.Today
                        : m.RegisterDate;

                AddText(cmd, "@CreateBy", 100, scope.CreateBy);
                AddText(cmd, "@Remark", 1000, m.Remark);

                AddText(cmd, "@BankerName", 200, m.BankerName);
                AddText(cmd, "@BankerAddress", 500, m.BankerAddress);
                AddText(cmd, "@SwiftCode", 100, m.SwiftCode);
                AddText(cmd, "@AccountNo", 100, m.AccountNo);
                AddText(cmd, "@AccountName", 200, m.AccountName);
                AddText(cmd, "@BankerRemark", 500, m.BankerRemark);

                AddText(cmd, "@ContactName", 200, m.ContactName);
                AddText(cmd, "@ContactPersonName", 200, m.ContactPersonName);
                AddText(cmd, "@Position", 100, m.Position);
                AddText(cmd, "@ContactPhoneNumber", 50, m.ContactPhoneNumber);

                AddText(cmd, "@PaymentTerms", 100, m.PaymentTerms);
                AddInt(cmd, "@NumberofDays", m.NumberofDays);

                AddText(cmd, "@VATTIN", 100, m.VATTIN);
                AddText(cmd, "@VATTINPrefix", 10, m.VATTINPrefix);
                AddText(cmd, "@CompanyAddress", 500, m.CompanyAddress);
                AddText(cmd, "@CompanyName", 200, m.CompanyName);

                AddInt(cmd, "@SystemID", scope.SystemID);
                AddInt(cmd, "@WarehouseID", scope.WarehouseID);
                AddText(cmd, "@DomainID", 50, scope.DomainID);

                AddInt(cmd, "@UserID", scope.UserID);
                AddInt(cmd, "@CreateByUserID", scope.CreateByUserID);

                AddText(cmd, "@Currency", 10, m.Currency);

                AddText(cmd, "@HouseNo", 100, m.HouseNo);
                AddText(cmd, "@Street", 200, m.Street);
                AddText(cmd, "@Commune", 150, m.Commune);
                AddText(cmd, "@District", 150, m.District);

                AddText(cmd, "@LocalVendorName", 200, m.LocalVendorName);
                AddText(cmd, "@LocalHouseNo", 100, m.LocalHouseNo);
                AddText(cmd, "@LocalStreet", 200, m.LocalStreet);
                AddText(cmd, "@LocalCommune", 150, m.LocalCommune);
                AddText(cmd, "@LocalDistrict", 150, m.LocalDistrict);
                AddText(cmd, "@LocalCity", 150, m.LocalCity);

                AddText(cmd, "@Photo", 500, m.Photo);
                AddText(cmd, "@AttachmentFile", 500, m.AttachmentFile);
                AddText(cmd, "@VendorPicture", 500, m.VendorPicture);
                AddText(cmd, "@VendorProfilePicture", 500, m.VendorProfilePicture);

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException(
                            "Vendor insert did not return an identity.");

                    return new VendorCreateResult
                    {
                        VendorID = Convert.ToInt32(reader["VendorID"]),
                        VendorCode = Convert.ToString(reader["VendorCode"]),
                        Seq = Convert.ToInt32(reader["Seq"])
                    };
                }
            }
        }

        private static void AddText(
            SqlCommand cmd,
            string name,
            int size,
            string value)
        {
            var p = cmd.Parameters.Add(
                name,
                SqlDbType.NVarChar,
                size);

            p.Value = string.IsNullOrWhiteSpace(value)
                ? (object)DBNull.Value
                : value.Trim();
        }

        private static void AddInt(
            SqlCommand cmd,
            string name,
            int? value)
        {
            cmd.Parameters.Add(name, SqlDbType.Int).Value =
                value.HasValue
                    ? (object)value.Value
                    : DBNull.Value;
        }
    }
}