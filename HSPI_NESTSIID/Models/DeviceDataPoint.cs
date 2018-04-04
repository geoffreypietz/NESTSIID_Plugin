using Scheduler.Classes;


namespace HSPI_NESTSIID.Models
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
