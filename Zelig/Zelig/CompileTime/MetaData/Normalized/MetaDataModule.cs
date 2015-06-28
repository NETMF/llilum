//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataModule : MetaDataObject,
        IMetaDataResolutionScope,
        IMetaDataUnique
    {
        //
        // State
        //

        internal short  m_generation;
        internal String m_name;
        internal Guid   m_mvid;

        //
        // Constructor Methods
        //

        internal MetaDataModule( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataModule) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataModule other = (MetaDataModule)obj;

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

        public short Generation
        {
            get
            {
                return m_generation;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public Guid MVID
        {
            get
            {
                return m_mvid;
            }
        }

        // Debug Methods

        public override String ToString()
        {
            return "MetaDataModule(" + m_name + "," + m_mvid + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
