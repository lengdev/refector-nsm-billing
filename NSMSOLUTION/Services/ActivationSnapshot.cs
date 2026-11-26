using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Collections.Generic;
using System.Linq;

namespace NSMSOLUTION.Services
{
    // "before" state, read with AsNoTracking so later edits to tracked entities don't change it
    public class ActivationSnapshot
    {
        public ActivateCustomer Act { get; private set; }
        public CustomerDeviceInfo Info { get; private set; }
        public List<CustomerDeviceInfoDetail> Items { get; private set; }

        public static ActivationSnapshot Load(DBContext ctx, int activateCustomerId)
        {
            return new ActivationSnapshot
            {
                Act = ctx.TblActivateCustomer.AsNoTracking()
                    .First(x => x.ActivateCustomerID == activateCustomerId),
                Info = ctx.TblCustomerDeviceInfo.AsNoTracking()
                    .FirstOrDefault(x => x.ActivateCustomerID == activateCustomerId),
                Items = ctx.TblCustomerDeviceInfoDetail.AsNoTracking()
                    .Where(x => x.ActivateCustomerID == activateCustomerId)
                    .ToList()
            };
        }
    }
}