//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class XmlHelper
    {
        public static System.Xml.XmlElement AddElement( System.Xml.XmlDocument doc  ,
                                                        string                 name )
        {
            System.Xml.XmlElement subNode = doc.CreateElement( name );

            doc.AppendChild( subNode );

            return subNode;
        }

        public static System.Xml.XmlElement AddElement( System.Xml.XmlNode node ,
                                                        string             name )
        {
            System.Xml.XmlElement subNode = node.OwnerDocument.CreateElement( name );

            node.AppendChild( subNode );

            return subNode;
        }

        //--//

        public static System.Xml.XmlAttribute AddAttribute( System.Xml.XmlNode node  ,
                                                            string             name  ,
                                                            string             value )
        {
            System.Xml.XmlAttribute attrib = node.OwnerDocument.CreateAttribute( name );

            if(value != null)
            {
                attrib.Value = value;
            }

            node.Attributes.Append( attrib  );

            return attrib;
        }

        public static System.Xml.XmlAttribute AddAttribute( System.Xml.XmlNode node  ,
                                                            string             name  ,
                                                            bool               value )
        {
            return AddAttribute( node, name, value.ToString() );
        }

        public static System.Xml.XmlAttribute AddAttribute( System.Xml.XmlNode node  ,
                                                            string             name  ,
                                                            int                value )
        {
            return AddAttribute( node, name, value.ToString() );
        }

        public static System.Xml.XmlAttribute AddAttribute( System.Xml.XmlNode node  ,
                                                            string             name  ,
                                                            uint               value )
        {
            return AddAttribute( node, name, string.Format( "0x{0:X8}", value ) );
        }

        public static System.Xml.XmlAttribute AddAttribute( System.Xml.XmlNode node  ,
                                                            string             name  ,
                                                            DateTime           value )
        {
            return AddAttribute( node, name, value.ToString() );
        }

        public static System.Xml.XmlAttribute AddAttribute( System.Xml.XmlNode node  ,
                                                            string             name  ,
                                                            Guid               value )
        {
            return AddAttribute( node, name, value.ToString() );
        }

        //--//

        public static System.Xml.XmlAttribute FindAttribute( System.Xml.XmlNode node ,
                                                             string             name )
        {
            if(node != null)
            {
                foreach(System.Xml.XmlAttribute attrib in node.Attributes)
                {
                    if(attrib.Name == name)
                    {
                        return attrib;
                    }
                }
            }

            return null;
        }

        public static string GetAttribute( System.Xml.XmlNode node ,
                                           string             name )
        {
            System.Xml.XmlAttribute attrib = FindAttribute( node, name );
            if(attrib != null)
            {
                return attrib.Value;
            }

            return null;
        }

        public static bool GetAttribute( System.Xml.XmlNode node         ,
                                         string             name         ,
                                         bool               defaultValue )
        {
            string val = GetAttribute( node, name );
            bool   res;

            if(val != null && bool.TryParse( val, out res ))
            {
                return res;
            }

            return defaultValue;
        }

        public static int GetAttribute( System.Xml.XmlNode node         ,
                                        string             name         ,
                                        int                defaultValue )
        {
            string val = GetAttribute( node, name );
            int    res;

            if(val != null && int.TryParse( val, out res ))
            {
                return res;
            }

            return defaultValue;
        }

        public static uint GetAttribute( System.Xml.XmlNode node         ,
                                         string             name         ,
                                         uint               defaultValue )
        {
            string val = GetAttribute( node, name );
            uint   res;

            if(val != null)
            {
                if(uint.TryParse( val, out res ))
                {
                    return res;
                }

                if(val.StartsWith( "0x" ) && uint.TryParse( val.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, null, out res ))
                {
                    return res;
                }
            }

            return defaultValue;
        }

        public static Guid GetAttribute( System.Xml.XmlNode node         ,
                                         string             name         ,
                                         Guid               defaultValue )
        {
            string val = GetAttribute( node, name );

            if(val != null)
            {
                try
                {
                    return new Guid( val );
                }
                catch
                {
                }
            }

            return defaultValue;
        }

        public static DateTime GetAttribute( System.Xml.XmlNode node         ,
                                             string             name         ,
                                             DateTime           defaultValue )
        {
            string   val = GetAttribute( node, name );
            DateTime res;

            if(val != null && DateTime.TryParse( val, out res ))
            {
                return res;
            }

            return defaultValue;
        }
    }
}
