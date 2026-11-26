using HtmlAgilityPack;
using MVC;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    public class GetExchangeRateController : ApiController
    {
        // GET api/<controller>
        public async Task<string> Get()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string url = "https://www.nbc.gov.kh/english/economic_research/exchange_rate.php";
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);
            string ExchangeRate = doc.DocumentNode.SelectNodes("//form[@id='fm-ex']/table/tr[2]/td/font")[0].InnerHtml.ToString().Trim();
            string CuDate = doc.DocumentNode.SelectNodes("//form[@id='fm-ex']/table/tr[1]/td/font")[0].InnerHtml.ToString().Trim();

            string getCDate = DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yyyy-MM-dd");

            if (ExchangeRate != "0000" && CuDate == getCDate)
            {
                //                string sql = @"declare @IsInsert bit = 0; if(not exists(select DateEntry from TblExchangeRate where format(DateEntry, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')))
                //begin
                //INSERT INTO [dbo].[TblExchangeRate]
                //           ([CurrencyID]
                //           ,[ExchangeRate]
                //           ,[DateEntry]
                //           ,[AddBy]
                //           ,[Memo]
                //           ,[CreateBy]
                //           ,[CreateDate]
                //           ,[SystemID]
                //           ,[WarehouseID]
                //           ,[DomainID])
                //     VALUES
                //           (5
                //           ,'" + ExchangeRate + @"'
                //           ,getdate()
                //           ,'System'
                //           ,'Rate in ' + format(getdate(), 'dd-MMM-yyyy') + '; Auto add from system.'
                //           ,'System'
                //           ,getdate()
                //           ,13
                //           ,16
                //           ,'001,002');
                //select @IsInsert = 1;
                //end; select @IsInsert IsInsert;";
                string sql = @"declare @IsInsert bit = 0; if(not exists(select DateEntry from TblExchangeRate where format(DateEntry, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')))
begin
INSERT INTO [dbo].[TblExchangeRate]
select 
           5 CurrencyID
           ,'" + ExchangeRate + @"' ExchangeRate
           ,getdate() DateEntry
           ,'System' AddBy
           ,'Rate in ' + format(getdate(), 'dd-MMM-yyyy') + '; Auto add from system.' Memo
           ,'System' CreateBy
           ,getdate() CreateDate
           ,ws.SystemID 
           ,ws.WarehouseID		   
           ,'001,002' DomainID
from dbo.TblWarehouseSystem ws
inner join dbo.TblSystem s on s.SystemID = ws.SystemID;
select @IsInsert = 1;
end; select @IsInsert IsInsert;";
                sqlResult = html.SaveChangeData(sql, false, true);
                if (sqlResult.Result == ResultType.Successful)
                {
                    if (sqlResult.Value == "True")
                    {
                        NotificationTelegram notificationTelegram = new NotificationTelegram();
                        await notificationTelegram.NotificationTelegramBotToTopic("Date: " + DateTime.Parse(html.GetCurrentDate()).ToString("dd-MM-yyyy hh:mm tt") + "\n\nExchange Rate: <b>" + double.Parse(ExchangeRate).ToString("N2") + "</b>", -1545132903, 7060); // finance exchange rate dialy
                    }
                }
            }

            return sqlResult.Message;
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }
        public string GetInnerTextFromHtml(string htmlText)
        {
            //Match any Html tag (opening or closing tags) 
            // followed by any successive whitespaces
            //consider the Html text as a single line

            Regex regex = new Regex("(<.*?>\\s*)+", RegexOptions.Singleline);

            // replace all html tags (and consequtive whitespaces) by spaces
            // trim the first and last space

            string resultText = regex.Replace(htmlText, " ").Trim();

            return resultText;
        }
    }
}