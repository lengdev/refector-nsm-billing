namespace NSMSOLUTION.Features.Vendors.Models
{
    public class VendorSearchModel
    {
        public string SearchText { get; set; }

        public int? VendorTypeID { get; set; }

        public int? StatusID { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}