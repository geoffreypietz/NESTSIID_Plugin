using System.Runtime.Serialization;

namespace HSPI_NESTSIID.Models
{
    [DataContract]
    class Thermostat
    {
        [DataMember(Name = "name")]
        public string name { get; set; }
        [DataMember(Name = "name_long")]
        public string name_long { get; set; }
        [DataMember(Name = "device_id")]
        public string device_id { get; set; }
        [DataMember(Name = "is_online")]
        public bool is_online { get; set; }
        [DataMember(Name = "temperature_scale")]
        public string temperature_scale { get; set; }
        [DataMember(Name = "target_temperature_c")]
        public double target_temperature_c { get; set; }
        [DataMember(Name = "target_temperature_f")]
        public double target_temperature_f { get; set; }
        [DataMember(Name = "target_temperature_high_c")]
        public double target_temperature_high_c { get; set; }
        [DataMember(Name = "target_temperature_high_f")]
        public double target_temperature_high_f { get; set; }
        [DataMember(Name = "target_temperature_low_c")]
        public double target_temperature_low_c { get; set; }
        [DataMember(Name = "target_temperature_low_f")]
        public double target_temperature_low_f { get; set; }
        [DataMember(Name = "away_temperature_high_c")]
        public double away_temperature_high_c { get; set; }
        [DataMember(Name = "away_temperature_high_f")]
        public double away_temperature_high_f { get; set; }
        [DataMember(Name = "away_temperature_low_c")]
        public double away_temperature_low_c { get; set; }
        [DataMember(Name = "away_temperature_low_f")]
        public double away_temperature_low_f { get; set; }
        [DataMember(Name = "eco_temperature_high_c")]
        public double eco_temperature_high_c { get; set; }
        [DataMember(Name = "eco_temperature_high_f")]
        public double eco_temperature_high_f { get; set; }
        [DataMember(Name = "eco_temperature_low_c")]
        public double eco_temperature_low_c { get; set; }
        [DataMember(Name = "eco_temperature_low_f")]
        public double eco_temperature_low_f { get; set; }
        [DataMember(Name = "hvac_mode")]
        public string hvac_mode { get; set; }
        [DataMember(Name = "hvac_state")]
        public string hvac_state { get; set; }
        [DataMember(Name = "ambient_temperature_c")]
        public double ambient_temperature_c { get; set; }
        [DataMember(Name = "ambient_temperature_f")]
        public double ambient_temperature_f { get; set; }
        [DataMember(Name = "humidity")]
        public double humidity { get; set; }
        [DataMember(Name = "structure_id")]
        public string structure_id { get; set; }
        [DataMember(Name = "is_using_emergency_heat")]
        public bool is_using_emergency_heat { get; set; }
        [DataMember(Name = "can_heat")]
        public bool can_heat { get; set; }
        [DataMember(Name = "can_cool")]
        public bool can_cool { get; set; }
    }
}
