using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HSPI_NESTSIID.Models
{
    [DataContract]
    class Devices
    {
        [DataMember(Name = "thermostats")]
        public Dictionary<string,Thermostat> thermostats { get; set; }
        [DataMember(Name = "cameras")]
        public Dictionary<string, Camera> cameras { get; set; }
    }
}
