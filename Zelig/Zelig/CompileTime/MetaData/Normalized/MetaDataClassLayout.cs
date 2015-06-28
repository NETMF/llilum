//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataClassLayout : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal short m_packingSize;
        internal int   m_classSize;

        //
        // Constructor Methods
        //

        internal MetaDataClassLayout( int token ) : base( token )
        {
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataClassLayout) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataClassLayout other = (MetaDataClassLayout)obj;

                if(m_packingSize == other.m_packingSize &&
                   m_classSize   == other.m_classSize    )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_packingSize << 10 ^
                   (int)m_classSize         ;
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

        public short PackingSize
        {
            get
            {
                return m_packingSize;
            }
        }

        public int ClassSize
        {
            get
            {
                return m_classSize;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataClassLayout(" + m_packingSize + "," + m_classSize + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
