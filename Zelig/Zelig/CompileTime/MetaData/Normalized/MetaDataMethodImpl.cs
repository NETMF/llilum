//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataMethodImpl : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MetaDataTypeDefinitionBase m_classObject;
        internal MetaDataMethodBase         m_body;
        internal MetaDataMethodAbstract     m_declaration;

        //
        // Constructor Methods
        //

        internal MetaDataMethodImpl( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataMethodImpl) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataMethodImpl other = (MetaDataMethodImpl)obj;

                if(m_classObject == other.m_classObject &&
                   m_body        == other.m_body        &&
                   m_declaration == other.m_declaration  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_classObject.GetHashCode() ^
                   m_body       .GetHashCode() ^
                   m_declaration.GetHashCode();
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

        public MetaDataTypeDefinitionBase Class
        {
            get
            {
                return m_classObject;
            }
        }

        public MetaDataMethodBase Body
        {
            get
            {
                return m_body;
            }
        }

        public MetaDataMethodAbstract Declaration
        {
            get
            {
                return m_declaration;
            }
        }

        // Debug Methods

        public override String ToString()
        {
            return "MetaDataMethodImpl(" + m_classObject + "," + m_body + "," + m_declaration + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
