//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataMethodSemantics : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MethodSemanticAttributes m_semantic;
        internal MetaDataMethod           m_method;
        internal IMetaDataHasSemantic     m_association;

        //
        // Constructor Methods
        //

        internal MetaDataMethodSemantics( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataMethodSemantics) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataMethodSemantics other = (MetaDataMethodSemantics)obj;

                throw new NotNormalized( "MetaDataEquality" );
            }

            return false;
        }

        public override int GetHashCode()
        {
            throw new NotNormalized( "MetaDataEquality" );
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

        public MethodSemanticAttributes Semantic
        {
            get
            {
                return m_semantic;
            }
        }

        public MetaDataMethod Method
        {
            get
            {
                return m_method;
            }
        }

        public IMetaDataHasSemantic Association
        {
            get
            {
                return m_association;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataMethodSemantics(" + m_semantic + "," + m_method + "," + m_association + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
