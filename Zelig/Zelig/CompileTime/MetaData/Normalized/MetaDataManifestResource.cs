//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;
    using System.Collections.Generic;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataManifestResource : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataAssembly    m_owner;
        internal int                          m_offset;
        internal ManifestResourceAttributes   m_flags;
        internal String                       m_name;
        internal byte[]                       m_data;
        internal IMetaDataImplementation      m_implementation;
        internal Dictionary< string, object > m_values;

        //
        // Constructor Methods
        //

        internal MetaDataManifestResource( MetaDataAssembly owner ,
                                           int              token ) : base( token )
        {
            m_owner = owner;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataManifestResource) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataManifestResource other = (MetaDataManifestResource)obj;

                if(m_owner          == other.m_owner          &&
                   m_offset         == other.m_offset         &&
                   m_flags          == other.m_flags          &&
                   m_name           == other.m_name           &&
                   m_implementation == other.m_implementation  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            int res = m_name.GetHashCode();

            res ^= m_owner.GetHashCode();

            return res;
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return (MetaDataObject)m_owner.MakeUnique( (IMetaDataUnique)this );
        }

        //
        // Access Methods
        //

        public MetaDataAssembly Owner
        {
            get
            {
                return m_owner;
            }
        }

        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        public ManifestResourceAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public IMetaDataImplementation Implementation
        {
            get
            {
                return m_implementation;
            }
        }

        public byte[] Data
        {
            get
            {
                return m_data;
            }
        }

        public Dictionary< string, object > Values
        {
            get
            {
                return m_values;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataManifestResource(" + m_name + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            writer.WriteLine( ".mresource {0} {1} {2}", TokenToString( m_token ), m_flags, m_name );
        }
    }
}
