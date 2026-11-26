
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NSMSOLUTION.Features.Vendors.Models
{
    public class VendorModel
    {
        public int VendorID { get; set; }

        public int? VendorTypeID { get; set; }

        [StringLength(200)]
        public string VendorType { get; set; }

        [StringLength(100)]
        public string VendorCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vendor name is required.")]
        [StringLength(4000)]
        public string VendorName { get; set; }

        public int? StatusID { get; set; } = 3;

        [StringLength(50)]
        public string Status { get; set; } = "Enable";

        [StringLength(20)]
        public string Gender { get; set; }

        [StringLength(1000)]
        public string Address { get; set; }

        public int? CityID { get; set; }
        public string City { get; set; }

        public int? StateRegionID { get; set; }
        public string StateRegion { get; set; }

        public int? ZIPPostalCodeID { get; set; }
        public string ZIPPostalCode { get; set; }

        public int? CountryID { get; set; }
        public string Country { get; set; }

        [StringLength(100)]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(300)]
        public string Website { get; set; }

        [StringLength(200)]
        public string ContactName { get; set; }

        [StringLength(200)]
        public string ContactPersonName { get; set; }

        [StringLength(100)]
        public string ContactPhoneNumber { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(100)]
        public string PaymentTerms { get; set; }

        [StringLength(50)]
        public string NumberofDays { get; set; }

        [StringLength(100)]
        public string VATTIN { get; set; }

        [StringLength(300)]
        public string CompanyName { get; set; }

        [StringLength(1000)]
        public string CompanyAddress { get; set; }

        [StringLength(200)]
        public string BankerName { get; set; }

        [StringLength(500)]
        public string BankerAddress { get; set; }

        [StringLength(100)]
        public string SwiftCode { get; set; }

        [StringLength(100)]
        public string AccountNo { get; set; }

        [StringLength(200)]
        public string AccountName { get; set; }

        [StringLength(1000)]
        public string BankerRemark { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

        public string Photo { get; set; }
        public string AttachmentFile { get; set; }
        public string VendorPicture { get; set; }
        public string VendorProfilePicture { get; set; }

        public DateTime? RegisterDate { get; set; }

        public int? Seq { get; set; }

        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }

        [StringLength(50)]
        public string DomainID { get; set; }

        public int? UserID { get; set; }
        public int? RefID { get; set; }

        [StringLength(100)]
        public string RefType { get; set; }

        public int? CreateByUserID { get; set; }

        [StringLength(200)]
        public string CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public bool IsDeleted { get; set; }

        // Additional fields from TblVendor

        public string Currency { get; set; }

        public string HouseNo { get; set; }
        public string Street { get; set; }
        public string Commune { get; set; }
        public string District { get; set; }

        public string VATTINPrefix { get; set; }

        public string LocalVendorName { get; set; }
        public string LocalHouseNo { get; set; }
        public string LocalStreet { get; set; }
        public string LocalCommune { get; set; }
        public string LocalDistrict { get; set; }
        public string LocalCity { get; set; }

        public int? PrimaryFieldValue { get; set; }

        public List<SelectListItem> VendorTypes { get; set; } = null;
    }
}
