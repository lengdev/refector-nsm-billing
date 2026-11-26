using NSMSOLUTION.Models.ApprovalUtility;
using System;
using System.Threading.Tasks;
using Utilities;

namespace NSMSOLUTION.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        public async Task<string> GenerateAsync(ApprovalRequest request)
        {
            SqlResult sqlResult = new SqlResult();
            try
            {
                // Await PDF generation
                sqlResult = await request.html.HTMLToPDF(request.HtmlData, request.Destination, request.FileName);
                if (sqlResult.Result == ResultType.Failed)
                    throw new Exception(sqlResult.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return sqlResult.OutPutFile;
        }
    }
}
