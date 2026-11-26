using NSMSOLUTION.Models.ApprovalUtility;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NSMSOLUTION.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly TranRepository _tranRepository;
        private readonly TelegramNotifier _telegramNotifier;
        private readonly PdfGenerator _pdfGenerator;

        public ApprovalService(TranRepository tranRepository,
                               TelegramNotifier telegramNotifier,
                               PdfGenerator pdfGenerator)
        {
            _tranRepository = tranRepository;
            _telegramNotifier = telegramNotifier;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<ApprovalResult> HandleApprovalAsync(ApprovalRequest request)
        {
            //Generate PDF
            var pdfPath = "";
            if (request.IsSentWithFilePdf)
                pdfPath = await _pdfGenerator.GenerateAsync(request);

            //Send Telegram
            await _telegramNotifier.SendApprovalNotificationAsync(request, pdfPath);

            //Delete temp file
            if (File.Exists(pdfPath)) File.Delete(pdfPath);

            return new ApprovalResult { Success = true, Message = "Message sent successful" };
        }
        public async Task<ApprovalResult> ApprovalValidateAsync(ApprovalRequest request)
        {
            var isValid = await _tranRepository.ValidateUserAsync(request);
            var result = new ApprovalResult
            {
                Success = true
            };
            try
            {
                if (!isValid)
                {
                    if (request.ApprovalFrom == ApprovalSource.System)
                    {
                        result.Success = false;
                        result.Message = "⚠️ You can't take this action";
                    }
                    else
                    {
                        string msg = $"@{request.UserName}, You can't take this action.";
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                //await _telegramNotifier.SendErrorAsync(request, ex.Message);
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
