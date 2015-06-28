//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataEvent : MetaDataObject,
        IMetaDataHasSemantic,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionAbstract m_owner;
        internal          EventAttributes                m_flags;
        internal          String                         m_name;
        internal          MetaDataTypeDefinitionAbstract m_eventType;

        //
        // Constructor Methods
        //

        internal MetaDataEvent( MetaDataTypeDefinitionAbstract owner ,
                                int                            token ) : base( token )
        {
            m_owner = owner;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataEvent) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataEvent other = (MetaDataEvent)obj;

                if(m_owner     == other.m_owner     &&
                   m_flags     == other.m_flags     &&
                   m_name      == other.m_name      &&
                   m_eventType == other.m_eventType  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_flags               ^
                        m_name .GetHashCode() ^ 0x00DEAD03;
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return m_owner.MakeUnique( this );
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract Owner
        {
            get
            {
                return m_owner;
            }
        }

        public EventAttributes Flags
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

        // Returns one of MetaData{TypeDef,TypeRef,TypeSpec}
        public MetaDataTypeDefinitionAbstract Type
        {
            get
            {
                return m_eventType;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataEvent(" + m_flags + "," + m_name + "," + m_eventType + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            writer.WriteLine( ".event {0} {1} {2} {3}", TokenToString( m_token ), m_flags, m_name, m_eventType.ToStringWithAbbreviations( writer ) );
        }
    }
}
