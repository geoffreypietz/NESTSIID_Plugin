using System;
using System.Collections;
using System.Collections.Generic;
using HomeSeerAPI;
using Scheduler.Classes;
using HSPI_Nest_Thermostat_and_Camera_Plugin.Models;

namespace HSPI_Nest_Thermostat_and_Camera_Plugin
{

    static class Util
    {

        // interface Status
        // for InterfaceStatus function call
        public const int ERR_NONE = 0;
        public const int ERR_SEND = 1;

        public const int ERR_INIT = 2;
        public static HomeSeerAPI.IHSApplication hs;
        public static HomeSeerAPI.IAppCallbackAPI callback;
        public const string IFACE_NAME = "Nest Thermostat and Camera Plugin";
        //public const string IFACE_NAME = "Sample Plugin";
        // set when SupportMultipleInstances is TRUE
        public static string Instance = "";
        public static string gEXEPath = "";

        public static bool gGlobalTempScaleF = true;
        public static SortedList colTrigs_Sync;
        public static SortedList colTrigs;
        public static SortedList colActs_Sync;

        public static SortedList colActs;





        public static bool StringIsNullOrEmpty(ref string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            return string.IsNullOrEmpty(s.Trim());
        }

        public enum LogType
        {
            LOG_TYPE_INFO = 0,
            LOG_TYPE_ERROR = 1,
            LOG_TYPE_WARNING = 2
        }

