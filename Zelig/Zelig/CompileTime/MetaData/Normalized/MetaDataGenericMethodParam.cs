//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataGenericMethodParam : MetaDataGenericParam,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataMethodGeneric m_owner;

        //
        // Constructor Methods
        //

        internal MetaDataGenericMethodParam( MetaDataMethodGeneric owner ,
                                             int                   token ) : base( token )
        {
            m_owner = owner;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataGenericMethodParam) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataGenericMethodParam other = (MetaDataGenericMethodParam)obj;

                if(m_owner == other.m_owner)
                {
                    return InnerEquals( other );
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return InnerGetHashCode();
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return m_owner.MakeUnique( this );
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataGenericMethodParam(" + m_owner.FullName + "," + m_name + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
