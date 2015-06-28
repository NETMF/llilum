//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MetaDataGenericParam : MetaDataObject
    {
        //
        // State
        //

        internal short                            m_number;
        internal GenericParameterAttributes       m_flags;
        internal String                           m_name;
        internal MetaDataTypeDefinitionAbstract[] m_genericParamConstraints;

        //
        // Constructor Methods
        //

        internal MetaDataGenericParam( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        protected bool InnerEquals( MetaDataGenericParam other )
        {
            if(m_number == other.m_number &&
               m_flags  == other.m_flags  &&
               m_name   == other.m_name    )
            {
                if(ArrayUtility.ArrayEquals( m_genericParamConstraints, other.m_genericParamConstraints ))
                {
                    return true;
                }
            }

            return false;
        }

        protected int InnerGetHashCode()
        {
            return (int)m_number               ^
                   (int)m_flags                ^
                        m_name  .GetHashCode() ;
        }

        //
        // Access Methods
        //

        public short Number
        {
            get
            {
                return m_number;
            }
        }

        public GenericParameterAttributes Flags
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

        public MetaDataTypeDefinitionAbstract[] GenericParamConstraints
        {
            get
            {
                return m_genericParamConstraints;
            }
        }

        //
        // Debug Methods
        //

        public String ToString( IMetaDataDumper context )
        {
            return MetaDataTypeDefinitionAbstract.ParameterToString( context, m_number, this is MetaDataGenericMethodParam, m_name );
        }
    }
}
