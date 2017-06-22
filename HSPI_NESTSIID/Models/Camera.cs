using System.Runtime.Serialization;

namespace HSPI_NESTSIID.Models
{
    [DataContract]
    class Camera
    {
        [DataMember(Name = "name")]
        public string name { get; set; }
        [DataMember(Name = "name_long")]
        public string name_long { get; set; }
        [DataMember(Name = "device_id")]
        public string device_id { get; set; }
        [DataMember(Name = "is_online")]
        public bool is_online { get; set; }
        [DataMember(Name = "is_streaming")]
        public bool is_streaming { get; set; }
        [DataMember(Name = "web_url")]
        public string web_url { get; set; }
        [DataMember(Name = "app_url")]
        public string app_url { get; set; }
    }
}
