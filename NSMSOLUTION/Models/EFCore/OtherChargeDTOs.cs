using System.Collections.Generic;

namespace NSMSOLUTION.Models.EFCore
{
    /// <summary>
    /// Flat response for a single Product (OtherCharge).
    /// Used by GET /api/products and GET /api/products/{id}.
    /// </summary>
    public class OtherChargeResponseDTO
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string UM { get; set; }
        public int? CategoryID { get; set; }
        public string Category { get; set; }
        public List<string> Images { get; set; }
    }

    /// <summary>
    /// A category with its nested Products.
    /// Used by GET /api/products/categories.
    /// </summary>
    public class ServiceCategoryResponseDTO
    {
        public int CategoryID { get; set; }
        public string CategoryCode { get; set; }
        public string Category { get; set; }
        public List<OtherChargeResponseDTO> Products { get; set; }
    }
}
