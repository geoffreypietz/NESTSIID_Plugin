using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HSPI_NESTSIID.Models
{
    [DataContract]
    class Structures
    {
        [DataMember(Name = "name")]
        public string name { get; set; }
        [DataMember(Name = "away")]
        public string away { get; set; }
        [DataMember(Name = "thermostats")]
        public List<string> thermostats { get; set; }
        [DataMember(Name = "structure_id")]
        public string structure_id { get; set; }
    }
}
