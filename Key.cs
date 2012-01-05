using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

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
    /// This class handle the parsing of the <key/> objects in the settings config file.
    /// It also hold all the parsed data.
    /// </summary>
    public class Key
    {
        public string path;
        public string name;
        public RegistryValueKind type;
        public string valueRaw;
        public object value;

        public void ReadFromXml(XmlReader reader)
        {
            reader.MoveToContent();
            if( reader.IsEmptyElement ) { reader.Read(); return; }
            reader.Read();
            while( ! reader.EOF )
            {
                if( reader.IsStartElement() )
                {
                    switch( reader.Name )
                    {
                        case "path":
                            path = reader.ReadElementContentAsString();
                            break;
                        case "name":
                            name = reader.ReadElementContentAsString();
                            break;
                        case "value":
                            valueRaw = reader.ReadElementContentAsString();
                            break;
                        case "type":
                            string valueTxt = reader.ReadElementContentAsString();
                            type = RegistryValueKind.Unknown;
                            switch (valueTxt)
                            {
                                case "REG_DWORD":
                                    type = RegistryValueKind.DWord;
                                    break;
                                case "REG_BINARY":
                                    type = RegistryValueKind.Binary;
                                    break;
                                case "REG_SZ":
                                    type = RegistryValueKind.String;
                                    break;
                            }
                
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
                else
                {
                    reader.Read();
                    break;
                }
            }       
        }

        public void enforceType()
        {
            switch (type) {
                case RegistryValueKind.String:
                    value = valueRaw;
                    break;
                case RegistryValueKind.Binary:
                    //remove everything that is not pure ascii hex.
                    string hex = Regex.Replace(valueRaw, "[^0-9ABCDEFabcdef]", "");
                    //then convert it back to bytes.
                    Byte[] valueTyped = new Byte[hex.Length/2];
                    for (int i = 0; i < hex.Length; i += 2)
                    {
                        string hs = hex.Substring(i, 2);
                        valueTyped[i/2] = Convert.ToByte(hs, 16);
                    }
                    value = valueTyped;
                    break;
                case RegistryValueKind.DWord:
                    value = Convert.ToUInt32(valueRaw);
                    break;
            }
        }
    }

}
