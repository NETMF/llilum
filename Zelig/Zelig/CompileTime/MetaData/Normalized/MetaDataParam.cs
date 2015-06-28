//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataParam : MetaDataObject,
        IMetaDataHasConstant,
        IMetaDataHasFieldMarshal,
        IMetaDataUnique
    {
        //
        // State
        //

        internal ParamAttributes  m_flags;
        internal short            m_sequence;
        internal String           m_name;
        internal MetaDataConstant m_constant;

        //
        // Constructor Methods
        //

        internal MetaDataParam( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataParam) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataParam other = (MetaDataParam)obj;

                if(m_flags    == other.m_flags    &&
                   m_sequence == other.m_sequence &&
                   m_name     == other.m_name     &&
                   m_constant == other.m_constant  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_flags                  ^
                   (int)m_sequence               ^
                        m_name    .GetHashCode() ;
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            throw new NotNormalized( "MakeUnique" );
        }

        //
        // Access Methods
        //

        public ParamAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public short Sequence
        {
            get
            {
                return m_sequence;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public string FullName
        {
            get
            {
                return this.Name;
            }
        }

        public Object DefaultValue
        {
            get
            {
                if((m_flags & ParamAttributes.HasDefault) != 0)
                {
                    return m_constant.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        // Debug Methods

        public override String ToString()
        {
            return "MetaDataParam(" + m_name + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
