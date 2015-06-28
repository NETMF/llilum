//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataImplMap : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal ImplementationMapAttributes m_flags;
        internal IMetaDataMemberForwarded    m_memberForwarded;
        internal String                      m_importName;
        internal MetaDataModule              m_importScope;

        //
        // Constructor Methods
        //

        internal MetaDataImplMap( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataImplMap) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataImplMap other = (MetaDataImplMap)obj;

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

        public ImplementationMapAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public IMetaDataMemberForwarded MemberForwarded
        {
            get
            {
                return m_memberForwarded;
            }
        }

        public String ImportName
        {
            get
            {
                return m_importName;
            }
        }

        public MetaDataModule ImportScope
        {
            get
            {
                return m_importScope;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataImplMap(" + m_flags.ToString( "x" ) + "," + m_memberForwarded + "," + m_importName + "," + m_importScope + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
