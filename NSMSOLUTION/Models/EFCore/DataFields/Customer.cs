using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public int? EmployeeID { get; set; }
        public int? CustomerTypeID { get; set; }
        public int? IndustryTypeID { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameLatin { get; set; }
        public string CustomerGender { get; set; }

        public string CustomerAddress { get; set; }
        public string CustomerCommune { get; set; }
        public string CustomerDistrict { get; set; }
        public string CustomerProvinceCity { get; set; }

        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerNationality { get; set; }
        public string CustomerIdentification { get; set; }
        public DateTime? IdentificationExpireDate { get; set; }

        public string CustomerContactName { get; set; }
        public string CustomerContactMobileNumber { get; set; }
        public string CustomerContactEmail { get; set; }
        public string CustomerContactPosition { get; set; }

        public string CustomerPhoto { get; set; }
        public string CustomerProfilePicture { get; set; }

        public string AttachmentFile { get; set; }
        public string DestinationFile { get; set; }
        public string ATTName { get; set; }

        public DateTime? RegisterDate { get; set; }
        public DateTime? NewRegisterDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public string RegisterBy { get; set; }
        public int? RegisterByUserLoginID { get; set; }
        public string ModifyBy { get; set; }

        public int? StatusID { get; set; }
        public string CustomerStatus { get; set; }

        public string PreviousISP { get; set; }
        public string Memo { get; set; }
        public string CustomerNoted { get; set; }
        public string CustomerNote { get; set; }

        public int? CustomerGroupID { get; set; }
        public string CustomerGroup { get; set; }

        public int? AgencyCommissionID { get; set; }

        public bool? IsQuotation { get; set; }
        public bool? IsSaleOrder { get; set; }
        public bool? IsToDMCTAX { get; set; }
        public bool? IsFeedback { get; set; }
        public bool? ISApplyCID { get; set; }

        public string TypeOfInvoice { get; set; }
        public string CustomerType_PrefixVAT { get; set; }
        public string CustomerType_PrefixCM { get; set; }

        public int? ClassNameID { get; set; }
        public string ClassName { get; set; }

        public int? CommuneID { get; set; }
        public int? DistrictID { get; set; }
        public int? ProvinceCityID { get; set; }

        public string Salutation { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
