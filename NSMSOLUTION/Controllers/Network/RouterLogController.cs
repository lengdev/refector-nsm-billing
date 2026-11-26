using Newtonsoft.Json;
using NSMSOLUTION.Models.Network;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Network
{
    [Authorize]
    public class RouterLogController : Controller
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // GET: RouterLog
        public ActionResult Index()
        {
            return View();
        }

        // GET: /RouterLog/GetLogs
        //[Authorize]
        [HttpGet]
        public async Task<JsonResult> GetLogs(string searchTerm, string startDate, string endDate, string sourceIp = null, int? sourcePort = null, int page = 1, int pageSize = 100)
        {
            try
            {
                string sortBy = "id";
                string sortOrder = "desc";

                string kendoSortField = Request.QueryString["sort[0][field]"];
                string kendoSortDir = Request.QueryString["sort[0][dir]"];

                if (!string.IsNullOrEmpty(kendoSortField))
                {
                    switch (kendoSortField.ToLower())
                    {
                        case "timestamp":
                            sortBy = "created_at";
                            break;
                        case "srcip":
                            sortBy = "source_ip";
                            break;
                        case "srcport":
                            sortBy = "source_port";
                            break;
                        case "dstip":
                            sortBy = "destination_ip";
                            break;
                        case "dstport":
                            sortBy = "destination_port";
                            break;
                        case "router":
                            sortBy = "router";
                            break;
                        case "protocol":
                            sortBy = "protocol";
                            break;
                        case "incominginterface":
                            sortBy = "incoming_interface";
                            break;
                        case "action":
                            sortBy = "action";
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(kendoSortDir))
                {
                    if (kendoSortDir.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                        kendoSortDir.Equals("desc", StringComparison.OrdinalIgnoreCase))
                    {
                        sortOrder = kendoSortDir.ToLower();
                    }
                }

                // Retrieve cursor for page if page > 1
                long? cursor = null;
                if (page == 1)
                {
                    Session["RouterLogCursors"] = null;
                }
                else
                {
                    var cursorMap = Session["RouterLogCursors"] as Dictionary<int, long>;
                    if (cursorMap != null && cursorMap.TryGetValue(page, out long cachedCursor))
                    {
                        cursor = cachedCursor;
                    }
                }

                var queryParams = new List<string>
                {
                    "page=" + page,
                    "page_size=" + pageSize,
                    "sort_by=" + Uri.EscapeDataString(sortBy),
                    "sort_order=" + Uri.EscapeDataString(sortOrder)
                };

                if (cursor.HasValue)
                {
                    queryParams.Add("cursor=" + cursor.Value);
                    queryParams.Add("next_cursor=" + cursor.Value);
                }

                if (!string.IsNullOrWhiteSpace(sourceIp))
                {
                    string cleanedIp = sourceIp.Trim();
                    if (!string.IsNullOrEmpty(cleanedIp))
                    {
                        if (System.Net.IPAddress.TryParse(cleanedIp, out _))
                        {
                            queryParams.Add("source_ip=" + Uri.EscapeDataString(cleanedIp));
                        }
                        else
                        {
                            queryParams.Add("router=" + Uri.EscapeDataString(cleanedIp));
                        }
                    }
                }

                if (sourcePort.HasValue)
                {
                    queryParams.Add("source_port=" + sourcePort.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        string termLower = searchTerm.ToLower();

                        if (termLower == "tcp" || termLower == "udp")
                        {
                            queryParams.Add("protocol=" + Uri.EscapeDataString(searchTerm));
                        }
                        else if (termLower == "srcnat")
                        {
                            queryParams.Add("action=" + Uri.EscapeDataString(searchTerm));
                        }
                        else if (termLower.StartsWith("ix-"))
                        {
                            queryParams.Add("router=" + Uri.EscapeDataString(searchTerm));
                        }
                        else
                        {
                            // Check if the searchTerm itself contains IP and Port, e.g. "10.20.1.194:8567"
                            int separatorIndex = searchTerm.LastIndexOf(':');
                            if (separatorIndex == -1)
                            {
                                separatorIndex = searchTerm.LastIndexOf(' ');
                            }

                            if (separatorIndex != -1)
                            {
                                string ipPart = searchTerm.Substring(0, separatorIndex).Trim();
                                string portPart = searchTerm.Substring(separatorIndex + 1).Trim();

                                if (System.Net.IPAddress.TryParse(ipPart, out _) && int.TryParse(portPart, out int port))
                                {
                                    queryParams.Add("source_ip=" + Uri.EscapeDataString(ipPart));
                                    queryParams.Add("source_port=" + port);
                                }
                                else if (System.Net.IPAddress.TryParse(searchTerm, out _))
                                {
                                    queryParams.Add("source_ip=" + Uri.EscapeDataString(searchTerm));
                                }
                                else
                                {
                                    queryParams.Add("router=" + Uri.EscapeDataString(searchTerm));
                                }
                            }
                            else
                            {
                                if (System.Net.IPAddress.TryParse(searchTerm, out _))
                                {
                                    queryParams.Add("source_ip=" + Uri.EscapeDataString(searchTerm));
                                }
                                else
                                {
                                    queryParams.Add("router=" + Uri.EscapeDataString(searchTerm));
                                }
                            }
                        }
                    }
                }

                string url = "http://192.168.69.83:8001/logs?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

                int total = 0;
                var gridList = new List<GridLogEntry>();

                if (apiResponse != null && apiResponse.Data != null)
                {
                    total = apiResponse.Total > 0
                        ? apiResponse.Total
                        : apiResponse.HasNext ? (page * pageSize) + 1 : ((page - 1) * pageSize) + apiResponse.Data.Count;

                    // Save next cursor to session
                    if (apiResponse.HasNext && apiResponse.NextCursor.HasValue)
                    {
                        var cursorMap = Session["RouterLogCursors"] as Dictionary<int, long> ?? new Dictionary<int, long>();
                        cursorMap[page + 1] = apiResponse.NextCursor.Value;
                        Session["RouterLogCursors"] = cursorMap;
                    }
                    foreach (var entry in apiResponse.Data)
                    {
                        string formattedTimestamp = entry.Timestamp;
                        if (!string.IsNullOrEmpty(entry.CreatedAt))
                        {
                            if (DateTime.TryParse(entry.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                            {
                                formattedTimestamp = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }

                        gridList.Add(new GridLogEntry
                        {
                            Timestamp = formattedTimestamp,
                            SrcIP = entry.SourceIp ?? "",
                            SrcPort = entry.SourcePort,
                            DstIP = entry.DestinationIp ?? "",
                            DstPort = entry.DestinationPort,
                            SrcMac = entry.IncomingInterface ?? entry.Router ?? "",
                            Category = entry.Action ?? entry.Protocol ?? "",
                            Router = entry.Router ?? "",
                            Protocol = entry.Protocol ?? "",
                            Action = entry.Action ?? "",
                            ConnectionState = entry.ConnectionState ?? "",
                            IncomingInterface = entry.IncomingInterface ?? ""
                        });
                    }
                }

                var data = JsonConvert.SerializeObject(gridList);

                return Json(new
                {
                    data = data,
                    total = total
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}