using Scheduler.Classes;


namespace HSPI_Nest_Thermostat_and_Camera_Plugin.Models
{
    class DeviceDataPoint
    {
        public int dvRef { get; set; }
        public DeviceClass device { get; set; }

        public DeviceDataPoint(int dvRef, DeviceClass device)
        {
            this.dvRef = dvRef;
            this.device = device;
        }
    }
}
