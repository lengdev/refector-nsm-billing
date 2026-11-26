using System;
using System.ComponentModel.DataAnnotations;

namespace NSMSOLUTION.Features.Vendors.Models
{
    public class VendorCreateViewModel
    {
        // General
        public int? VendorTypeID { get; set; }

        [Required(ErrorMessage = "Vendor type is required.")]
        [StringLength(100)]
        public string VendorType { get; set; }

        [Required(ErrorMessage = "Vendor name is required.")]
        [StringLength(200)]
        public string VendorName { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(250)]
        public string Website { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        public string Status { get; set; } = "Enable";

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(100)]
        public string PaymentTerms { get; set; }

        public int? NumberofDays { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        public DateTime RegisterDate { get; set; } = DateTime.Today;

        // English address
        [StringLength(100)]
        public string HouseNo { get; set; }

        [StringLength(200)]
        public string Street { get; set; }

        [StringLength(150)]
        public string Commune { get; set; }

        [StringLength(150)]
        public string District { get; set; }

        [StringLength(150)]
        public string City { get; set; }

        // Location IDs - optional
        public int? CityID { get; set; }
        public int? StateRegionID { get; set; }
        public int? ZIPPostalCodeID { get; set; }
        public int? CountryID { get; set; }

        [StringLength(150)]
        public string StateRegion { get; set; }

        [StringLength(50)]
        public string ZIPPostalCode { get; set; }

        [StringLength(150)]
        public string Country { get; set; }

        // Tax
        [StringLength(10)]
        public string VATTINPrefix { get; set; }

        [StringLength(100)]
        public string VATTIN { get; set; }

        // Local address
        [StringLength(200)]
        public string LocalVendorName { get; set; }

        [StringLength(100)]
        public string LocalHouseNo { get; set; }

        [StringLength(200)]
        public string LocalStreet { get; set; }

        [StringLength(150)]
        public string LocalCommune { get; set; }

        [StringLength(150)]
        public string LocalDistrict { get; set; }

        [StringLength(150)]
        public string LocalCity { get; set; }

        // Contact and company
        [StringLength(200)]
        public string ContactPersonName { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(50)]
        public string ContactPhoneNumber { get; set; }

        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(500)]
        public string CompanyAddress { get; set; }

        // Additional contact
        [StringLength(200)]
        public string ContactName { get; set; }

        // Bank
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

        [StringLength(500)]
        public string BankerRemark { get; set; }

        // Optional files
        public string Photo { get; set; }
        public string AttachmentFile { get; set; }
        public string VendorPicture { get; set; }
        public string VendorProfilePicture { get; set; }
    }
}
