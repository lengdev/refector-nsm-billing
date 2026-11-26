using NSMSOLUTION.Repositories;
using System;
using System.Threading.Tasks;

namespace NSMSOLUTION.Models.ApprovalUtility
{
    public class TranRepository : ITranRepository
    {
        public Task<bool> ValidateUserAsync(ApprovalRequest request)
        {
            return request.TakeActionByID != request.UserApprovalID ? Task.FromResult(false) : Task.FromResult(true);
        }

        public Task UpdateApprovalStatusAsync(ApprovalRequest request)
        {
            try
            {
                request.html.SaveChangeData(request.sql, false, request.para);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update approval status: " + ex.Message, ex);
            }

            return Task.CompletedTask;
        }

    }
}