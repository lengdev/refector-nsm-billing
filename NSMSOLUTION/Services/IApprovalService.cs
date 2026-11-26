using NSMSOLUTION.Models.ApprovalUtility;
using System.Threading.Tasks;

namespace NSMSOLUTION.Services
{
    public interface IApprovalService
    {
        Task<ApprovalResult> HandleApprovalAsync(ApprovalRequest request);
    }
}
