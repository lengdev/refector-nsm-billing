using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace NSMSOLUTION.Controllers.API
{
    [RoutePrefix("api/proxy")]
    public class ProxyController : ApiController
    {
        private static readonly HttpClient _httpClient;

        static ProxyController()
        {
            var handler = new HttpClientHandler()
            {
                // Bypass SSL certificate validation (not recommended for production)
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        // POST api/proxy/verify-id
        [HttpPost]
        [Route("verify-id")]
        public async Task<IHttpActionResult> VerifyID([FromBody] RequestData requestData)
        {
            if (string.IsNullOrEmpty(requestData?.Id))
            {
                return BadRequest("Customer Identification is required.");
            }

            var url = $"https://khmerdx.com/WebAPI/api/v2/GetVerifyNEC?id={requestData.Id}";
            //var url = $"https://localhost:7036/api/v2/GetVerifyNEC?id={requestData.Id}";

            try
            {
                // Make the GET request to the external API
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return Content(response.StatusCode, "Failed to fetch data.");
                }

                // Read the content from the response
                var responseContent = await response.Content.ReadAsStringAsync();

                // Return the response as JSON
                return Content(System.Net.HttpStatusCode.OK, responseContent, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                // Log the exception (you can replace this with your logging system)
                return InternalServerError(ex);
            }
        }

        // Define a model to receive the request data
        public class RequestData
        {
            public string Id { get; set; }
        }
    }
}
