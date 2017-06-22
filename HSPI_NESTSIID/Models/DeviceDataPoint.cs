using Scheduler.Classes;


namespace HSPI_NESTSIID.Models
{
    class DeviceDataPoint
    {
        public int dvRef { get; set; }
        public string address { get; set; }
        public DeviceClass device { get; set; }

        public DeviceDataPoint(int dvRef, string address, DeviceClass device)
        {
            this.dvRef = dvRef;
            this.address = address;
            this.device = device;
        }
    }
}
