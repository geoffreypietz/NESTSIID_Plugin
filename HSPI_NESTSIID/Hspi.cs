﻿using System;
using System.Collections.Generic;
using HomeSeerAPI;
using HSPI_NESTSIID.Models;
using Scheduler.Classes;

namespace HSPI_NESTSIID
{
    public class HSPI : IPlugInAPI
    {
        // this API is required for ALL plugins


        public string OurInstanceFriendlyName = ""; //nice version of instance name
                                                    // a jquery web page
        public OptionsPage pluginpage;
        //public WebTestPage pluginTestPage;
        string WebPageName = Util.IFACE_NAME;

        public static bool bShutDown = false;

        private bool running;
        public static bool observeRunning = false;

        #region "Externally Accessible Methods/Procedures - e.g. Script Commands"

        #endregion

        #region "Common Interface"

        // For search demonstration purposes only.
        string[] Zone = new string[6];
        Scheduler.Classes.DeviceClass OneOfMyDevices = new Scheduler.Classes.DeviceClass();
        public HomeSeerAPI.SearchReturn[] Search(string SearchString, bool RegEx)//TODO add search
        {
            // Not yet implemented in the Sample
            //
            // Normally we would do a search on plug-in actions, triggers, devices, etc. for the string provided, using
            //   the string as a regular expression if RegEx is True.
            //
            List<SearchReturn> colRET = new List<SearchReturn>();
            SearchReturn RET;

            //So let's pretend we searched through all of the plug-in resources (triggers, actions, web pages, perhaps zone names, songs, etc.) 
            // and found a few matches....  

            //   The matches can be returned as just the string value...:
            RET = new SearchReturn();
            RET.RType = eSearchReturn.r_String_Other;
            RET.RDescription = "Found in the zone description for zone 4";
            RET.RValue = Zone[4];
            colRET.Add(RET);
            //   The matches can be returned as a URL:
            RET = new SearchReturn();
            RET.RType = eSearchReturn.r_URL;
            RET.RValue = Util.IFACE_NAME + Util.Instance;
            // Could have put something such as /DeviceUtility?ref=12345&edit=1     to take them directly to the device properties of a device.
            colRET.Add(RET);
            //   The matches can be returned as an Object:
            //   This will be VERY infrequently used as it is restricted to object types that can go through the HomeSeer-Plugin interface.
            //   Normal data type objects (Date, String, Integer, Enum, etc.) can go through, but very few complex objects such as the 
            //       HomeSeer DeviceClass will make it through the interface unscathed.
            RET = new SearchReturn();
            RET.RType = eSearchReturn.r_Object;
            RET.RDescription = "Found in a device.";
            RET.RValue = Util.hs.DeviceName(OneOfMyDevices.get_Ref(Util.hs));
            //Returning a string in the RValue is optional since this is an object type return
            RET.RObject = OneOfMyDevices;
            colRET.Add(RET);

            return colRET.ToArray();

        }


        // a custom call to call a specific procedure in the plugin
        public object PluginFunction(string proc, object[] parms) //Required method, default return value
        {

            return null;
        }

        public object PluginPropertyGet(string proc, object[] parms)//Required method, default return value
        {

            return null;
        }

        public void PluginPropertySet(string proc, object value)//Required method, default return value
        {

        }


        public string Name
        {
            get { return Util.IFACE_NAME; }
        }

        public int Capabilities() //Returns the capabilities needed
        {
            return (int)(HomeSeerAPI.Enums.eCapabilities.CA_IO);
        }

        // return 1 for a free plugin
        // return 2 for a licensed (for pay) plugin
        public int AccessLevel()
        {
            return 1;
        }

        public bool HSCOMPort
        {
            //We want HS to give us a com port number for accessing the hardware via a serial port
            get { return false; }
        }

        public bool SupportsMultipleInstances() //we don't right now
        {
            return false;
        }

        public bool SupportsMultipleInstancesSingleEXE()//we don't right now
        {
            return false;
        }

