//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataProperty : MetaDataObject,
        IMetaDataHasConstant,
        IMetaDataHasSemantic,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionAbstract m_owner;
        internal          PropertyAttributes             m_flags;
        internal          String                         m_name;
        internal          SignatureMethod                m_type;
        internal          MetaDataConstant               m_constant;

        //
        // Constructor Methods
        //

        internal MetaDataProperty( MetaDataTypeDefinitionAbstract owner ,
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
            if(obj is MetaDataProperty) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataProperty other = (MetaDataProperty)obj;

                if(m_owner == other.m_owner &&
                   m_flags == other.m_flags &&
                   m_name  == other.m_name  &&
                   m_type  == other.m_type   )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_flags               ^
                        m_name .GetHashCode() ;
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

        public PropertyAttributes Flags
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

        public SignatureMethod Type
        {
            get
            {
                return m_type;
            }
        }

        public Object DefaultValue
        {
            get
            {
                if((m_flags & PropertyAttributes.HasDefault) != 0)
                {
                    return m_constant.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataProperty(" + m_flags.ToString( "x" ) + "," + m_name + "," + m_type + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
