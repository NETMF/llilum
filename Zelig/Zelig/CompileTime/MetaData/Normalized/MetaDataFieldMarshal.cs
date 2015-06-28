//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataFieldMarshal : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal IMetaDataHasFieldMarshal m_owner;
        internal MarshalSpec              m_nativeType;

        //
        // Constructor Methods
        //

        internal MetaDataFieldMarshal( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataFieldMarshal) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataFieldMarshal other = (MetaDataFieldMarshal)obj;

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

        public IMetaDataHasFieldMarshal Owner
        {
            get
            {
                return m_owner;
            }
        }

        public MarshalSpec NativeType
        {
            get
            {
                return m_nativeType;
            }
        }

        // Debug Methods

        public override String ToString()
        {
            return "MetaDataFieldMarshal(" + ((MetaDataObject)m_owner).ToString() + "," + m_nativeType + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