        public string InstanceFriendlyName()//getter for friendly name
        {
            return OurInstanceFriendlyName;
        }

        public System.Timers.Timer test_timer = new System.Timers.Timer();

        public void start_test_timer()
        {
            Console.WriteLine("Starting timer...");

            running = true;
            updateStatusValues();
            running = false;

            test_timer.Interval = 60000; // 60 sec frequency 

            Console.WriteLine("Time interval set to " + test_timer.Interval/1000 + " seconds");

            test_timer.Enabled = true;

            test_timer.Elapsed += test_timer_Elapsed;
        }

        private void test_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {          
                System.Threading.Tasks.Task.Factory.StartNew(() => updateStatusValues());        
        }

        #region - UpdateUI
        private void updateStatusValues()
        {
            try
            {
                if (!observeRunning) // Set True in NestConnection.connectNestData()
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() => RunObservableUpdate());
                }
                if (!running)
                {
                    using (var nest = new NestConnection())
                    {
                        nest.setInitialConnectionProps();
                        if (nest.isAccessNotNull())
                        {

                            running = true;
                            using (var nestData = nest.getNestData())
                            {
                                Util.Find_Create_Devices(nestData.Devices);
                                Util.Find_Create_Structures(nestData.Structures);
                            }
                            running = false;
                        }

                        else
                        {
                            Util.Log( "Access Token is null", Util.LogType.LOG_TYPE_ERROR);
                            running = false;
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Failed: No API Key?");
                Util.Log(ex.ToString(), Util.LogType.LOG_TYPE_ERROR);
                System.IO.File.WriteAllText(@"Data/hspi_nestsiid/debug.txt", ex.ToString());
                if(ex.ToString().Contains("connectNestData"))
                {
                    observeRunning = false;
                }
                running = false;
            }
        }
        #endregion

        public void RunObservableUpdate()
        {
            
            using (var nest = new NestConnection())
            {
                nest.setInitialConnectionProps();

                if (nest.isAccessNotNull())
                {
                    nest.connectNestData();  
                }
            }
        }

#if PlugDLL //Internal, leave as is
		// These 2 functions for internal use only
		public HomeSeerAPI.IHSApplication HSObj {
			get { return hs; }
			set { hs = value; }
		}

		public HomeSeerAPI.IAppCallbackAPI CallBackObj {
			get { return callback; }
			set { callback = value; }
		}
#endif

        public string InitIO(string port) //Init plugin
        {
            Console.WriteLine("InitIO called with parameter port as " + port);

            string[] plugins = Util.hs.GetPluginsList();
            Util.gEXEPath = Util.hs.GetAppPath();

            try
            {

                // create our jquery web page
                pluginpage = new OptionsPage(WebPageName);
                // register the page with the HS web server, HS will post back to the WebPage class
                // "pluginpage" is the URL to access this page
                // comment this out if you are going to use the GenPage/PutPage API istead
                if (string.IsNullOrEmpty(Util.Instance))
                {

                    Util.hs.RegisterPage(Util.IFACE_NAME, Util.IFACE_NAME, Util.Instance);
                }
                else
                {
                    Util.hs.RegisterPage(Util.IFACE_NAME + Util.Instance, Util.IFACE_NAME, Util.Instance);
                }
                Console.WriteLine("Creates options page...");
                WebPageDesc wpd = new WebPageDesc();
                // create test page


                // register a normal page to appear in the HomeSeer menu

                wpd.link = Util.IFACE_NAME + Util.Instance;
                if (!string.IsNullOrEmpty(Util.Instance))
                {
                    wpd.linktext = Util.IFACE_NAME + " Page instance " + Util.Instance;
                }
                else
                {
                    wpd.linktext = Util.IFACE_NAME + " Status/Options";
                }
                wpd.page_title = Util.IFACE_NAME + " Status/Options";
                wpd.plugInName = Util.IFACE_NAME;
                wpd.plugInInstance = Util.Instance;
                Util.callback.RegisterLink(wpd);

                // register a normal page to appear in the HomeSeer menu

                // init a speak proxy
                //Util.callback.RegisterProxySpeakPlug(Util.IFACE_NAME, "")

                // register a generic Util.callback for other plugins to raise to use
                Util.callback.RegisterGenericEventCB("sample_type", Util.IFACE_NAME, "");

                Util.Log( "InitIO called, plug-in is being initialized...", Util.LogType.LOG_TYPE_INFO);



                // register for events from homeseer if a device changes value
                Util.callback.RegisterEventCB(Enums.HSEvent.VALUE_CHANGE, Util.IFACE_NAME, "");

                start_test_timer();

                //Util.hs.SaveINISetting("Settings", "test", null, "hspi_HSTouch.ini");

                // example of how to save a file to the HS images folder, mainly for use by plugins that are running remotely, album art, etc.
                //SaveImageFileToHS(gEXEPath & "\html\images\browser.png", "sample\browser.png")//TODO look here
                //SaveFileToHS(gEXEPath & "\html\images\browser.png", "sample\browser.png")
            }
            catch (Exception ex)
            {
                bShutDown = true;
                return "Error on InitIO: " + ex.Message;
            }

            bShutDown = false;
            return "";
            // return no error, or an error message

        }


        public string ConfigDevice(int dvRef, string user, int userRights, bool newDevice)//TODO Search for the call things in different pages
        {

            return "";
        }

        public Enums.ConfigDevicePostReturn ConfigDevicePost(int dvRef, string data, string user, int userRights)//TODO DIDO ABOVE, seach for form
        {

            return Enums.ConfigDevicePostReturn.DoneAndCancelAndStay;
        }

        // Web Page Generation - OLD METHODS These are required
        // ================================================================================================
        public string GenPage(string link)
        {
            return "Generated from GenPage in plugin " + Util.IFACE_NAME;
        }
        public string PagePut(string data)
        {
            return "";
        }
        // ================================================================================================

        // Web Page Generation - NEW METHODS
        // ================================================================================================
        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            //If you have more than one web page, use pageName to route it to the proper GetPagePlugin
            Console.WriteLine("GetPagePlugin pageName: " + pageName);
            // get the correct page
            switch (pageName)
            {
                case Util.IFACE_NAME:
                    return (pluginpage.GetPagePlugin(pageName, user, userRights, queryString));
            }
            return "page not registered";
        }

