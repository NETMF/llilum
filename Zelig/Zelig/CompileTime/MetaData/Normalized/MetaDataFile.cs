//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataFile : MetaDataObject,
        IMetaDataImplementation,
        IMetaDataUnique
    {
        //
        // State
        //

        internal FileAttributes m_flags;
        internal String         m_name;
        internal byte[]         m_hashValue;

        //
        // Constructor Methods
        //

        internal MetaDataFile( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataFile) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataFile other = (MetaDataFile)obj;

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

        public FileAttributes Flags
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

        public byte[] HashValue
        {
            get
            {
                return m_hashValue;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataFile(" );

            sb.Append( m_flags.ToString( "x" ) );
            sb.Append( "," );
            sb.Append( m_name );
            sb.Append( ",[" );
            for(int i = 0; i < m_hashValue.Length; i++)
            {
                if(i != 0) sb.Append( "," );

                sb.Append( "0x" );
                sb.Append( m_hashValue[i].ToString( "x2" ) );
            }
            sb.Append( "])" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
