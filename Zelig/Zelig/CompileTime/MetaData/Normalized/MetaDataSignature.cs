//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;


    public abstract class MetaDataSignature : MetaDataObject
    {
        //
        // State
        //

        internal MetaDataTypeDefinitionAbstract[] m_modifiers; 

        //
        // Constructor Methods
        //

        protected MetaDataSignature( int token ) : base( token )
        {
        }

        //
        // Helper Methods
        //

        protected bool InnerEquals( MetaDataSignature other )
        {
            if(ArrayUtility.ArrayEquals( m_modifiers, other.m_modifiers ))
            {
                return true;
            }

            return false;
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract[] Modifiers
        {
            get
            {
                return m_modifiers;
            }
        }

        //
        // Debug Methods
        //
    }
}
