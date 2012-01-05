using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;

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
    /// This class handle the parsing of the <keys/> object in the settings config file.
    /// It also hold all the parsed data.
    /// </summary>
    class Keys : IEnumerable<Key>
    {
        List<Key> keys = new List<Key>();

        public void ReadFromXml(XmlReader reader)
        {
            reader.MoveToContent();
            if( reader.IsEmptyElement ) { reader.Read(); return; }

            reader.Read();
            while( ! reader.EOF )
            {
                if( reader.IsStartElement() )
                {
                    if( reader.Name == "key" )
                    {
                        Key key = new Key();
                        key.ReadFromXml( reader );
                        key.enforceType();
                        keys.Add( key );               
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
        }

        public int Count {
            get
            {
                return keys.Count;
            }
        }

        public IEnumerator<Key> GetEnumerator()
        {
            foreach (Key key in keys)
            {
                yield return key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
