//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MetaDataTypeDefinitionArray : MetaDataTypeDefinitionAbstract
    {
        //
        // State
        //

        internal MetaDataTypeDefinitionAbstract   m_objectType;
        internal MetaDataTypeDefinitionAbstract[] m_interfaces;
        internal MetaDataMethodBase[]             m_methods;


        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionArray( MetaDataAssembly owner ,
                                              int              token ) : base( owner, token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        protected bool InnerEquals( MetaDataTypeDefinitionArray other )
        {
            if(m_objectType == other.m_objectType)
            {
                return true;
            }

            return false;
        }

        protected int InnerGetHashCode()
        {
            return m_objectType.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool IsOpenType
        {
            get
            {
                return m_objectType.IsOpenType;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_objectType.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_objectType.UsesMethodParameters;
            }
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract ObjectType
        {
            get
            {
                return m_objectType;
            }
        }

        public MetaDataTypeDefinitionAbstract[] Interfaces
        {
            get
            {
                return m_interfaces;
            }
        }

        public MetaDataMethodBase[] Methods
        {
            get
            {
                return m_methods;
            }
        }
    }
}
