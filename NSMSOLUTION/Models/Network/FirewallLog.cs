namespace NSMSOLUTION.Models.Network
{
    public class FirewallLog
    {
        public int Id { get; set; }
        public string LogTime { get; set; }
        public string SrcIp { get; set; }
        public int SrcPort { get; set; }
        public string DstIp { get; set; }
        public int DstPort { get; set; }
        public string SrcMac { get; set; }
        public string NatInfo { get; set; }
        public string FullMessage { get; set; }
    }


}