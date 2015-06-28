//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataFieldLayout : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal int m_offset;

        //
        // Constructor Methods
        //

        internal MetaDataFieldLayout( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataFieldLayout) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataFieldLayout other = (MetaDataFieldLayout)obj;

                if(m_offset == other.m_offset)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_offset;
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

        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        // Debug Methods

        public override String ToString()
        {
            return "MetaDataFieldLayout(" + m_offset + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
