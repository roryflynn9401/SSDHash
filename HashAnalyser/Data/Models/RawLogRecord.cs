namespace HashAnalyser.Data.Models
{
    public class RawLogRecord
    {
        public string ts { get; set; }
        public string uid { get; set; }
        public string id_orig_h { get; set; }
        public string id_orig_p { get; set; }
        public string id_resp_h { get; set; }
        public string id_resp_p { get; set; }
        public string proto { get; set; }
        public string service { get; set; }
        public string duration { get; set; }
        public string orig_bytes { get; set; }
        public string resp_bytes { get; set; }
        public string conn_state { get; set; }
        public string local_orig { get; set; }
        public string local_resp { get; set; }
        public string missed_bytes { get; set; }
        public string history { get; set; }
        public string orig_pkts { get; set; }
        public string orig_ip_bytes { get; set; }
        public string resp_pkts { get; set; }
        public string resp_ip_bytes { get; set; }
        public string label { get; set; }
        public string detailed_label { get; set; }
    }
}
