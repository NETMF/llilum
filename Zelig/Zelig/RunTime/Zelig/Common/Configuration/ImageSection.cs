//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public class ImageSection
    {
        //
        // State
        //

        private uint                     m_address;
        private byte[]                   m_payload;
        private string                   m_sectionName;
        private Runtime.MemoryAttributes m_attributes;
        private Runtime.MemoryUsage      m_usage;

        //
        // Constructor Methods
        //

        public ImageSection( uint                     address     ,
                             byte[]                   payload     ,
                             string                   sectionName ,
                             Runtime.MemoryAttributes attributes  ,
                             Runtime.MemoryUsage      usage       )
        {
            m_address     = address;
            m_payload     = payload;
            m_sectionName = sectionName;
            m_attributes  = attributes;
            m_usage       = usage;
        }

        //
        // Helper Methods
        //

        public bool InRange( uint address )
        {
            return (m_address <= address && address < m_address + m_payload.Length);
        }

        //
        // Access Methods
        //

        public bool NeedsRelocation
        {
            get
            {
                if((m_usage      & Runtime.MemoryUsage     .Relocation        ) != 0 ||
                   (m_attributes & Runtime.MemoryAttributes.LoadedAtEntrypoint) != 0  )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public uint Address
        {
            get
            {
                return m_address;
            }
        }

        public byte[] Payload
        {
            get
            {
                return m_payload;
            }
        }

        public string SectionName
        {
            get
            {
                return m_sectionName;
            }
        }

        public Runtime.MemoryAttributes Attributes
        {
            get
            {
                return m_attributes;
            }
        }

        public Runtime.MemoryUsage Usage
        {
            get
            {
                return m_usage;
            }
        }
    }
}