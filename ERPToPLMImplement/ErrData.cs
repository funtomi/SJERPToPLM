using System.Runtime.Serialization;

namespace ERPToPLMImplement {
    [DataContract]
    class ErrData {
        [DataMember]
        public string ret { get; set; }
        [DataMember]
        public string s3 { get; set; }
        [DataMember]
        public string s4 { get; set; }
    }
}
