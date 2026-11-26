using NSMSOLUTION.Models.ApprovalUtility;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace NSMSOLUTION.Services
{
    public interface ITelegramNotifier
    {
        Task<ApprovalRequest> SendApprovalNotificationAsync(ApprovalRequest request, string pdfPath);
        Task SendErrorAsync(ApprovalRequest request, string errorMessage);
        InlineKeyboardMarkup CreateApprovalButtons(ApprovalRequest request);
    }
}
