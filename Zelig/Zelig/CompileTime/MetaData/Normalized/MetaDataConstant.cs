//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataConstant : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal Object m_value;

        //
        // Constructor Methods
        //

        internal MetaDataConstant( int token ) : base( token )
        {
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataConstant) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataConstant other = (MetaDataConstant)obj;

                return Object.Equals( m_value, other.m_value );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 0xDEAD01; // Just to have something...
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

        public object Value
        {
            get
            {
                return m_value;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataConstant(" );

            sb.Append( m_value );

            sb.Append( ")" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
