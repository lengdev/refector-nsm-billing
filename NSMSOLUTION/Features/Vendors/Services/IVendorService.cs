using NSMSOLUTION.Features.Vendors.Models;

namespace NSMSOLUTION.Features.Vendors.Services
{
    public interface IVendorService
    {
        VendorCreateResult Create(
            VendorCreateViewModel model,
            VendorCreateScope scope);
    }
}