        public string PostBackProc(string pageName, string data, string user, int userRights)//Required, but not sure I need it
        {
            //If you have more than one web page, use pageName to route it to the proper postBackProc
            switch (pageName)
            {
                case Util.IFACE_NAME:
                    return pluginpage.postBackProc(pageName, data, user, userRights);
            }

            return "";
        }

        // ================================================================================================

        public void HSEvent(Enums.HSEvent EventType, object[] parms)
        {
            //Don't need anything
        }



        public HomeSeerAPI.IPlugInAPI.strInterfaceStatus InterfaceStatus()
        {
            IPlugInAPI.strInterfaceStatus es = new IPlugInAPI.strInterfaceStatus();
            es.intStatus = IPlugInAPI.enumInterfaceStatus.OK;
            return es;
        }

        public IPlugInAPI.PollResultInfo PollDevice(int dvref)
        {

            IPlugInAPI.PollResultInfo ri = default(IPlugInAPI.PollResultInfo);
            if (true)
            {
                if (true)
                {
                    ri.Result = IPlugInAPI.enumPollResult.OK;
                }
                else
                {
                    ri.Result = IPlugInAPI.enumPollResult.Could_Not_Reach_Plugin;
                }
            }
            else
            {
                ri.Result = IPlugInAPI.enumPollResult.Device_Not_Found;
            }

            return ri;
        }

        public bool RaisesGenericCallbacks()
        {
            return true;
        }

