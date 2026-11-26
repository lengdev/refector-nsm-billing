using NSMSOLUTION.Models.ApprovalUtility;
using System.Threading.Tasks;

namespace NSMSOLUTION.Repositories
{
    public interface ITranRepository
    {
        Task<bool> ValidateUserAsync(ApprovalRequest request);
        Task UpdateApprovalStatusAsync(ApprovalRequest request);
    }
}

