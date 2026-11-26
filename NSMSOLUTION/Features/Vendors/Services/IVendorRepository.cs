using NSMSOLUTION.Features.Vendors.Models;
using NSMSOLUTION.Features.Vendors.Repositories;

namespace NSMSOLUTION.Features.Vendors.Services
{
    public interface IVendorRepository
    {
        bool ExistsVendorName(string vendorName, VendorCreateScope scope);

        VendorCreateResult Create(
            VendorModel model,
            VendorCreateScope scope);
    }
}