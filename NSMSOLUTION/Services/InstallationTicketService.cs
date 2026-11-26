using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NSMSOLUTION.Services
{

    public class InstallationTicketRequest
    {
        [JsonProperty("titleId")]
        public string TitleId { get; set; } = InstallationTicketService.NewInstallationTitleId;

        [JsonProperty("title")]
        public string Title { get; set; } = "New Installation";

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("telegramId")]
        public string TelegramId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "CUSTOMER";

        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; } = "HIGH";
    }

    public class InstallationTicketResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string RawResponse { get; set; }
        public JToken Data { get; set; }
    }

    public static class InstallationTicketService
    {
        public const string ApiUrl = "http://192.168.200.183:3002/api/v1/tickets/external";
        public const string NewInstallationTitleId = "99e3ae08-b782-4b4f-b53d-a6065e57b524";

        public const string ApiStaticToken = "f8e687997b0cfa6f3401a8890817a0e739582a25e1a290321a804a8ebc80162a";

        private static readonly HttpClient _httpClient;

        static InstallationTicketService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Sends the ticket creation request to the API
        /// </summary>
        public static async Task<InstallationTicketResponse> CreateTicketAsync(InstallationTicketRequest request)
        {
            if (request == null)
            {
                return new InstallationTicketResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Ticket request payload cannot be null."
                };
            }

            if (string.IsNullOrWhiteSpace(request.TitleId))
            {
                request.TitleId = NewInstallationTitleId;
            }

            if (string.IsNullOrWhiteSpace(request.Sid))
            {
                request.Sid = "Pending";
            }

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(request, Formatting.Indented);

                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiUrl))
                {
                    requestMessage.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    if (!string.IsNullOrWhiteSpace(ApiStaticToken))
                    {
                        string token = ApiStaticToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? ApiStaticToken.Substring(7).Trim()
                            : ApiStaticToken.Trim();

                        // Attach headers supported by /api/v1/tickets/external
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        requestMessage.Headers.TryAddWithoutValidation("x-api-key", token);
                        requestMessage.Headers.TryAddWithoutValidation("EXTERNAL_TICKET_API_TOKEN", token);
                    }

                    HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                    string rawResponse = await response.Content.ReadAsStringAsync();

                    JToken parsedData = null;
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(rawResponse))
                        {
                            parsedData = JToken.Parse(rawResponse);
                        }
                    }
                    catch
                    {
                        // Response is not JSON
                    }

                    return new InstallationTicketResponse
                    {
                        Success = response.IsSuccessStatusCode,
                        StatusCode = (int)response.StatusCode,
                        Message = response.IsSuccessStatusCode
                            ? "Ticket created successfully."
                            : $"Failed to create ticket: {response.StatusCode} - {rawResponse}",
                        RawResponse = rawResponse,
                        Data = parsedData
                    };
                }
            }
            catch (Exception ex)
            {
                return new InstallationTicketResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Exception occurred while calling ticket API: {ex.Message}",
                    RawResponse = ex.ToString()
                };
            }
        }

        public static async Task<InstallationTicketResponse> CreateNewInstallationTicketAsync(
            int customerId,
            string cid,
            string sid,
            string description,
            string title = "New Installation",
            string telegramId = null,
            string priority = "HIGH",
            string titleId = NewInstallationTitleId)
        {
            var req = new InstallationTicketRequest
            {
                TitleId = string.IsNullOrWhiteSpace(titleId) ? NewInstallationTitleId : titleId,
                Title = title,
                CustomerId = customerId,
                Cid = cid,
                Sid = sid,
                Description = description,
                TelegramId = telegramId,
                Type = "CUSTOMER",
                Priority = string.IsNullOrWhiteSpace(priority) ? "HIGH" : priority
            };

            return await CreateTicketAsync(req);
        }

        /// <summary>
        /// Automatically pulls customer and billing info from database to create the New Installation ticket
        /// </summary>
        public static async Task<InstallationTicketResponse> CreateFromCustomerInvoiceAsync(
            int customerId,
            string invoiceNo = null,
            string connectionId = null,
            string title = "New Installation",
            string customDescription = null,
            string telegramId = null,
            string priority = "HIGH")
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var customer = await ctx.TblCustomer.FindAsync(customerId);
                    if (customer == null)
                    {
                        return new InstallationTicketResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = $"Customer with ID {customerId} not found."
                        };
                    }

                    var billInfo = await ctx.TblCustomerBillInformation
                        .Where(x => x.CustomerID == customerId && (string.IsNullOrEmpty(invoiceNo) || x.InvoiceNo == invoiceNo))
                        .OrderByDescending(x => x.CustomerBillInformationID)
                        .FirstOrDefaultAsync();

                    var billingInvoice = await ctx.TblCustomerBillingInvoice
                        .Where(x => x.CustomerID == customerId && (string.IsNullOrEmpty(invoiceNo) || x.InvoiceNo == invoiceNo))
                        .OrderByDescending(x => x.BillingInvoiceID)
                        .FirstOrDefaultAsync();

                    string cid = customer.CustomerCode;
                    string sid = string.IsNullOrWhiteSpace(billingInvoice?.SID) ? "Pending" : billingInvoice.SID;
                    string customerName = !string.IsNullOrWhiteSpace(customer.CustomerName) ? customer.CustomerName : customer.CustomerNameLatin;

                    string description = customDescription;
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"New Installation for {customerName} (CID: {cid})");
                        if (!string.IsNullOrWhiteSpace(connectionId))
                        {
                            sb.AppendLine($"Connection ID: {connectionId}");
                        }
                        if (!string.IsNullOrWhiteSpace(sid))
                        {
                            sb.AppendLine($"SID: {sid}");
                        }
                        if (!string.IsNullOrWhiteSpace(customer.CustomerMobile))
                        {
                            sb.AppendLine($"Phone: {customer.CustomerMobile}");
                        }
                        if (billInfo != null)
                        {
                            string addr = $"{billInfo.InstallAddress}, {billInfo.InstallCommune}, {billInfo.InstallDistrict}, {billInfo.InstallProvinceCity}".Trim(',', ' ');
                            if (!string.IsNullOrWhiteSpace(addr))
                            {
                                sb.AppendLine($"Install Address: {addr}");
                            }
                            if (!string.IsNullOrWhiteSpace(billInfo.GoogleMap_Latitude) && !string.IsNullOrWhiteSpace(billInfo.GoogleMap_Longitude))
                            {
                                sb.AppendLine($"Google Maps: https://www.google.com/maps/place/{billInfo.GoogleMap_Latitude},{billInfo.GoogleMap_Longitude}");
                            }
                        }

                        description = sb.ToString();
                    }

                    var req = new InstallationTicketRequest
                    {
                        TitleId = NewInstallationTitleId,
                        Title = title,
                        CustomerId = customerId,
                        Cid = cid,
                        Sid = sid,
                        Description = description,
                        TelegramId = telegramId,
                        Type = "CUSTOMER",
                        Priority = string.IsNullOrWhiteSpace(priority) ? "HIGH" : priority
                    };

                    return await CreateTicketAsync(req);
                }
            }
            catch (Exception ex)
            {
                return new InstallationTicketResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Error preparing installation ticket: {ex.Message}",
                    RawResponse = ex.ToString()
                };
            }
        }
    }
}
