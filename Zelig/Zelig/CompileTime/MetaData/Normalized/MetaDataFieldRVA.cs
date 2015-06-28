//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataFieldRVA : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal byte[] m_dataBytes;

        //
        // Constructor Methods
        //

        internal MetaDataFieldRVA( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataFieldRVA) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataFieldRVA other = (MetaDataFieldRVA)obj;

                return ArrayUtility.ByteArrayEquals( m_dataBytes, other.m_dataBytes );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_dataBytes.GetHashCode();
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

        public byte[] DataBytes
        {
            get
            {
                return m_dataBytes;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataFieldRVA(" );

            for(int i = 0; i < m_dataBytes.Length; i++)
            {
                if(i != 0) sb.Append( "," );

                sb.Append( m_dataBytes[i].ToString( "x2" ) );
            }

            sb.Append( ")" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
