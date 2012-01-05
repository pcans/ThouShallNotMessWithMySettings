using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

/*
 *  Copyright 2012 pascal cans
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

namespace ThouShallNotMessWithMySettings
{
    /// <summary>
    /// Console application that will set some registry keys and
    /// change it back if another process is changing them.
    /// Keys to watch and values to set are done using a settings.xml file found in the current folder.
    /// </summary>
    class Program
    {
        /// <summary>
        /// name of the config file to read
        /// </summary>
        const String DEFAULT_PARAM = "settings.xml";

        /// <summary>
        /// the data that we read from the config file
        /// </summary>
        private static Keys keys = new Keys();

        /// <summary>
        /// the path to the config file
        /// </summary>
        private static string settingsPath;
        
        /// <summary>
        /// Main method of the console app
        /// </summary>
        /// <param name="args">you can provide a filename that will be use to read settings from</param>
        static void Main(string[] args)
        {
            ParseArguments(args);

            //ensure settings.xml file exists
            EnsureSettingsExists();

            //validate settings file
            ValidateSettings();

            //parse config file
            ParseKeys();

            //show to the user we understood everything.
            DisplayKeys();
            
            //set values
            ApplyKeys();
            
            //monitor keys
            SetKeyMonitors();

            //wait 'til 21/12/2012
            while (true) {
                System.Threading.Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// Parse arguments
        /// </summary>
        /// <param name="args">you can provide a filename that will be use to read settings from</param>
        static private void ParseArguments(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "/h")
                {
                    //usage
                    Console.WriteLine("Description: ");
                    Console.WriteLine("This application will read a config file with registry keys.");
                    Console.WriteLine("If the settings.xml doesn't exist, one will be created for you.");
                    Console.WriteLine("It will then save thoses keys to the registry and then monitors them to detect changes on them. Every time a change is detected, the values will be set back to the specified values.");
                    Console.WriteLine();
                    Console.WriteLine("Usage: ");
                    Console.WriteLine("         /h Display this message");
                    Console.WriteLine("         filename.xml The filename to read keys from. default value if not specified is settings.xml");
                    Tools.Harakiri();
                }
                else
                {
                    settingsPath = Environment.CurrentDirectory + "\\" + args[0];
                }
            }
            else
            {
                settingsPath = Environment.CurrentDirectory + "\\" + DEFAULT_PARAM;
            }
        }

        /// <summary>
        /// Check if settings file exists, create one for the user.
        /// Method will exists the console app if no settings file is found.
        /// </summary>
        static private void EnsureSettingsExists()
        {
            if (!File.Exists(settingsPath))
            {
                Console.WriteLine("Unable to read setting file: " + settingsPath);
                bool reply = Tools.ReadConsoleBool("would you like me to create an empty one for you?");
                if (reply)
                {
                    Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    //reflection, cuz we are in static world.
                    string xmlResouce = assembly.GetName().Name + ".settings-example.xml";

                    using (Stream input = assembly.GetManifestResourceStream(xmlResouce))
                    using (Stream output = File.Create(settingsPath))
                    {
                        Tools.CopyStream(input, output);
                    }
                    Console.WriteLine("The config file has been created, you can edit it and restart me.");
                }
                Tools.Harakiri();
            }
        }

        /// <summary>
        /// Validate the config file using the embedded xsd.
        /// If errors are found, display the error to the user, then quit.
        /// </summary>
        static private void ValidateSettings()
        {
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //reflection, cuz we are in static world.
            string xmlResouce = assembly.GetName().Name + ".settings.xsd";
            using (Stream streamXsd = assembly.GetManifestResourceStream(xmlResouce))
            {
                using (StreamReader xsdReader = new StreamReader(streamXsd))
                {
                    XmlSchema schema = new XmlSchema();
                    schema = XmlSchema.Read(xsdReader, new ValidationEventHandler(XSDValidationEventHandler));
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.ValidationType = ValidationType.Schema;
                    readerSettings.Schemas.Add(schema);
                    readerSettings.ValidationEventHandler += new ValidationEventHandler(XMLValidationEventHandler);
                    using (XmlTextReader xmlTextReader = new XmlTextReader(settingsPath))
                    {
                        using (XmlReader reader = XmlReader.Create(xmlTextReader, readerSettings))
                        {
                            while (reader.Read()) { }
                            reader.Close();
                        }
                        xmlTextReader.Close();
                    }
                    xsdReader.Close();
                }
                streamXsd.Close();
            }
        }

        /// <summary>
        /// Read the config file. all data can then be retreived from the <see cref="keys"/> variable.
        /// </summary>
        static private void ParseKeys()
        {            
            //read keys from file
            using (XmlReader reader = new XmlTextReader(settingsPath))
            {
                reader.Read();
                while (!reader.EOF)
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "keys")
                        {
                            keys.ReadFromXml(reader);
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                    else
                    {
                        reader.Read();
                        break;
                    }
                }
                reader.Close();
            }
            Console.WriteLine("file " + settingsPath + " has been read. " + keys.Count + " entrie(s) found.");
            Console.WriteLine();
        }

        /// <summary>
        /// register all the monitors that must watch registry entries.
        /// </summary>
        static private void SetKeyMonitors()
        {
            List<String> monitoredPaths = new List<string>();
            foreach (Key key in keys)
            {
                if (!monitoredPaths.Contains(key.path))
                {
                    try
                    {
                        RegistryMonitor monitor = new RegistryMonitor(key.path);
                        monitor.RegChanged += new EventHandler(OnRegChanged);
                        monitor.Start();
                        Console.WriteLine("Monitoring " + key.path);
                        monitoredPaths.Add(key.path);

                    } catch (ArgumentException ae)
                    {
                        Console.WriteLine("Error: Unable to monitor the key " + key.path + "\\" + key.name + " " + ae.Message);
                    }
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// simply display all the keys data from the <see cref="keys"/> variable to the screen
        /// </summary>
        static private void DisplayKeys()
        {
            foreach (Key key in keys)
            {
                Console.WriteLine(key.path + "\\" + key.name + " = " + key.valueRaw + " (" + key.type + ")");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// apply all the keys to the registry.
        /// </summary>
        static private void ApplyKeys()
        {
            Console.WriteLine("Setting keys to new values.");
            foreach (Key key in keys)
            {
                try
                {
                    Registry.SetValue(key.path, key.name, key.value, key.type);
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine("Error: Unable to set key " + key.path + "\\" + key.name + " " + ae.Message);
                }
            }
            Console.WriteLine();

        }
        
        /// <summary>
        /// Event listener called if the registry has been updated by another process.
        /// </summary>
        /// <param name="sender">the monitor that fired the events</param>
        /// <param name="e">always null</param>
        static private void OnRegChanged(object sender, EventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("dd/MM HH:mm") + " Someone trying to mess with my settings? no-no.");
            ApplyKeys();
        }

        /// <summary>
        /// Event listener called when there is xsd validation errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">the exception fired by the parser</param>
        private static void XSDValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Console.WriteLine("XML validation Error: " + e.Message);
            Tools.Harakiri();
        }

        /// <summary>
        /// Event listener called when there is xml validation errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">the exception fired by the parser</param>
        private static void XMLValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Console.WriteLine("XML Error: " + e.Message);
            Tools.Harakiri();
        }

    }
}
