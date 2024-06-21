namespace HashAnalyser.Data.Models
{
    public class MulticlassRawLogModel
    {
        public MulticlassRawLogModel(RawLogRecord rawLogRecord, string mappedLabel) 
        {
            ts = rawLogRecord.ts;
            uid = rawLogRecord.uid;
            id_orig_h = rawLogRecord.id_orig_h;
            id_orig_p = rawLogRecord.id_orig_p;
            id_resp_h = rawLogRecord.id_resp_h;
            id_resp_p = rawLogRecord.id_resp_p;
            proto = rawLogRecord.proto;
            service = rawLogRecord.service;
            duration = rawLogRecord.duration;
            orig_bytes = rawLogRecord.orig_bytes;
            resp_bytes = rawLogRecord.resp_bytes;
            conn_state = rawLogRecord.conn_state;
            local_orig = rawLogRecord.local_orig;
            local_resp = rawLogRecord.local_resp;
            missed_bytes = rawLogRecord.missed_bytes;
            history = rawLogRecord.history;
            orig_pkts = rawLogRecord.orig_pkts;
            orig_ip_bytes = rawLogRecord.orig_ip_bytes;
            resp_pkts = rawLogRecord.resp_pkts;
            resp_ip_bytes = rawLogRecord.resp_ip_bytes;
            label = mappedLabel;
        }

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
    }
}
