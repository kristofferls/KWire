using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace KWire
{
    public static class Config
    {
                      
        public static string Ember_IP = null;
        public static int Ember_Port = 0;
        public static IPAddress AutoCam_IP = null;
        public static int AutoCam_Port = 0;
        public static int[] DeviceIDs; // depr. 
        public static bool Debug;
        public static string[,] EGPIs;
        public static List<string[]> Devices; 
        public static int Sources;
        public static string Ember_ProviderName;

        

        public static void ReadConfig()
        {


            string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
            string Filename = @"KWire_Config.xml";

            string ConfigFile = CurrentDir + @Filename;

            //Set 

            Logfile.Write("CONFIG :: Current XML Config file is: " + ConfigFile);
            Logfile.Write("-----------------------< Start reading config > --------------------------");

            //PARSE SITES.XML
            if (File.Exists(ConfigFile))
            {

                // Read XMl data from sites.xml 
                XmlDocument confFile = new XmlDocument();
                confFile.Load(ConfigFile);

                //Read config-data from xml to temp-vars
                XmlNode settingsNode = confFile.DocumentElement.SelectSingleNode("Settings");

                XmlNode ember_IP = settingsNode.SelectSingleNode("Ember_IP");
                XmlNode ember_Port = settingsNode.SelectSingleNode("Ember_Port");
                XmlNode autoCam_IP = settingsNode.SelectSingleNode("AutoCam_IP");
                XmlNode autoCam_Port = settingsNode.SelectSingleNode("AutoCam_Port");
                XmlNode debug = settingsNode.SelectSingleNode("Debug");
                XmlNode providerName = settingsNode.SelectSingleNode("Ember_ProviderName");

                //Resolve IPs and ports and store in memory for later use. 
                Ember_IP = ember_IP.InnerXml;
                Ember_Port = Convert.ToInt32(ember_Port.InnerXml);
                AutoCam_IP = Dns.GetHostAddresses(autoCam_IP.InnerXml)[0];
                AutoCam_Port = Convert.ToInt32(autoCam_Port.InnerXml);
                Ember_ProviderName = providerName.InnerXml;

                XmlDocument xml = new XmlDocument();
                xml.Load(ConfigFile);
                string xmlContents = xml.InnerXml;
                xml.LoadXml(xmlContents);


                //Get all <DEVICE>-tags, and put them into a string array of IDs.
                Devices = new List<string[]>();

                int numOfDevInCfg = XDocument.Load(ConfigFile).Descendants("Device").Count(); //Number of devices in file. 

                if(numOfDevInCfg != 0) 
                {
                    Sources = numOfDevInCfg; // update global number of devices. Used by Core. 
                    XmlNodeList aDevices = xml.SelectNodes("/KWire/AudioDevices/Device");

                    foreach (XmlNode xn in aDevices)
                    {
                        string order = xn["ORDER"].InnerText;
                        string name = xn["NAME"].InnerText;
                        string source = xn["SOURCE"].InnerText;
                        string devID = xn["DEVICE_ID"].InnerText;

                        string[] devs = { order, name, source,devID };
                        Devices.Add(devs);

                        if (devID.Length != 0) //If there is a Device_ID tag in config, make a note of this. 
                        {
                            Logfile.Write("CONFIG :: Added: <" + name + "> with source: " + source + " and order: " + order + " to device list");
                            Logfile.Write("CONFIG :: " + name + " has a DeviceID " + devID + " set in config. This will override name search");
                        }

                        else 
                        {
                            Logfile.Write("CONFIG :: Added: <" + name + "> with source: " + source + " and order: " + order + " to device list");
                        }
                        
                    }

                    Logfile.Write("CONFIG :: Found " + Convert.ToString(Devices.Count) + " audio inputs in config file.");
                } 
                else 
                {
                    Logfile.Write("CONFIG :: WARN :: Found no <DEVICE> tags under <AudioDevices>.. did you forget?");

                }
                    

                //Get all <EGPI>-tags, and put them into a temp List of objects.




                XmlNodeList eGPIList = xml.SelectNodes("/KWire/EmberGPIs/EGPI");

                if (eGPIList.Count != 0) 
                {
                
                               
                    EGPIs = new string[eGPIList.Count,2];


                    foreach(XmlNode xn in eGPIList) 
                    {
                        int id = Convert.ToInt32(xn["ID"].InnerText);
                        string name = xn["NAME"].InnerText;
                        
                        //Create EGPI Objects and store them in the EGPI object list in Core. 
                        Core.EGPIs.Add(new EGPI(id, name));
                        System.Threading.Thread.Sleep(1000); // Give the PowerCore some time to think before hammering it again... 
                    }


                }
                else
                {
                    Logfile.Write("CONFIG :: WARN :: Found no EGPI tags under <EmberGPIs> ..");
                }



                if (Debug != null)
                {
                    string debugNode = debug.InnerXml;
                    debugNode = debugNode.ToLower();

                    if (debugNode.Contains("false"))
                    {
                        Debug = false;
                        Logfile.Write("CONFIG :: Debug is currently not enabled.");
                    }
                    if (debugNode.Contains("true"))
                    {
                        Debug = true;
                        Logfile.Write("CONFIG :: Debug is set to ON. - feature not implemented yet - sorry");
                    }
                }
                else
                {
                    Logfile.Write("CONFIG :: Debug tag not found in config file!");
                    Debug = false;
                }

                Logfile.Write("-----------------------< Finished reading config > --------------------------");
            }
            else
            {
                Logfile.Write(" Cannot read file ");
            }// END OF XML parsing. 
        }


    }
}