        public static void Log(string msg, LogType logType)
        {
            try
            {
                if (msg == null)
                    msg = "";
                if (!Enum.IsDefined(typeof(LogType), logType))
                {
                    logType = Util.LogType.LOG_TYPE_ERROR;
                }
                Console.WriteLine(msg);
                switch (logType)
                {
                    case LogType.LOG_TYPE_ERROR:
                        hs.WriteLog(Util.IFACE_NAME + " Error", msg);
                        break;
                    case LogType.LOG_TYPE_WARNING:
                        hs.WriteLog(Util.IFACE_NAME + " Warning", msg);
                        break;
                    case LogType.LOG_TYPE_INFO:
                        hs.WriteLog(Util.IFACE_NAME, msg);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in LOG of " + Util.IFACE_NAME + ": " + ex.Message);
            }

        }


        public static int MyDevice = -1;

        public static int MyTempDevice = -1;





        static internal List<DeviceDataPoint> Get_Device_List(List<DeviceDataPoint> deviceList)
        {
            // Gets relevant devices from HomeSeer
            DeviceClass dv = new DeviceClass();
            try
            {
                Scheduler.Classes.clsDeviceEnumeration EN = default(Scheduler.Classes.clsDeviceEnumeration);
                EN = (Scheduler.Classes.clsDeviceEnumeration)Util.hs.GetDeviceEnumerator();
                if (EN == null)
                    throw new Exception(IFACE_NAME + " failed to get a device enumerator from HomeSeer.");
                int dvRef;

                do
                {
                    dv = EN.GetNext();
                    if (dv == null)
                        continue;
                    if (dv.get_Interface(null) != IFACE_NAME)
                        continue;
                    dvRef = dv.get_Ref(null);
                    
                    var ddp = new DeviceDataPoint(dvRef, dv);
                    deviceList.Add(ddp);

                } while (!(EN.Finished));
            }
            catch (Exception ex)
            {
                Log("Exception in Get_Device_List: " + ex.Message, LogType.LOG_TYPE_ERROR);
            }

            return deviceList;
        }

        static internal void Update_ThermostatDevice(Thermostat thermostat, Structures structure, DeviceDataPoint ddPoint)
        {
            string name;
            string id = GetDeviceKeys(ddPoint.device, out name);

            /*
            if (name.Contains("thermostat"))    // if the device is a structure thermostat
                name = "thermostat";
            */
            switch (name)
            {
                case "Is Online":
                    {
                        if (thermostat.is_online)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                        }
                        break;
                    }

                case "HVAC Mode":
                    {
                        switch (thermostat.hvac_mode)
                        {
                            case "off":
                                hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                                break;
                            case "heat-cool":
                                hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                                break;
                            case "cool":
                                hs.SetDeviceValueByRef(ddPoint.dvRef, 2, true);
                                break;
                            case "heat":
                                hs.SetDeviceValueByRef(ddPoint.dvRef, 3, true);
                                break;
                            case "eco":
                                hs.SetDeviceValueByRef(ddPoint.dvRef, 4, true);
                                break;
                        }
                        break;
                    }
                case "Status":
                    {
                        hs.SetDeviceString(ddPoint.dvRef, thermostat.hvac_state, true);
                        break;
                    }
                case "Target Temperature":
                    {
                        if (thermostat.temperature_scale == "F")
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, thermostat.target_temperature_f, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, thermostat.target_temperature_c, true);
                        }

                        ddPoint.device.set_ScaleText(hs, thermostat.temperature_scale);

                        break;
                    }
                case "Target Temperature High":
                    {
                        double temp;
                        if (thermostat.temperature_scale == "F")
                        {
                            if (thermostat.hvac_mode.Equals("eco"))
                            {
                                temp = thermostat.eco_temperature_high_f;
                            }
                            else
                            {
                                temp = thermostat.target_temperature_high_f;
                            }
                        }
                        else
                        {
                            if (thermostat.hvac_mode.Equals("eco"))
                            {
                                temp = thermostat.eco_temperature_high_c;
                            }
                            else
                            {
                                temp = thermostat.target_temperature_high_c;
                            }
                        }
                        hs.SetDeviceValueByRef(ddPoint.dvRef, temp, true);
                        ddPoint.device.set_ScaleText(hs, thermostat.temperature_scale);

                        break;
                    }
                case "Target Temperature Low":
                    {
                        double temp;
                        if (thermostat.temperature_scale == "F")
                        {
                            if (thermostat.hvac_mode.Equals("eco"))
                            {
                                temp = thermostat.eco_temperature_low_f;
                            }
                            else
                            {
                                temp = thermostat.target_temperature_low_f;
                            }
                        }
                        else
                        {
                            if (thermostat.hvac_mode.Equals("eco"))
                            {
                                temp = thermostat.eco_temperature_low_c;
                            }
                            else
                            {
                                temp = thermostat.target_temperature_low_c;
                            }
                        }
                        hs.SetDeviceValueByRef(ddPoint.dvRef, temp, true);
                        ddPoint.device.set_ScaleText(hs, thermostat.temperature_scale);

                        break;
                    }
                case "Ambient Temperature":
                    {
                        if (thermostat.temperature_scale == "F")
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, thermostat.ambient_temperature_f, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, thermostat.ambient_temperature_c, true);
                        }
                        ddPoint.device.set_ScaleText(hs, thermostat.temperature_scale);

