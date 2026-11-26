using MVC;
using System.Collections.Generic;
using System.Web.Http;

namespace NewInventorySystem.Controllers.API
{
    public class InvInfoController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [Authorize]
        public string Get(string id)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY tb.CustomerBalanceID desc) AS[No], tb.InvoiceNo, convert(varchar, tb.InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, tb.InvoiceDate, GETDATE())[Day], tb.SubTotal, tb.VAT, tb.Total, tb.DiscountP, ";//GETUTCDATE
            SQL += "tb.DiscountUSD, tb.GrandTotal, tb.Paid, tb.Unpaid, tb.IsIncludedVAT, tb.StatusID, ";
            SQL += "format(tb.PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate from dbo.TblCustomerBalance tb ";
            SQL += "inner join dbo.TblCustomer c on c.CustomerID = tb.CustomerID ";
            SQL += "where c.CustomerContactMobileNumber = @PhoneNumber";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "PhoneNumber", id }
            };
            return html.JsonData(SQL, false, Para);
        }
        [Authorize]
        public string Get(string phone, string statusid)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY tb.CustomerBalanceID desc) AS[No], tb.InvoiceNo, convert(varchar, tb.InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, tb.InvoiceDate, GETDATE())[Day], tb.SubTotal, tb.VAT, tb.Total, tb.DiscountP, ";//GETUTCDATE
            SQL += "tb.DiscountUSD, tb.GrandTotal, tb.Paid, tb.Unpaid, tb.IsIncludedVAT, tb.StatusID, ";
            SQL += "format(tb.PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate from dbo.TblCustomerBalance tb ";
            SQL += "inner join dbo.TblCustomer c on c.CustomerID = tb.CustomerID ";
            SQL += "where c.CustomerContactMobileNumber = @PhoneNumber and tb.StatusID = @StatusID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "PhoneNumber", phone },
                { "StatusID", statusid }
            };
            return html.JsonData(SQL, false, Para);
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}