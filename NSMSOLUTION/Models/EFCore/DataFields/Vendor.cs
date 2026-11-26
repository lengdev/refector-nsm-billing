using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Vendor
    {
        public int VendorID { get; set; }
        public int VendorTypeID { get; set; }
        public string VendorType { get; set; }
        public string VendorCode { get; set; }
        public int Seq { get; set; }
        public string VendorName { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int CityID { get; set; }
        public string City { get; set; }
        public int StateRegionID { get; set; }
        public string StateRegion { get; set; }
        public int ZIPPostalCodeID { get; set; }
        public string ZIPPostalCode { get; set; }
        public int CountryID { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Photo { get; set; }
        public string AttachmentFile { get; set; }
        public DateTime RegisterDate { get; set; }
        public string CreateBy { get; set; }
        public string Remark { get; set; }
        public string VendorPicture { get; set; }
        public string BankerName { get; set; }
        public string BankerAddress { get; set; }
        public string SwiftCode { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string BankerRemark { get; set; }
        public string ContactName { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string PaymentTerms { get; set; }
        public string NumberofDays { get; set; }
        public string ContactPersonName { get; set; }
        public string Position { get; set; }
        public string VATTIN { get; set; }
        public string CompanyAddress { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public string CompanyName { get; set; }
        public string PrimaryFieldValue { get; set; }
        public string VendorProfilePicture { get; set; }
        public int UserID { get; set; }
        public int RefID { get; set; }
        public string RefType { get; set; }
        public int CreateByUserID { get; set; }
        public DateTime CreateDate { get; set; }
    }
}