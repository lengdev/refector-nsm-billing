using NSMSOLUTION.Models.ApprovalUtility;
using System.Threading.Tasks;

namespace NSMSOLUTION.Services
{
    public interface IPdfGenerator
    {
        Task<string> GenerateAsync(ApprovalRequest request);
    }
}
