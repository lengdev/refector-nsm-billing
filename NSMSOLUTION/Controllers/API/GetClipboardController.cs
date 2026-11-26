//using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
//using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using TextCopy;

namespace NSMSOLUTION.Controllers.API
{
    public class GetClipboardController : ApiController
    {
        // GET api/<controller>
        public GetClipboardController()
        {
        }
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [Route("api/GetClipboard/GetClipboard")]
        public async Task<HttpResponseMessage> GetClipboard()
        {
            //var text = ClipboardCom.GetText();
            var text = await ClipboardService.GetTextAsync();
            return Request.CreateResponse(HttpStatusCode.OK, text);
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