        public void SetIOMulti(System.Collections.Generic.List<HomeSeerAPI.CAPI.CAPIControl> colSend)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() => NestControls(colSend));
        }

        public void NestControls(List<HomeSeerAPI.CAPI.CAPIControl> colSend)
        {
            foreach (var CC in colSend)
            {
                try
                {
                    Util.Log("SetIOMulti set value: " + CC.ControlValue.ToString() + "->ref:" + CC.Ref.ToString(), Util.LogType.LOG_TYPE_INFO);
                    using (var nest = new NestConnection())
                    {
                        nest.setInitialConnectionProps();
                        DeviceClass dv = (DeviceClass)Util.hs.GetDeviceByRef(CC.Ref);

                        string name;
                        string id = Util.GetDeviceKeys(dv, out name);

                        if (name == "Is Streaming")
                        {
                            if (CC.Label == "On")
                            {
                                nest.setApiJson("{\"is_streaming\": true}", "devices/cameras", id);
                                //Util.hs.SetDeviceValueByRef(CC.Ref, 1, true);
                            }
                            if (CC.Label == "Off")
                            {
                                nest.setApiJson("{\"is_streaming\": false}", "devices/cameras", id);
                                //Util.hs.SetDeviceValueByRef(CC.Ref, 0, true);
                            }
                        }
                        if (name == "HVAC Mode")
                        {
                            if (CC.Label.Equals("Auto"))
                            {
                                nest.setApiJson("{\"hvac_mode\": \"" + "heat-cool" + "\"}", "devices/thermostats", id);
                            }
                            else
                            {
                                nest.setApiJson("{\"hvac_mode\": \"" + CC.Label.ToLower() + "\"}", "devices/thermostats", id);
                            }

                            /*
                            if (CC.Label.Equals("Off"))
                            {
                                Util.hs.SetDeviceValueByRef(CC.Ref, 0, true);
                            }
                            if (CC.Label.Equals("Auto"))
                            {
                                Util.hs.SetDeviceValueByRef(CC.Ref, 1, true);
                            }
                            if (CC.Label.Equals("Cool"))
                            {
                                Util.hs.SetDeviceValueByRef(CC.Ref, 2, true);
                            }
                            if (CC.Label.Equals("Heat"))
                            {
                                Util.hs.SetDeviceValueByRef(CC.Ref, 3, true);
                            }
                            if (CC.Label.Equals("Eco"))
                            {
                                Util.hs.SetDeviceValueByRef(CC.Ref, 4, true);
                            }
                            */
                        }

                        if (name.Contains("Temperature"))
                        {
                            using (var nestData = nest.getNestData())
                            {
                                var currentTemp = dv.get_devValue(Util.hs);

                                var targetWithScale = "";
                                foreach (var thermostat in nestData.Devices.thermostats)
                                {
                                    //Console.WriteLine(thermostat.Value.hvac_mode);
                                    //Console.WriteLine(id[5]);

                                    if (!thermostat.Value.hvac_mode.Equals("eco"))
                                    {
                                        if (name == "Target Temperature")
                                        {
                                            if (thermostat.Value.temperature_scale.Equals("F"))
                                            {
                                                targetWithScale = "target_temperature_f";
                                            }
                                            else
                                            {
                                                targetWithScale = "target_temperature_c";
                                            }

                                        }
                                        if (name == "Target Temperature Low")
                                        {
                                            if (thermostat.Value.temperature_scale.Equals("F"))
                                            {
                                                targetWithScale = "target_temperature_low_f";
                                                currentTemp = thermostat.Value.target_temperature_low_f;
                                            }
                                            else
                                            {
                                                targetWithScale = "target_temperature_low_c";
                                                currentTemp = thermostat.Value.target_temperature_low_c;
                                            }
                                        }
                                        if (name == "Target Temperature High")
                                        {
                                            if (thermostat.Value.temperature_scale.Equals("F"))
                                            {
                                                targetWithScale = "target_temperature_high_f";
                                                currentTemp = thermostat.Value.target_temperature_high_f;
                                            }
                                            else
                                            {
                                                targetWithScale = "target_temperature_high_c";
                                                currentTemp = thermostat.Value.target_temperature_high_c;
                                            }
                                        }
                                    }
                                    else
                                    {

                                        if (name == "Target Temperature Low")
                                        {
                                            if (thermostat.Value.temperature_scale.Equals("F"))
                                            {
                                                targetWithScale = "eco_temperature_low_f";
                                                currentTemp = thermostat.Value.eco_temperature_low_f;
                                            }
                                            else
                                            {
                                                targetWithScale = "eco_temperature_low_c";
                                                currentTemp = thermostat.Value.eco_temperature_low_c;
                                            }
                                        }
                                        if (name == "Target Temperature High")
                                        {
                                            if (thermostat.Value.temperature_scale.Equals("F"))
                                            {
                                                targetWithScale = "eco_temperature_high_f";
                                                currentTemp = thermostat.Value.eco_temperature_high_f;
                                            }
                                            else
                                            {
                                                targetWithScale = "eco_temperature_high_c";
                                                currentTemp = thermostat.Value.eco_temperature_high_c;
                                            }
                                        }
                                    }
                                }
                                nest.setApiJson("{\"" + targetWithScale + "\": " + CC.ControlValue + "}", "devices/thermostats", id);
                            }
                        }
                        if (name == "Structure")
                        {
                            var structId = "";
                            using (var nestData = nest.getNestData())
                            {
                                foreach (var structure in nestData.Structures)
                                {
                                    if (structure.Value.thermostats.Contains(id))
                                    {
                                        structId = structure.Value.structure_id;
                                    }
                                }
                                string jsonValue = "";

                                if (CC.Label == "Away")
                                {
                                    jsonValue = "away";
                                    //Util.hs.SetDeviceValueByRef(CC.Ref, 0, true);
                                }
                                if (CC.Label == "Home")
                                {
                                    jsonValue = "home";
                                    //Util.hs.SetDeviceValueByRef(CC.Ref, 1, true);
                                }
                                Console.WriteLine("{\"away\": \"" + jsonValue + "\"}");

                                nest.setApiJson("{\"away\": \"" + jsonValue + "\"}", "structures", structId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Log(e.ToString(), Util.LogType.LOG_TYPE_ERROR);
                }
            }
            updateStatusValues();//after sending a command, update with the new status
        }

        public void setHighLowTemps()
        {
            
        }

        public void ShutdownIO()
        {
            // do your shutdown stuff here

            bShutDown = true;
            // setting this flag will cause the plugin to disconnect immediately from HomeSeer
        }

        public bool SupportsConfigDevice()
        {
            return true;
        }

        public bool SupportsConfigDeviceAll()
        {
            return false;
        }

        public bool SupportsAddDevice()
        {
            return false;
        }


        #endregion

        #region "Actions Interface"

        public int ActionCount()
        {
            return 0;
        }

        private bool mvarActionAdvanced;
        public bool ActionAdvancedMode
        {
            get { return mvarActionAdvanced; }
            set { mvarActionAdvanced = value; }
        }

        public string ActionBuildUI(string sUnique, IPlugInAPI.strTrigActInfo ActInfo)
        {

            return "";
        }

        public bool ActionConfigured(IPlugInAPI.strTrigActInfo ActInfo)
        {

            return false;
        }

        public bool ActionReferencesDevice(IPlugInAPI.strTrigActInfo ActInfo, int dvRef)
        {

            return false;
        }

        public string ActionFormatUI(IPlugInAPI.strTrigActInfo ActInfo)
        {
            return "";
        }

        public string get_ActionName(int ActionNumber)
        {

            return "";
        }

        public IPlugInAPI.strMultiReturn ActionProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, IPlugInAPI.strTrigActInfo ActInfoIN)
        {

            return new IPlugInAPI.strMultiReturn();

        }

        public bool HandleAction(IPlugInAPI.strTrigActInfo ActInfo)
        {


            return false;

        }

        #endregion

        #region "Trigger Interface"

        /// <summary>
        /// Indicates (when True) that the Trigger is in Condition mode - it is for triggers that can also operate as a condition
        ///    or for allowing Conditions to appear when a condition is being added to an event.
        /// </summary>
        /// <param name="TrigInfo">The event, group, and trigger info for this particular instance.</param>
        /// <value></value>
        /// <returns>The current state of the Condition flag.</returns>
        /// <remarks></remarks>
        public bool get_Condition(IPlugInAPI.strTrigActInfo TrigInfo)
        {

            return false;
        }
        public void set_Condition(IPlugInAPI.strTrigActInfo TrigInfo, bool Value)
        {

        }

        public bool get_HasConditions(int TriggerNumber)
        {

            return false;
        }

        public bool HasTriggers
        {
            get { return false; }
        }

        public int TriggerCount
        {
            get { return 0; }
        }

        public string get_TriggerName(int TriggerNumber)
        {

            return "";
        }

        public int get_SubTriggerCount(int TriggerNumber)
        {

            return 0;
        }

        public string get_SubTriggerName(int TriggerNumber, int SubTriggerNumber)
        {

            return "";
        }

        public string TriggerBuildUI(string sUnique, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {

            return "";

        }

        public bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo TrigInfo)
        {

            return false;
        }

        public bool TriggerReferencesDevice(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo, int dvRef)
        {

            return false;
        }

        public string TriggerFormatUI(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {

            return "";
        }

        public HomeSeerAPI.IPlugInAPI.strMultiReturn TriggerProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfoIn)
        {

            return new HomeSeerAPI.IPlugInAPI.strMultiReturn();

        }

        public bool TriggerTrue(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {

            return false;

        }

        #endregion



        public enum enumTAG
        {
            Unknown = 0,
            Trigger = 1,
            Action = 2,
            Group = 3
        }

        public struct EventWebControlInfo
        {
            public bool Decoded;
            public int EventTriggerGroupID;
            public int GroupID;
            public int EvRef;
            public int TriggerORActionID;
            public string Name_or_ID;
            public string Additional;
            public enumTAG TrigActGroup;
        }

        static internal EventWebControlInfo U_Get_Control_Info(string sIN)
        {

            return new EventWebControlInfo();
        }
        internal bool ValidTrigInfo(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {

            return false;
        }
        internal bool ValidActInfo(HomeSeerAPI.IPlugInAPI.strTrigActInfo ActInfo)
        {

            return false;
        }
        internal bool ValidTrig(int TrigIn)
        {

            return false;
        }
        internal bool ValidAct(int ActIn)
        {

            return false;
        }
        internal bool ValidSubTrig(int TrigIn, int SubTrigIn)
        {

            return false;
        }
        internal bool ValidSubAct(int ActIn, int SubActIn)
        {

            return false;
        }
        public HSPI() : base()//TODO make connections
        {
            // Create a thread-safe collection by using the .Synchronized wrapper.
            Util.colTrigs_Sync = new System.Collections.SortedList();
            Util.colTrigs = System.Collections.SortedList.Synchronized(Util.colTrigs_Sync);

            Util.colActs_Sync = new System.Collections.SortedList();
            Util.colActs = System.Collections.SortedList.Synchronized(Util.colActs_Sync);

        }


        // called if speak proxy is installed
        public void SpeakIn(int device, string txt, bool w, string host)
        {
            Console.WriteLine("Speaking from HomeSeer, txt: " + txt);
            // speak back
            Util.hs.SpeakProxy(device, txt + " the plugin added this", w, host);
        }

        // save an image file to HS, images can only be saved in a subdir of html\images so a subdir must be given
        // save an image object to HS
        private void SaveImageFileToHS(string src_filename, string des_filename)//TODO possibly use these for OAuth2
        {
            System.Drawing.Image im = System.Drawing.Image.FromFile(src_filename);
            Util.hs.WriteHTMLImage(im, des_filename, true);
        }

        // save a file as an array of bytes to HS
        private void SaveFileToHS(string src_filename, string des_filename)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(src_filename);
            if (bytes != null)
            {
                Util.hs.WriteHTMLImageFile(bytes, des_filename, true);
            }
        }
    }
}
