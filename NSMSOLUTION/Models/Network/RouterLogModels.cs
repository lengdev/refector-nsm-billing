using Newtonsoft.Json;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.Network
{
    public class ApiResponse
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("page_size")]
        public int PageSize { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("has_next")]
        public bool HasNext { get; set; }

        [JsonProperty("next_cursor")]
        public long? NextCursor { get; set; }

        [JsonProperty("data")]
        public List<LogEntryDto> Data { get; set; }
    }

    public class LogEntryDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("source_device_ip")]
        public string SourceDeviceIp { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("router")]
        public string Router { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("incoming_interface")]
        public string IncomingInterface { get; set; }

        [JsonProperty("outgoing_interface")]
        public string OutgoingInterface { get; set; }

        [JsonProperty("connection_state")]
        public string ConnectionState { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("source_ip")]
        public string SourceIp { get; set; }

        [JsonProperty("source_port")]
        public int? SourcePort { get; set; }

        [JsonProperty("destination_ip")]
        public string DestinationIp { get; set; }

        [JsonProperty("destination_port")]
        public int? DestinationPort { get; set; }

        [JsonProperty("packet_length")]
        public int PacketLength { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }

    public class GridLogEntry
    {
        public string Timestamp { get; set; }
        public string SrcIP { get; set; }
        public int? SrcPort { get; set; }
        public string DstIP { get; set; }
        public int? DstPort { get; set; }
        public string SrcMac { get; set; }
        public string Category { get; set; }
        public string Router { get; set; }
        public string Protocol { get; set; }
        public string Action { get; set; }
        public string ConnectionState { get; set; }
        public string IncomingInterface { get; set; }
    }
}