                        break;
                    }
                case "Humidity":
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, thermostat.humidity, true);
                        break;
                    }
                case "Battery Health":
                    {
                        if (!thermostat.is_using_emergency_heat)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                        }
                        break;
                    }
            }
        }

        static internal void Update_StructureDevice(Structures structure, DeviceDataPoint ddPoint)
        {
            switch (structure.away)
            {
                case "away":
                    hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                    break;
                case "home":
                    hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                    break;
            }
        }

        static internal void Update_CameraDevice(Camera camera, DeviceDataPoint ddPoint)
        {
            string name;
            string id = GetDeviceKeys(ddPoint.device, out name);

            switch (name)
            {
                case "Web Url":
                    {
                        hs.SetDeviceString(ddPoint.dvRef, camera.web_url, true);
                        break;
                    }
                case "App Url":
                    {
                        hs.SetDeviceString(ddPoint.dvRef, camera.app_url, true);
                        break;
                    }
                case "Is Online":
                    {
                        if (camera.is_online)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                        }
                        break;
                    }
                case "Is Streaming":
                    {
                        if (camera.is_streaming)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                        }
                        break;
                    }
            }

        }



        static internal bool Find_Create_Devices(Devices devices)
        {
            List<DeviceDataPoint> deviceList = new List<DeviceDataPoint>();

            deviceList = Get_Device_List(deviceList);

            var setAssociates = false;

            if (Find_Create_Thermostats(devices, deviceList))
            {
                setAssociates = true;
            }
            if (Find_Create_Cameras(devices, deviceList))
            {
                setAssociates = true;
            }

            if (setAssociates)
            {
                SetAssociatedDevices();

            }
            return true;
        }

        static internal bool Find_Create_Thermostats(Devices devices, List<DeviceDataPoint> deviceList)
        {
            bool create;
            bool associates = false;
            List<string> tStrings = getThermostatStrings();

            try
            {
                foreach (var thermostat in devices.thermostats)
                {
                    foreach (var tString in tStrings)
                    {
                        create = Thermostat_Devices(tString, thermostat.Value, null, deviceList);
                        if (create) // True if a device was created
                            associates = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception in Find_Create_Thermostats: " + ex.Message, LogType.LOG_TYPE_ERROR);
                System.IO.File.WriteAllText(@"Data/HSPI_Nest_Thermostat_and_Camera_Plugin/debug.txt", ex.ToString());
            }
            return associates;
        }

        static internal void Find_Create_Structures(Dictionary<string, Structures> structures)
        {
            List<DeviceDataPoint> deviceList = new List<DeviceDataPoint>();

            deviceList = Get_Device_List(deviceList);

            bool create;
            bool associates = false;

            try
            {
                foreach (var structure in structures)
                {
                    if (structure.Value.thermostats == null || structure.Value.thermostats.Capacity == 0)
                    {
                        continue;
                    }
                    foreach (var thermId in structure.Value.thermostats)
                    {
                        create = Structure_Devices(structure.Value, thermId, deviceList);
                        if (create) // True if a device was created
                            associates = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception in Find_Create_Structures: " + ex.Message, LogType.LOG_TYPE_ERROR);
                System.IO.File.WriteAllText(@"Data/HSPI_Nest_Thermostat_and_Camera_Plugin/debug.txt", ex.ToString());
            }

            if (associates)
            {
                SetAssociatedDevices();
            }
        }

        static internal bool Find_Create_Cameras(Devices devices, List<DeviceDataPoint> deviceList)
        {
            bool create;
            bool associates = false;
            List<string> cStrings = getCameraStrings();

            try
            {
                foreach (var camera in devices.cameras)
                {
                    foreach (var cString in cStrings)
                    {
                        create = Camera_Devices(cString, camera.Value, deviceList);
                        if (create) // True if a device was created
                            associates = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception in Find_Create_Cameras: " + ex.Message, LogType.LOG_TYPE_ERROR);
            }
            return associates;
        }

        static internal bool Thermostat_Devices(string tString, Thermostat thermostat, Structures structure, List<DeviceDataPoint> deviceList)
        {
            string name;
            string id;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name);
                if (id == thermostat.device_id && name == tString)
                {
                    Update_ThermostatDevice(thermostat, structure, ddPoint);
                    return false;
                }
            }

            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, tString, thermostat.name_long, thermostat.device_id, tString.Equals("Is Online"));
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name);
            switch (name)
            {
                case "Is Online":
                    {
                        DeviceTypeInfo_m.DeviceTypeInfo dt = new DeviceTypeInfo_m.DeviceTypeInfo();
                        dt.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        dt.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Root;
                        dv.set_DeviceType_Set(hs, dt);
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                        dv.set_Device_Type_String(hs, "Nest Root Thermostat");
                        dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 1;
                        SPair.Status = "Online";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.Value = 0;
                        SPair.Status = "Offline";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images/HomeSeer/contemporary/ok.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }
                case "HVAC Mode":
                    {
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        DeviceTypeInfo_m.DeviceTypeInfo dt = new DeviceTypeInfo_m.DeviceTypeInfo();
                        dt.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        dt.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Mode_Set;
                        dt.Device_SubType = 0;
                        dv.set_DeviceType_Set(hs, dt);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Render = Enums.CAPIControlType.Button;
                        SPair.Value = 0;
                        SPair.Status = "Off";
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 1;
                        SPair.ControlUse = ePairControlUse._ThermModeOff;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        if (thermostat.can_cool == true && thermostat.can_heat == true)
                        {
                            SPair.Value = 1;
                            SPair.Status = "Auto";
                            SPair.Render_Location.Row = 1;
                            SPair.Render_Location.Column = 2;
                            SPair.ControlUse = ePairControlUse._ThermModeAuto;
                            hs.DeviceVSP_AddPair(dvRef, SPair);

                            GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                            GPair.Set_Value = 1;
                            GPair.Graphic = "/images/HomeSeer/contemporary/auto-mode.png";
                            hs.DeviceVGP_AddPair(dvRef, GPair);
                        }

                        if (thermostat.can_cool == true)
                        {
                            SPair.Value = 2;
                            SPair.Status = "Cool";
                            SPair.Render_Location.Row = 1;
                            SPair.Render_Location.Column = 2;
                            SPair.ControlUse = ePairControlUse._ThermModeCool;
                            hs.DeviceVSP_AddPair(dvRef, SPair);

                            GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                            GPair.Set_Value = 2;
                            GPair.Graphic = "/images/HomeSeer/contemporary/cooling.png";
                            hs.DeviceVGP_AddPair(dvRef, GPair);
                        }

                        if (thermostat.can_heat == true)
                        {
                            SPair.Value = 3;
                            SPair.Status = "Heat";
                            SPair.Render_Location.Row = 1;
                            SPair.Render_Location.Column = 2;
                            SPair.ControlUse = ePairControlUse._ThermModeHeat;
                            hs.DeviceVSP_AddPair(dvRef, SPair);

                            GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                            GPair.Set_Value = 3;
                            GPair.Graphic = "/images/HomeSeer/contemporary/heating.png";
                            hs.DeviceVGP_AddPair(dvRef, GPair);
                        }

                        SPair.Value = 4;
                        SPair.Status = "Eco";
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 2;
                        SPair.ControlUse = ePairControlUse.Not_Specified;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 4;
                        GPair.Graphic = "/images/HomeSeer/contemporary/custom-color.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Target Temperature":
                case "Target Temperature High":
                case "Target Temperature Low":
                case "Ambient Temperature":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.RangeStart = -100;
                        SPair.RangeEnd = 150;
                        SPair.RangeStatusSuffix = " °" + VSVGPairs.VSPair.ScaleReplace;
                        SPair.HasScale = true;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        if (name.Equals("Target Temperature") || name.Equals("Target Temperature High") || name.Equals("Target Temperature Low"))
                        {
                            dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                            SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Control);
                            SPair.PairType = VSVGPairs.VSVGPairType.Range;
                            SPair.Render = Enums.CAPIControlType.TextBox_Number;
                            SPair.Render_Location.Row = 1;
                            SPair.Render_Location.Column = 1;
                            SPair.Status = "Enter target:";
                            SPair.RangeStart = 0;
                            SPair.RangeEnd = 100;
                            if (name.Equals("Target Temperature Low"))
                            {
                                SPair.ControlUse = ePairControlUse._HeatSetPoint;
                                DeviceTypeInfo_m.DeviceTypeInfo dt = new DeviceTypeInfo_m.DeviceTypeInfo();
                                dt.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                                dt.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Setpoint;
                                dt.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Setpoint.Heating_1;
                                dv.set_DeviceType_Set(hs, dt);
                            }
                            else if (name.Equals("Target Temperature High"))
                            {
                                SPair.ControlUse = ePairControlUse._CoolSetPoint;
                                DeviceTypeInfo_m.DeviceTypeInfo dt = new DeviceTypeInfo_m.DeviceTypeInfo();
                                dt.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                                dt.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Setpoint;
                                dt.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Setpoint.Cooling_1;
                                dv.set_DeviceType_Set(hs, dt);
                            }
                            hs.DeviceVSP_AddPair(dvRef, SPair);
                        }
                        else if(name.Equals("Ambient Temperature"))
                        {
                            DeviceTypeInfo_m.DeviceTypeInfo dt = new DeviceTypeInfo_m.DeviceTypeInfo();
                            dt.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                            dt.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Temperature;
                            dv.set_DeviceType_Set(hs, dt);
                        }

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = -100;
                        GPair.RangeEnd = 150;
                        GPair.Graphic = "/images/HomeSeer/contemporary/thermometer-70.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }

                case "Status":
                    {
                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Graphic = "/images/HomeSeer/contemporary/alarmheartbeat.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }

                case "Humidity":
                    {
                        DeviceTypeInfo_m.DeviceTypeInfo dt = new DeviceTypeInfo_m.DeviceTypeInfo();
                        dt.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        dt.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Temperature;
                        dt.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Temperature.Humidity;
                        dv.set_DeviceType_Set(hs, dt);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 100;
                        SPair.RangeStatusSuffix = " %";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 100;
                        GPair.Graphic = "/images/HomeSeer/contemporary/water.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }
                case "Battery Health":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 1;
                        SPair.Status = "Ok";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.Value = 0;
                        SPair.Status = "Battery Low";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images/HomeSeer/contemporary/battery_100.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/battery_25.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }
            }

            //Update_ThermostatDevice(thermostat, structure, dv, nest);


            return true;
        }

        static internal bool Structure_Devices(Structures structure, string thermId, List<DeviceDataPoint> deviceList)
        {
            string name;
            string id;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name);
                if (id == thermId && name == "Structure")
                {
                    Update_StructureDevice(structure, ddPoint);
                    return false;
                }
            }

            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, "Structure", structure.name, thermId, false);
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name);

            dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

            VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
            SPair = new VSVGPairs.VSPair(ePairStatusControl.Both);
            SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
            SPair.Render = Enums.CAPIControlType.Button;
 

            SPair.Value = 0;
            SPair.Status = "Away";
            SPair.Render_Location.Row = 1;
            SPair.Render_Location.Column = 2;
            hs.DeviceVSP_AddPair(dvRef, SPair);

            SPair.Value = 1;
            SPair.Status = "Home";
            SPair.Render_Location.Row = 1;
            SPair.Render_Location.Column = 2;
            hs.DeviceVSP_AddPair(dvRef, SPair);

            VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
            GPair.PairType = VSVGPairs.VSVGPairType.Range;
            GPair.RangeStart = 0;
            GPair.RangeEnd = 1;
            GPair.Graphic = "/images/HomeSeer/contemporary/home.png";
            hs.DeviceVGP_AddPair(dvRef, GPair);

            return true;
        }
        static internal bool Camera_Devices(string cString, Camera camera, List<DeviceDataPoint> deviceList)
        {
            string name;
            string id;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name);
                if (id == camera.device_id && name == cString)
                {
                    Update_CameraDevice(camera, ddPoint);
                    return false;
                }
            }

            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, cString, camera.name_long, camera.device_id, cString.Equals("Is Streaming"));
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name);
            switch (name)
            {
                case "Is Streaming":
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                        dv.set_Device_Type_String(hs, "Nest Root Camera");
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Render = Enums.CAPIControlType.Button;
                        SPair.Value = 1;
                        SPair.Status = "On";
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 1;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.Value = 0;
                        SPair.Status = "Off";
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 2;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images/HomeSeer/contemporary/on.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
            }

            //Update_CameraDevice(camera, dv, nest);


            return true;
        }

        static internal string GetDeviceKeys(DeviceClass dev, out string name)
        {
            string id = "";
            name = "";
            PlugExtraData.clsPlugExtraData pData = dev.get_PlugExtraData_Get(hs);
            if (pData != null)
            {
                id = (string)pData.GetNamed("id");
                name = (string)pData.GetNamed("name");
            }
            return id;
        }

        static internal void SetDeviceKeys(DeviceClass dev, string id, string name)
        {
            PlugExtraData.clsPlugExtraData pData = dev.get_PlugExtraData_Get(hs);
            if (pData == null)
                pData = new PlugExtraData.clsPlugExtraData();
            pData.AddNamed("id", id);
            pData.AddNamed("name", name);
            dev.set_PlugExtraData_Set(hs, pData);
        }

        static internal DeviceClass GenericHomeSeerDevice(DeviceClass dv, string dvName, string dvName_long, string device_id, bool root)
        {
            int dvRef;
            Log("Creating Device: " + dvName_long + " " + dvName, LogType.LOG_TYPE_INFO);
            var DT = new DeviceTypeInfo_m.DeviceTypeInfo();
            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            if (root)
            {
                DT.Device_Type = 99;
            }

            hs.NewDeviceRef(dvName_long + " " + dvName);
            dvRef = hs.GetDeviceRefByName(dvName_long + " " + dvName);
            dv = (DeviceClass)hs.GetDeviceByRef(dvRef);
            dv.set_Address(hs, "");
            SetDeviceKeys(dv, device_id, dvName);
            //dv.set_Code(hs, device_id + "-" + dvName_long + "-" + dvName);
            if (dvName_long.Contains("Camera"))
            {
                dv.set_Location(hs, "Camera");
            }
            else
            {
                dv.set_Location(hs, "Thermostat");
            }
            dv.set_Location2(hs, "Nest");
            dv.set_Interface(hs, IFACE_NAME);
            dv.set_Status_Support(hs, true);
            dv.set_Can_Dim(hs, false);
            dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
            dv.set_DeviceType_Set(hs, DT);
            dv.set_Relationship(hs, Enums.eRelationship.Child);
            dv.set_Device_Type_String(hs, "Nest Child Device");
            return dv;
        }


        private static void Default_VS_Pairs_AddUpdateUtil(int dvRef, VSVGPairs.VSPair Pair)
        {
            if (Pair == null)
                return;
            if (dvRef < 1)
                return;
            if (!hs.DeviceExistsRef(dvRef))
                return;

            VSVGPairs.VSPair Existing = null;

            // The purpose of this procedure is to add the protected, default VS/VG pairs WITHOUT overwriting any user added
            //   pairs unless absolutely necessary (because they conflict).

            try
            {
                Existing = hs.DeviceVSP_Get(dvRef, Pair.Value, Pair.ControlStatus);
                //VSPairs.GetPairByValue(Pair.Value, Pair.ControlStatus)


                if (Existing != null)
                {
                    // This is unprotected, so it is a user's value/Status pair.
                    if (Existing.ControlStatus == HomeSeerAPI.ePairStatusControl.Both & Pair.ControlStatus != HomeSeerAPI.ePairStatusControl.Both)
                    {
                        // The existing one is for BOTH, so try changing it to the opposite of what we are adding and then add it.
                        if (Pair.ControlStatus == HomeSeerAPI.ePairStatusControl.Status)
                        {
                            if (!hs.DeviceVSP_ChangePair(dvRef, Existing, HomeSeerAPI.ePairStatusControl.Control))
                            {
                                hs.DeviceVSP_ClearBoth(dvRef, Pair.Value);
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                            else
                            {
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                        }
                        else
                        {
                            if (!hs.DeviceVSP_ChangePair(dvRef, Existing, HomeSeerAPI.ePairStatusControl.Status))
                            {
                                hs.DeviceVSP_ClearBoth(dvRef, Pair.Value);
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                            else
                            {
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                        }
                    }
                    else if (Existing.ControlStatus == HomeSeerAPI.ePairStatusControl.Control)
                    {
                        // There is an existing one that is STATUS or CONTROL - remove it if ours is protected.
                        hs.DeviceVSP_ClearControl(dvRef, Pair.Value);
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                    }
                    else if (Existing.ControlStatus == HomeSeerAPI.ePairStatusControl.Status)
                    {
                        // There is an existing one that is STATUS or CONTROL - remove it if ours is protected.
                        hs.DeviceVSP_ClearStatus(dvRef, Pair.Value);
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                    }

                }
                else
                {
                    // There is not a pair existing, so just add it.
                    hs.DeviceVSP_AddPair(dvRef, Pair);

                }


            }
            catch (Exception)
            {
            }


        }

        // This is called at the end of device creation
        // It works by first finding the root device of the designated family (ie Device, Current Weather, Todays Forecast, Tomorrows Forecast)
        // Then, it finds the expected associates and adds them to the root (eg CurrentWeatherRootDevice.AssociateDevice_Add(hs, WindSpeedRef#))
        // TODO: ASSOCIATED DEVICES
        static internal void SetAssociatedDevices()
        {
            var deviceList = new List<DeviceDataPoint>();
            string name;
            string id;

            deviceList = Get_Device_List(deviceList);
            foreach (var ddPoint in deviceList)
            {

                id = GetDeviceKeys(ddPoint.device, out name);

                if (name == "Is Streaming")
                {
                    for (int i = 1; i < 3; i++)   // Now it's time to find the associate devices using the presumed addresses (IFACE_NAME-cString) in DeviceStrings[4-1]
                    {
                        foreach (var aDDPoint in deviceList)
                        {
                            string aName;
                            string aId = GetDeviceKeys(aDDPoint.device, out aName);
                            if (aId == id &&  aName == getCameraStrings()[i])
                            {
                                ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.dvRef);
                            }
                        }
                    }

                }
                if (name == "Is Online")
                {
                    for (int i = 1; i < 9; i++)   // Now it's time to find the associate devices using the presumed addresses (IFACE_NAME-cString) in DeviceStrings[4-1]
                    {
                        foreach (var aDDPoint in deviceList)
                        {
                            string aName;
                            string aId = GetDeviceKeys(aDDPoint.device, out aName);

                            if (aId == id && aName == getThermostatStrings()[i])
                            {
                                ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.dvRef);

                            }
                            if (aId == id && aName == "Structure")
                            {
                                ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.dvRef);
                            }
                        }
                    }

                }
            }
        }

        static internal string getTemperatureUnits(string units)
        {
            if (units.Equals("C"))
            {
                return "°C";
            }
            else
            {
                return "°F";
            }
        }

        // Gets the difference between event time(since) and now in minutes
        static internal double getTimeSince(double since)
        {
            since = getNowSinceEpoch() - since;
            since = Math.Round(since / 60);  // to minutes
            return since;
        }
        static internal double getNowSinceEpoch()
        {
            TimeSpan unixTime = DateTime.UtcNow - new DateTime(1970, 1, 1);
            double secondsSinceEpoch = (double)unixTime.TotalSeconds;
            return Math.Round(secondsSinceEpoch);
        }

        static internal List<string> getCameraStrings()
        {
            var cStrings = new List<string>
            {
                "Is Streaming",
                "Web Url",
                "App Url"
            };

            return cStrings;
        }

        static internal List<string> getThermostatStrings()
        {
            var tStrings = new List<string>
            {
                "Is Online",
                "Target Temperature",
                "Target Temperature High",
                "Target Temperature Low",
                "HVAC Mode",
                "Status",
                "Ambient Temperature",
                "Humidity",
                "Battery Health"
            };

            return tStrings;
        }
    }

}
